\set ON_ERROR_STOP on

DROP TABLE IF EXISTS staging_industry_fields;

CREATE TEMP TABLE staging_industry_fields (
    industry_field TEXT
);

\copy staging_industry_fields(industry_field) FROM '/home/andres/Desktop/6Semester/SoftwareDesign/PPO/database/datasets/industry_fields_dataset.csv' CSV HEADER;

SELECT count(*) AS staged_rows FROM staging_industry_fields;

BEGIN;
INSERT INTO marketstat.dim_industry_field (industry_field_name)
SELECT DISTINCT industry_field
    FROM staging_industry_fields
ON CONFLICT (industry_field_name) DO NOTHING;
COMMIT;

DROP TABLE staging_industry_fields;
