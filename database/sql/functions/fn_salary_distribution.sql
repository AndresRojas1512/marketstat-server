-- Run as marketstat_administrator
SET search_path = marketstat, public;

DROP FUNCTION IF EXISTS marketstat.fn_salary_distribution(TEXT,INT,INT,INT,INT,INT,INT,DATE,DATE);

CREATE OR REPLACE FUNCTION marketstat.fn_salary_distribution(
    p_source_temp_table_name TEXT    DEFAULT NULL,
    p_industry_field_id      INT     DEFAULT NULL,
    p_standard_job_role_id   INT     DEFAULT NULL,
    p_hierarchy_level_id     INT     DEFAULT NULL,
    p_district_id            INT     DEFAULT NULL,
    p_oblast_id              INT     DEFAULT NULL,
    p_city_id                INT     DEFAULT NULL,
    p_date_start             DATE    DEFAULT NULL,
    p_date_end               DATE    DEFAULT NULL
)
RETURNS TABLE( lower_bound NUMERIC, upper_bound NUMERIC, bucket_count BIGINT )
LANGUAGE plpgsql SECURITY DEFINER AS $$
DECLARE
    _min_val NUMERIC;
    _max_val NUMERIC;
    _n BIGINT;
    _m INT;
    _delta NUMERIC;
    _query_source_sql TEXT;
    _final_sql TEXT;
BEGIN
  CREATE TEMP TABLE tmp_distrib_data_for_calc (salary_amount NUMERIC) ON COMMIT DROP;

  IF p_source_temp_table_name IS NOT NULL THEN
    RAISE NOTICE '[fn_salary_distribution] Using pre-filtered data from temp table: %', p_source_temp_table_name;
    _query_source_sql := format('SELECT src.salary_amount FROM %I src WHERE src.salary_amount IS NOT NULL', p_source_temp_table_name);
  ELSE
    RAISE NOTICE '[fn_salary_distribution] Filtering data internally. Filters: IndustryID=% SJRID=% HLevelID=% DistrictID=% OblastID=% CityID=% DateStart=% DateEnd=%',
                 p_industry_field_id, p_standard_job_role_id, p_hierarchy_level_id, p_district_id, p_oblast_id, p_city_id, p_date_start, p_date_end;
    _query_source_sql := format('SELECT fs.salary_amount FROM marketstat.fn_filtered_salaries(%L, %L, %L, %L, %L, %L, %L, %L) fs WHERE fs.salary_amount IS NOT NULL',
           p_industry_field_id, p_standard_job_role_id, p_hierarchy_level_id,
           p_district_id, p_oblast_id, p_city_id, p_date_start, p_date_end);
  END IF;

  RAISE NOTICE '[fn_salary_distribution] Data source query: %', _query_source_sql;
  EXECUTE 'INSERT INTO tmp_distrib_data_for_calc (salary_amount) ' || _query_source_sql;

  SELECT COUNT(s.salary_amount) INTO _n FROM tmp_distrib_data_for_calc s;
  RAISE NOTICE '[fn_salary_distribution] Rows inserted into tmp_distrib_data_for_calc: %', _n;

  SELECT MIN(s.salary_amount), MAX(s.salary_amount)
  INTO _min_val, _max_val
  FROM tmp_distrib_data_for_calc s;
  RAISE NOTICE '[fn_salary_distribution] Calculated from tmp_distrib_data_for_calc: _min_val=%, _max_val=%, _n=%', _min_val, _max_val, _n;

  IF _n IS NULL OR _n = 0 THEN
    RAISE NOTICE '[fn_salary_distribution] No data or zero count, returning empty set.';
    RETURN QUERY SELECT NULL::NUMERIC, NULL::NUMERIC, 0::BIGINT WHERE FALSE;
    RETURN;
  END IF;
  IF _n = 1 THEN
    RAISE NOTICE '[fn_salary_distribution] Single data point, returning: min=%, max=%, count=1', _min_val, _max_val;
    RETURN QUERY SELECT _min_val, _max_val, 1::BIGINT;
    RETURN;
  END IF;
  IF _min_val = _max_val THEN
    RAISE NOTICE '[fn_salary_distribution] All data points are identical: value=%, count=%', _min_val, _n;
    RETURN QUERY SELECT _min_val, _max_val, _n;
    RETURN;
  END IF;

  _m := FLOOR(LOG(2, _n))::INT + 2;
  IF _m < 2 THEN _m := 2; END IF;
  IF _m = 0 THEN _m := 2; END IF;

  _delta := (_max_val - _min_val) / _m;

  IF _delta = 0 THEN
      IF _max_val > _min_val THEN
          _delta := (_max_val - _min_val) / 2; _m := 2;
          RAISE NOTICE '[fn_salary_distribution] Delta was 0, recalculated: _delta=%, _m=%', _delta, _m;
      ELSE
          RAISE NOTICE '[fn_salary_distribution] Delta is 0 and min=max, returning single bucket.';
          RETURN QUERY SELECT _min_val, _max_val, _n; RETURN;
      END IF;
  END IF;

  RAISE NOTICE '[fn_salary_distribution DEBUG] Using values: _min_val=%, _max_val=%, _m=%, _delta=%',
               _min_val, _max_val, _m, _delta;

  _final_sql := '
    WITH buckets AS (
        SELECT width_bucket(s.salary_amount, $1, ($2) + 0.00001, $3) AS bucket_no
        FROM tmp_distrib_data_for_calc s
    )
    SELECT
        (($1) + (b.bucket_no - 1) * ($4))::NUMERIC,
        CASE
            WHEN b.bucket_no = ($3) THEN ($2)
            ELSE (($1) + b.bucket_no * ($4))::NUMERIC
        END,
        COUNT(*)::BIGINT
    FROM buckets b
    GROUP BY b.bucket_no
    ORDER BY b.bucket_no;';

  RAISE NOTICE '[fn_salary_distribution DEBUG] SQL for EXECUTE USING: %', _final_sql;

  RETURN QUERY EXECUTE _final_sql
    USING _min_val, _max_val, _m, _delta;
END; $$;

ALTER FUNCTION marketstat.fn_salary_distribution(TEXT,INT,INT,INT,INT,INT,INT,DATE,DATE) OWNER TO marketstat_administrator;
GRANT EXECUTE ON FUNCTION marketstat.fn_salary_distribution(TEXT,INT,INT,INT,INT,INT,INT,DATE,DATE) TO marketstat_analyst;
\echo 'Function marketstat.fn_salary_distribution (hybrid, using EXECUTE...USING) created/replaced.'