CREATE OR REPLACE PROCEDURE marketstat.sp_save_benchmark(
    OUT p_new_benchmark_history_id  BIGINT,

    IN p_user_id INT,
    IN p_benchmark_result_json      JSONB,

    IN p_benchmark_name                 VARCHAR(255) DEFAULT NULL,
    IN p_filter_industry_field_name     TEXT DEFAULT NULL,
    IN p_filter_standard_job_role_title TEXT DEFAULT NULL,
    IN p_filter_hierarchy_level_name    TEXT DEFAULT NULL,
    IN p_filter_district_name           TEXT DEFAULT NULL,
    IN p_filter_oblast_name             TEXT DEFAULT NULL,
    IN p_filter_city_name               TEXT DEFAULT NULL,
    IN p_filter_date_start              DATE DEFAULT NULL,
    IN p_filter_date_end                DATE DEFAULT NULL,
    IN p_filter_target_percentile       INT DEFAULT NULL,
    IN p_filter_granularity             TEXT DEFAULT NULL,
    IN p_filter_periods                 INT DEFAULT NULL
)
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
BEGIN
    INSERT INTO marketstat.benchmark_history (
        user_id,
        benchmark_name,
        filter_industry_field_name,
        filter_standard_job_role_title,
        filter_hierarchy_level_name,
        filter_district_name,
        filter_oblast_name,
        filter_city_name,
        filter_date_start,
        filter_date_end,
        filter_target_percentile,
        filter_granularity,
        filter_periods,
        benchmark_result_json
    ) VALUES (
        p_user_id,
        p_benchmark_name,
        p_filter_industry_field_name,
        p_filter_standard_job_role_title,
        p_filter_hierarchy_level_name,
        p_filter_district_name,
        p_filter_oblast_name,
        p_filter_city_name,
        p_filter_date_start,
        p_filter_date_end,
        p_filter_target_percentile,
        p_filter_granularity,
        p_filter_periods,
        p_benchmark_result_json
    )
    RETURNING benchmark_history_id INTO p_new_benchmark_history_id;
    RAISE NOTICE 'Saved benchmark with ID: % for user ID: %', p_new_benchmark_history_id, p_user_id;
END;
$$;