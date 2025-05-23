DROP PROCEDURE IF EXISTS marketstat.bulk_load_salary_facts_from_staging(TEXT);
CREATE OR REPLACE PROCEDURE marketstat.bulk_load_salary_facts_from_staging(
    p_source_staging_table_name TEXT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_inserted_fact_count INT := 0;
    v_skipped_row_count INT := 0;
    v_processing_staging_table_name TEXT := 'processing_facts_temp_' || replace(gen_random_uuid()::text, '-', '');
    v_sql TEXT;
BEGIN
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
        ) ON COMMIT DROP;', -- Ensures cleanup
        v_processing_staging_table_name
    );
    EXECUTE v_sql;

    -- 2. Populate the internal temporary processing table from the source staging table
    RAISE NOTICE 'Loading data from source staging table % into processing table: %', p_source_staging_table_name, v_processing_staging_table_name;
    BEGIN
        v_sql := format('
            INSERT INTO %I (recorded_date_text, city_name, oblast_name, employer_name, standard_job_role_title, job_role_title, hierarchy_level_name, employee_birth_date_text, employee_career_start_date_text, salary_amount, bonus_amount)
            SELECT
                recorded_date_text, city_name, oblast_name, employer_name, standard_job_role_title, job_role_title, hierarchy_level_name, employee_birth_date_text, employee_career_start_date_text, salary_amount, bonus_amount
            FROM %I;',
            v_processing_staging_table_name,
            p_source_staging_table_name
        );
        EXECUTE v_sql;
    EXCEPTION
        WHEN OTHERS THEN
            RAISE NOTICE 'Error during INSERT from source staging table %: %. Processing Table: %', p_source_staging_table_name, SQLERRM, v_processing_staging_table_name;
            EXECUTE format('DROP TABLE IF EXISTS %I;', v_processing_staging_table_name);
            RAISE;
            RETURN;
    END;

    -- Trim whitespace from text fields in the internal processing table
    EXECUTE format('UPDATE %I SET
        recorded_date_text = TRIM(recorded_date_text), city_name = TRIM(city_name), oblast_name = TRIM(oblast_name),
        employer_name = TRIM(employer_name), standard_job_role_title = TRIM(standard_job_role_title),
        job_role_title = TRIM(job_role_title), hierarchy_level_name = TRIM(hierarchy_level_name),
        employee_birth_date_text = TRIM(employee_birth_date_text), employee_career_start_date_text = TRIM(employee_career_start_date_text);',
        v_processing_staging_table_name
    );

    RAISE NOTICE 'Data loaded into processing table. Starting dimension ID resolution...';

    -- 4.1 Get/Create dim_date IDs
    RAISE NOTICE 'Processing dates...';
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
            ON CONFLICT (full_date) DO UPDATE SET full_date = EXCLUDED.full_date
            RETURNING date_id, full_date
        ),
        all_relevant_dates AS (
             SELECT date_id, full_date FROM inserted_dates
             UNION ALL
             SELECT dd.date_id, dd.full_date FROM marketstat.dim_date dd JOIN dates_to_process dtp ON dd.full_date = CAST(dtp.recorded_date_text AS DATE)
             WHERE NOT EXISTS (SELECT 1 FROM inserted_dates id2 WHERE id2.full_date = dd.full_date)
        )
        UPDATE %I ssf SET date_id = ard.date_id
        FROM all_relevant_dates ard
        WHERE CAST(ssf.recorded_date_text AS DATE) = ard.full_date AND ssf.date_id IS NULL AND ssf.error_message IS NULL AND ssf.recorded_date_text IS NOT NULL AND ssf.recorded_date_text <> '''';',
        v_processing_staging_table_name, v_processing_staging_table_name
    );
    EXECUTE format('UPDATE %I SET error_message = COALESCE(error_message, '''') || %L WHERE date_id IS NULL AND recorded_date_text IS NOT NULL AND recorded_date_text <> '''' AND error_message IS NULL;',
                   v_processing_staging_table_name, 'Date not resolved; ');

    -- 4.2 Get dim_oblast IDs (Lookup only)
    RAISE NOTICE 'Processing oblasts...';
    EXECUTE format('UPDATE %I ssf SET oblast_id = dof.oblast_id FROM marketstat.dim_oblast dof
                    WHERE ssf.oblast_name = dof.oblast_name AND ssf.oblast_id IS NULL AND ssf.error_message IS NULL;', v_processing_staging_table_name);
    EXECUTE format('UPDATE %I SET error_message = COALESCE(error_message, '''') || %L WHERE oblast_id IS NULL AND oblast_name IS NOT NULL AND oblast_name <> '''' AND error_message IS NULL;',
                   v_processing_staging_table_name, 'Oblast not found; ');

    -- 4.3 Get/Create dim_city IDs
    RAISE NOTICE 'Processing cities...';
    EXECUTE format('
        WITH cities_to_process AS (
            SELECT DISTINCT city_name, oblast_id FROM %I WHERE city_id IS NULL AND oblast_id IS NOT NULL AND error_message IS NULL AND city_name IS NOT NULL AND city_name <> ''''
        ),
        inserted_cities AS (
            INSERT INTO marketstat.dim_city (city_name, oblast_id)
            SELECT ctp.city_name, ctp.oblast_id FROM cities_to_process ctp
            ON CONFLICT (city_name, oblast_id) DO UPDATE SET city_name = EXCLUDED.city_name
            RETURNING city_id, city_name, oblast_id
        ),
        all_relevant_cities AS (
            SELECT city_id, city_name, oblast_id FROM inserted_cities
            UNION ALL
            SELECT dc.city_id, dc.city_name, dc.oblast_id FROM marketstat.dim_city dc JOIN cities_to_process ctp ON dc.city_name = ctp.city_name AND dc.oblast_id = ctp.oblast_id
            WHERE NOT EXISTS (SELECT 1 FROM inserted_cities ic WHERE ic.city_name = dc.city_name AND ic.oblast_id = dc.oblast_id)
        )
        UPDATE %I ssf SET city_id = arc.city_id
        FROM all_relevant_cities arc
        WHERE ssf.city_name = arc.city_name AND ssf.oblast_id = arc.oblast_id AND ssf.city_id IS NULL AND ssf.error_message IS NULL AND ssf.city_name IS NOT NULL AND ssf.city_name <> '''';',
        v_processing_staging_table_name, v_processing_staging_table_name
    );
    EXECUTE format('UPDATE %I SET error_message = COALESCE(error_message, '''') || %L WHERE city_id IS NULL AND oblast_id IS NOT NULL AND city_name IS NOT NULL AND city_name <> '''' AND error_message IS NULL;',
                   v_processing_staging_table_name, 'City not resolved; ');

    -- 4.4 Get/Create dim_employer IDs
    RAISE NOTICE 'Processing employers...';
    EXECUTE format('
        WITH employers_to_process AS (
            SELECT DISTINCT employer_name FROM %I WHERE employer_id IS NULL AND error_message IS NULL AND employer_name IS NOT NULL AND employer_name <> ''''
        ),
        inserted_employers AS (
            INSERT INTO marketstat.dim_employer (employer_name) -- is_public defaults to FALSE
            SELECT etp.employer_name FROM employers_to_process etp
            ON CONFLICT (employer_name) DO UPDATE SET employer_name = EXCLUDED.employer_name
            RETURNING employer_id, employer_name
        ),
        all_relevant_employers AS (
             SELECT employer_id, employer_name FROM inserted_employers
             UNION ALL
             SELECT de.employer_id, de.employer_name FROM marketstat.dim_employer de JOIN employers_to_process etp ON de.employer_name = etp.employer_name
             WHERE NOT EXISTS (SELECT 1 FROM inserted_employers ie WHERE ie.employer_name = de.employer_name)
        )
        UPDATE %I ssf SET employer_id = are.employer_id
        FROM all_relevant_employers are
        WHERE ssf.employer_name = are.employer_name AND ssf.employer_id IS NULL AND ssf.error_message IS NULL AND ssf.employer_name IS NOT NULL AND ssf.employer_name <> '''';',
        v_processing_staging_table_name, v_processing_staging_table_name
    );
    EXECUTE format('UPDATE %I SET error_message = COALESCE(error_message, '''') || %L WHERE employer_id IS NULL AND employer_name IS NOT NULL AND employer_name <> '''' AND error_message IS NULL;',
                   v_processing_staging_table_name, 'Employer not resolved; ');

    -- 4.5 Get dim_standard_job_role IDs (Lookup only)
    RAISE NOTICE 'Processing standard job roles...';
    EXECUTE format('UPDATE %I ssf SET standard_job_role_id = dsjr.standard_job_role_id FROM marketstat.dim_standard_job_role dsjr
                    WHERE ssf.standard_job_role_title = dsjr.standard_job_role_title AND ssf.standard_job_role_id IS NULL AND ssf.error_message IS NULL;',
                    v_processing_staging_table_name);
    EXECUTE format('UPDATE %I SET error_message = COALESCE(error_message, '''') || %L WHERE standard_job_role_id IS NULL AND standard_job_role_title IS NOT NULL AND standard_job_role_title <> '''' AND error_message IS NULL;',
                   v_processing_staging_table_name, 'Standard Job Role not found; ');

    -- 4.6 Get dim_hierarchy_level IDs (Lookup only)
    RAISE NOTICE 'Processing hierarchy levels...';
    EXECUTE format('UPDATE %I ssf SET hierarchy_level_id = dhl.hierarchy_level_id FROM marketstat.dim_hierarchy_level dhl
                    WHERE ssf.hierarchy_level_name = dhl.hierarchy_level_name AND ssf.hierarchy_level_id IS NULL AND ssf.error_message IS NULL;',
                    v_processing_staging_table_name);
    EXECUTE format('UPDATE %I SET error_message = COALESCE(error_message, '''') || %L WHERE hierarchy_level_id IS NULL AND hierarchy_level_name IS NOT NULL AND hierarchy_level_name <> '''' AND error_message IS NULL;',
                   v_processing_staging_table_name, 'Hierarchy Level not found; ');

    -- 4.7 Get/Create dim_job_role IDs
    RAISE NOTICE 'Processing job roles (dim_job_role)...';
    EXECUTE format('
        WITH job_roles_to_process AS (
            SELECT DISTINCT job_role_title, standard_job_role_id, hierarchy_level_id
            FROM %I
            WHERE job_role_id IS NULL AND error_message IS NULL
              AND standard_job_role_id IS NOT NULL AND hierarchy_level_id IS NOT NULL AND job_role_title IS NOT NULL AND job_role_title <> ''''
        ),
        existing_job_roles AS (
            SELECT djr.job_role_id, djr.job_role_title, djr.standard_job_role_id, djr.hierarchy_level_id
            FROM marketstat.dim_job_role djr JOIN job_roles_to_process jrp
                ON djr.job_role_title = jrp.job_role_title AND djr.standard_job_role_id = jrp.standard_job_role_id AND djr.hierarchy_level_id = jrp.hierarchy_level_id
        ),
        job_roles_to_insert_final AS (
            SELECT jrp.job_role_title, jrp.standard_job_role_id, jrp.hierarchy_level_id
            FROM job_roles_to_process jrp LEFT JOIN existing_job_roles ejr
                ON jrp.job_role_title = ejr.job_role_title AND jrp.standard_job_role_id = ejr.standard_job_role_id AND jrp.hierarchy_level_id = ejr.hierarchy_level_id
            WHERE ejr.job_role_id IS NULL
        ),
        inserted_job_roles AS (
            INSERT INTO marketstat.dim_job_role (job_role_title, standard_job_role_id, hierarchy_level_id)
            SELECT jrfinal.job_role_title, jrfinal.standard_job_role_id, jrfinal.hierarchy_level_id FROM job_roles_to_insert_final jrfinal
            ON CONFLICT (job_role_title, standard_job_role_id, hierarchy_level_id) DO UPDATE SET job_role_title = EXCLUDED.job_role_title -- Assumes a unique constraint exists here
            RETURNING job_role_id, job_role_title, standard_job_role_id, hierarchy_level_id
        ),
        all_relevant_job_roles AS (
            SELECT job_role_id, job_role_title, standard_job_role_id, hierarchy_level_id FROM inserted_job_roles
            UNION ALL
            SELECT ejr.job_role_id, ejr.job_role_title, ejr.standard_job_role_id, ejr.hierarchy_level_id FROM existing_job_roles ejr
            WHERE NOT EXISTS (
                SELECT 1 FROM inserted_job_roles ijr
                WHERE ijr.job_role_title = ejr.job_role_title
                  AND ijr.standard_job_role_id = ejr.standard_job_role_id
                  AND ijr.hierarchy_level_id = ejr.hierarchy_level_id
            )
        )
        UPDATE %I ssf
        SET job_role_id = arjr.job_role_id
        FROM all_relevant_job_roles arjr
        WHERE ssf.job_role_title = arjr.job_role_title
          AND ssf.standard_job_role_id = arjr.standard_job_role_id
          AND ssf.hierarchy_level_id = arjr.hierarchy_level_id
          AND ssf.job_role_id IS NULL AND ssf.error_message IS NULL
          AND ssf.standard_job_role_id IS NOT NULL AND ssf.hierarchy_level_id IS NOT NULL AND ssf.job_role_title IS NOT NULL AND ssf.job_role_title <> '''';',
        v_processing_staging_table_name, v_processing_staging_table_name
    );
    EXECUTE format('UPDATE %I SET error_message = COALESCE(error_message, '''') || %L
                    WHERE job_role_id IS NULL AND standard_job_role_id IS NOT NULL AND hierarchy_level_id IS NOT NULL
                      AND job_role_title IS NOT NULL AND job_role_title <> '''' AND error_message IS NULL;',
                   v_processing_staging_table_name, 'Job Role (dim_job_role) not resolved; ');

    -- 4.8 Get/Create dim_employee IDs
    RAISE NOTICE 'Processing employees...';
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
            ON CONFLICT (birth_date, career_start_date) DO UPDATE SET birth_date = EXCLUDED.birth_date -- Assumes a unique constraint exists here
            RETURNING employee_id, birth_date, career_start_date
        ),
        all_relevant_employees AS (
            SELECT employee_id, birth_date, career_start_date FROM inserted_employees
            UNION ALL
            SELECT de.employee_id, de.birth_date, de.career_start_date FROM marketstat.dim_employee de JOIN employees_to_process etp ON de.birth_date = etp.birth_date AND de.career_start_date = etp.career_start_date
            WHERE NOT EXISTS (
                SELECT 1 FROM inserted_employees ie
                WHERE ie.birth_date = de.birth_date AND ie.career_start_date = de.career_start_date
            )
        )
        UPDATE %I ssf SET employee_id = are.employee_id
        FROM all_relevant_employees are
        WHERE CAST(ssf.employee_birth_date_text AS DATE) = are.birth_date AND CAST(ssf.employee_career_start_date_text AS DATE) = are.career_start_date
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
                   v_processing_staging_table_name, 'Employee not resolved; ');

    -- 5. Insert successfully resolved rows into fact_salaries
    RAISE NOTICE 'Inserting into fact_salaries...';
    v_sql := format('
        INSERT INTO marketstat.fact_salaries (date_id, city_id, employer_id, job_role_id, employee_id, salary_amount, bonus_amount)
        SELECT
            date_id, city_id, employer_id, job_role_id, employee_id, salary_amount, bonus_amount
        FROM %I
        WHERE error_message IS NULL
          AND date_id IS NOT NULL AND city_id IS NOT NULL AND employer_id IS NOT NULL
          AND job_role_id IS NOT NULL AND employee_id IS NOT NULL;',
        v_processing_staging_table_name
    );
    EXECUTE v_sql;
    GET DIAGNOSTICS v_inserted_fact_count = ROW_COUNT;
    RAISE NOTICE 'Successfully inserted % salary facts.', v_inserted_fact_count;

    -- 6. Log skipped rows and optionally save them
    EXECUTE format('SELECT COUNT(*) FROM %I WHERE error_message IS NOT NULL;', v_processing_staging_table_name) INTO v_skipped_row_count;
    RAISE NOTICE 'Skipped % rows due to errors.', v_skipped_row_count;

    IF v_skipped_row_count > 0 THEN
        DROP TABLE IF EXISTS marketstat.failed_salary_facts_load;
        EXECUTE format('CREATE TABLE marketstat.failed_salary_facts_load AS SELECT recorded_date_text, city_name, oblast_name, employer_name, standard_job_role_title, job_role_title, hierarchy_level_name, employee_birth_date_text, employee_career_start_date_text, salary_amount, bonus_amount, error_message FROM %I WHERE error_message IS NOT NULL;', v_processing_staging_table_name);
        RAISE NOTICE 'Details of failed rows saved to marketstat.failed_salary_facts_load';
    END IF;

    -- 7. Clean up (internal processing table is ON COMMIT DROP, so it's handled if the procedure commits)
    -- If an exception occurs before commit and it's not caught, the session's temp tables are dropped on session end.
    -- Explicit drop here if not using ON COMMIT DROP or for clarity if preferred.
    EXECUTE format('DROP TABLE IF EXISTS %I;', v_processing_staging_table_name);


    RAISE NOTICE 'Bulk load procedure finished.';

EXCEPTION
    WHEN OTHERS THEN
        RAISE NOTICE 'An unexpected error occurred in bulk_load_salary_facts_from_staging: % - %', SQLSTATE, SQLERRM;
        -- Ensure internal temp table is attempted to be dropped if it exists from a partial run
        EXECUTE format('DROP TABLE IF EXISTS %I;', v_processing_staging_table_name);
        RAISE; -- Re-raise the exception
END;
$$;