\set ON_ERROR_STOP on
SET search_path = marketstat, public;

DROP TABLE IF EXISTS staging_standard_job_roles_temp;
CREATE TEMP TABLE staging_standard_job_roles_temp (
    standard_job_role_code  TEXT,
    standard_job_role_title TEXT,
    industry_field_name     TEXT
);

\set enriched_sjr_csv_path '/home/andres/Desktop/6Semester/SoftwareDesign/PPO/database/datasets/dim_standard_job_role_industry_field.csv'

\copy staging_standard_job_roles_temp(standard_job_role_code, standard_job_role_title, industry_field_name) FROM '/home/andres/Desktop/6Semester/SoftwareDesign/PPO/database/datasets/dim_standard_job_role_industry_field_dataset.csv' WITH (FORMAT CSV, HEADER TRUE, DELIMITER ',');

SELECT count(*) AS staged_rows FROM staging_standard_job_roles_temp;

BEGIN;

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
ON CONFLICT (standard_job_role_code) DO NOTHING;

COMMIT;


SELECT * FROM marketstat.dim_standard_job_role ORDER BY standard_job_role_id LIMIT 5;

SELECT COUNT(*) FROM marketstat.dim_standard_job_role;

