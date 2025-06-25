\set ON_ERROR_STOP on
SET search_path = marketstat, public;

DROP TABLE IF EXISTS staging_standard_job_roles_temp;
CREATE TEMP TABLE staging_standard_job_roles_temp (
    standard_job_role_code  TEXT,
    standard_job_role_title TEXT,
    industry_field_name     TEXT
);
\echo 'Temporary staging table "staging_standard_job_roles_temp" created.'

-- Define the path to your CSV file.
-- IMPORTANT: Update this path to your actual source file.
\set enriched_sjr_csv_path '/home/andres/Desktop/6Semester/SoftwareDesign/PPO/database/datasets/dim_standard_job_role_industry_field.csv'

-- Copy data from the CSV into the temporary staging table
\echo 'Copying data from CSV into temporary staging table...'
\copy staging_standard_job_roles_temp(standard_job_role_code, standard_job_role_title, industry_field_name) FROM '/home/andres/Desktop/6Semester/SoftwareDesign/PPO/database/datasets/dim_standard_job_role_industry_field_dataset.csv' WITH (FORMAT CSV, HEADER TRUE, DELIMITER ',');

SELECT count(*) AS staged_rows FROM staging_standard_job_roles_temp;
\echo 'Data copied successfully to staging table.'

-- Begin a transaction to insert data into the final dimension table
\echo 'Inserting data from staging table into marketstat.dim_standard_job_role...'
BEGIN;

-- Insert data from the staging table, joining with dim_industry_field to get the foreign key ID.
INSERT INTO marketstat.dim_standard_job_role (standard_job_role_code, standard_job_role_title, industry_field_id)
SELECT
    TRIM(s.standard_job_role_code),
    TRIM(s.standard_job_role_title),
    dif.industry_field_id
FROM
    staging_standard_job_roles_temp s
JOIN
    marketstat.dim_industry_field dif ON TRIM(s.industry_field_name) = dif.industry_field_name
WHERE
    s.standard_job_role_code IS NOT NULL AND s.standard_job_role_code <> ''
    AND s.standard_job_role_title IS NOT NULL AND s.standard_job_role_title <> ''
-- Use the stable code for conflict resolution.
ON CONFLICT (standard_job_role_code) DO NOTHING;

COMMIT;
\echo 'Transaction committed. Data insertion into dim_standard_job_role attempted.'

-- The TEMP TABLE is automatically dropped at the end of the session.

-- Verify by selecting some data
\echo '--- First 5 rows from marketstat.dim_standard_job_role: ---'
SELECT * FROM marketstat.dim_standard_job_role ORDER BY standard_job_role_id LIMIT 5;

\echo '--- Total rows in marketstat.dim_standard_job_role: ---'
SELECT COUNT(*) FROM marketstat.dim_standard_job_role;

\echo '--- dim_standard_job_role seeding script finished. ---'counts above for staged, unmapped, and final total.'