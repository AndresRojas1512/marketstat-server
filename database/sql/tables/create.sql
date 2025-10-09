CREATE SCHEMA IF NOT EXISTS marketstat;
SET search_path = marketstat, public;


CREATE TABLE IF NOT EXISTS dim_date (
    date_id     INT GENERATED ALWAYS AS IDENTITY
                    CONSTRAINT pk_dim_date PRIMARY KEY,
    full_date   DATE NOT NULL
                    CONSTRAINT uq_dim_date_full_date UNIQUE,
    year        SMALLINT NOT NULL
                    CONSTRAINT ck_dim_date_year CHECK (year > 0),
    quarter     SMALLINT NOT NULL
                    CONSTRAINT ck_dim_date_quarter CHECK (quarter BETWEEN 1 AND 4),
    month       SMALLINT NOT NULL
                    CONSTRAINT ck_dim_date_month CHECK (month BETWEEN 1 AND 12)
);


CREATE TABLE IF NOT EXISTS dim_industry_field (
    industry_field_id   INT GENERATED ALWAYS AS IDENTITY
                            CONSTRAINT pk_dim_industry_field PRIMARY KEY,
    industry_field_code VARCHAR(10) NOT NULL
                            CONSTRAINT uq_dim_industry_field_code UNIQUE,
    industry_field_name VARCHAR(255) NOT NULL
                            CONSTRAINT uq_dim_industry_field_name UNIQUE
);


CREATE TABLE IF NOT EXISTS dim_employer (
    employer_id         INT GENERATED ALWAYS AS IDENTITY
                            CONSTRAINT pk_dim_employer PRIMARY KEY,
    employer_name       VARCHAR(255) NOT NULL
                            CONSTRAINT uq_dim_employer_name UNIQUE,
    inn                 VARCHAR(12) NOT NULL
                            CONSTRAINT uq_dim_employer_inn UNIQUE,
    ogrn                VARCHAR(13) NOT NULL
                            CONSTRAINT uq_dim_employer_ogrn UNIQUE,
    kpp                 VARCHAR(9) NOT NULL,
    registration_date   DATE NOT NULL,
    legal_address       TEXT NOT NULL,
    contact_email       VARCHAR(255) NOT NULL,
    contact_phone       VARCHAR(50) NOT NULL,
    industry_field_id   INT NOT NULL,
                            CONSTRAINT fk_dim_employer_industry FOREIGN KEY (industry_field_id) REFERENCES dim_industry_field(industry_field_id)
);


CREATE TABLE IF NOT EXISTS dim_location (
    location_id     INT GENERATED ALWAYS AS IDENTITY
                        CONSTRAINT pk_dim_location PRIMARY KEY,
    city_name       VARCHAR(255) NOT NULL,
    oblast_name     VARCHAR(255) NOT NULL,
    district_name   VARCHAR(255) NOT NULL,
    CONSTRAINT uq_dim_location UNIQUE (city_name, oblast_name, district_name)
)


CREATE TABLE IF NOT EXISTS dim_job (
    job_id                  INT GENERATED ALWAYS AS IDENTITY
                                CONSTRAINT pk_dim_job PRIMARY KEY,
    job_role_title          VARCHAR(255) NOT NULL,
    standard_job_role_title VARCHAR(255) NOT NULL,
    hierarchy_level_name    VARCHAR(255) NOT NULL,
    industry_field_id       INT NOT NULL,
    CONSTRAINT fk_dim_job_industry FOREIGN KEY (industry_field_id) REFERENCES dim_industry_field(industry_field_id),
    CONSTRAINT uq_dim_job UNIQUE (job_role_title, standard_job_role_title, hierarchy_level_name, industry_field_id)

);
CREATE INDEX IF NOT EXISTS idx_dim_job_industry_field_id ON dim_job(industry_field_id);


CREATE TABLE IF NOT EXISTS dim_education (
    education_id            INT GENERATED ALWAYS AS IDENTITY
                                CONSTRAINT pk_dim_education PRIMARY KEY,
    specialty_name          VARCHAR(255) NOT NULL,
    specialty_code          VARCHAR(255) NOT NULL
                                CONSTRAINT uq_dim_education_specialty_code UNIQUE,
    education_level_name    VARCHAR(255) NOT NULL,
                                CONSTRAINT uq_dim_education UNIQUE (specialty_name, education_level_name)
);


CREATE TABLE IF NOT EXISTS dim_employee (
    employee_id         INT GENERATED ALWAYS AS IDENTITY
                            CONSTRAINT pk_dim_employee PRIMARY KEY,
    employee_ref_id     VARCHAR(255) NOT NULL
                            CONSTRAINT uq_dim_employee_ref_id UNIQUE,
    birth_date          DATE NOT NULL,
                            CONSTRAINT ck_dim_emp_birth_date CHECK (birth_date <= CURRENT_DATE),
    career_start_date   DATE NOT NULL
                            CONSTRAINT ck_dim_emp_career_start CHECK (career_start_date <= CURRENT_DATE),
    graduation_year     SMALLINT NULL,
    education_id        INT NULL,
    CONSTRAINT fk_dim_employee_education FOREIGN KEY (education_id) REFERENCES dim_education(education_id),
    CONSTRAINT ck_career_after_birth CHECK (career_start_date > birth_date),
    CONSTRAINT ck_career_min_age CHECK (career_start_date >= birth_date + INTERVAL '16 years')
);
CREATE INDEX IF NOT EXISTS idx_dim_employee_education ON dim_employee(education_id);


CREATE TABLE IF NOT EXISTS fact_salaries (
    salary_fact_id  BIGINT GENERATED ALWAYS AS IDENTITY
                        CONSTRAINT pk_fact_salaries PRIMARY KEY,
    date_id         INT NOT NULL,
    location_id     INT NOT NULL,
    employer_id     INT NOT NULL,
    job_id          INT NOT NULL,
    employee_id     INT NOT NULL,
    salary_amount   NUMERIC(18,2) NOT NULL
                        CONSTRAINT ck_fact_salary_amt CHECK (salary_amount >= 0),
    CONSTRAINT fk_fact_date         FOREIGN KEY (date_id)       REFERENCES dim_date(date_id),
    CONSTRAINT fk_fact_location     FOREIGN KEY (location_id)   REFERENCES dim_location(location_id),
    CONSTRAINT fk_fact_employer     FOREIGN KEY (employer_id)   REFERENCES dim_employer(employer_id),
    CONSTRAINT fk_fact_job          FOREIGN KEY (job_id)        REFERENCES dim_job(job_id),
    CONSTRAINT fk_fact_employee     FOREIGN KEY (employee_id)   REFERENCES dim_employee(employee_id)
);
CREATE INDEX IF NOT EXISTS idx_fact_date        ON fact_salaries (date_id);
CREATE INDEX IF NOT EXISTS idx_fact_location    ON fact_salaries (location_id);
CREATE INDEX IF NOT EXISTS idx_fact_employer    ON fact_salaries (employer_id);
CREATE INDEX IF NOT EXISTS idx_fact_job         ON fact_salaries (job_id);
CREATE INDEX IF NOT EXISTS idx_fact_employee    ON fact_salaries (employee_id);


CREATE TABLE IF NOT EXISTS users (
    user_id                 INT GENERATED ALWAYS AS IDENTITY
                                CONSTRAINT pk_users PRIMARY KEY,
    username                VARCHAR(100) NOT NULL
                                CONSTRAINT uq_users_username UNIQUE,
    password_hash           TEXT NOT NULL,
    email                   VARCHAR(255) NOT NULL
                                CONSTRAINT uq_users_email UNIQUE,
    full_name               VARCHAR(255) NOT NULL,
    is_active               BOOLEAN NOT NULL DEFAULT TRUE,
    created_at              TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    last_login_at           TIMESTAMPTZ NULL,
    is_etl_user             BOOLEAN NOT NULL DEFAULT FALSE
);


CREATE TABLE IF NOT EXISTS benchmark_history (
    benchmark_history_id        BIGINT GENERATED ALWAYS AS IDENTITY
                                    CONSTRAINT pk_benchmark_history PRIMARY KEY,
    user_id                     INT NOT NULL,
                                    CONSTRAINT fk_benchmark_user FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE,
    benchmark_name              VARCHAR(255) NULL,
    saved_at                    TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,

    filter _location_id         INT NULL,
    filter _job_id              INT NULL,
    filter _industry_field_id   INT NULL,
    filter _date_start          DATE NULL,
    filter _date_end            DATE NULL,
    filter _target_percentile   INT NULL,
    filter _granularity         TEXT NULL,
    filter _periods             INT NULL,

    benchmark_result_json       JSONB NOT NULL
);
CREATE INDEX IF NOT EXISTS idx_benchmark_history_user_id ON benchmark_history(user_id);


CREATE TABLE IF NOT EXISTS failed_salary_facts_load (
    failed_load_id                  SERIAL PRIMARY KEY,
    run_timestamp                   TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    recorded_date_text              TEXT,
    city_name                       TEXT,
    oblast_name                     TEXT,
    employer_name                   TEXT,
    standard_job_role_title         TEXT,
    job_role_title                  TEXT,
    hierarchy_level_name            TEXT,
    employee_ref_id                 TEXT,
    employee_birth_date_text        TEXT,
    employee_career_start_date_text TEXT,
    gender                          TEXT,
    education_level_name            TEXT,
    specialty                       TEXT,
    specialty_code                  TEXT,
    graduation_year                 SMALLINT,
    salary_amount                   NUMERIC(18,2),
    bonus_amount                    NUMERIC(18,2),
    error_message                   TEXT
);



CREATE TABLE IF NOT EXISTS api_fact_uploads_staging (
    recorded_date_text                TEXT,
    city_name                         TEXT,
    oblast_name                       TEXT,
    employer_name                     TEXT,
    standard_job_role_title           TEXT,
    job_role_title                    TEXT,
    hierarchy_level_name              TEXT,
    employee_ref_id                   TEXT,
    employee_birth_date_text          TEXT,
    employee_career_start_date_text   TEXT,
    gender                            TEXT,
    education_level_name              TEXT,
    specialty                         TEXT,
    specialty_code                    TEXT,
    graduation_year                   SMALLINT,
    salary_amount                     NUMERIC(18,2),
    bonus_amount                      NUMERIC(18,2)
);
