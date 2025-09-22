SET search_path = marketstat, public;

DROP FUNCTION IF EXISTS marketstat.fn_public_get_roles_by_location_industry(INT, INT, INT, INT, INT);

CREATE OR REPLACE FUNCTION marketstat.fn_public_get_roles_by_location_industry(
    p_industry_field_id            INT,
    p_federal_district_id          INT DEFAULT NULL,
    p_oblast_id                    INT DEFAULT NULL,
    p_city_id                      INT DEFAULT NULL,
    p_min_salary_records_for_role  INT DEFAULT 3
)
RETURNS TABLE(
    standard_job_role_title VARCHAR(255),
    average_salary          NUMERIC,
    salary_record_count     BIGINT
)
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
DECLARE
    v_period_start_date DATE;
    v_period_end_date DATE;
BEGIN
    IF p_industry_field_id IS NULL THEN
        RAISE EXCEPTION 'Industry Field ID (p_industry_field_id) is a mandatory parameter.';
    END IF;

    v_period_end_date := (date_trunc('month', CURRENT_DATE) - INTERVAL '1 day');
    v_period_start_date := (date_trunc('month', CURRENT_DATE) - INTERVAL '12 months');

    RAISE NOTICE 'Public Roles: Filtering salaries from % to %', v_period_start_date, v_period_end_date;

    RETURN QUERY
    SELECT
        sjr.standard_job_role_title,
        ROUND(AVG(fs.salary_amount), 0)::NUMERIC AS average_salary,
        COUNT(fs.salary_fact_id) AS salary_record_count
    FROM
        marketstat.fact_salaries fs
    JOIN
        marketstat.dim_date d_filter ON fs.date_id = d_filter.date_id
    JOIN
        marketstat.dim_job_role jr ON fs.job_role_id = jr.job_role_id
    JOIN
        marketstat.dim_standard_job_role sjr ON jr.standard_job_role_id = sjr.standard_job_role_id
    JOIN
        marketstat.dim_city c ON fs.city_id = c.city_id
    JOIN
        marketstat.dim_oblast o ON c.oblast_id = o.oblast_id
    JOIN
        marketstat.dim_federal_district fd ON o.district_id = fd.district_id
    WHERE
        sjr.industry_field_id = p_industry_field_id
        AND d_filter.full_date >= v_period_start_date
        AND d_filter.full_date <= v_period_end_date
        AND (p_city_id IS NULL OR fs.city_id = p_city_id)
        AND (p_oblast_id IS NULL OR c.oblast_id = p_oblast_id)
        AND (p_federal_district_id IS NULL OR fd.district_id = p_federal_district_id)
    GROUP BY
        sjr.standard_job_role_title
    HAVING
        COUNT(fs.salary_fact_id) >= p_min_salary_records_for_role
    ORDER BY
        average_salary DESC, salary_record_count DESC, sjr.standard_job_role_title ASC;
END;
$$;

ALTER FUNCTION marketstat.fn_public_get_roles_by_location_industry(INT, INT, INT, INT, INT) OWNER TO marketstat_administrator;
GRANT EXECUTE ON FUNCTION marketstat.fn_public_get_roles_by_location_industry(INT, INT, INT, INT, INT) TO marketstat_public_guest;
GRANT EXECUTE ON FUNCTION marketstat.fn_public_get_roles_by_location_industry(INT, INT, INT, INT, INT) TO marketstat_analyst;
