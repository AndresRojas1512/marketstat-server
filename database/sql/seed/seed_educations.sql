\set ON_ERROR_STOP on

\echo === Dropping any old staging table ===
DROP TABLE IF EXISTS staging_education;

\echo === Creating temp staging table for full CSV ===
CREATE TEMP TABLE staging_education (
    code            TEXT,
    specialty       TEXT,
    field           TEXT,
    general_field   TEXT,
    education_level TEXT
);

\echo === Loading CSV into staging ===
\copy staging_education(code,specialty,field,general_field,education_level) FROM '/home/andres/Desktop/6Semester/SoftwareDesign/PPO/database/datasets/education_dataset.csv' WITH (FORMAT csv, HEADER true);

\echo === How many rows staged? ===
SELECT COUNT(*) AS staged_rows FROM staging_education;

\echo === Inserting into dim_education ===
BEGIN;
INSERT INTO marketstat.dim_education (
    specialty_code,
    specialty,
    education_level_id
)
SELECT
    se.code,
    se.specialty,
    lvl.education_level_id
FROM staging_education AS se
JOIN marketstat.dim_education_level AS lvl
  ON lvl.education_level_name = se.education_level
-- skip any duplicate specialty_code
WHERE NOT EXISTS (
  SELECT 1
    FROM marketstat.dim_education de
   WHERE de.specialty_code = se.code
);
COMMIT;

\echo === Dropping staging table ===
DROP TABLE staging_education;
