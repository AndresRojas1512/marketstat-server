\set ON_ERROR_STOP on
SET search_path = marketstat, public;

CREATE TEMP TABLE staging_locations_temp (
    id_from_csv   INT,
    city_name     TEXT,
    oblast_name   TEXT,
    district_name TEXT
);

\copy staging_locations_temp(id_from_csv, city_name, oblast_name, district_name) FROM '/home/andres/Desktop/7-semester/marketstat/server/database/datasets/static/dim_location_dataset.csv' WITH (FORMAT csv, HEADER TRUE, DELIMITER ',');

BEGIN;

INSERT INTO marketstat.dim_location (district_name, oblast_name, city_name)
SELECT DISTINCT
    TRIM(s.district_name),
    TRIM(s.oblast_name),
    TRIM(s.city_name)
FROM
    staging_locations_temp s
WHERE NOT EXISTS (
    SELECT 1
    FROM marketstat.dim_location dl
    WHERE dl.district_name = TRIM(s.district_name)
        AND dl.oblast_name = TRIM(s.oblast_name)
        AND dl.city_name = TRIM(s.city_name)
);


COMMIT;
