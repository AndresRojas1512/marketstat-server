SET search_path = marketstat, public;

DROP FUNCTION IF EXISTS marketstat.fn_compute_benchmark_data(INT,INT,INT,INT,INT,INT,DATE,DATE,INT,TEXT,INT);

CREATE OR REPLACE FUNCTION marketstat.fn_compute_benchmark_data(
    p_industry_field_id            INT     DEFAULT NULL,
    p_standard_job_role_id         INT     DEFAULT NULL,
    p_hierarchy_level_id           INT     DEFAULT NULL,
    p_district_id                  INT     DEFAULT NULL,
    p_oblast_id                    INT     DEFAULT NULL,
    p_city_id                      INT     DEFAULT NULL,
    p_date_start                   DATE    DEFAULT NULL,
    p_date_end                     DATE    DEFAULT NULL,
    p_target_percentile            INT     DEFAULT 90,
    p_granularity                  TEXT    DEFAULT 'month',
    p_periods                      INT     DEFAULT 12
)
RETURNS JSONB
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
DECLARE
    v_salary_distribution JSONB;
    v_salary_summary      JSONB;
    v_salary_time_series  JSONB;
    v_final_json_payload  JSONB;
    v_temp_table_name     TEXT    := 'temp_benchmark_filtered_data_' || translate(gen_random_uuid()::text, '-', '');
    v_row_count_in_temp   BIGINT;
    v_sql_create_temp     TEXT;
BEGIN
    RAISE NOTICE '[fn_compute_benchmark_data] Starting. Input Filters: IndustryFieldId=%, StandardJobRoleId=%, HierarchyLevelId=%, DistrictId=%, OblastId=%, CityId=%, DateStart=%, DateEnd=%. Analytical Params: TargetPercentile=%, Granularity=%, Periods=%',
                 p_industry_field_id, p_standard_job_role_id, p_hierarchy_level_id, p_district_id, p_oblast_id, p_city_id, p_date_start, p_date_end,
                 p_target_percentile, p_granularity, p_periods;
    RAISE NOTICE '[fn_compute_benchmark_data] Using temporary table name: %', v_temp_table_name;

    v_sql_create_temp := format(
        'CREATE TEMP TABLE %I ON COMMIT DROP AS
         SELECT
            f.salary_fact_id,
            f.date_id,
            f.city_id_from_fact,
            f.employer_id,
            f.job_role_id_from_fact,
            f.employee_id,
            f.salary_amount,
            f.bonus_amount,
            f.full_date,
            f.resolved_city_id,
            f.resolved_oblast_id,
            f.resolved_district_id,
            f.resolved_standard_job_role_id,
            f.resolved_hierarchy_level_id,
            f.resolved_industry_field_id
         FROM marketstat.fn_filtered_salaries(%L, %L, %L, %L, %L, %L, %L, %L) AS f;',
        v_temp_table_name,
        p_industry_field_id, p_standard_job_role_id, p_hierarchy_level_id,
        p_district_id, p_oblast_id, p_city_id, p_date_start, p_date_end
    );

    BEGIN
        EXECUTE v_sql_create_temp;
        GET DIAGNOSTICS v_row_count_in_temp = ROW_COUNT;
        RAISE NOTICE '[fn_compute_benchmark_data] Temporary table % created and populated with % rows.', v_temp_table_name, v_row_count_in_temp;

        IF v_row_count_in_temp = 0 THEN
             RAISE NOTICE '[fn_compute_benchmark_data] No data matched filters for %. Analytical functions will operate on an empty set.', v_temp_table_name;
        END IF;
    EXCEPTION
        WHEN OTHERS THEN
            RAISE WARNING '[fn_compute_benchmark_data] CRITICAL ERROR creating/populating temp table %: % - %', v_temp_table_name, SQLSTATE, SQLERRM;
            v_final_json_payload := jsonb_build_object(
                'salaryDistribution', '[]'::JSONB,
                'salarySummary',      '{}'::JSONB,
                'salaryTimeSeries',   '[]'::JSONB,
                'error',              'Failed to prepare filtered data for benchmarking due to an internal error.',
                'detail',             SQLERRM
            );
            EXECUTE format('DROP TABLE IF EXISTS %I;', v_temp_table_name);
            RETURN v_final_json_payload;
    END;

    RAISE NOTICE '[fn_compute_benchmark_data] Fetching salary distribution using temp table: %', v_temp_table_name;
    SELECT COALESCE(jsonb_agg(dist_data), '[]'::JSONB)
    INTO v_salary_distribution
    FROM marketstat.fn_salary_distribution(
        p_source_temp_table_name := v_temp_table_name
    ) AS dist_data;

    RAISE NOTICE '[fn_compute_benchmark_data] Fetching salary summary using temp table: %, TargetPercentile: %', v_temp_table_name, p_target_percentile;
    SELECT COALESCE(row_to_json(summary_data)::JSONB, '{}'::JSONB)
    INTO v_salary_summary
    FROM marketstat.fn_salary_summary(
            p_source_temp_table_name := v_temp_table_name,
            p_target_percentile      := p_target_percentile
         ) AS summary_data;

    RAISE NOTICE '[fn_compute_benchmark_data] Fetching salary time series using temp table: %, DateEndForSeries: %, Granularity: %, Periods: %',
                 v_temp_table_name, p_date_end, p_granularity, p_periods;
    SELECT COALESCE(jsonb_agg(ts_data), '[]'::JSONB)
    INTO v_salary_time_series
    FROM marketstat.fn_salary_time_series(
             p_source_temp_table_name      := v_temp_table_name,
             p_filter_date_end             := p_date_end,
             p_granularity                 := p_granularity,
             p_periods                     := p_periods
         ) AS ts_data;

    RAISE NOTICE '[fn_compute_benchmark_data] Building final JSON payload...';
    v_final_json_payload := jsonb_build_object(
        'salaryDistribution', v_salary_distribution,
        'salarySummary',      v_salary_summary,
        'salaryTimeSeries',   v_salary_time_series
    );

    RAISE NOTICE '[fn_compute_benchmark_data] Computation finished.';
    RETURN v_final_json_payload;
END;
$$;

ALTER FUNCTION marketstat.fn_compute_benchmark_data(INT,INT,INT,INT,INT,INT,DATE,DATE,INT,TEXT,INT) OWNER TO marketstat_administrator;
GRANT EXECUTE ON FUNCTION marketstat.fn_compute_benchmark_data(INT,INT,INT,INT,INT,INT,DATE,DATE,INT,TEXT,INT) TO marketstat_analyst;
\echo 'Function marketstat.fn_compute_benchmark_data (orchestrator, cleaned logging) created/replaced.'
