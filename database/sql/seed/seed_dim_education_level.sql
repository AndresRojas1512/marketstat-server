\set ON_ERROR_STOP on

DROP TABLE IF EXISTS staging_education_levels;

CREATE TEMP TABLE staging_education_levels (
    code            TEXT,
    specialty       TEXT,
    field           TEXT,
    general_field   TEXT,
    education_level TEXT
);

\copy staging_education_levels(code,specialty,field,general_field,education_level) FROM '/home/andres/Desktop/6Semester/SoftwareDesign/PPO/database/datasets/dim_education_dataset.csv' WITH (FORMAT csv, HEADER true);

SELECT COUNT(DISTINCT education_level) AS staged_levels
  FROM staging_education_levels;

BEGIN;
INSERT INTO marketstat.dim_education_level (education_level_name)
SELECT DISTINCT education_level
  FROM staging_education_levels
ON CONFLICT (education_level_name) DO NOTHING;
COMMIT;

DROP TABLE staging_education_levels;
