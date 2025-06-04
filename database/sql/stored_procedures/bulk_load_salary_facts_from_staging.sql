-- Run as marketstat_administrator to CREATE OR REPLACE
SET search_path = marketstat, public;

-- Drop the procedure if it exists to ensure clean replacement with the correct signature
DROP PROCEDURE IF EXISTS marketstat.bulk_load_salary_facts_from_staging(IN TEXT, OUT INT, OUT INT);

CREATE OR REPLACE PROCEDURE marketstat.bulk_load_salary_facts_from_staging(
    IN    p_source_staging_table_name     TEXT,
    OUT   p_inserted_count                INT,
    OUT   p_skipped_count                 INT
)
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
DECLARE
    v_processing_staging_table_name TEXT := 'processing_facts_temp_' || translate(gen_random_uuid()::text, '-', '');
    v_sql TEXT;
    v_updated_count INT;
BEGIN
    p_inserted_count := 0;
    p_skipped_count := 0;

    RAISE NOTICE '[ETL PROCEDURE] Starting bulk load. Source staging table: %', p_source_staging_table_name;

    -- 1. Create the internal temporary processing table
    v_sql := format('CREATE TEMP TABLE %I (id SERIAL PRIMARY KEY, recorded_date_text TEXT, city_name TEXT, oblast_name TEXT, employer_name TEXT, standard_job_role_title TEXT, job_role_title TEXT, hierarchy_level_name TEXT, employee_birth_date_text TEXT, employee_career_start_date_text TEXT, salary_amount NUMERIC(18,2), bonus_amount NUMERIC(18,2), date_id INT, oblast_id INT, city_id INT, employer_id INT, standard_job_role_id INT, hierarchy_level_id INT, job_role_id INT, employee_id INT, error_message TEXT) ON COMMIT DROP;', v_processing_staging_table_name);
    EXECUTE v_sql;
    RAISE NOTICE '[ETL PROCEDURE] Internal processing table % created.', v_processing_staging_table_name;

    -- 2. Populate the internal temporary processing table
    RAISE NOTICE '[ETL PROCEDURE] Loading data from source % into %', p_source_staging_table_name, v_processing_staging_table_name;
    BEGIN
        v_sql := format('INSERT INTO %I (recorded_date_text, city_name, oblast_name, employer_name, standard_job_role_title, job_role_title, hierarchy_level_name, employee_birth_date_text, employee_career_start_date_text, salary_amount, bonus_amount) SELECT recorded_date_text, city_name, oblast_name, employer_name, standard_job_role_title, job_role_title, hierarchy_level_name, employee_birth_date_text, employee_career_start_date_text, salary_amount, bonus_amount FROM %I;', v_processing_staging_table_name, p_source_staging_table_name);
        EXECUTE v_sql;
    EXCEPTION WHEN OTHERS THEN RAISE WARNING '[ETL PROCEDURE] CRITICAL ERROR during INSERT: %. SQLSTATE: %', SQLERRM, SQLSTATE; EXECUTE format('DROP TABLE IF EXISTS %I;', v_processing_staging_table_name); p_inserted_count := -1; p_skipped_count := -1; RAISE; END;

    -- 3. Trim whitespace
    EXECUTE format('UPDATE %I SET recorded_date_text = TRIM(recorded_date_text), city_name = TRIM(city_name), oblast_name = TRIM(oblast_name), employer_name = TRIM(employer_name), standard_job_role_title = TRIM(standard_job_role_title), job_role_title = TRIM(job_role_title), hierarchy_level_name = TRIM(hierarchy_level_name), employee_birth_date_text = TRIM(employee_birth_date_text), employee_career_start_date_text = TRIM(employee_career_start_date_text) WHERE error_message IS NULL;',v_processing_staging_table_name);
    RAISE NOTICE '[ETL PROCEDURE] Data loaded and trimmed. Starting dimension ID resolution...';

    -- 4. Dimension ID Resolution
    -- 4.1 Dates (Get-or-Create)
    RAISE NOTICE '[ETL PROCEDURE] Processing dates...';
    EXECUTE format('WITH s AS (SELECT recorded_date_text FROM %I WHERE date_id IS NULL AND error_message IS NULL AND recorded_date_text IS NOT NULL AND recorded_date_text <> '''' AND pg_catalog.pg_input_is_valid(recorded_date_text, ''date'') GROUP BY recorded_date_text), u AS (INSERT INTO marketstat.dim_date (full_date, year, quarter, month) SELECT CAST(s.recorded_date_text AS DATE), EXTRACT(YEAR FROM CAST(s.recorded_date_text AS DATE))::SMALLINT, EXTRACT(QUARTER FROM CAST(s.recorded_date_text AS DATE))::SMALLINT, EXTRACT(MONTH FROM CAST(s.recorded_date_text AS DATE))::SMALLINT FROM s ON CONFLICT (full_date) DO UPDATE SET full_date = EXCLUDED.full_date RETURNING date_id, full_date) UPDATE %I t SET date_id = u.date_id FROM u WHERE CAST(t.recorded_date_text AS DATE) = u.full_date AND t.date_id IS NULL AND t.error_message IS NULL;',v_processing_staging_table_name,v_processing_staging_table_name);
    EXECUTE format('UPDATE %I SET error_message = COALESCE(error_message, '''') || %L WHERE date_id IS NULL AND recorded_date_text IS NOT NULL AND recorded_date_text <> '''' AND error_message IS NULL AND NOT pg_catalog.pg_input_is_valid(recorded_date_text, ''date'');',v_processing_staging_table_name,'Date string has invalid format for casting; ');
    EXECUTE format('UPDATE %I SET error_message = COALESCE(error_message, '''') || %L WHERE date_id IS NULL AND recorded_date_text IS NOT NULL AND recorded_date_text <> '''' AND error_message IS NULL;',v_processing_staging_table_name,'Date not resolved (valid format but failed processing); ');

    -- 4.2 Oblasts (Lookup-Only)
    RAISE NOTICE '[ETL PROCEDURE] Processing oblasts (lookup only)...';
    EXECUTE format('UPDATE %I ssf SET oblast_id = dof.oblast_id FROM marketstat.dim_oblast dof WHERE ssf.oblast_name = dof.oblast_name AND ssf.oblast_id IS NULL AND ssf.error_message IS NULL;', v_processing_staging_table_name);
    EXECUTE format('UPDATE %I SET error_message = COALESCE(error_message, '''') || %L WHERE oblast_id IS NULL AND oblast_name IS NOT NULL AND oblast_name <> '''' AND error_message IS NULL;', v_processing_staging_table_name, 'Oblast not found in dim_oblast; ');

    -- 4.3 Cities (Lookup-Only, depends on resolved OblastId)
    RAISE NOTICE '[ETL PROCEDURE] Processing cities (lookup only)...';
    EXECUTE format('UPDATE %I ssf SET city_id = dc.city_id FROM marketstat.dim_city dc WHERE ssf.city_name = dc.city_name AND ssf.oblast_id = dc.oblast_id AND ssf.city_id IS NULL AND ssf.oblast_id IS NOT NULL AND ssf.error_message IS NULL;', v_processing_staging_table_name);
    EXECUTE format('UPDATE %I SET error_message = COALESCE(error_message, '''') || %L WHERE city_id IS NULL AND oblast_id IS NOT NULL AND city_name IS NOT NULL AND city_name <> '''' AND error_message IS NULL;', v_processing_staging_table_name, 'City not found in specified Oblast in dim_city; ');
    EXECUTE format('UPDATE %I SET error_message = COALESCE(error_message, '''') || %L WHERE city_id IS NULL AND oblast_id IS NULL AND city_name IS NOT NULL AND city_name <> '''' AND error_message IS NULL;', v_processing_staging_table_name, 'City not resolved (Oblast was not found); ');


    -- 4.4 Employers (Lookup-Only)
    RAISE NOTICE '[ETL PROCEDURE] Processing employers (lookup only)...';
    EXECUTE format('UPDATE %I ssf SET employer_id = de.employer_id FROM marketstat.dim_employer de WHERE ssf.employer_name = de.employer_name AND ssf.employer_id IS NULL AND ssf.error_message IS NULL;',v_processing_staging_table_name);
    EXECUTE format('UPDATE %I SET error_message = COALESCE(error_message, '''') || %L WHERE employer_id IS NULL AND employer_name IS NOT NULL AND employer_name <> '''' AND error_message IS NULL;', v_processing_staging_table_name, 'Employer not found in dim_employer; ');

    -- 4.5 Standard Job Roles (Lookup-Only)
    RAISE NOTICE '[ETL PROCEDURE] Processing standard job roles (lookup only)...';
    EXECUTE format('UPDATE %I ssf SET standard_job_role_id = dsjr.standard_job_role_id FROM marketstat.dim_standard_job_role dsjr WHERE ssf.standard_job_role_title = dsjr.standard_job_role_title AND ssf.standard_job_role_id IS NULL AND ssf.error_message IS NULL;', v_processing_staging_table_name);
    EXECUTE format('UPDATE %I SET error_message = COALESCE(error_message, '''') || %L WHERE standard_job_role_id IS NULL AND standard_job_role_title IS NOT NULL AND standard_job_role_title <> '''' AND error_message IS NULL;', v_processing_staging_table_name, 'Standard Job Role not found in dim_standard_job_role; ');

    -- 4.6 Hierarchy Levels (Lookup-Only)
    RAISE NOTICE '[ETL PROCEDURE] Processing hierarchy levels (lookup only)...';
    EXECUTE format('UPDATE %I ssf SET hierarchy_level_id = dhl.hierarchy_level_id FROM marketstat.dim_hierarchy_level dhl WHERE ssf.hierarchy_level_name = dhl.hierarchy_level_name AND ssf.hierarchy_level_id IS NULL AND ssf.error_message IS NULL;', v_processing_staging_table_name);
    EXECUTE format('UPDATE %I SET error_message = COALESCE(error_message, '''') || %L WHERE hierarchy_level_id IS NULL AND hierarchy_level_name IS NOT NULL AND hierarchy_level_name <> '''' AND error_message IS NULL;', v_processing_staging_table_name, 'Hierarchy Level not found in dim_hierarchy_level; ');

    -- 4.7 Job Roles (dim_job_role) (Get-or-Create, depends on resolved StandardJobRoleId and HierarchyLevelId)
    RAISE NOTICE '[ETL PROCEDURE] Processing job roles (dim_job_role)...';
    EXECUTE format('WITH jrp AS (SELECT DISTINCT staging.job_role_title, staging.standard_job_role_id, staging.hierarchy_level_id FROM %I staging WHERE staging.job_role_id IS NULL AND staging.error_message IS NULL AND staging.standard_job_role_id IS NOT NULL AND staging.hierarchy_level_id IS NOT NULL AND staging.job_role_title IS NOT NULL AND staging.job_role_title <> ''''), ujr AS (INSERT INTO marketstat.dim_job_role (job_role_title, standard_job_role_id, hierarchy_level_id) SELECT jrp.job_role_title, jrp.standard_job_role_id, jrp.hierarchy_level_id FROM jrp ON CONFLICT (job_role_title, standard_job_role_id, hierarchy_level_id) DO UPDATE SET job_role_title = EXCLUDED.job_role_title RETURNING job_role_id, job_role_title, standard_job_role_id, hierarchy_level_id) UPDATE %I st SET job_role_id = ujr.job_role_id FROM ujr WHERE st.job_role_title = ujr.job_role_title AND st.standard_job_role_id = ujr.standard_job_role_id AND st.hierarchy_level_id = ujr.hierarchy_level_id AND st.job_role_id IS NULL AND st.error_message IS NULL;',v_processing_staging_table_name, v_processing_staging_table_name);
    GET DIAGNOSTICS v_updated_count = ROW_COUNT;
    RAISE NOTICE '[ETL PROCEDURE] Step 4.7: Updated job_role_id for % rows in staging table.', v_updated_count;
    EXECUTE format('UPDATE %I SET error_message = COALESCE(error_message, '''') || %L WHERE job_role_id IS NULL AND standard_job_role_id IS NOT NULL AND hierarchy_level_id IS NOT NULL AND job_role_title IS NOT NULL AND job_role_title <> '''' AND error_message IS NULL;', v_processing_staging_table_name, 'Job Role (dim_job_role) not resolved (ensure Standard Job Role and Hierarchy Level were found); ');

    -- 4.8 Employees (Get-or-Create)
    RAISE NOTICE '[ETL PROCEDURE] Processing employees...';
    EXECUTE format('WITH etp AS (SELECT DISTINCT employee_birth_date_text, employee_career_start_date_text FROM %I WHERE employee_id IS NULL AND error_message IS NULL AND employee_birth_date_text IS NOT NULL AND employee_birth_date_text <> '''' AND pg_catalog.pg_input_is_valid(employee_birth_date_text, ''date'') AND employee_career_start_date_text IS NOT NULL AND employee_career_start_date_text <> '''' AND pg_catalog.pg_input_is_valid(employee_career_start_date_text, ''date'')), cetp AS (SELECT etp.employee_birth_date_text, etp.employee_career_start_date_text, CAST(etp.employee_birth_date_text AS DATE) AS birth_date, CAST(etp.employee_career_start_date_text AS DATE) AS career_start_date FROM etp), ie AS (INSERT INTO marketstat.dim_employee (birth_date, career_start_date) SELECT cetp.birth_date, cetp.career_start_date FROM cetp ON CONFLICT (birth_date, career_start_date) DO UPDATE SET birth_date = EXCLUDED.birth_date RETURNING employee_id, birth_date, career_start_date) UPDATE %I ssf SET employee_id = ie.employee_id FROM ie JOIN cetp ON ie.birth_date = cetp.birth_date AND ie.career_start_date = cetp.career_start_date WHERE ssf.employee_birth_date_text = cetp.employee_birth_date_text AND ssf.employee_career_start_date_text = cetp.employee_career_start_date_text AND ssf.employee_id IS NULL AND ssf.error_message IS NULL;',v_processing_staging_table_name, v_processing_staging_table_name);
    EXECUTE format('UPDATE %I SET error_message = COALESCE(error_message, '''') || %L WHERE employee_id IS NULL AND employee_birth_date_text IS NOT NULL AND employee_birth_date_text <> '''' AND employee_career_start_date_text IS NOT NULL AND employee_career_start_date_text <> '''' AND error_message IS NULL AND (NOT pg_catalog.pg_input_is_valid(employee_birth_date_text, ''date'') OR NOT pg_catalog.pg_input_is_valid(employee_career_start_date_text, ''date''));', v_processing_staging_table_name, 'Employee birth/career date string has invalid format for casting; ');
    EXECUTE format('UPDATE %I SET error_message = COALESCE(error_message, '''') || %L WHERE employee_id IS NULL AND employee_birth_date_text IS NOT NULL AND employee_birth_date_text <> '''' AND employee_career_start_date_text IS NOT NULL AND employee_career_start_date_text <> '''' AND error_message IS NULL;', v_processing_staging_table_name, 'Employee not resolved (valid format but failed processing); ');

    -- 5. Insert into fact_salaries
    RAISE NOTICE '[ETL PROCEDURE] Inserting into fact_salaries...';
    v_sql := format('INSERT INTO marketstat.fact_salaries (date_id, city_id, employer_id, job_role_id, employee_id, salary_amount, bonus_amount) SELECT date_id, city_id, employer_id, job_role_id, employee_id, salary_amount, bonus_amount FROM %I WHERE error_message IS NULL AND date_id IS NOT NULL AND city_id IS NOT NULL AND employer_id IS NOT NULL AND job_role_id IS NOT NULL AND employee_id IS NOT NULL;', v_processing_staging_table_name );
    EXECUTE v_sql; GET DIAGNOSTICS p_inserted_count = ROW_COUNT;
    RAISE NOTICE '[ETL PROCEDURE] Successfully inserted % salary facts.', p_inserted_count;

    -- 6. Log skipped rows
    EXECUTE format('SELECT COUNT(*) FROM %I WHERE error_message IS NOT NULL;', v_processing_staging_table_name) INTO p_skipped_count;
    RAISE NOTICE '[ETL PROCEDURE] Skipped % rows due to errors.', p_skipped_count;
    IF p_skipped_count > 0 THEN
        RAISE NOTICE '[ETL PROCEDURE] Clearing previous failed load data from marketstat.failed_salary_facts_load...';
        TRUNCATE TABLE marketstat.failed_salary_facts_load;
        RAISE NOTICE '[ETL PROCEDURE] Inserting % new failed rows into marketstat.failed_salary_facts_load...', p_skipped_count;
        v_sql := format('INSERT INTO marketstat.failed_salary_facts_load (run_timestamp, recorded_date_text, city_name, oblast_name, employer_name, standard_job_role_title, job_role_title, hierarchy_level_name, employee_birth_date_text, employee_career_start_date_text, salary_amount, bonus_amount, error_message) SELECT CURRENT_TIMESTAMP, recorded_date_text, city_name, oblast_name, employer_name, standard_job_role_title, job_role_title, hierarchy_level_name, employee_birth_date_text, employee_career_start_date_text, salary_amount, bonus_amount, error_message FROM %I WHERE error_message IS NOT NULL;', v_processing_staging_table_name);
        EXECUTE v_sql;
        RAISE NOTICE '[ETL PROCEDURE] Details of % failed rows saved to marketstat.failed_salary_facts_load', p_skipped_count;
    ELSE
        RAISE NOTICE '[ETL PROCEDURE] No failed rows to log for this run. Clearing marketstat.failed_salary_facts_load.';
        TRUNCATE TABLE marketstat.failed_salary_facts_load;
    END IF;

    -- 7. Clean up
    EXECUTE format('DROP TABLE IF EXISTS %I;', v_processing_staging_table_name);
    RAISE NOTICE '[ETL PROCEDURE] Internal processing table % dropped.', v_processing_staging_table_name;
    RAISE NOTICE '[ETL PROCEDURE] Bulk load procedure finished. Inserted: %, Skipped: %.', p_inserted_count, p_skipped_count;
EXCEPTION WHEN OTHERS THEN RAISE WARNING '[ETL PROCEDURE] An unexpected error: % - %', SQLSTATE, SQLERRM; EXECUTE format('DROP TABLE IF EXISTS %I;', v_processing_staging_table_name); IF p_inserted_count = 0 AND p_skipped_count = 0 THEN BEGIN EXECUTE format('SELECT COUNT(*) FROM %I', p_source_staging_table_name) INTO p_skipped_count; p_inserted_count := 0; EXCEPTION WHEN OTHERS THEN p_inserted_count := -1; p_skipped_count := -1; END; END IF; RAISE; END;
$$;

ALTER PROCEDURE marketstat.bulk_load_salary_facts_from_staging(IN TEXT, OUT INT, OUT INT) OWNER TO marketstat_administrator;
GRANT EXECUTE ON PROCEDURE marketstat.bulk_load_salary_facts_from_staging(IN TEXT, OUT INT, OUT INT) TO marketstat_etl_user;

\echo 'Procedure marketstat.bulk_load_salary_facts_from_staging (lookup-only for key dims) created/replaced.'
