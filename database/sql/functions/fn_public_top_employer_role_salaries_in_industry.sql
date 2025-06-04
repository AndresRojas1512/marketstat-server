SET search_path = marketstat, public;

-- The input parameter signature (4 INTs) remains the same.
DROP FUNCTION IF EXISTS marketstat.fn_public_top_employer_role_salaries_in_industry(INT, INT, INT, INT);

CREATE OR REPLACE FUNCTION marketstat.fn_public_top_employer_role_salaries_in_industry(
    p_industry_field_id INT, -- Mandatory
    p_top_n_employers INT DEFAULT 5,
    p_top_m_roles_per_employer INT DEFAULT 3,
    p_min_salary_records_for_role_at_employer INT DEFAULT 3
)
RETURNS TABLE (
    employer_name VARCHAR(255),
    standard_job_role_title VARCHAR(255),
    average_salary_for_role NUMERIC,
    salary_record_count_for_role BIGINT,
    employer_rank BIGINT,
    role_rank_within_employer BIGINT
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

    -- Calculate the start and end of the "last 12 completed months" window
    v_period_end_date := (date_trunc('month', CURRENT_DATE) - INTERVAL '1 day');
    v_period_start_date := (date_trunc('month', CURRENT_DATE) - INTERVAL '12 months');

    RAISE NOTICE '[fn_public_top_employer_role_salaries_in_industry] Filtering salaries from % to %', v_period_start_date, v_period_end_date;

    RETURN QUERY
    WITH
    -- 1. Get all relevant salary records with employer and standard job role details
    --    for the given industry AND within the last 12 completed months.
    base_salary_data AS (
        SELECT
            fs.employer_id,
            de.employer_name,
            sjr.standard_job_role_id,
            sjr.standard_job_role_title,
            fs.salary_amount,
            fs.salary_fact_id
        FROM marketstat.fact_salaries fs
        JOIN marketstat.dim_date d_filter ON fs.date_id = d_filter.date_id -- Join dim_date
        JOIN marketstat.dim_employer de ON fs.employer_id = de.employer_id
        JOIN marketstat.dim_job_role jr ON fs.job_role_id = jr.job_role_id
        JOIN marketstat.dim_standard_job_role sjr ON jr.standard_job_role_id = sjr.standard_job_role_id
        WHERE
            sjr.industry_field_id = p_industry_field_id
            AND d_filter.full_date >= v_period_start_date -- Date filter
            AND d_filter.full_date <= v_period_end_date   -- Date filter
    ),
    -- 2. Rank employers based on their overall presence (number of salary records) in this industry (within the date range)
    ranked_employers AS (
        SELECT
            bsd.employer_id,
            bsd.employer_name,
            COUNT(bsd.salary_fact_id) AS total_records_for_employer_in_industry,
            RANK() OVER (ORDER BY COUNT(bsd.salary_fact_id) DESC, bsd.employer_name ASC) AS er_rank
        FROM base_salary_data bsd
        GROUP BY bsd.employer_id, bsd.employer_name
    ),
    -- 3. Select the top N employers
    top_employers AS (
        SELECT * FROM ranked_employers WHERE er_rank <= p_top_n_employers
    ),
    -- 4. For these top employers, calculate stats for each standard job role they have in this industry (within the date range)
    role_stats_at_top_employers AS (
        SELECT
            bsd.employer_id,
            bsd.standard_job_role_id,
            bsd.standard_job_role_title,
            COUNT(bsd.salary_fact_id) AS count_for_role,
            AVG(bsd.salary_amount) AS avg_salary_for_role,
            RANK() OVER (PARTITION BY bsd.employer_id
                         ORDER BY COUNT(bsd.salary_fact_id) DESC, AVG(bsd.salary_amount) DESC, bsd.standard_job_role_title ASC
                        ) AS role_rank_in_emp
        FROM base_salary_data bsd
        WHERE bsd.employer_id IN (SELECT employer_id FROM top_employers)
        GROUP BY bsd.employer_id, bsd.standard_job_role_id, bsd.standard_job_role_title
        HAVING COUNT(bsd.salary_fact_id) >= p_min_salary_records_for_role_at_employer
    )
    -- 5. Final selection of top M roles for each of the top N employers
    SELECT
        te.employer_name,
        rstate.standard_job_role_title,
        ROUND(rstate.avg_salary_for_role, 0)::NUMERIC AS average_salary_for_role,
        rstate.count_for_role AS salary_record_count_for_role,
        te.er_rank AS employer_rank,
        rstate.role_rank_in_emp AS role_rank_within_employer
    FROM role_stats_at_top_employers rstate
    JOIN top_employers te ON rstate.employer_id = te.employer_id
    WHERE rstate.role_rank_in_emp <= p_top_m_roles_per_employer
    ORDER BY
        te.er_rank ASC,
        te.employer_name ASC,
        rstate.role_rank_in_emp ASC,
        rstate.standard_job_role_title ASC;
END;
$$;

-- The input parameter signature (4 INTs) remains the same.
ALTER FUNCTION marketstat.fn_public_top_employer_role_salaries_in_industry(INT, INT, INT, INT) OWNER TO marketstat_administrator;
GRANT EXECUTE ON FUNCTION marketstat.fn_public_top_employer_role_salaries_in_industry(INT, INT, INT, INT) TO marketstat_public_guest;
GRANT EXECUTE ON FUNCTION marketstat.fn_public_top_employer_role_salaries_in_industry(INT, INT, INT, INT) TO marketstat_analyst;

\echo 'Function marketstat.fn_public_top_employer_role_salaries_in_industry (filtered for last 12 completed months) created/replaced.'
