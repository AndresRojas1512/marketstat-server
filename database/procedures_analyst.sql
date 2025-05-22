CREATE OR REPLACE PROCEDURE marketstat.sp_get_benchmarking_data(
    -- OUT parameter first
    OUT p_result_json                 JSONB,

    -- String-based filter IN parameters
    IN p_industry_field_name_filter   TEXT    DEFAULT NULL,
    IN p_standard_job_role_title_filter TEXT  DEFAULT NULL,
    IN p_hierarchy_level_name_filter  TEXT    DEFAULT NULL,
    IN p_district_name_filter         TEXT    DEFAULT NULL,
    IN p_oblast_name_filter           TEXT    DEFAULT NULL,
    IN p_city_name_filter             TEXT    DEFAULT NULL,

    -- Date filter IN parameters (remain as DATE)
    IN p_date_start                   DATE    DEFAULT NULL,
    IN p_date_end                     DATE    DEFAULT NULL,

    -- Parameters specific to fn_salary_summary (IN)
    IN p_target_percentile            INT     DEFAULT 90,

    -- Parameters specific to fn_salary_time_series (IN)
    IN p_granularity                  TEXT    DEFAULT 'month',
    IN p_periods                      INT     DEFAULT 12
)
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
BEGIN
    RAISE NOTICE 'Resolving filter strings to IDs...';

    -- Step 1: Resolve string filter parameters to their corresponding IDs
    IF p_industry_field_name_filter IS NOT NULL THEN
        SELECT dif.industry_field_id INTO v_industry_field_id
        FROM marketstat.dim_industry_field dif
        WHERE dif.industry_field_name = p_industry_field_name_filter LIMIT 1;
        IF NOT FOUND THEN RAISE NOTICE 'Industry field name "%" not found.', p_industry_field_name_filter; END IF;
    END IF;

    IF p_standard_job_role_title_filter IS NOT NULL THEN
        SELECT dsjr.standard_job_role_id INTO v_standard_job_role_id
        FROM marketstat.dim_standard_job_role dsjr
        WHERE dsjr.standard_job_role_title = p_standard_job_role_title_filter LIMIT 1;
        IF NOT FOUND THEN RAISE NOTICE 'Standard job role title "%" not found.', p_standard_job_role_title_filter; END IF;
    END IF;

    IF p_hierarchy_level_name_filter IS NOT NULL THEN
        SELECT dhl.hierarchy_level_id INTO v_hierarchy_level_id
        FROM marketstat.dim_hierarchy_level dhl
        WHERE dhl.hierarchy_level_name = p_hierarchy_level_name_filter LIMIT 1;
        IF NOT FOUND THEN RAISE NOTICE 'Hierarchy level name "%" not found.', p_hierarchy_level_name_filter; END IF;
    END IF;

    IF p_district_name_filter IS NOT NULL THEN
        SELECT dfd.district_id INTO v_district_id
        FROM marketstat.dim_federal_district dfd
        WHERE dfd.district_name = p_district_name_filter LIMIT 1;
        IF NOT FOUND THEN RAISE NOTICE 'Federal district name "%" not found.', p_district_name_filter; END IF;
    END IF;

    IF p_oblast_name_filter IS NOT NULL THEN
        SELECT dof.oblast_id INTO v_oblast_id
        FROM marketstat.dim_oblast dof
        WHERE dof.oblast_name = p_oblast_name_filter LIMIT 1;
        IF NOT FOUND THEN RAISE NOTICE 'Oblast name "%" not found.', p_oblast_name_filter; END IF;
    END IF;

    IF p_city_name_filter IS NOT NULL THEN
        IF v_oblast_id IS NOT NULL THEN
            SELECT dc.city_id INTO v_city_id
            FROM marketstat.dim_city dc
            WHERE dc.city_name = p_city_name_filter AND dc.oblast_id = v_oblast_id LIMIT 1;
            IF NOT FOUND THEN RAISE NOTICE 'City name "%" in oblast ID % not found.', p_city_name_filter, v_oblast_id; END IF;
        ELSE
            RAISE NOTICE 'City name filter "%" provided, but corresponding oblast_id could not be determined. City filter may be ineffective or ambiguous.', p_city_name_filter;
            v_city_id := NULL;
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

    -- Step 3: Assign the combined JSONB object to the OUT parameter
    RAISE NOTICE 'Building final JSON payload...';
    p_result_json := jsonb_build_object(
        'salaryDistribution', v_salary_distribution,
        'salarySummary',      v_salary_summary,
        'salaryTimeSeries',   v_salary_time_series
    );

    RAISE NOTICE 'Benchmarking data procedure finished.';
END;
$$;