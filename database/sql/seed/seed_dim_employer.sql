SET search_path = marketstat, public;
\set ON_ERROR_STOP on

DROP TABLE IF EXISTS staging_employers_temp;

CREATE TEMP TABLE staging_employers_temp (
    csv_id INT,
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


\copy staging_employers_temp FROM '/home/andres/Desktop/6Semester/SoftwareDesign/PPO/database/datasets/dim_employer_dataset.csv' WITH (FORMAT CSV, HEADER TRUE, DELIMITER ',');

SELECT count(*) AS staged_rows FROM staging_employers_temp;

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
    TO_DATE(s.registration_date, 'YYYY-MM-DD'),
    s.legal_address,
    s.website,
    s.contact_email,
    s.contact_phone
FROM staging_employers_temp s
ON CONFLICT (employer_name) DO NOTHING;


COMMIT;

DROP TABLE staging_employers_temp;

SELECT * FROM marketstat.dim_employer ORDER BY employer_id LIMIT 5;

SELECT COUNT(*) FROM marketstat.dim_employer;
