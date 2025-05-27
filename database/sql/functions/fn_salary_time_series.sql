SET search_path = marketstat, public;

DROP FUNCTION IF EXISTS marketstat.fn_salary_time_series(TEXT,INT,INT,INT,INT,INT,INT,DATE,DATE,TEXT,INT);

CREATE OR REPLACE FUNCTION marketstat.fn_salary_time_series(
    p_source_temp_table_name TEXT    DEFAULT NULL,
    p_industry_field_id      INT     DEFAULT NULL,
    p_standard_job_role_id   INT     DEFAULT NULL,
    p_hierarchy_level_id     INT     DEFAULT NULL,
    p_district_id            INT     DEFAULT NULL,
    p_oblast_id              INT     DEFAULT NULL,
    p_city_id                INT     DEFAULT NULL,
    p_filter_date_start      DATE    DEFAULT NULL,
    p_filter_date_end        DATE    DEFAULT NULL,
    p_granularity            TEXT    DEFAULT 'month',
    p_periods                INT     DEFAULT 12
)
RETURNS TABLE(
    period_start            DATE,
    avg_salary              NUMERIC,
    salary_count_in_period  BIGINT
)
LANGUAGE plpgsql SECURITY DEFINER AS $$
DECLARE
    _step                     INTERVAL;
    _actual_series_start_date DATE;
    _actual_series_end_date   DATE;
    _reference_end_date       DATE;
    _sql                      TEXT;
BEGIN
    IF   p_granularity = 'month'   THEN _step := INTERVAL '1 month';
    ELSIF p_granularity = 'quarter' THEN _step := INTERVAL '3 months';
    ELSIF p_granularity = 'year'    THEN _step := INTERVAL '1 year';
    ELSE RAISE EXCEPTION 'Invalid granularity: %. Must be "month", "quarter", or "year".', p_granularity;
    END IF;

    _reference_end_date := COALESCE(p_filter_date_end, CURRENT_DATE);
    _actual_series_end_date := date_trunc(p_granularity, date_trunc(p_granularity, _reference_end_date) - INTERVAL '1 microsecond');
    _actual_series_start_date := _actual_series_end_date - (p_periods - 1) * _step;

    RAISE NOTICE '[fn_salary_time_series] Calculated series range: % to %', _actual_series_start_date, _actual_series_end_date;

    IF p_source_temp_table_name IS NOT NULL THEN
        RAISE NOTICE '[fn_salary_time_series] Using pre-filtered data from temp table: %', p_source_temp_table_name;
        _sql := format('
            WITH series AS (
                SELECT generate_series(%L::DATE, %L::DATE, %L::INTERVAL)::DATE AS period_start
            ), data_for_series AS (
                SELECT
                    date_trunc(%L, src.full_date)::DATE AS period_start,
                    src.salary_amount -- Need salary_amount for AVG and COUNT
                FROM %I src
                WHERE src.full_date >= %L::DATE AND src.full_date <= (%L::DATE + %L::INTERVAL - INTERVAL ''1 day'')
                  AND src.salary_amount IS NOT NULL
            )
            SELECT
                s.period_start,
                (AVG(dfs.salary_amount))::NUMERIC AS avg_salary,
                COUNT(dfs.salary_amount)::BIGINT AS salary_count_in_period -- Count non-null salaries
            FROM series s
            LEFT JOIN data_for_series dfs ON dfs.period_start = s.period_start
            GROUP BY s.period_start
            ORDER BY s.period_start;',
            _actual_series_start_date, _actual_series_end_date, _step,
            p_granularity, p_source_temp_table_name,
            _actual_series_start_date, _actual_series_end_date, _step);
    ELSE
        DECLARE
            _effective_filter_start_date DATE;
            _effective_filter_end_date   DATE;
        BEGIN
            RAISE NOTICE '[fn_salary_time_series] Filtering data internally.';
            _effective_filter_start_date := _actual_series_start_date;
            IF p_filter_date_start IS NOT NULL AND p_filter_date_start > _effective_filter_start_date THEN
                _effective_filter_start_date := p_filter_date_start;
            END IF;
            _effective_filter_end_date := _actual_series_end_date + _step - INTERVAL '1 day';
            IF p_filter_date_end IS NOT NULL AND p_filter_date_end < _effective_filter_end_date THEN
                _effective_filter_end_date := p_filter_date_end;
            END IF;

            IF _effective_filter_start_date > _effective_filter_end_date THEN
                RAISE NOTICE '[fn_salary_time_series] (Standalone) Effective date range for filtering ( % to % ) is invalid. Returning empty set.', _effective_filter_start_date, _effective_filter_end_date;
                RETURN QUERY SELECT NULL::DATE, NULL::NUMERIC, 0::BIGINT WHERE FALSE;
                RETURN;
            END IF;

            _sql := format('
                WITH series AS (
                    SELECT generate_series(%L::DATE, %L::DATE, %L::INTERVAL)::DATE AS period_start
                ), data_for_series AS (
                    SELECT
                        date_trunc(%L, fs.full_date)::DATE AS period_start,
                        fs.salary_amount -- Need salary_amount for AVG and COUNT
                    FROM marketstat.fn_filtered_salaries(%L, %L, %L, %L, %L, %L, %L, %L) fs
                    WHERE fs.full_date >= %L::DATE AND fs.full_date <= (%L::DATE + %L::INTERVAL - INTERVAL ''1 day'')
                      AND fs.salary_amount IS NOT NULL
                )
                SELECT
                    s.period_start,
                    (AVG(dfs.salary_amount))::NUMERIC AS avg_salary,
                    COUNT(dfs.salary_amount)::BIGINT AS salary_count_in_period -- Count non-null salaries
                FROM series s
                LEFT JOIN data_for_series dfs ON dfs.period_start = s.period_start
                GROUP BY s.period_start
                ORDER BY s.period_start;',
                _actual_series_start_date, _actual_series_end_date, _step,
                p_granularity,
                p_industry_field_id, p_standard_job_role_id, p_hierarchy_level_id, p_district_id, p_oblast_id, p_city_id,
                _effective_filter_start_date, _effective_filter_end_date,
                _actual_series_start_date, _actual_series_end_date, _step);
        END;
    END IF;
    RETURN QUERY EXECUTE _sql;
END;
$$;

ALTER FUNCTION marketstat.fn_salary_time_series(TEXT,INT,INT,INT,INT,INT,INT,DATE,DATE,TEXT,INT) OWNER TO marketstat_administrator;
GRANT EXECUTE ON FUNCTION marketstat.fn_salary_time_series(TEXT,INT,INT,INT,INT,INT,INT,DATE,DATE,TEXT,INT) TO marketstat_analyst;
\echo 'Function marketstat.fn_salary_time_series (hybrid, with count, no salary_ids) created/replaced.'