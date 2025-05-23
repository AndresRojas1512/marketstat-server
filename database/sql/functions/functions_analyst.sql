-- Filter function
DROP FUNCTION IF EXISTS marketstat.fn_filtered_salaries(integer,integer,integer,integer,integer,integer,date,date);
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
DROP FUNCTION IF EXISTS marketstat.fn_salary_distribution(integer,integer,integer,integer,integer,integer,date,date);
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
DROP FUNCTION IF EXISTS marketstat.fn_salary_time_summary(integer,integer,integer,integer,integer,integer,date,date,integer);
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
      (percentile_cont(0.25) WITHIN GROUP (ORDER BY fs.salary_amount))::NUMERIC AS percentile25,
      (percentile_cont(0.50) WITHIN GROUP (ORDER BY fs.salary_amount))::NUMERIC AS percentile50,
      (percentile_cont(0.75) WITHIN GROUP (ORDER BY fs.salary_amount))::NUMERIC AS percentile75,
      (percentile_cont(p_target_percentile / 100.0)
        WITHIN GROUP (ORDER BY fs.salary_amount))::NUMERIC                     AS percentile_target,
      (AVG(fs.salary_amount))::NUMERIC                                         AS average_salary,
      COUNT(*)                                                                 AS total_count
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
DROP FUNCTION IF EXISTS marketstat.fn_salary_time_series(integer,integer,integer,integer,integer,integer,date,date, text, integer);
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
    avg_salary   NUMERIC -- Expected type
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
      RAISE NOTICE 'Date range for filtering is invalid (_filter_start_date: %, _filter_end_date: %). Returning empty set.', _filter_start_date, _filter_end_date;
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
    (AVG(d.salary_amount))::NUMERIC AS avg_salary -- Added ::NUMERIC cast here
  FROM series s
  LEFT JOIN data d
    ON d.period_start = s.period_start
  GROUP BY s.period_start
  ORDER BY s.period_start;
END;
$$;




-- Benchmarking
CREATE OR REPLACE FUNCTION marketstat.fn_get_benchmarking_data(
    -- String-based filter IN parameters
    p_industry_field_name_filter   TEXT    DEFAULT NULL,
    p_standard_job_role_title_filter TEXT  DEFAULT NULL,
    p_hierarchy_level_name_filter  TEXT    DEFAULT NULL,
    p_district_name_filter         TEXT    DEFAULT NULL,
    p_oblast_name_filter           TEXT    DEFAULT NULL,
    p_city_name_filter             TEXT    DEFAULT NULL,

    -- Date filter IN parameters
    p_date_start                   DATE    DEFAULT NULL,
    p_date_end                     DATE    DEFAULT NULL,

    -- Parameters specific to fn_salary_summary
    p_target_percentile            INT     DEFAULT 90,

    -- Parameters specific to fn_salary_time_series
    p_granularity                  TEXT    DEFAULT 'month',
    p_periods                      INT     DEFAULT 12
)
RETURNS JSONB -- Specifies the return type
LANGUAGE plpgsql
AS $$
DECLARE
    -- Variables to hold resolved IDs from string lookups
    v_industry_field_id     INT     DEFAULT NULL;
    v_standard_job_role_id  INT     DEFAULT NULL;
    v_hierarchy_level_id    INT     DEFAULT NULL;
    v_district_id           INT     DEFAULT NULL;
    v_oblast_id             INT     DEFAULT NULL;
    v_city_id               INT     DEFAULT NULL;

    -- Variables to store JSON results from underlying functions
    v_salary_distribution JSONB;
    v_salary_summary      JSONB;
    v_salary_time_series  JSONB;

    -- Variable to hold the final combined JSON payload
    v_final_json_payload  JSONB;
BEGIN
    RAISE NOTICE 'Resolving filter strings to IDs...';

    -- Step 1: Resolve string filter parameters to their corresponding IDs
    IF p_industry_field_name_filter IS NOT NULL THEN
        SELECT dif.industry_field_id INTO v_industry_field_id
        FROM marketstat.dim_industry_field dif
        WHERE dif.industry_field_name = p_industry_field_name_filter LIMIT 1;
        IF NOT FOUND THEN RAISE NOTICE 'Industry field name "%" not found. Filter for industry will not be applied.', p_industry_field_name_filter; END IF;
    END IF;

    IF p_standard_job_role_title_filter IS NOT NULL THEN
        SELECT dsjr.standard_job_role_id INTO v_standard_job_role_id
        FROM marketstat.dim_standard_job_role dsjr
        WHERE dsjr.standard_job_role_title = p_standard_job_role_title_filter LIMIT 1;
        IF NOT FOUND THEN RAISE NOTICE 'Standard job role title "%" not found. Filter for standard job role will not be applied.', p_standard_job_role_title_filter; END IF;
    END IF;

    IF p_hierarchy_level_name_filter IS NOT NULL THEN
        SELECT dhl.hierarchy_level_id INTO v_hierarchy_level_id
        FROM marketstat.dim_hierarchy_level dhl
        WHERE dhl.hierarchy_level_name = p_hierarchy_level_name_filter LIMIT 1;
        IF NOT FOUND THEN RAISE NOTICE 'Hierarchy level name "%" not found. Filter for hierarchy level will not be applied.', p_hierarchy_level_name_filter; END IF;
    END IF;

    IF p_district_name_filter IS NOT NULL THEN
        SELECT dfd.district_id INTO v_district_id
        FROM marketstat.dim_federal_district dfd
        WHERE dfd.district_name = p_district_name_filter LIMIT 1;
        IF NOT FOUND THEN RAISE NOTICE 'Federal district name "%" not found. Filter for federal district will not be applied.', p_district_name_filter; END IF;
    END IF;

    IF p_oblast_name_filter IS NOT NULL THEN
        SELECT dof.oblast_id INTO v_oblast_id
        FROM marketstat.dim_oblast dof
        WHERE dof.oblast_name = p_oblast_name_filter LIMIT 1;
        IF NOT FOUND THEN RAISE NOTICE 'Oblast name "%" not found. Oblast and dependent City filter will not be applied.', p_oblast_name_filter; END IF;
    END IF;

    IF p_city_name_filter IS NOT NULL THEN
        IF v_oblast_id IS NOT NULL THEN -- City lookup requires a valid oblast_id
            SELECT dc.city_id INTO v_city_id
            FROM marketstat.dim_city dc
            WHERE dc.city_name = p_city_name_filter AND dc.oblast_id = v_oblast_id LIMIT 1;
            IF NOT FOUND THEN RAISE NOTICE 'City name "%" in oblast ID % (name: "%") not found. Filter for city will not be applied.', p_city_name_filter, v_oblast_id, p_oblast_name_filter; END IF;
        ELSE
            RAISE NOTICE 'City name filter "%" provided, but corresponding oblast_id could not be determined (either oblast name filter was NULL, or the name was not found). City filter will not be applied.', p_city_name_filter;
            v_city_id := NULL; -- Ensure it's NULL if oblast_id is not available
        END IF;
    END IF;

    RAISE NOTICE 'Calling analytical functions with resolved IDs: industry_field_id=%, standard_job_role_id=%, hierarchy_level_id=%, district_id=%, oblast_id=%, city_id=%',
                 v_industry_field_id, v_standard_job_role_id, v_hierarchy_level_id, v_district_id, v_oblast_id, v_city_id;

    -- Step 2: Call the underlying analytical functions using the resolved IDs
    RAISE NOTICE 'Fetching salary distribution...';
    SELECT COALESCE(jsonb_agg(dist_data), '[]'::JSONB)
    INTO v_salary_distribution
    FROM marketstat.fn_salary_distribution(
             v_industry_field_id, v_standard_job_role_id, v_hierarchy_level_id,
             v_district_id, v_oblast_id, v_city_id, p_date_start, p_date_end
         ) AS dist_data;

    RAISE NOTICE 'Fetching salary summary...';
    SELECT COALESCE(row_to_json(summary_data)::JSONB, '{}'::JSONB)
    INTO v_salary_summary
    FROM marketstat.fn_salary_summary(
             v_industry_field_id, v_standard_job_role_id, v_hierarchy_level_id,
             v_district_id, v_oblast_id, v_city_id, p_date_start, p_date_end,
             p_target_percentile
         ) AS summary_data;

    RAISE NOTICE 'Fetching salary time series...';
    SELECT COALESCE(jsonb_agg(ts_data), '[]'::JSONB)
    INTO v_salary_time_series
    FROM marketstat.fn_salary_time_series(
             v_industry_field_id, v_standard_job_role_id, v_hierarchy_level_id,
             v_district_id, v_oblast_id, v_city_id, p_date_start, p_date_end,
             p_granularity, p_periods
         ) AS ts_data;

    -- Step 3: Build the final JSONB object
    RAISE NOTICE 'Building final JSON payload...';
    v_final_json_payload := jsonb_build_object(
        'salaryDistribution', v_salary_distribution,
        'salarySummary',      v_salary_summary,
        'salaryTimeSeries',   v_salary_time_series
    );

    RAISE NOTICE 'Benchmarking data function finished.';
    RETURN v_final_json_payload; -- Return the JSONB object
END;
$$;