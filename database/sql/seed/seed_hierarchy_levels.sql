\set ON_ERROR_STOP on
SET search_path = marketstat, public;

DROP TABLE IF EXISTS staging_hierarchy_levels;

CREATE TEMP TABLE staging_hierarchy_levels (
    level_code          TEXT, -- Corresponds to 'level_code' in CSV
    hierarchy_level     TEXT  -- Corresponds to 'hierarchy_level' in CSV
);

\copy staging_hierarchy_levels(level_code, hierarchy_level) FROM '/home/andres/Desktop/6Semester/SoftwareDesign/PPO/database/datasets/hierarchy_levels_dataset.csv' WITH (FORMAT csv, HEADER true, DELIMITER ',');

SELECT COUNT(DISTINCT hierarchy_level) AS staged_hierarchy_levels_count
  FROM staging_hierarchy_levels;

BEGIN;

INSERT INTO marketstat.dim_hierarchy_level (hierarchy_level_name)
SELECT DISTINCT hierarchy_level
  FROM staging_hierarchy_levels
ORDER BY hierarchy_level
ON CONFLICT (hierarchy_level_name) DO NOTHING;

COMMIT;

DROP TABLE staging_hierarchy_levels;
