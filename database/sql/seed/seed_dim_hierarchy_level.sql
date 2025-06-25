\set ON_ERROR_STOP on
SET search_path = marketstat, public;

DROP TABLE IF EXISTS staging_hierarchy_levels;

DROP TABLE IF EXISTS staging_hierarchy_levels_temp;
CREATE TEMP TABLE staging_hierarchy_levels_temp (
    level_code          TEXT,
    hierarchy_level_name TEXT -- Renamed column for clarity
);
\echo 'Temporary staging table "staging_hierarchy_levels_temp" created.'

-- Define the path to your CSV file
\set hierarchy_level_csv_path '/home/andres/Desktop/6Semester/SoftwareDesign/PPO/database/datasets/dim_hierarchy_level_dataset.csv'

-- Copy data from the CSV into the temporary staging table
\echo 'Copying data from CSV into temporary staging table...'
\copy staging_hierarchy_levels_temp(level_code, hierarchy_level_name) FROM '/home/andres/Desktop/6Semester/SoftwareDesign/PPO/database/datasets/dim_hierarchy_level_dataset.csv' WITH (FORMAT CSV, HEADER TRUE, DELIMITER ',');

SELECT count(*) AS staged_rows FROM staging_hierarchy_levels_temp;
\echo 'Data copied successfully to staging table.'

-- Begin a transaction to insert data into the final dimension table
\echo 'Inserting data into marketstat.dim_hierarchy_level...'
BEGIN;

-- Insert data from the staging table into the permanent dimension table.
INSERT INTO marketstat.dim_hierarchy_level (hierarchy_level_code, hierarchy_level_name)
SELECT
    TRIM(s.level_code),
    TRIM(s.hierarchy_level_name)
FROM
    staging_hierarchy_levels_temp s
WHERE
    s.level_code IS NOT NULL AND s.level_code <> ''
    AND s.hierarchy_level_name IS NOT NULL AND s.hierarchy_level_name <> ''
-- Use the stable code for conflict resolution.
ON CONFLICT (hierarchy_level_code) DO NOTHING;

COMMIT;
\echo 'Transaction committed. Data insertion into dim_hierarchy_level attempted.'

-- The TEMP TABLE is automatically dropped at the end of the session.

-- Verify by selecting some data
\echo '--- First 5 rows from marketstat.dim_hierarchy_level: ---'
SELECT * FROM marketstat.dim_hierarchy_level ORDER BY hierarchy_level_id LIMIT 5;

\echo '--- Total rows in marketstat.dim_hierarchy_level: ---'
SELECT COUNT(*) FROM marketstat.dim_hierarchy_level;

\echo '--- dim_hierarchy_level seeding script finished. ---'
