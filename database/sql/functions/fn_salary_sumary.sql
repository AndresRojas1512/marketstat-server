DROP FUNCTION IF EXISTS marketstat.fn_salary_summary(TEXT,INT);
CREATE OR REPLACE FUNCTION marketstat.fn_salary_summary(

    p_source_temp_table_name TEXT    DEFAULT NULL,
    p_industry_field_id      INT     DEFAULT NULL,
    p_standard_job_role_id   INT     DEFAULT NULL,
    p_hierarchy_level_id     INT     DEFAULT NULL,
    p_district_id            INT     DEFAULT NULL,
    p_oblast_id              INT     DEFAULT NULL,
    p_city_id                INT     DEFAULT NULL,
    p_date_start             DATE    DEFAULT NULL,
    p_date_end               DATE    DEFAULT NULL,
    p_target_percentile      INT     DEFAULT 90
)
RETURNS TABLE( percentile25 NUMERIC, percentile50 NUMERIC, percentile75 NUMERIC, percentile_target NUMERIC, average_salary NUMERIC, total_count BIGINT )
LANGUAGE plpgsql SECURITY DEFINER AS $$
DECLARE _sql TEXT;
BEGIN
  IF p_target_percentile < 0 OR p_target_percentile > 100 THEN RAISE EXCEPTION 'Target percentile must be between 0 and 100. Received: %', p_target_percentile; END IF;

  IF p_source_temp_table_name IS NOT NULL THEN
    RAISE NOTICE 'fn_salary_summary: Using pre-filtered data from temp table %', p_source_temp_table_name;
    _sql := format('
        SELECT
            (percentile_cont(0.25) WITHIN GROUP (ORDER BY src.salary_amount))::NUMERIC,
            (percentile_cont(0.50) WITHIN GROUP (ORDER BY src.salary_amount))::NUMERIC,
            (percentile_cont(0.75) WITHIN GROUP (ORDER BY src.salary_amount))::NUMERIC,
            (percentile_cont(%L / 100.0) WITHIN GROUP (ORDER BY src.salary_amount))::NUMERIC,
            (AVG(src.salary_amount))::NUMERIC,
            COUNT(src.salary_amount)::BIGINT
        FROM %I src WHERE src.salary_amount IS NOT NULL;', p_target_percentile, p_source_temp_table_name);
  ELSE
    RAISE NOTICE 'fn_salary_summary: Filtering data internally.';
    _sql := format('
        SELECT
            (percentile_cont(0.25) WITHIN GROUP (ORDER BY fs.salary_amount))::NUMERIC,
            (percentile_cont(0.50) WITHIN GROUP (ORDER BY fs.salary_amount))::NUMERIC,
            (percentile_cont(0.75) WITHIN GROUP (ORDER BY fs.salary_amount))::NUMERIC,
            (percentile_cont(%L / 100.0) WITHIN GROUP (ORDER BY fs.salary_amount))::NUMERIC,
            (AVG(fs.salary_amount))::NUMERIC,
            COUNT(fs.salary_amount)::BIGINT
        FROM marketstat.fn_filtered_salaries(%L, %L, %L, %L, %L, %L, %L, %L) fs WHERE fs.salary_amount IS NOT NULL;',
        p_target_percentile, p_industry_field_id, p_standard_job_role_id, p_hierarchy_level_id,
        p_district_id, p_oblast_id, p_city_id, p_date_start, p_date_end);
  END IF;
  RETURN QUERY EXECUTE _sql;
END; $$;
ALTER FUNCTION marketstat.fn_salary_summary(TEXT,INT,INT,INT,INT,INT,INT,DATE,DATE,INT) OWNER TO marketstat_administrator;
GRANT EXECUTE ON FUNCTION marketstat.fn_salary_summary(TEXT,INT,INT,INT,INT,INT,INT,DATE,DATE,INT) TO marketstat_analyst;
\echo 'Function marketstat.fn_salary_summary (hybrid) created/replaced.'