\set ON_ERROR_STOP on
SET search_path = marketstat, public;

DROP TABLE IF EXISTS staging_standard_job_roles;

CREATE TEMP TABLE staging_standard_job_roles (
    csv_code                TEXT,
    standard_job_role_name  TEXT,
    industry_field_name     TEXT
);


\copy staging_standard_job_roles(csv_code, standard_job_role_name, industry_field_name) FROM '/home/andres/Desktop/6Semester/SoftwareDesign/PPO/database/datasets/standard_job_roles_industry_fields.csv' WITH (FORMAT csv, HEADER true, DELIMITER ',');

SELECT COUNT(*) AS staged_job_roles_count
  FROM staging_standard_job_roles;

BEGIN;

INSERT INTO marketstat.dim_standard_job_role (standard_job_role_title, industry_field_id)
SELECT
    sjr.standard_job_role_name,
    dif.industry_field_id
FROM
    staging_standard_job_roles sjr
JOIN
    marketstat.dim_industry_field dif ON sjr.industry_field_name = dif.industry_field_name
WHERE
    sjr.standard_job_role_name IS NOT NULL AND sjr.standard_job_role_name <> '' -- Ensure title is not null or empty
ON CONFLICT (standard_job_role_title) DO NOTHING;

COMMIT;

DROP TABLE staging_standard_job_roles;

SELECT COUNT(*) AS total_standard_job_roles_in_dim FROM marketstat.dim_standard_job_role;
\echo 'Successfully attempted to seed dim_standard_job_role table.'
\echo 'Check counts above for staged, unmapped, and final total.'