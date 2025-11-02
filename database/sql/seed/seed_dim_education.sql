\set ON_ERROR_STOP on
SET search_path = marketstat, public;

CREATE TEMP TABLE staging_education_temp (
    code            TEXT,
    specialty       TEXT,
    field           TEXT,
    general_field   TEXT,
    education_level TEXT
);

\copy staging_education_temp(code, specialty, field, general_field, education_level) FROM '/home/andres/Desktop/7-semester/marketstat/server/database/datasets/dim_education_dataset.csv' WITH (FORMAT csv, HEADER true, DELIMITER ',');

BEGIN;
INSERT INTO marketstat.dim_education (
    specialty_code,
    specialty_name,
    education_level_name
)
SELECT DISTINCT
	TRIM(s.code),
	TRIM(s.specialty),
	TRIM(s.education_level)
FROM
	staging_education_temp s
WHERE NOT EXISTS (
	SELECT 1
	FROM marketstat.dim_education de
	WHERE de.specialty_code = TRIM(s.code)
		OR (de.specialty_name = TRIM(s.specialty) AND de.education_level_name = TRIM(s.education_level))
);

COMMIT;
