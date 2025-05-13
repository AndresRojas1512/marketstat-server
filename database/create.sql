CREATE SCHEMA IF NOT EXISTS marketstat;
SET search_path = marketstat, public;



CREATE TABLE IF NOT EXISTS dim_date (
    date_id   INT GENERATED ALWAYS AS IDENTITY
               CONSTRAINT pk_dim_date PRIMARY KEY,
    full_date DATE NOT NULL
               CONSTRAINT uq_dim_date_full_date UNIQUE,
    year      SMALLINT NOT NULL
               CONSTRAINT ck_dim_date_year CHECK (year > 0),
    quarter   SMALLINT NOT NULL
               CONSTRAINT ck_dim_date_quarter CHECK (quarter BETWEEN 1 AND 4),
    month     SMALLINT NOT NULL
               CONSTRAINT ck_dim_date_month CHECK (month BETWEEN 1 AND 12)
);



CREATE TABLE IF NOT EXISTS dim_federal_district (
    district_id   INT GENERATED ALWAYS AS IDENTITY
                   CONSTRAINT pk_dim_federal_district PRIMARY KEY,
    district_name VARCHAR(100) NOT NULL,
    CONSTRAINT uq_dim_federal_district_name UNIQUE (district_name)
);



CREATE TABLE IF NOT EXISTS dim_oblast (
    oblast_id   INT GENERATED ALWAYS AS IDENTITY
                 CONSTRAINT pk_dim_oblast PRIMARY KEY,
    oblast_name VARCHAR(100) NOT NULL,
    district_id INT NOT NULL,
    CONSTRAINT fk_dim_oblast_district FOREIGN KEY (district_id)
        REFERENCES dim_federal_district(district_id),
    CONSTRAINT uq_dim_oblast_name UNIQUE (oblast_name)
);
CREATE INDEX IF NOT EXISTS idx_dim_oblast_district_id
    ON dim_oblast (district_id);



CREATE TABLE IF NOT EXISTS dim_city (
    city_id   INT GENERATED ALWAYS AS IDENTITY
               CONSTRAINT pk_dim_city PRIMARY KEY,
    city_name VARCHAR(100) NOT NULL,
    oblast_id INT NOT NULL,
    CONSTRAINT fk_dim_city_oblast FOREIGN KEY (oblast_id)
        REFERENCES dim_oblast(oblast_id)
);
CREATE INDEX IF NOT EXISTS idx_dim_city_oblast_id
    ON dim_city (oblast_id);



CREATE TABLE IF NOT EXISTS dim_employer (
    employer_id   INT GENERATED ALWAYS AS IDENTITY
                   CONSTRAINT pk_dim_employer PRIMARY KEY,
    employer_name VARCHAR(255) NOT NULL,
    is_public     BOOLEAN    NOT NULL DEFAULT FALSE
);



CREATE TABLE IF NOT EXISTS dim_industry_field (
    industry_field_id   INT GENERATED ALWAYS AS IDENTITY
                         CONSTRAINT pk_dim_industry_field PRIMARY KEY,
    industry_field_name VARCHAR(100) NOT NULL,
    CONSTRAINT uq_dim_industry_field_name UNIQUE (industry_field_name)
);



CREATE TABLE IF NOT EXISTS dim_employer_industry_field (
    employer_id       INT NOT NULL,
    industry_field_id INT NOT NULL,
    CONSTRAINT pk_dim_employer_industry_field PRIMARY KEY (employer_id, industry_field_id),
    CONSTRAINT fk_dim_eif_employer FOREIGN KEY (employer_id)
        REFERENCES dim_employer(employer_id),
    CONSTRAINT fk_dim_eif_field    FOREIGN KEY (industry_field_id)
        REFERENCES dim_industry_field(industry_field_id)
);
CREATE INDEX IF NOT EXISTS idx_dim_eif_field
    ON dim_employer_industry_field (industry_field_id);



CREATE TABLE IF NOT EXISTS dim_hierarchy_level (
    hierarchy_level_id   INT
                         CONSTRAINT pk_dim_hierarchy_level PRIMARY KEY,
    hierarchy_level_name VARCHAR(100) NOT NULL
);



CREATE TABLE IF NOT EXISTS dim_standard_job_role (
    standard_job_role_id    INT GENERATED ALWAYS AS IDENTITY
                             CONSTRAINT pk_dim_standard_job_role PRIMARY KEY,
    standard_job_role_title VARCHAR(100) NOT NULL,
    industry_field_id       INT NOT NULL,
    CONSTRAINT fk_dim_sjr_field FOREIGN KEY (industry_field_id)
        REFERENCES dim_industry_field(industry_field_id),
    CONSTRAINT uq_dim_sjr_title UNIQUE (standard_job_role_title)
);
CREATE INDEX IF NOT EXISTS idx_dim_sjr_field
    ON dim_standard_job_role (industry_field_id);



CREATE TABLE IF NOT EXISTS dim_standard_job_role_hierarchy (
    standard_job_role_id INT NOT NULL,
    hierarchy_level_id   INT NOT NULL,
    CONSTRAINT pk_dim_sjrh PRIMARY KEY (standard_job_role_id, hierarchy_level_id),
    CONSTRAINT fk_dim_sjrh_sjr FOREIGN KEY (standard_job_role_id)
        REFERENCES dim_standard_job_role(standard_job_role_id),
    CONSTRAINT fk_dim_sjrh_hl  FOREIGN KEY (hierarchy_level_id)
        REFERENCES dim_hierarchy_level(hierarchy_level_id)
);



CREATE TABLE IF NOT EXISTS dim_job_role (
    job_role_id          INT GENERATED ALWAYS AS IDENTITY
                         CONSTRAINT pk_dim_job_role PRIMARY KEY,
    job_role_title       VARCHAR(150) NOT NULL,
    standard_job_role_id INT NOT NULL,
    hierarchy_level_id   INT NOT NULL,
    CONSTRAINT fk_dim_jr_sjr FOREIGN KEY (standard_job_role_id)
        REFERENCES dim_standard_job_role(standard_job_role_id),
    CONSTRAINT fk_dim_jr_hl  FOREIGN KEY (hierarchy_level_id)
        REFERENCES dim_hierarchy_level(hierarchy_level_id)
);
CREATE INDEX IF NOT EXISTS idx_dim_jr_sjr
    ON dim_job_role (standard_job_role_id);
CREATE INDEX IF NOT EXISTS idx_dim_jr_hl
    ON dim_job_role (hierarchy_level_id);



CREATE TABLE IF NOT EXISTS dim_education_level (
    education_level_id   INT GENERATED ALWAYS AS IDENTITY
                         CONSTRAINT pk_dim_education_level PRIMARY KEY,
    education_level_name VARCHAR(100) NOT NULL
);



CREATE TABLE IF NOT EXISTS dim_education (
    education_id       INT GENERATED ALWAYS AS IDENTITY
                       CONSTRAINT pk_dim_education PRIMARY KEY,
    specialty          VARCHAR(100) NOT NULL,
    specialty_code     VARCHAR(100) NOT NULL,
    education_level_id INT NOT NULL,
    industry_field_id  INT NOT NULL,
    CONSTRAINT fk_dim_edu_lvl FOREIGN KEY (education_level_id)
        REFERENCES dim_education_level(education_level_id),
    CONSTRAINT fk_dim_edu_if  FOREIGN KEY (industry_field_id)
        REFERENCES dim_industry_field(industry_field_id)
);
CREATE INDEX IF NOT EXISTS idx_dim_edu_lvl
    ON dim_education (education_level_id);
CREATE INDEX IF NOT EXISTS idx_dim_edu_if
    ON dim_education (industry_field_id);



CREATE TABLE IF NOT EXISTS dim_employee (
    employee_id       INT GENERATED ALWAYS AS IDENTITY
                       CONSTRAINT pk_dim_employee PRIMARY KEY,
    birth_date        DATE CHECK (birth_date <= CURRENT_DATE),
    career_start_date DATE NOT NULL
                       CONSTRAINT ck_dim_emp_career_start CHECK (career_start_date <= CURRENT_DATE)
);



CREATE TABLE IF NOT EXISTS dim_employee_education (
    employee_id     INT NOT NULL,
    education_id    INT NOT NULL,
    graduation_year SMALLINT NOT NULL
                       CONSTRAINT ck_dim_ee_grad_year CHECK (graduation_year BETWEEN 1900 AND EXTRACT(YEAR FROM CURRENT_DATE)),
    CONSTRAINT pk_dim_ee PRIMARY KEY (employee_id, education_id),
    CONSTRAINT fk_dim_ee_emp FOREIGN KEY (employee_id)
        REFERENCES dim_employee(employee_id),
    CONSTRAINT fk_dim_ee_edu FOREIGN KEY (education_id)
        REFERENCES dim_education(education_id)
);
CREATE INDEX IF NOT EXISTS idx_dim_ee_edu
    ON dim_employee_education (education_id);



CREATE TABLE IF NOT EXISTS fact_salaries (
    salary_fact_id BIGINT GENERATED ALWAYS AS IDENTITY
                   CONSTRAINT pk_fact_salaries PRIMARY KEY,
    date_id        INT NOT NULL,
    city_id        INT NOT NULL,
    employer_id    INT NOT NULL,
    job_role_id    INT NOT NULL,
    employee_id    INT NOT NULL,
    salary_amount  NUMERIC(18,2) NOT NULL
                   CONSTRAINT ck_fact_salary_amt CHECK (salary_amount >= 0),
    bonus_amount   NUMERIC(18,2) NOT NULL DEFAULT 0
                   CONSTRAINT ck_fact_bonus_amt CHECK (bonus_amount >= 0),
    load_date      TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT fk_fact_date    FOREIGN KEY (date_id)     REFERENCES dim_date(date_id),
    CONSTRAINT fk_fact_city    FOREIGN KEY (city_id)     REFERENCES dim_city(city_id),
    CONSTRAINT fk_fact_emp     FOREIGN KEY (employer_id) REFERENCES dim_employer(employer_id),
    CONSTRAINT fk_fact_jrole   FOREIGN KEY (job_role_id) REFERENCES dim_job_role(job_role_id),
    CONSTRAINT fk_fact_employee FOREIGN KEY (employee_id) REFERENCES dim_employee(employee_id)
);
CREATE INDEX IF NOT EXISTS idx_fact_date      ON fact_salaries (date_id);
CREATE INDEX IF NOT EXISTS idx_fact_city      ON fact_salaries (city_id);
CREATE INDEX IF NOT EXISTS idx_fact_employer  ON fact_salaries (employer_id);
CREATE INDEX IF NOT EXISTS idx_fact_jrole     ON fact_salaries (job_role_id);
CREATE INDEX IF NOT EXISTS idx_fact_employee  ON fact_salaries (employee_id);
