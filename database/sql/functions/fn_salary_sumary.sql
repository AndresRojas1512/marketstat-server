-- Run as marketstat_administrator
SET search_path = marketstat, public;

DROP FUNCTION IF EXISTS marketstat.fn_salary_summary(TEXT,INT,INT,INT,INT,INT,INT,DATE,DATE,INT);

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
RETURNS TABLE(
    percentile25      NUMERIC,
    percentile50      NUMERIC,
    percentile75      NUMERIC,
    percentile_target NUMERIC,
    average_salary    NUMERIC,
    total_count       BIGINT
)
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
DECLARE
    _sql TEXT;
    _source_query_fragment TEXT;
BEGIN
    IF p_target_percentile < 0 OR p_target_percentile > 100 THEN
        RAISE EXCEPTION 'Target percentile must be between 0 and 100 (inclusive). Received: %', p_target_percentile;
    END IF;

    IF p_source_temp_table_name IS NOT NULL THEN
        RAISE NOTICE '[fn_salary_summary] Using pre-filtered data from temp table: %', p_source_temp_table_name;
        _source_query_fragment := format('%I src WHERE src.salary_amount IS NOT NULL', p_source_temp_table_name);

        _sql := format('
            SELECT
                (percentile_cont(0.25) WITHIN GROUP (ORDER BY data_source.salary_amount))::NUMERIC,
                (percentile_cont(0.50) WITHIN GROUP (ORDER BY data_source.salary_amount))::NUMERIC,
                (percentile_cont(0.75) WITHIN GROUP (ORDER BY data_source.salary_amount))::NUMERIC,
                (percentile_cont(%L / 100.0) WITHIN GROUP (ORDER BY data_source.salary_amount))::NUMERIC,
                (AVG(data_source.salary_amount))::NUMERIC,
                COUNT(data_source.salary_amount)::BIGINT
            FROM (SELECT salary_amount FROM %s) AS data_source;',
            p_target_percentile,
            _source_query_fragment
        );

        RAISE NOTICE '[fn_salary_summary] Executing SQL for temp table: %', _sql;
        BEGIN
            RETURN QUERY EXECUTE _sql;
        EXCEPTION
            WHEN undefined_table THEN
                RAISE WARNING '[fn_salary_summary] ERROR: Source temporary table % not found. Returning empty summary.', p_source_temp_table_name;
                RETURN QUERY SELECT
                                NULL::NUMERIC, NULL::NUMERIC, NULL::NUMERIC,
                                NULL::NUMERIC, NULL::NUMERIC, 0::BIGINT
                             WHERE FALSE;
                RETURN;
        END;
    ELSE
        RAISE NOTICE '[fn_salary_summary] Filtering data internally. Filters: IndustryID=% SJRID=% HLevelID=% DistrictID=% OblastID=% CityID=% DateStart=% DateEnd=%',
                 p_industry_field_id, p_standard_job_role_id, p_hierarchy_level_id, p_district_id, p_oblast_id, p_city_id, p_date_start, p_date_end;
        _source_query_fragment := format(
            'marketstat.fn_filtered_salaries(%L, %L, %L, %L, %L, %L, %L, %L) fs WHERE fs.salary_amount IS NOT NULL',
            p_industry_field_id, p_standard_job_role_id, p_hierarchy_level_id,
            p_district_id, p_oblast_id, p_city_id, p_date_start, p_date_end
        );

        _sql := format('
            SELECT
                (percentile_cont(0.25) WITHIN GROUP (ORDER BY data_source.salary_amount))::NUMERIC,
                (percentile_cont(0.50) WITHIN GROUP (ORDER BY data_source.salary_amount))::NUMERIC,
                (percentile_cont(0.75) WITHIN GROUP (ORDER BY data_source.salary_amount))::NUMERIC,
                (percentile_cont(%L / 100.0) WITHIN GROUP (ORDER BY data_source.salary_amount))::NUMERIC,
                (AVG(data_source.salary_amount))::NUMERIC,
                COUNT(data_source.salary_amount)::BIGINT
            FROM (SELECT salary_amount FROM %s) AS data_source;',
            p_target_percentile,
            _source_query_fragment
        );

        RAISE NOTICE '[fn_salary_summary] Executing SQL for internal filtering: %', _sql;
        RETURN QUERY EXECUTE _sql;
    END IF;
END;
$$;

ALTER FUNCTION marketstat.fn_salary_summary(TEXT,INT,INT,INT,INT,INT,INT,DATE,DATE,INT) OWNER TO marketstat_administrator;
GRANT EXECUTE ON FUNCTION marketstat.fn_salary_summary(TEXT,INT,INT,INT,INT,INT,INT,DATE,DATE,INT) TO marketstat_analyst;
\echo 'Function marketstat.fn_salary_summary (hybrid, with exception handling for temp table) created/replaced.'
