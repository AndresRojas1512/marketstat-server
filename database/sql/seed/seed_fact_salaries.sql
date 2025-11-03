\set ON_ERROR_STOP on
SET search_path = marketstat, public;

CREATE TEMP TABLE staging_raw_salary_facts (
    employee_ref_id         TEXT,
    birth_date              TEXT,
    career_start_date       TEXT,
    education_code          TEXT,
    graduation_year         TEXT,
    employer_name           TEXT,
    job_role_title          TEXT,
    standard_job_role_title TEXT,
    hierarchy_level         TEXT,
    city_name               TEXT,
    oblast_name             TEXT,
    district_name           TEXT,
    industry_name           TEXT,
    salary_amount           TEXT,
    date                    TEXT
);

\copy staging_raw_salary_facts FROM '/home/andres/Desktop/7-semester/marketstat/server/database/datasets/fact/salary_facts_raw.csv' WITH (FORMAT CSV, HEADER TRUE, DELIMITER ',');

BEGIN;

INSERT INTO marketstat.dim_employee (employee_ref_id, birth_date, career_start_date, education_id, graduation_year)
SELECT DISTINCT
    TRIM(s.employee_ref_id),
    s.birth_date::date,
    s.career_start_date::date,
    de.education_id,
    NULLIF(TRIM(s.graduation_year), '')::smallint
FROM staging_raw_salary_facts s
LEFT JOIN marketstat.dim_education de ON TRIM(s.education_code) = de.specialty_code
ON CONFLICT (employee_ref_id) DO NOTHING;

INSERT INTO marketstat.dim_job (job_role_title, standard_job_role_title, hierarchy_level_name, industry_field_id)
SELECT DISTINCT
    TRIM(s.job_role_title),
    TRIM(s.standard_job_role_title),
    TRIM(s.hierarchy_level),
    dif.industry_field_id
FROM staging_raw_salary_facts s
JOIN marketstat.dim_industry_field dif ON TRIM(s.industry_name) = dif.industry_field_name
ON CONFLICT (job_role_title, standard_job_role_title, hierarchy_level_name, industry_field_id) DO NOTHING;

COMMIT;


BEGIN;

INSERT INTO marketstat.fact_salaries (date_id, location_id, employer_id, job_id, employee_id, salary_amount)
SELECT
    dd.date_id,
    dl.location_id,
    de.employer_id,
    dj.job_id,
    dem.employee_id,
    s.salary_amount::numeric(18, 2)
FROM staging_raw_salary_facts s
JOIN marketstat.dim_date dd ON s.date::date = dd.full_date
JOIN marketstat.dim_location dl ON TRIM(s.city_name) = dl.city_name AND TRIM(s.oblast_name) = dl.oblast_name AND TRIM(s.district_name) = dl.district_name
JOIN marketstat.dim_employer de ON TRIM(s.employer_name) = de.employer_name
JOIN marketstat.dim_job dj ON TRIM(s.job_role_title) = dj.job_role_title AND TRIM(s.standard_job_role_title) = dj.standard_job_role_title AND TRIM(s.hierarchy_level) = dj.hierarchy_level_name
JOIN marketstat.dim_employee dem ON TRIM(s.employee_ref_id) = dem.employee_ref_id;

COMMIT;