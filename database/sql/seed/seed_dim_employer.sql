-- Script to seed marketstat.dim_employer table using a temporary staging table
-- Run as marketstat_administrator or a user with sufficient privileges.

SET search_path = marketstat, public;
\set ON_ERROR_STOP on

\echo '--- Dropping temporary staging table for employers (if it exists from a previous failed run) ---'
DROP TABLE IF EXISTS staging_employers_temp;

\echo '--- Creating temporary staging table: staging_employers_temp ---'
CREATE TEMP TABLE staging_employers_temp (
    csv_id INT, -- Corresponds to the 'id' column in your CSV
    employer_name       VARCHAR(255),
    inn                 VARCHAR(12),
    ogrn                VARCHAR(13),
    kpp                 VARCHAR(9),
    registration_date   TEXT,
    legal_address       TEXT,
    website             VARCHAR(255),
    contact_email       VARCHAR(255),
    contact_phone       VARCHAR(50)
);

\echo 'Temporary staging table created.'

\echo '--- Copying data from CSV into staging_employers_temp ---'
\copy staging_employers_temp FROM '/home/andres/Desktop/6Semester/SoftwareDesign/PPO/database/datasets/dim_employer_dataset.csv' WITH (FORMAT CSV, HEADER TRUE, DELIMITER ',');

SELECT count(*) AS staged_rows FROM staging_employers_temp;
\echo 'Data copied to temporary staging table.'

\echo '--- Inserting data from staging_employers_temp into marketstat.dim_employer ---'
BEGIN;

INSERT INTO marketstat.dim_employer (
    employer_name,
    inn,
    ogrn,
    kpp,
    registration_date,
    legal_address,
    website,
    contact_email,
    contact_phone
)
SELECT
    s.employer_name,
    s.inn,
    s.ogrn,
    s.kpp,
    TO_DATE(s.registration_date, 'YYYY-MM-DD'), -- Explicitly cast text to DATE
    s.legal_address,
    s.website,
    s.contact_email,
    s.contact_phone
FROM staging_employers_temp s
ON CONFLICT (employer_name) DO NOTHING;

\echo 'Data insertion into marketstat.dim_employer attempted.'

COMMIT;
\echo 'Transaction committed.'

DROP TABLE staging_employers_temp;
\echo 'Temporary staging table dropped.'

\echo '--- First 5 rows from marketstat.dim_employer: ---'
SELECT * FROM marketstat.dim_employer ORDER BY employer_id LIMIT 5;

\echo '--- Total rows in marketstat.dim_employer: ---'
SELECT COUNT(*) FROM marketstat.dim_employer;

\echo '--- dim_employer seeding script finished ---'
