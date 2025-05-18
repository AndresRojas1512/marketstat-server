CREATE OR REPLACE PROCEDURE marketstat.sp_run_benchmark(
    IN  p_industry_field_id     INT,
    IN  p_standard_job_role_id  INT,
    IN  p_hierarchy_level_id    INT,
    IN  p_district_id           INT,
    IN  p_oblast_id             INT,
    IN  p_city_id               INT,
    IN  p_date_start            DATE,
    IN  p_date_end              DATE,
    IN  p_granularity           TEXT,    /* 'month','quarter','year' */
    IN  p_periods               INT,     /* e.g. 12 months, 4 quarters, 5 years */
    IN  p_target_percentile     INT,     /* e.g. 80, 90 */
    INOUT o_dist_cursor         REFCURSOR,
    INOUT o_summary_cursor      REFCURSOR,
    INOUT o_ts_cursor           REFCURSOR
)
LANGUAGE plpgsql
AS $$
BEGIN
  OPEN o_dist_cursor FOR
    SELECT * FROM marketstat.fn_salary_distribution(
        p_industry_field_id,
        p_standard_job_role_id,
        p_hierarchy_level_id,
        p_district_id,
        p_oblast_id,
        p_city_id,
        p_date_start,
        p_date_end
    );

  OPEN o_summary_cursor FOR
    SELECT * FROM marketstat.fn_salary_summary(
        p_industry_field_id,
        p_standard_job_role_id,
        p_hierarchy_level_id,
        p_district_id,
        p_oblast_id,
        p_city_id,
        p_date_start,
        p_date_end,
        p_target_percentile
    );

  OPEN o_ts_cursor FOR
    SELECT * FROM marketstat.fn_salary_time_series(
        p_industry_field_id,
        p_standard_job_role_id,
        p_hierarchy_level_id,
        p_district_id,
        p_oblast_id,
        p_city_id,
        p_date_start,
        p_date_end,
        p_granularity,
        p_periods
    );
END;
$$;



CREATE OR REPLACE PROCEDURE marketstat.sp_load_fact_salaries_from_csv(
  IN  p_file_path  TEXT  -- absolute path on the DB server
)
LANGUAGE plpgsql
AS $$
BEGIN
  ----------------------------------------------------------------
  -- 1) Create a temp staging table (dropped automatically at end)
  ----------------------------------------------------------------
  CREATE TEMP TABLE tmp_fact_salaries_flat (
    pay_date             DATE,
    city_name            TEXT,
    employer_name        TEXT,
    job_role_title       TEXT,
    hierarchy_level_id   INT,
    employee_id          INT,
    salary_amount        NUMERIC(18,2),
    bonus_amount         NUMERIC(18,2)
  ) ON COMMIT DROP;

  ----------------------------------------------------------------
  -- 2) Bulk-load the CSV into staging
  ----------------------------------------------------------------
  EXECUTE format($fmt$
    COPY tmp_fact_salaries_flat(
      pay_date,
      city_name,
      employer_name,
      job_role_title,
      hierarchy_level_id,
      employee_id,
      salary_amount,
      bonus_amount
    )
    FROM %L
    WITH (FORMAT csv, HEADER true)
  $fmt$, p_file_path);

  ----------------------------------------------------------------
  -- 3) Populate dim_date (if any new dates)
  ----------------------------------------------------------------
  INSERT INTO marketstat.dim_date(full_date, year, quarter, month)
  SELECT DISTINCT
    f.pay_date,
    EXTRACT(YEAR   FROM f.pay_date)::INT,
    EXTRACT(QUARTER FROM f.pay_date)::INT,
    EXTRACT(MONTH  FROM f.pay_date)::INT
  FROM tmp_fact_salaries_flat AS f
  ON CONFLICT (full_date) DO NOTHING;

  ----------------------------------------------------------------
  -- 4) Insert facts
  ----------------------------------------------------------------
  INSERT INTO marketstat.fact_salaries(
    date_id,
    city_id,
    employer_id,
    job_role_id,
    employee_id,
    salary_amount,
    bonus_amount
  )
  SELECT
    d.date_id,
    c.city_id,
    e.employer_id,
    jr.job_role_id,
    f.employee_id,
    f.salary_amount,
    f.bonus_amount
  FROM tmp_fact_salaries_flat AS f
  JOIN marketstat.dim_date                 AS d  ON d.full_date            = f.pay_date
  JOIN marketstat.dim_city                 AS c  ON c.city_name            = f.city_name
  JOIN marketstat.dim_employer             AS e  ON e.employer_name        = f.employer_name
  JOIN marketstat.dim_job_role             AS jr ON jr.job_role_title      = f.job_role_title
  -- if hierarchy_level matters in your join logic, you can also JOIN it here
  ON CONFLICT DO NOTHING;

  -- temp table is dropped automatically
END;
$$;
