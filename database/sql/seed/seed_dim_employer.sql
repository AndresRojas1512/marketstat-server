\set ON_ERROR_STOP on
SET search_path = marketstat, public;

CREATE TEMP TABLE staging_employers_temp (
    employer_name       TEXT,
    inn                 TEXT,
    ogrn                TEXT,
    kpp                 TEXT,
    registration_date   TEXT,
    legal_address       TEXT,
    contact_email       TEXT,
    contact_phone       TEXT,
    industry_name       TEXT
);

\copy staging_employers_temp FROM '/home/andres/Desktop/7-semester/marketstat/server/database/datasets/static/dim_employer_dataset.csv' WITH (FORMAT CSV, HEADER TRUE, DELIMITER ',');

BEGIN;

INSERT INTO marketstat.dim_employer (
    employer_name,
    inn,
    ogrn,
    kpp,
    registration_date,
    legal_address,
    contact_email,
    contact_phone,
    industry_field_id
)
SELECT
    TRIM(se.employer_name),
    TRIM(se.inn),
    TRIM(se.ogrn),
    TRIM(se.kpp),
    se.registration_date::date,
    TRIM(se.legal_address),
    TRIM(se.contact_email),
    TRIM(se.contact_phone),
    dif.industry_field_id
FROM staging_employers_temp se
JOIN marketstat.dim_industry_field dif ON TRIM(se.industry_name) = dif.industry_field_name
ON CONFLICT (employer_name) DO NOTHING;

COMMIT;
