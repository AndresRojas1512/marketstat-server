\set ON_ERROR_STOP on
SET search_path = marketstat, public;

DROP TABLE IF EXISTS staging_hierarchy_levels;

DROP TABLE IF EXISTS staging_hierarchy_levels_temp;
CREATE TEMP TABLE staging_hierarchy_levels_temp (
    level_code          TEXT,
    hierarchy_level_name TEXT
);

\set hierarchy_level_csv_path '/home/andres/Desktop/6Semester/SoftwareDesign/PPO/database/datasets/dim_hierarchy_level_dataset.csv'

\copy staging_hierarchy_levels_temp(level_code, hierarchy_level_name) FROM '/home/andres/Desktop/6Semester/SoftwareDesign/PPO/database/datasets/dim_hierarchy_level_dataset.csv' WITH (FORMAT CSV, HEADER TRUE, DELIMITER ',');

SELECT count(*) AS staged_rows FROM staging_hierarchy_levels_temp;

BEGIN;

INSERT INTO marketstat.dim_hierarchy_level (hierarchy_level_code, hierarchy_level_name)
SELECT
    TRIM(s.level_code),
    TRIM(s.hierarchy_level_name)
FROM
    staging_hierarchy_levels_temp s
WHERE
    s.level_code IS NOT NULL AND s.level_code <> ''
    AND s.hierarchy_level_name IS NOT NULL AND s.hierarchy_level_name <> ''
ON CONFLICT (hierarchy_level_code) DO NOTHING;

COMMIT;


SELECT * FROM marketstat.dim_hierarchy_level ORDER BY hierarchy_level_id LIMIT 5;

SELECT COUNT(*) FROM marketstat.dim_hierarchy_level;

