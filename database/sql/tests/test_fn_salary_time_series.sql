SELECT * FROM marketstat.fn_salary_time_series(
    p_source_temp_table_name := NULL, -- Standalone mode
    p_periods                := 6,
    p_granularity            := 'quarter'
    -- All other filter parameters default to NULL
) ORDER BY period_start;

SELECT * FROM marketstat.fn_salary_time_series(
    p_city_id           := 44,
    p_filter_date_end   := '2020-12-31',
    p_granularity       := 'quarter',
    p_periods           := 2
) ORDER BY period_start;