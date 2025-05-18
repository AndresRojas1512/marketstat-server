-- Filter function
DROP FUNCTION marketstat.fn_filtered_salaries(integer,integer,integer,integer,integer,integer,date,date);
CREATE OR REPLACE FUNCTION marketstat.fn_filtered_salaries(
    p_industry_field_id     INT     DEFAULT NULL,
    p_standard_job_role_id  INT     DEFAULT NULL,
    p_hierarchy_level_id    INT     DEFAULT NULL,
    p_district_id           INT     DEFAULT NULL,
    p_oblast_id             INT     DEFAULT NULL,
    p_city_id               INT     DEFAULT NULL,
    p_date_start            DATE    DEFAULT NULL,
    p_date_end              DATE    DEFAULT NULL
)
RETURNS TABLE(
    salary_amount           NUMERIC,
    full_date               DATE,
    city_id                 INT,
    oblast_id               INT,
    district_id             INT,
    standard_job_role_id    INT,
    hierarchy_level_id      INT,
    industry_field_id       INT
)
LANGUAGE plpgsql
AS $$
BEGIN
  RETURN QUERY
    SELECT
      fs.salary_amount,
      d.full_date,
      c.city_id,
      o.oblast_id,
      di.district_id,
      jr.standard_job_role_id,
      jr.hierarchy_level_id,
      sjr.industry_field_id
    FROM marketstat.fact_salaries fs
    JOIN marketstat.dim_date             d   ON fs.date_id     = d.date_id
    JOIN marketstat.dim_city             c   ON fs.city_id     = c.city_id
    JOIN marketstat.dim_oblast           o   ON c.oblast_id    = o.oblast_id
    JOIN marketstat.dim_federal_district di  ON o.district_id  = di.district_id
    JOIN marketstat.dim_job_role         jr  ON fs.job_role_id = jr.job_role_id
    JOIN marketstat.dim_standard_job_role sjr ON jr.standard_job_role_id = sjr.standard_job_role_id
    WHERE
      (p_city_id                 IS NULL OR c.city_id               = p_city_id)
      AND (p_oblast_id            IS NULL OR o.oblast_id             = p_oblast_id)
      AND (p_district_id          IS NULL OR di.district_id          = p_district_id)
      AND (p_standard_job_role_id IS NULL OR jr.standard_job_role_id = p_standard_job_role_id)
      AND (p_hierarchy_level_id   IS NULL OR jr.hierarchy_level_id   = p_hierarchy_level_id)
      AND (p_industry_field_id    IS NULL OR sjr.industry_field_id   = p_industry_field_id)
      AND (p_date_start           IS NULL OR d.full_date            >= p_date_start)
      AND (p_date_end             IS NULL OR d.full_date            <= p_date_end);
END;
$$;





-- Histogram
CREATE OR REPLACE FUNCTION marketstat.fn_salary_distribution(
    p_industry_field_id     INT     DEFAULT NULL,
    p_standard_job_role_id  INT     DEFAULT NULL,
    p_hierarchy_level_id    INT     DEFAULT NULL,
    p_district_id           INT     DEFAULT NULL,
    p_oblast_id             INT     DEFAULT NULL,
    p_city_id               INT     DEFAULT NULL,
    p_date_start            DATE    DEFAULT NULL,
    p_date_end              DATE    DEFAULT NULL
)
RETURNS TABLE(
    lower_bound   NUMERIC,
    upper_bound   NUMERIC,
    bucket_count  BIGINT
)
LANGUAGE plpgsql
AS $$
DECLARE
    _min_val NUMERIC;
    _max_val NUMERIC;
    _n BIGINT;
    _m INT;
    _delta NUMERIC;
BEGIN
  CREATE TEMP TABLE tmp_filtered_salaries AS
  SELECT salary_amount
  FROM marketstat.fn_filtered_salaries(
         p_industry_field_id,
         p_standard_job_role_id,
         p_hierarchy_level_id,
         p_district_id,
         p_oblast_id,
         p_city_id,
         p_date_start,
         p_date_end
       );

  SELECT
    MIN(tfs.salary_amount),
    MAX(tfs.salary_amount),
    COUNT(tfs.salary_amount)
  INTO _min_val, _max_val, _n
  FROM tmp_filtered_salaries tfs;

  IF _n = 0 THEN
    DROP TABLE tmp_filtered_salaries;
    RETURN QUERY SELECT NULL::NUMERIC, NULL::NUMERIC, 0::BIGINT WHERE FALSE;
    RETURN;
  END IF;

  IF _n = 1 THEN
    DROP TABLE tmp_filtered_salaries;
    RETURN QUERY SELECT _min_val, _max_val, 1::BIGINT;
    RETURN;
  END IF;

  IF _min_val = _max_val THEN
    DROP TABLE tmp_filtered_salaries;
    RETURN QUERY SELECT _min_val, _max_val, _n;
    RETURN;
  END IF;

  _m := FLOOR(LOG(2, _n))::INT + 2;
  IF _m < 2 THEN _m := 2; END IF;
  _delta := (_max_val - _min_val) / _m;

  IF _delta = 0 AND _max_val > _min_val THEN
      _delta := (_max_val - _min_val) / 2;
      _m := 2;
  END IF;


  RETURN QUERY
  WITH buckets AS (
    SELECT
      width_bucket(tfs.salary_amount, _min_val, _max_val, _m) AS bucket_no
    FROM tmp_filtered_salaries tfs
  )
  SELECT
    _min_val + (b.bucket_no - 1) * _delta    AS lower_bound,
    CASE
      WHEN b.bucket_no = _m THEN _max_val
      ELSE _min_val + b.bucket_no * _delta
    END                                      AS upper_bound,
    COUNT(*)                                 AS bucket_count
  FROM buckets b
  GROUP BY b.bucket_no
  ORDER BY b.bucket_no;

  DROP TABLE tmp_filtered_salaries;
END;
$$;





-- Salary summary (percentiles + average + count)
CREATE OR REPLACE FUNCTION marketstat.fn_salary_summary(
    p_industry_field_id     INT     DEFAULT NULL,
    p_standard_job_role_id  INT     DEFAULT NULL,
    p_hierarchy_level_id    INT     DEFAULT NULL,
    p_district_id           INT     DEFAULT NULL,
    p_oblast_id             INT     DEFAULT NULL,
    p_city_id               INT     DEFAULT NULL,
    p_date_start            DATE    DEFAULT NULL,
    p_date_end              DATE    DEFAULT NULL,
    p_target_percentile     INT     DEFAULT 90
)
RETURNS TABLE(
    percentile25      NUMERIC,
    percentile50      NUMERIC,
    percentile75      NUMERIC,
    percentile_target NUMERIC,
    average_salary    NUMERIC,
    total_count       BIGINT
)
LANGUAGE plpgsql
AS $$
BEGIN
  IF p_target_percentile < 0 OR p_target_percentile > 100 THEN
    RAISE EXCEPTION 'Target percentile must be between 0 and 100 (inclusive). Received: %', p_target_percentile;
  END IF;

  RETURN QUERY
    SELECT
      percentile_cont(0.25) WITHIN GROUP (ORDER BY fs.salary_amount) AS percentile25,
      percentile_cont(0.50) WITHIN GROUP (ORDER BY fs.salary_amount) AS percentile50,
      percentile_cont(0.75) WITHIN GROUP (ORDER BY fs.salary_amount) AS percentile75,
      percentile_cont(p_target_percentile / 100.0)
        WITHIN GROUP (ORDER BY fs.salary_amount)                     AS percentile_target,
      AVG(fs.salary_amount)                                          AS average_salary,
      COUNT(*)                                                       AS total_count
    FROM marketstat.fn_filtered_salaries(
           p_industry_field_id,
           p_standard_job_role_id,
           p_hierarchy_level_id,
           p_district_id,
           p_oblast_id,
           p_city_id,
           p_date_start,
           p_date_end
         ) AS fs;
END;
$$;




-- Time‐series of average salary
CREATE OR REPLACE FUNCTION marketstat.fn_salary_time_series(
    p_industry_field_id    INT     DEFAULT NULL,
    p_standard_job_role_id INT     DEFAULT NULL,
    p_hierarchy_level_id   INT     DEFAULT NULL,
    p_district_id          INT     DEFAULT NULL,
    p_oblast_id            INT     DEFAULT NULL,
    p_city_id              INT     DEFAULT NULL,
    p_date_start           DATE    DEFAULT NULL,
    p_date_end             DATE    DEFAULT NULL,
    p_granularity          TEXT    DEFAULT 'month',
    p_periods              INT     DEFAULT 12
)
RETURNS TABLE(
    period_start DATE,
    avg_salary   NUMERIC
)
LANGUAGE plpgsql
AS $$
DECLARE
  _step    INTERVAL;
  _actual_series_start_date DATE;
  _actual_series_end_date   DATE;
  _filter_start_date        DATE;
  _filter_end_date          DATE;
BEGIN
  IF   p_granularity = 'month'   THEN _step := INTERVAL '1 month';
  ELSIF p_granularity = 'quarter' THEN _step := INTERVAL '3 months';
  ELSIF p_granularity = 'year'    THEN _step := INTERVAL '1 year';
  ELSE
    RAISE EXCEPTION 'fn_salary_time_series: invalid granularity "%". Must be "month", "quarter", or "year".', p_granularity;
  END IF;

  _actual_series_end_date := date_trunc(p_granularity, COALESCE(p_date_end, CURRENT_DATE));
  _actual_series_start_date := _actual_series_end_date - (p_periods - 1) * _step;

  _filter_start_date := _actual_series_start_date;
  IF p_date_start IS NOT NULL AND p_date_start > _filter_start_date THEN
      _filter_start_date := p_date_start;
  END IF;

  _filter_end_date := _actual_series_end_date + _step - INTERVAL '1 day';
  IF p_date_end IS NOT NULL AND p_date_end < _filter_end_date THEN
      _filter_end_date := p_date_end;
  END IF;

  IF _filter_start_date > _filter_end_date THEN
    RETURN QUERY SELECT NULL::DATE, NULL::NUMERIC WHERE FALSE;
    RETURN;
END IF;

  RETURN QUERY
  WITH series AS (
    SELECT generate_series(
             _actual_series_start_date,
             _actual_series_end_date,
             _step
           )::date AS period_start
  ),
  data AS (
    SELECT
      date_trunc(p_granularity, fs.full_date)::date AS period_start,
      fs.salary_amount
    FROM marketstat.fn_filtered_salaries(
           p_industry_field_id,
           p_standard_job_role_id,
           p_hierarchy_level_id,
           p_district_id,
           p_oblast_id,
           p_city_id,
           _filter_start_date,
           _filter_end_date
         ) AS fs
    WHERE date_trunc(p_granularity, fs.full_date)::date >= _actual_series_start_date
      AND date_trunc(p_granularity, fs.full_date)::date <= _actual_series_end_date
  )
  SELECT
    s.period_start,
    AVG(d.salary_amount) AS avg_salary
  FROM series s
  LEFT JOIN data d
    ON d.period_start = s.period_start
  GROUP BY s.period_start
  ORDER BY s.period_start;
END;
$$;

