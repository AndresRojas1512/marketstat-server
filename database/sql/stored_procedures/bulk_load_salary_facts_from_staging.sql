-- Run as marketstat_administrator
SET search_path = marketstat, public;

DROP PROCEDURE IF EXISTS marketstat.bulk_load_salary_facts_from_staging(TEXT);

CREATE OR REPLACE PROCEDURE marketstat.bulk_load_salary_facts_from_staging(
    p_source_staging_table_name TEXT -- Name of the caller-created temp table with raw CSV-like data
)
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
DECLARE
    v_inserted_fact_count INT := 0;
    v_skipped_row_count INT := 0;
    v_processing_staging_table_name TEXT := 'processing_facts_temp_' || replace(replace(gen_random_uuid()::text, '-', ''), '{', '') || '}';
    v_sql TEXT;
BEGIN
    RAISE NOTICE '[ETL PROCEDURE] Starting bulk load. Source staging table: %', p_source_staging_table_name;

    -- 1. Create the internal temporary processing table with extra columns for resolved IDs and errors
    v_sql := format('
        CREATE TEMP TABLE %I (
            id SERIAL PRIMARY KEY,
            recorded_date_text TEXT,
            city_name TEXT,
            oblast_name TEXT,
            employer_name TEXT,
            standard_job_role_title TEXT,
            job_role_title TEXT,
            hierarchy_level_name TEXT,
            employee_birth_date_text TEXT,
            employee_career_start_date_text TEXT,
            salary_amount NUMERIC(18,2),
            bonus_amount NUMERIC(18,2),
            -- Resolved IDs
            date_id INT,
            oblast_id INT,
            city_id INT,
            employer_id INT,
            standard_job_role_id INT,
            hierarchy_level_id INT,
            job_role_id INT,
            employee_id INT,
            -- Status tracking
            error_message TEXT
        ) ON COMMIT DROP;', -- Ensures cleanup at the end of the transaction
        v_processing_staging_table_name
    );
    EXECUTE v_sql;
    RAISE NOTICE '[ETL PROCEDURE] Internal processing table % created.', v_processing_staging_table_name;

    -- 2. Populate the internal temporary processing table from the source staging table
    RAISE NOTICE '[ETL PROCEDURE] Loading data from source staging table % into processing table %', p_source_staging_table_name, v_processing_staging_table_name;
    BEGIN
        v_sql := format('
            INSERT INTO %I (
                recorded_date_text, city_name, oblast_name, employer_name,
                standard_job_role_title, job_role_title, hierarchy_level_name,
                employee_birth_date_text, employee_career_start_date_text,
                salary_amount, bonus_amount
            )
            SELECT
                recorded_date_text, city_name, oblast_name, employer_name,
                standard_job_role_title, job_role_title, hierarchy_level_name,
                employee_birth_date_text, employee_career_start_date_text,
                salary_amount, bonus_amount
            FROM %I;',
            v_processing_staging_table_name,
            p_source_staging_table_name
        );
        EXECUTE v_sql;
    EXCEPTION
        WHEN OTHERS THEN
            RAISE WARNING '[ETL PROCEDURE] CRITICAL ERROR during INSERT from source staging table % into %: %. SQLSTATE: %',
                        p_source_staging_table_name, v_processing_staging_table_name, SQLERRM, SQLSTATE;
            EXECUTE format('DROP TABLE IF EXISTS %I;', v_processing_staging_table_name);
            RAISE;
    END;

    -- 3. Trim whitespace from text fields in the internal processing table
    EXECUTE format('UPDATE %I SET
        recorded_date_text = TRIM(recorded_date_text), city_name = TRIM(city_name), oblast_name = TRIM(oblast_name),
        employer_name = TRIM(employer_name), standard_job_role_title = TRIM(standard_job_role_title),
        job_role_title = TRIM(job_role_title), hierarchy_level_name = TRIM(hierarchy_level_name),
        employee_birth_date_text = TRIM(employee_birth_date_text), employee_career_start_date_text = TRIM(employee_career_start_date_text)
        WHERE error_message IS NULL;',
        v_processing_staging_table_name
    );
    RAISE NOTICE '[ETL PROCEDURE] Data loaded and trimmed in processing table. Starting dimension ID resolution...';

    -- 4. Dimension ID Resolution (Steps 4.1 to 4.8)

    -- 4.1 Get/Create dim_date IDs
    RAISE NOTICE '[ETL PROCEDURE] Processing dates...';
    EXECUTE format('
        WITH dates_to_process AS (
            SELECT DISTINCT recorded_date_text FROM %I WHERE date_id IS NULL AND error_message IS NULL AND recorded_date_text IS NOT NULL AND recorded_date_text <> ''''
        ),
        inserted_dates AS (
            INSERT INTO marketstat.dim_date (full_date, year, quarter, month)
            SELECT
                CAST(d.recorded_date_text AS DATE),
                EXTRACT(YEAR FROM CAST(d.recorded_date_text AS DATE))::SMALLINT,
                EXTRACT(QUARTER FROM CAST(d.recorded_date_text AS DATE))::SMALLINT,
                EXTRACT(MONTH FROM CAST(d.recorded_date_text AS DATE))::SMALLINT
            FROM dates_to_process d
            ON CONFLICT (full_date) DO UPDATE SET full_date = EXCLUDED.full_date -- Ensures row exists and returns ID
            RETURNING date_id, full_date
        )
        UPDATE %I staging_table SET date_id = d.date_id
        FROM marketstat.dim_date d -- Join directly with dim_date after upsert
        WHERE CAST(staging_table.recorded_date_text AS DATE) = d.full_date
          AND staging_table.date_id IS NULL AND staging_table.error_message IS NULL
          AND staging_table.recorded_date_text IS NOT NULL AND staging_table.recorded_date_text <> '''';',
        v_processing_staging_table_name, v_processing_staging_table_name
    );
    EXECUTE format('UPDATE %I SET error_message = COALESCE(error_message, '''') || %L WHERE date_id IS NULL AND recorded_date_text IS NOT NULL AND recorded_date_text <> '''' AND error_message IS NULL;',
                   v_processing_staging_table_name, 'Date not resolved or invalid format; ');

    -- 4.2 Get dim_oblast IDs (Lookup only)
    RAISE NOTICE '[ETL PROCEDURE] Processing oblasts...';
    EXECUTE format('UPDATE %I ssf SET oblast_id = dof.oblast_id FROM marketstat.dim_oblast dof
                    WHERE ssf.oblast_name = dof.oblast_name AND ssf.oblast_id IS NULL AND ssf.error_message IS NULL;', v_processing_staging_table_name);
    EXECUTE format('UPDATE %I SET error_message = COALESCE(error_message, '''') || %L WHERE oblast_id IS NULL AND oblast_name IS NOT NULL AND oblast_name <> '''' AND error_message IS NULL;',
                   v_processing_staging_table_name, 'Oblast not found; ');

    -- 4.3 Get/Create dim_city IDs
    RAISE NOTICE '[ETL PROCEDURE] Processing cities...';
    EXECUTE format('
        WITH cities_to_process AS (
            SELECT DISTINCT city_name, oblast_id FROM %I WHERE city_id IS NULL AND oblast_id IS NOT NULL AND error_message IS NULL AND city_name IS NOT NULL AND city_name <> ''''
        ),
        inserted_cities AS (
            INSERT INTO marketstat.dim_city (city_name, oblast_id)
            SELECT ctp.city_name, ctp.oblast_id FROM cities_to_process ctp
            ON CONFLICT (city_name, oblast_id) DO UPDATE SET city_name = EXCLUDED.city_name
            RETURNING city_id, city_name, oblast_id
        )
        UPDATE %I ssf SET city_id = dc.city_id
        FROM marketstat.dim_city dc
        WHERE ssf.city_name = dc.city_name AND ssf.oblast_id = dc.oblast_id
          AND ssf.city_id IS NULL AND ssf.oblast_id IS NOT NULL AND ssf.error_message IS NULL
          AND ssf.city_name IS NOT NULL AND ssf.city_name <> '''';',
        v_processing_staging_table_name, v_processing_staging_table_name
    );
    EXECUTE format('UPDATE %I SET error_message = COALESCE(error_message, '''') || %L WHERE city_id IS NULL AND oblast_id IS NOT NULL AND city_name IS NOT NULL AND city_name <> '''' AND error_message IS NULL;',
                   v_processing_staging_table_name, 'City not resolved (possibly due to missing Oblast ID); ');

    -- 4.4 Get/Create dim_employer IDs
    RAISE NOTICE '[ETL PROCEDURE] Processing employers...';
    EXECUTE format('
        WITH employers_to_process AS (
            SELECT DISTINCT employer_name FROM %I WHERE employer_id IS NULL AND error_message IS NULL AND employer_name IS NOT NULL AND employer_name <> ''''
        ),
        inserted_employers AS (
            INSERT INTO marketstat.dim_employer (employer_name) -- is_public defaults to FALSE
            SELECT etp.employer_name FROM employers_to_process etp
            ON CONFLICT (employer_name) DO UPDATE SET employer_name = EXCLUDED.employer_name
            RETURNING employer_id, employer_name
        )
        UPDATE %I ssf SET employer_id = de.employer_id
        FROM marketstat.dim_employer de
        WHERE ssf.employer_name = de.employer_name
          AND ssf.employer_id IS NULL AND ssf.error_message IS NULL
          AND ssf.employer_name IS NOT NULL AND ssf.employer_name <> '''';',
        v_processing_staging_table_name, v_processing_staging_table_name
    );
    EXECUTE format('UPDATE %I SET error_message = COALESCE(error_message, '''') || %L WHERE employer_id IS NULL AND employer_name IS NOT NULL AND employer_name <> '''' AND error_message IS NULL;',
                   v_processing_staging_table_name, 'Employer not resolved; ');

    -- 4.5 Get dim_standard_job_role IDs (Lookup only)
    RAISE NOTICE '[ETL PROCEDURE] Processing standard job roles...';
    EXECUTE format('UPDATE %I ssf SET standard_job_role_id = dsjr.standard_job_role_id FROM marketstat.dim_standard_job_role dsjr
                    WHERE ssf.standard_job_role_title = dsjr.standard_job_role_title AND ssf.standard_job_role_id IS NULL AND ssf.error_message IS NULL;',
                    v_processing_staging_table_name);
    EXECUTE format('UPDATE %I SET error_message = COALESCE(error_message, '''') || %L WHERE standard_job_role_id IS NULL AND standard_job_role_title IS NOT NULL AND standard_job_role_title <> '''' AND error_message IS NULL;',
                   v_processing_staging_table_name, 'Standard Job Role not found; ');

    -- 4.6 Get dim_hierarchy_level IDs (Lookup only)
    RAISE NOTICE '[ETL PROCEDURE] Processing hierarchy levels...';
    EXECUTE format('UPDATE %I ssf SET hierarchy_level_id = dhl.hierarchy_level_id FROM marketstat.dim_hierarchy_level dhl
                    WHERE ssf.hierarchy_level_name = dhl.hierarchy_level_name AND ssf.hierarchy_level_id IS NULL AND ssf.error_message IS NULL;',
                    v_processing_staging_table_name);
    EXECUTE format('UPDATE %I SET error_message = COALESCE(error_message, '''') || %L WHERE hierarchy_level_id IS NULL AND hierarchy_level_name IS NOT NULL AND hierarchy_level_name <> '''' AND error_message IS NULL;',
                   v_processing_staging_table_name, 'Hierarchy Level not found; ');

    -- 4.7 Get/Create dim_job_role IDs
    RAISE NOTICE '[ETL PROCEDURE] Processing job roles (dim_job_role)...';
    EXECUTE format('
        WITH job_roles_to_process AS (
            SELECT DISTINCT job_role_title, standard_job_role_id, hierarchy_level_id
            FROM %I
            WHERE job_role_id IS NULL AND error_message IS NULL
              AND standard_job_role_id IS NOT NULL AND hierarchy_level_id IS NOT NULL
              AND job_role_title IS NOT NULL AND job_role_title <> ''''
        ),
        inserted_job_roles AS (
            INSERT INTO marketstat.dim_job_role (job_role_title, standard_job_role_id, hierarchy_level_id)
            SELECT jrp.job_role_title, jrp.standard_job_role_id, jrp.hierarchy_level_id
            FROM job_roles_to_process jrp
            ON CONFLICT (job_role_title, standard_job_role_id, hierarchy_level_id)
            DO UPDATE SET job_role_title = EXCLUDED.job_role_title -- Relies on uq_dim_job_role_natural_key
            RETURNING job_role_id, job_role_title, standard_job_role_id, hierarchy_level_id
        )
        UPDATE %I ssf SET job_role_id = djr.job_role_id
        FROM marketstat.dim_job_role djr
        WHERE ssf.job_role_title = djr.job_role_title
          AND ssf.standard_job_role_id = djr.standard_job_role_id
          AND ssf.hierarchy_level_id = djr.hierarchy_level_id
          AND ssf.job_role_id IS NULL AND ssf.error_message IS NULL
          AND ssf.standard_job_role_id IS NOT NULL AND ssf.hierarchy_level_id IS NOT NULL
          AND ssf.job_role_title IS NOT NULL AND ssf.job_role_title <> '''';',
        v_processing_staging_table_name, v_processing_staging_table_name
    );
    EXECUTE format('UPDATE %I SET error_message = COALESCE(error_message, '''') || %L
                    WHERE job_role_id IS NULL AND standard_job_role_id IS NOT NULL AND hierarchy_level_id IS NOT NULL
                      AND job_role_title IS NOT NULL AND job_role_title <> '''' AND error_message IS NULL;',
                   v_processing_staging_table_name, 'Job Role (dim_job_role) not resolved; ');

    -- 4.8 Get/Create dim_employee IDs
    RAISE NOTICE '[ETL PROCEDURE] Processing employees...';
    EXECUTE format('
        WITH employees_to_process AS (
            SELECT DISTINCT
                CAST(employee_birth_date_text AS DATE) AS birth_date,
                CAST(employee_career_start_date_text AS DATE) AS career_start_date
            FROM %I
            WHERE employee_id IS NULL AND error_message IS NULL
              AND employee_birth_date_text IS NOT NULL AND employee_birth_date_text <> ''''
              AND employee_career_start_date_text IS NOT NULL AND employee_career_start_date_text <> ''''
        ),
        inserted_employees AS (
            INSERT INTO marketstat.dim_employee (birth_date, career_start_date)
            SELECT etp.birth_date, etp.career_start_date FROM employees_to_process etp
            ON CONFLICT (birth_date, career_start_date)
            DO UPDATE SET birth_date = EXCLUDED.birth_date -- Relies on uq_dim_employee_natural_key
            RETURNING employee_id, birth_date, career_start_date
        )
        UPDATE %I ssf SET employee_id = de.employee_id
        FROM marketstat.dim_employee de
        WHERE CAST(ssf.employee_birth_date_text AS DATE) = de.birth_date
          AND CAST(ssf.employee_career_start_date_text AS DATE) = de.career_start_date
          AND ssf.employee_id IS NULL AND ssf.error_message IS NULL
          AND ssf.employee_birth_date_text IS NOT NULL AND ssf.employee_birth_date_text <> ''''
          AND ssf.employee_career_start_date_text IS NOT NULL AND ssf.employee_career_start_date_text <> '''';',
        v_processing_staging_table_name, v_processing_staging_table_name
    );
    EXECUTE format('UPDATE %I SET error_message = COALESCE(error_message, '''') || %L
                    WHERE employee_id IS NULL
                      AND employee_birth_date_text IS NOT NULL AND employee_birth_date_text <> ''''
                      AND employee_career_start_date_text IS NOT NULL AND employee_career_start_date_text <> ''''
                      AND error_message IS NULL;',
                   v_processing_staging_table_name, 'Employee not resolved or invalid dates; ');

    -- 5. Insert successfully resolved rows into fact_salaries
    RAISE NOTICE '[ETL PROCEDURE] Inserting into fact_salaries...';
    v_sql := format('
        INSERT INTO marketstat.fact_salaries (date_id, city_id, employer_id, job_role_id, employee_id, salary_amount, bonus_amount)
        SELECT date_id, city_id, employer_id, job_role_id, employee_id, salary_amount, bonus_amount
        FROM %I
        WHERE error_message IS NULL
          AND date_id IS NOT NULL AND city_id IS NOT NULL AND employer_id IS NOT NULL
          AND job_role_id IS NOT NULL AND employee_id IS NOT NULL;',
        v_processing_staging_table_name
    );
    EXECUTE v_sql;
    GET DIAGNOSTICS v_inserted_fact_count = ROW_COUNT;
    RAISE NOTICE '[ETL PROCEDURE] Successfully inserted % salary facts.', v_inserted_fact_count;

    -- 6. Log skipped rows to the permanent 'failed_salary_facts_load' table
    EXECUTE format('SELECT COUNT(*) FROM %I WHERE error_message IS NOT NULL;', v_processing_staging_table_name) INTO v_skipped_row_count;
    RAISE NOTICE '[ETL PROCEDURE] Skipped % rows due to errors.', v_skipped_row_count;

    IF v_skipped_row_count > 0 THEN
        RAISE NOTICE '[ETL PROCEDURE] Clearing previous failed load data from marketstat.failed_salary_facts_load...';
        TRUNCATE TABLE marketstat.failed_salary_facts_load;

        RAISE NOTICE '[ETL PROCEDURE] Inserting % new failed rows into marketstat.failed_salary_facts_load...', v_skipped_row_count;
        v_sql := format('
            INSERT INTO marketstat.failed_salary_facts_load
                (recorded_date_text, city_name, oblast_name, employer_name,
                 standard_job_role_title, job_role_title, hierarchy_level_name,
                 employee_birth_date_text, employee_career_start_date_text,
                 salary_amount, bonus_amount, error_message)
            SELECT
                recorded_date_text, city_name, oblast_name, employer_name,
                standard_job_role_title, job_role_title, hierarchy_level_name,
                employee_birth_date_text, employee_career_start_date_text,
                salary_amount, bonus_amount, error_message
            FROM %I WHERE error_message IS NOT NULL;',
            v_processing_staging_table_name
        );
        EXECUTE v_sql;
        RAISE NOTICE '[ETL PROCEDURE] Details of % failed rows saved to marketstat.failed_salary_facts_load', v_skipped_row_count;
    ELSE
        RAISE NOTICE '[ETL PROCEDURE] No failed rows to log for this run. Previous errors in marketstat.failed_salary_facts_load (if any) are cleared.';
        TRUNCATE TABLE marketstat.failed_salary_facts_load;
    END IF;

    -- 7. Clean up internal processing table (it's ON COMMIT DROP, but explicit drop is also fine)
    EXECUTE format('DROP TABLE IF EXISTS %I;', v_processing_staging_table_name);
    RAISE NOTICE '[ETL PROCEDURE] Internal processing table % dropped.', v_processing_staging_table_name;

    RAISE NOTICE '[ETL PROCEDURE] Bulk load procedure finished.';

EXCEPTION
    WHEN OTHERS THEN
        RAISE WARNING '[ETL PROCEDURE] An unexpected error occurred in bulk_load_salary_facts_from_staging: % - %', SQLSTATE, SQLERRM;
        EXECUTE format('DROP TABLE IF EXISTS %I;', v_processing_staging_table_name);
        RAISE;
END;
$$;

\echo 'Procedure marketstat.bulk_load_salary_facts_from_staging (complete, using pre-created failed_salary_facts_load) created/replaced.'

