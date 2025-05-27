SET search_path = marketstat, public;

DROP PROCEDURE IF EXISTS marketstat.sp_save_benchmark(OUT BIGINT, IN INT, IN JSONB, IN VARCHAR(255), IN INT, IN INT, IN INT, IN INT, IN INT, IN INT, IN DATE, IN DATE, IN INT, IN TEXT, IN INT);

CREATE OR REPLACE PROCEDURE marketstat.sp_save_benchmark(
    OUT p_new_benchmark_history_id      BIGINT,

    IN p_user_id                        INT,
    IN p_benchmark_result_json          JSONB,

    IN p_benchmark_name                 VARCHAR(255) DEFAULT NULL,
    IN p_filter_industry_field_id       INT          DEFAULT NULL,
    IN p_filter_standard_job_role_id    INT          DEFAULT NULL,
    IN p_filter_hierarchy_level_id      INT          DEFAULT NULL,
    IN p_filter_district_id             INT          DEFAULT NULL,
    IN p_filter_oblast_id               INT          DEFAULT NULL,
    IN p_filter_city_id                 INT          DEFAULT NULL,
    IN p_filter_date_start              DATE         DEFAULT NULL,
    IN p_filter_date_end                DATE         DEFAULT NULL,
    IN p_filter_target_percentile       INT          DEFAULT NULL,
    IN p_filter_granularity             TEXT         DEFAULT NULL,
    IN p_filter_periods                 INT          DEFAULT NULL
)
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
BEGIN
    INSERT INTO marketstat.benchmark_history (
        user_id,
        benchmark_name,
        benchmark_result_json,
        filter_industry_field_id,
        filter_standard_job_role_id,
        filter_hierarchy_level_id,
        filter_district_id,
        filter_oblast_id,
        filter_city_id,
        filter_date_start,
        filter_date_end,
        filter_target_percentile,
        filter_granularity,
        filter_periods
    ) VALUES (
        p_user_id,
        p_benchmark_name,
        p_benchmark_result_json,
        p_filter_industry_field_id,
        p_filter_standard_job_role_id,
        p_filter_hierarchy_level_id,
        p_filter_district_id,
        p_filter_oblast_id,
        p_filter_city_id,
        p_filter_date_start,
        p_filter_date_end,
        p_filter_target_percentile,
        p_filter_granularity,
        p_filter_periods
    )
    RETURNING benchmark_history_id INTO p_new_benchmark_history_id;

    RAISE NOTICE '[sp_save_benchmark] Saved benchmark with ID: % for user ID: %', p_new_benchmark_history_id, p_user_id;
END;
$$;

ALTER PROCEDURE marketstat.sp_save_benchmark(OUT BIGINT, IN INT, IN JSONB, IN VARCHAR(255), IN INT, IN INT, IN INT, IN INT, IN INT, IN INT, IN DATE, IN DATE, IN INT, IN TEXT, IN INT)
    OWNER TO marketstat_administrator;

GRANT EXECUTE ON PROCEDURE marketstat.sp_save_benchmark(OUT BIGINT, IN INT, IN JSONB, IN VARCHAR(255), IN INT, IN INT, IN INT, IN INT, IN INT, IN INT, IN DATE, IN DATE, IN INT, IN TEXT, IN INT)
    TO marketstat_analyst;

\echo 'Procedure marketstat.sp_save_benchmark (ID-based filters) created/replaced and privileges granted.'