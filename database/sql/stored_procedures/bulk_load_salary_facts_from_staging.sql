-- This is the complete and final version of the bulk load procedure.
-- It correctly handles the enriched staging table with employee_ref_id, gender, and education details.

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
BEGIN
    -- Initialize OUT parameters
    p_inserted_count := 0;
    p_skipped_count := 0;

    RAISE NOTICE '[ETL PROCEDURE] Starting bulk load. Source staging table: %', p_source_staging_table_name;

    -- 1. Create the internal temporary processing table with ALL necessary columns
    v_sql := format('
        CREATE TEMP TABLE %I (
            id SERIAL PRIMARY KEY,
            recorded_date_text TEXT, city_name TEXT, oblast_name TEXT, employer_name TEXT,
            standard_job_role_title TEXT, job_role_title TEXT, hierarchy_level_name TEXT,
            employee_ref_id TEXT, employee_birth_date_text TEXT, employee_career_start_date_text TEXT, gender TEXT,
            education_level_name TEXT, specialty TEXT, specialty_code TEXT, graduation_year SMALLINT,
            salary_amount NUMERIC(18,2), bonus_amount NUMERIC(18,2),
            -- Resolved IDs
            date_id INT, oblast_id INT, city_id INT, employer_id INT,
            standard_job_role_id INT, hierarchy_level_id INT, job_role_id INT, employee_id INT,
            education_level_id INT, education_id INT,
            error_message TEXT
        ) ON COMMIT DROP;',
        v_processing_staging_table_name
    );
    EXECUTE v_sql;
    RAISE NOTICE '[ETL PROCEDURE] Internal processing table % created.', v_processing_staging_table_name;

    -- 2. Populate the internal temporary processing table from the source staging table
    RAISE NOTICE '[ETL PROCEDURE] Loading data from source % into %', p_source_staging_table_name, v_processing_staging_table_name;
    BEGIN
        v_sql := format('
            INSERT INTO %I (
                recorded_date_text, city_name, oblast_name, employer_name,
                standard_job_role_title, job_role_title, hierarchy_level_name,
                employee_ref_id, employee_birth_date_text, employee_career_start_date_text, gender,
                education_level_name, specialty, specialty_code, graduation_year,
                salary_amount, bonus_amount
            ) SELECT
                recorded_date_text, city_name, oblast_name, employer_name,
                standard_job_role_title, job_role_title, hierarchy_level_name,
                employee_ref_id, employee_birth_date_text, employee_career_start_date_text, gender,
                education_level_name, specialty, specialty_code, graduation_year,
                salary_amount, bonus_amount
            FROM %I;',
            v_processing_staging_table_name, p_source_staging_table_name
        );
        EXECUTE v_sql;
    EXCEPTION WHEN OTHERS THEN RAISE WARNING '[ETL PROCEDURE] CRITICAL ERROR during INSERT: %. SQLSTATE: %', SQLERRM, SQLSTATE; EXECUTE format('DROP TABLE IF EXISTS %I;', v_processing_staging_table_name); p_inserted_count := -1; p_skipped_count := -1; RAISE; END;

    -- 3. Trim whitespace from all text fields
    EXECUTE format('UPDATE %I SET
        recorded_date_text = TRIM(recorded_date_text), city_name = TRIM(city_name), oblast_name = TRIM(oblast_name),
        employer_name = TRIM(employer_name), standard_job_role_title = TRIM(standard_job_role_title),
        job_role_title = TRIM(job_role_title), hierarchy_level_name = TRIM(hierarchy_level_name),
        employee_ref_id = TRIM(employee_ref_id), employee_birth_date_text = TRIM(employee_birth_date_text),
        employee_career_start_date_text = TRIM(employee_career_start_date_text), gender = TRIM(gender),
        education_level_name = TRIM(education_level_name), specialty = TRIM(specialty), specialty_code = TRIM(specialty_code)
        WHERE error_message IS NULL;',v_processing_staging_table_name);
    RAISE NOTICE '[ETL PROCEDURE] Data loaded and trimmed. Starting dimension ID resolution...';

    -- 4. Dimension ID Resolution
    -- 4.1: Dates (Get-or-Create)
    RAISE NOTICE '[ETL PROCEDURE] Processing dates...';
    EXECUTE format('WITH s AS (SELECT recorded_date_text FROM %I WHERE date_id IS NULL AND error_message IS NULL AND recorded_date_text IS NOT NULL AND recorded_date_text <> '''' AND pg_catalog.pg_input_is_valid(recorded_date_text, ''date'') GROUP BY recorded_date_text), u AS (INSERT INTO marketstat.dim_date (full_date, year, quarter, month) SELECT CAST(s.recorded_date_text AS DATE), EXTRACT(YEAR FROM CAST(s.recorded_date_text AS DATE))::SMALLINT, EXTRACT(QUARTER FROM CAST(s.recorded_date_text AS DATE))::SMALLINT, EXTRACT(MONTH FROM CAST(s.recorded_date_text AS DATE))::SMALLINT FROM s ON CONFLICT (full_date) DO UPDATE SET full_date = EXCLUDED.full_date RETURNING date_id, full_date) UPDATE %I t SET date_id = u.date_id FROM u WHERE CAST(t.recorded_date_text AS DATE) = u.full_date AND t.date_id IS NULL AND t.error_message IS NULL;',v_processing_staging_table_name,v_processing_staging_table_name);

    -- 4.2: Lookup-Only Dimensions
    RAISE NOTICE '[ETL PROCEDURE] Processing lookup dimensions (Oblast, City, Employer, StandardRole, Hierarchy)...';
    EXECUTE format('UPDATE %I ssf SET oblast_id = dof.oblast_id FROM marketstat.dim_oblast dof WHERE ssf.oblast_name = dof.oblast_name AND ssf.oblast_id IS NULL AND ssf.error_message IS NULL;', v_processing_staging_table_name);
    EXECUTE format('UPDATE %I ssf SET city_id = dc.city_id FROM marketstat.dim_city dc WHERE ssf.city_name = dc.city_name AND ssf.oblast_id = dc.oblast_id AND ssf.city_id IS NULL AND ssf.oblast_id IS NOT NULL AND ssf.error_message IS NULL;', v_processing_staging_table_name);
    EXECUTE format('UPDATE %I ssf SET employer_id = de.employer_id FROM marketstat.dim_employer de WHERE ssf.employer_name = de.employer_name AND ssf.employer_id IS NULL AND ssf.error_message IS NULL;',v_processing_staging_table_name);
    EXECUTE format('UPDATE %I ssf SET standard_job_role_id = dsjr.standard_job_role_id FROM marketstat.dim_standard_job_role dsjr WHERE ssf.standard_job_role_title = dsjr.standard_job_role_title AND ssf.standard_job_role_id IS NULL AND ssf.error_message IS NULL;', v_processing_staging_table_name);
    EXECUTE format('UPDATE %I ssf SET hierarchy_level_id = dhl.hierarchy_level_id FROM marketstat.dim_hierarchy_level dhl WHERE TRIM(ssf.hierarchy_level_name) = TRIM(dhl.hierarchy_level_name) AND ssf.hierarchy_level_id IS NULL AND ssf.error_message IS NULL;', v_processing_staging_table_name);

    -- 4.3: Job Roles (Get-or-Create, composite key)
    RAISE NOTICE '[ETL PROCEDURE] Processing job roles (dim_job_role)...';
    EXECUTE format('WITH jrp AS (SELECT DISTINCT staging.job_role_title, staging.standard_job_role_id, staging.hierarchy_level_id FROM %I staging WHERE staging.job_role_id IS NULL AND staging.error_message IS NULL AND staging.standard_job_role_id IS NOT NULL AND staging.hierarchy_level_id IS NOT NULL AND staging.job_role_title IS NOT NULL AND staging.job_role_title <> ''''), ujr AS (INSERT INTO marketstat.dim_job_role (job_role_title, standard_job_role_id, hierarchy_level_id) SELECT jrp.job_role_title, jrp.standard_job_role_id, jrp.hierarchy_level_id FROM jrp ON CONFLICT (job_role_title, standard_job_role_id, hierarchy_level_id) DO UPDATE SET job_role_title = EXCLUDED.job_role_title RETURNING job_role_id, job_role_title, standard_job_role_id, hierarchy_level_id) UPDATE %I st SET job_role_id = ujr.job_role_id FROM ujr WHERE st.job_role_title = ujr.job_role_title AND st.standard_job_role_id = ujr.standard_job_role_id AND st.hierarchy_level_id = ujr.hierarchy_level_id AND st.job_role_id IS NULL AND st.error_message IS NULL;',v_processing_staging_table_name, v_processing_staging_table_name);

    -- 4.4: Employees (Get-or-Create, using employee_ref_id)
    RAISE NOTICE '[ETL PROCEDURE] Processing employees...';
    EXECUTE format('WITH etp AS (SELECT DISTINCT ssf.employee_ref_id, ssf.employee_birth_date_text, ssf.employee_career_start_date_text, ssf.gender FROM %I ssf WHERE ssf.employee_id IS NULL AND ssf.error_message IS NULL AND ssf.employee_ref_id IS NOT NULL AND ssf.employee_ref_id <> '''' AND pg_catalog.pg_input_is_valid(ssf.employee_birth_date_text, ''date'') AND pg_catalog.pg_input_is_valid(ssf.employee_career_start_date_text, ''date'')), ue AS (INSERT INTO marketstat.dim_employee (employee_ref_id, birth_date, career_start_date, gender) SELECT etp.employee_ref_id, CAST(etp.employee_birth_date_text AS DATE), CAST(etp.employee_career_start_date_text AS DATE), etp.gender FROM etp ON CONFLICT (employee_ref_id) DO UPDATE SET birth_date = EXCLUDED.birth_date, career_start_date = EXCLUDED.career_start_date, gender = EXCLUDED.gender RETURNING employee_id, employee_ref_id) UPDATE %I ssf SET employee_id = ue.employee_id FROM ue WHERE ssf.employee_ref_id = ue.employee_ref_id AND ssf.employee_id IS NULL AND ssf.error_message IS NULL;',v_processing_staging_table_name, v_processing_staging_table_name);

    -- 4.5: Education Resolution and Linking
    RAISE NOTICE '[ETL PROCEDURE] Processing education details...';
    EXECUTE format('WITH ltp AS (SELECT DISTINCT education_level_name FROM %I WHERE error_message IS NULL AND education_level_id IS NULL AND education_level_name IS NOT NULL AND education_level_name <> ''''), ul AS (INSERT INTO marketstat.dim_education_level(education_level_name) SELECT ltp.education_level_name FROM ltp ON CONFLICT (education_level_name) DO UPDATE SET education_level_name = EXCLUDED.education_level_name RETURNING education_level_id, education_level_name) UPDATE %I ssf SET education_level_id = ul.education_level_id FROM ul WHERE ssf.education_level_name = ul.education_level_name AND ssf.education_level_id IS NULL AND ssf.error_message IS NULL;',v_processing_staging_table_name, v_processing_staging_table_name);
    EXECUTE format('WITH etp AS (SELECT DISTINCT specialty, specialty_code, education_level_id FROM %I WHERE error_message IS NULL AND education_id IS NULL AND education_level_id IS NOT NULL AND specialty_code IS NOT NULL AND specialty_code <> ''''), ue AS (INSERT INTO marketstat.dim_education(specialty, specialty_code, education_level_id) SELECT etp.specialty, etp.specialty_code, etp.education_level_id FROM etp ON CONFLICT (specialty_code) DO UPDATE SET specialty = EXCLUDED.specialty, education_level_id = EXCLUDED.education_level_id RETURNING education_id, specialty_code) UPDATE %I ssf SET education_id = ue.education_id FROM ue WHERE ssf.specialty_code = ue.specialty_code AND ssf.education_id IS NULL AND ssf.error_message IS NULL;',v_processing_staging_table_name, v_processing_staging_table_name);
    EXECUTE format('INSERT INTO marketstat.dim_employee_education(employee_id, education_id, graduation_year) SELECT DISTINCT ssf.employee_id, ssf.education_id, ssf.graduation_year FROM %I ssf WHERE ssf.error_message IS NULL AND ssf.employee_id IS NOT NULL AND ssf.education_id IS NOT NULL AND ssf.graduation_year IS NOT NULL ON CONFLICT (employee_id, education_id) DO UPDATE SET graduation_year = EXCLUDED.graduation_year;', v_processing_staging_table_name);

    -- 4.6: Set error messages for all failed lookups/creations
    EXECUTE format('UPDATE %I SET error_message = COALESCE(error_message, '''') || %L WHERE date_id IS NULL AND recorded_date_text IS NOT NULL AND recorded_date_text <> '''' AND error_message IS NULL;',v_processing_staging_table_name,'Date not resolved; ');
    EXECUTE format('UPDATE %I SET error_message = COALESCE(error_message, '''') || %L WHERE oblast_id IS NULL AND oblast_name IS NOT NULL AND oblast_name <> '''' AND error_message IS NULL;', v_processing_staging_table_name, 'Oblast not found; ');
    EXECUTE format('UPDATE %I SET error_message = COALESCE(error_message, '''') || %L WHERE city_id IS NULL AND city_name IS NOT NULL AND city_name <> '''' AND error_message IS NULL;', v_processing_staging_table_name, 'City not found; ');
    EXECUTE format('UPDATE %I SET error_message = COALESCE(error_message, '''') || %L WHERE employer_id IS NULL AND employer_name IS NOT NULL AND employer_name <> '''' AND error_message IS NULL;', v_processing_staging_table_name, 'Employer not found; ');
    EXECUTE format('UPDATE %I SET error_message = COALESCE(error_message, '''') || %L WHERE standard_job_role_id IS NULL AND standard_job_role_title IS NOT NULL AND standard_job_role_title <> '''' AND error_message IS NULL;', v_processing_staging_table_name, 'Standard Job Role not found; ');
    EXECUTE format('UPDATE %I SET error_message = COALESCE(error_message, '''') || %L WHERE hierarchy_level_id IS NULL AND hierarchy_level_name IS NOT NULL AND hierarchy_level_name <> '''' AND error_message IS NULL;', v_processing_staging_table_name, 'Hierarchy Level not found; ');
    EXECUTE format('UPDATE %I SET error_message = COALESCE(error_message, '''') || %L WHERE job_role_id IS NULL AND job_role_title IS NOT NULL AND job_role_title <> '''' AND error_message IS NULL;', v_processing_staging_table_name, 'Job Role not resolved; ');
    EXECUTE format('UPDATE %I SET error_message = COALESCE(error_message, '''') || %L WHERE employee_id IS NULL AND employee_ref_id IS NOT NULL AND employee_ref_id <> '''' AND error_message IS NULL;', v_processing_staging_table_name, 'Employee not resolved; ');
    EXECUTE format('UPDATE %I SET error_message = COALESCE(error_message, '''') || %L WHERE education_id IS NULL AND specialty_code IS NOT NULL AND specialty_code <> '''' AND error_message IS NULL;', v_processing_staging_table_name, 'Education not resolved; ');


    -- 5. Insert successfully resolved rows into fact_salaries
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
        v_sql := format('INSERT INTO marketstat.failed_salary_facts_load (run_timestamp, recorded_date_text, city_name, oblast_name, employer_name, standard_job_role_title, job_role_title, hierarchy_level_name, employee_ref_id, employee_birth_date_text, employee_career_start_date_text, gender, education_level_name, specialty, specialty_code, graduation_year, salary_amount, bonus_amount, error_message) SELECT CURRENT_TIMESTAMP, recorded_date_text, city_name, oblast_name, employer_name, standard_job_role_title, job_role_title, hierarchy_level_name, employee_ref_id, employee_birth_date_text, employee_career_start_date_text, gender, education_level_name, specialty, specialty_code, graduation_year, salary_amount, bonus_amount, error_message FROM %I WHERE error_message IS NOT NULL;', v_processing_staging_table_name);
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

\echo 'Procedure marketstat.bulk_load_salary_facts_from_staging (with full education ETL) created/replaced.'

