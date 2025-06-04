\set ON_ERROR_STOP on

DROP TABLE IF EXISTS staging_locations;

CREATE TEMP TABLE staging_locations (
    location_id   INT,
    city_name     TEXT,
    oblast_name   TEXT,
    district_name TEXT
);

\copy staging_locations(location_id, city_name, oblast_name, district_name) FROM '/home/andres/Desktop/6Semester/SoftwareDesign/PPO/database/datasets/dim_locations_dataset.csv' WITH (FORMAT csv, HEADER);

BEGIN;

INSERT INTO marketstat.dim_federal_district (district_name)
SELECT DISTINCT district_name
    FROM staging_locations
ON CONFLICT (district_name) DO NOTHING;

INSERT INTO marketstat.dim_oblast (oblast_name, district_id)
SELECT DISTINCT sl.oblast_name, fd.district_id
    FROM staging_locations sl
    JOIN marketstat.dim_federal_district fd
        ON sl.district_name = fd.district_name
ON CONFLICT (oblast_name) DO NOTHING;

INSERT INTO marketstat.dim_city (city_name, oblast_id)
SELECT DISTINCT sl.city_name, ob.oblast_id
    FROM staging_locations sl
    JOIN marketstat.dim_oblast ob
        ON sl.oblast_name = ob.oblast_name
ON CONFLICT (city_name, oblast_id) DO NOTHING;

COMMIT;

DROP TABLE staging_locations;
