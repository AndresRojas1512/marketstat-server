\echo '--- Starting Load Process for Targeted Salary Facts ---'
\set ON_ERROR_STOP on

SET search_path = marketstat, public;

\set csv_file_path '/home/andres/Desktop/6Semester/SoftwareDesign/PPO/database/datasets/fact_salaries.csv'

SELECT translate(gen_random_uuid()::text, '-', '') AS temp_table_suffix_val \gset
\set source_csv_staging_table 'temp_source_csv_data_' :temp_table_suffix_val

\echo 'Generated temporary staging table name will be: ' :'source_csv_staging_table'

BEGIN;

\echo 'Step 2: Creating temporary staging table ' :'source_csv_staging_table' ' for CSV data...'
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

\echo 'Temporary staging table created: ' :'source_csv_staging_table'

\echo 'Step 3: Copying data from CSV file into staging table...'
\echo 'CSV File Path: ' :'csv_file_path'
\copy :source_csv_staging_table FROM :'csv_file_path' WITH (FORMAT CSV, HEADER TRUE, DELIMITER ',');

\echo 'Data copied from CSV to staging table ' :'source_csv_staging_table'.'
SELECT COUNT(*) AS rows_in_staging FROM :"source_csv_staging_table";

\echo 'Step 4: Calling stored procedure marketstat.bulk_load_salary_facts_from_staging...'
CALL marketstat.bulk_load_salary_facts_from_staging('api_fact_uploads_staging');

\echo 'Stored procedure executed.'


COMMIT;

\echo '--- Load Process for Targeted Salary Facts Finished ---'
\echo 'Check marketstat.fact_salaries for new records and marketstat.failed_salary_facts_load for any errors.'

