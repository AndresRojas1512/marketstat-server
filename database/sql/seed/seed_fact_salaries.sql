\echo '--- Starting Load Process for Targeted Salary Facts ---'
\set ON_ERROR_STOP on

SET search_path = marketstat, public;

\set csv_file_path '/home/andres/Desktop/6Semester/SoftwareDesign/PPO/database/datasets/fact_salaries.csv'

SELECT translate(gen_random_uuid()::text, '-', '') AS temp_table_suffix_val \gset
\set source_csv_staging_table 'temp_source_csv_data_' :temp_table_suffix_val


BEGIN;

CREATE TEMP TABLE :source_csv_staging_table (
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
    bonus_amount NUMERIC(18,2)
) ON COMMIT DROP;


\copy :source_csv_staging_table FROM :'csv_file_path' WITH (FORMAT CSV, HEADER TRUE, DELIMITER ',');

SELECT COUNT(*) AS rows_in_staging FROM :"source_csv_staging_table";

CALL marketstat.bulk_load_salary_facts_from_staging('api_fact_uploads_staging');

COMMIT;


