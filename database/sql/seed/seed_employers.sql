\set ON_ERROR_STOP on

DROP TABLE IF EXISTS staging_employers;

CREATE TEMP TABLE staging_employers (
    employer_name   TEXT,
    industry_field  TEXT
);

\copy staging_employers(employer_name, industry_field) FROM '/home/andres/Desktop/6Semester/SoftwareDesign/PPO/database/datasets/employers_dataset.csv' WITH (FORMAT csv, HEADER true);

BEGIN;
INSERT INTO marketstat.dim_industry_field (industry_field_name)
SELECT DISTINCT industry_field
    FROM staging_employers
ON CONFLICT (industry_field_name) DO NOTHING;
COMMIT;

BEGIN;
INSERT INTO marketstat.dim_employer (employer_name)
SELECT DISTINCT employer_name
  FROM staging_employers
ON CONFLICT (employer_name) DO NOTHING;
COMMIT;

BEGIN;
INSERT INTO marketstat.dim_employer_industry_field (employer_id, industry_field_id)
SELECT
    e.employer_id,
    f.industry_field_id
FROM staging_employers s
JOIN marketstat.dim_employer             e ON e.employer_name         = s.employer_name
JOIN marketstat.dim_industry_field       f ON f.industry_field_name   = s.industry_field
ON CONFLICT (employer_id, industry_field_id) DO NOTHING;
COMMIT;

DROP TABLE staging_employers;