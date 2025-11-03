\set ON_ERROR_STOP on
SET search_path = marketstat, public;

CREATE TEMP TABLE staging_industry_fields_temp (
    industry_field_code TEXT,
    industry_field_name TEXT
);

\copy staging_industry_fields_temp(industry_field_code, industry_field_name) FROM '/home/andres/Desktop/7-semester/marketstat/server/database/datasets/static/dim_industry_field_dataset.csv' WITH (FORMAT CSV, HEADER TRUE, DELIMITER ',');

SELECT count(*) FROM staging_industry_fields_temp;

BEGIN;

INSERT INTO marketstat.dim_industry_field (industry_field_code, industry_field_name)
SELECT
    TRIM(s.industry_field_code),
    TRIM(s.industry_field_name)
FROM
    staging_industry_fields_temp s
WHERE NOT EXISTS (
    SELECT 1
    FROM marketstat.dim_industry_field dif
    WHERE dif.industry_field_code = TRIM(s.industry_field_code)
        OR dif.industry_field_name = TRIM(s.industry_field_name)
);

COMMIT;
