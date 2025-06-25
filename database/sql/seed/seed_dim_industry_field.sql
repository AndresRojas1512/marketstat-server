SET search_path = marketstat, public;
\set ON_ERROR_STOP on

\echo '--- Preparing to seed dim_industry_field ---'


DROP TABLE IF EXISTS staging_industry_fields_temp;

CREATE TEMP TABLE staging_industry_fields_temp (
    industry_field_code TEXT,
    industry_field_name TEXT
);

\echo 'Temporary staging table "staging_industry_fields_temp" created.'

\set enriched_industry_csv_path '/home/andres/Desktop/6Semester/SoftwareDesign/PPO/database/datasets/dim_industry_field_dataset.csv'

\echo 'Copying data from CSV into temporary staging table...'
\copy staging_industry_fields_temp(industry_field_code, industry_field_name) FROM '/home/andres/Desktop/6Semester/SoftwareDesign/PPO/database/datasets/dim_industry_field_dataset.csv' WITH (FORMAT CSV, HEADER TRUE, DELIMITER ',');

SELECT count(*) AS staged_rows FROM staging_industry_fields_temp;
\echo 'Data copied successfully to staging table.'

\echo 'Inserting data into marketstat.dim_industry_field...'
BEGIN;

INSERT INTO marketstat.dim_industry_field (industry_field_code, industry_field_name)
SELECT
    TRIM(s.industry_field_code),
    TRIM(s.industry_field_name)
FROM
    staging_industry_fields_temp s
WHERE
    s.industry_field_code IS NOT NULL AND s.industry_field_name IS NOT NULL
ON CONFLICT (industry_field_code) DO NOTHING;

COMMIT;
\echo 'Transaction committed. Data inserted into dim_industry_field.'


\echo '--- dim_industry_field seeding script finished. ---'
