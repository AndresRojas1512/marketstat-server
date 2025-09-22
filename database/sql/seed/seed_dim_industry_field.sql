SET search_path = marketstat, public;
\set ON_ERROR_STOP on



DROP TABLE IF EXISTS staging_industry_fields_temp;

CREATE TEMP TABLE staging_industry_fields_temp (
    industry_field_code TEXT,
    industry_field_name TEXT
);


\set enriched_industry_csv_path '/home/andres/Desktop/6Semester/SoftwareDesign/PPO/database/datasets/dim_industry_field_dataset.csv'

\copy staging_industry_fields_temp(industry_field_code, industry_field_name) FROM '/home/andres/Desktop/6Semester/SoftwareDesign/PPO/database/datasets/dim_industry_field_dataset.csv' WITH (FORMAT CSV, HEADER TRUE, DELIMITER ',');

SELECT count(*) AS staged_rows FROM staging_industry_fields_temp;

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

