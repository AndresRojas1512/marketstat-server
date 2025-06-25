SET search_path = marketstat, public;

DROP FUNCTION IF EXISTS marketstat.fn_public_salary_by_education_in_industry(INT, INT, INT, INT);

CREATE OR REPLACE FUNCTION marketstat.fn_public_salary_by_education_in_industry(
    p_industry_field_id INT, -- Mandatory
    p_top_n_specialties INT DEFAULT 10,
    p_min_employees_per_specialty INT DEFAULT 5,
    p_min_employees_per_level_in_specialty INT DEFAULT 3
)
RETURNS TABLE (
    education_specialty                 VARCHAR(255),
    education_level_name                VARCHAR(255),
    average_salary                      NUMERIC,
    employee_count_for_level            BIGINT,
    overall_specialty_employee_count    BIGINT
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

    RAISE NOTICE '[fn_public_salary_by_education_in_industry] Filtering salaries from % to %', v_period_start_date, v_period_end_date;

    RETURN QUERY
    WITH
    industry_employee_education_salary AS (
        SELECT
            fs.employee_id,
            fs.salary_amount,
            de.specialty,
            del.education_level_name
        FROM marketstat.fact_salaries fs
        JOIN marketstat.dim_date d_filter ON fs.date_id = d_filter.date_id
        JOIN marketstat.dim_job_role jr ON fs.job_role_id = jr.job_role_id
        JOIN marketstat.dim_standard_job_role sjr ON jr.standard_job_role_id = sjr.standard_job_role_id
        JOIN marketstat.dim_employee_education dee ON fs.employee_id = dee.employee_id
        JOIN marketstat.dim_education de ON dee.education_id = de.education_id
        JOIN marketstat.dim_education_level del ON de.education_level_id = del.education_level_id
        WHERE sjr.industry_field_id = p_industry_field_id
            AND d_filter.full_date >= v_period_start_date
            AND d_filter.full_date <= v_period_end_date
    ),
    specialty_level_stats AS (
        SELECT
            ieds.specialty,
            ieds.education_level_name,
            COUNT(DISTINCT ieds.employee_id) AS employee_count_for_level,
            AVG(ieds.salary_amount) AS avg_salary_for_level
        FROM industry_employee_education_salary ieds
        GROUP BY ieds.specialty, ieds.education_level_name
        HAVING COUNT(DISTINCT ieds.employee_id) >= p_min_employees_per_level_in_specialty
    ),
    ranked_overall_specialties AS (
        SELECT
            sls.specialty,
            SUM(sls.employee_count_for_level) AS total_employees_for_specialty, -- This SUM returns NUMERIC
            RANK() OVER (ORDER BY SUM(sls.employee_count_for_level) DESC, sls.specialty ASC) AS specialty_rank
        FROM specialty_level_stats sls
        GROUP BY sls.specialty
        HAVING SUM(sls.employee_count_for_level) >= p_min_employees_per_specialty
    ),
    top_ranked_specialties AS (
        SELECT
            ros.specialty,
            ros.total_employees_for_specialty,
            ros.specialty_rank
        FROM ranked_overall_specialties ros
        WHERE ros.specialty_rank <= p_top_n_specialties
    )
    SELECT
        sls.specialty AS education_specialty,
        sls.education_level_name,
        ROUND(sls.avg_salary_for_level, 0)::NUMERIC AS average_salary,
        sls.employee_count_for_level, -- This is already BIGINT from COUNT(DISTINCT)
        trs.total_employees_for_specialty::BIGINT AS overall_specialty_employee_count -- CAST to BIGINT
    FROM
        specialty_level_stats sls
    JOIN
        top_ranked_specialties trs ON sls.specialty = trs.specialty
    ORDER BY
        trs.specialty_rank ASC,
        sls.education_level_name ASC;
END;
$$;

ALTER FUNCTION marketstat.fn_public_salary_by_education_in_industry(INT, INT, INT, INT) OWNER TO marketstat_administrator;
GRANT EXECUTE ON FUNCTION marketstat.fn_public_salary_by_education_in_industry(INT, INT, INT, INT) TO marketstat_public_guest;
GRANT EXECUTE ON FUNCTION marketstat.fn_public_salary_by_education_in_industry(INT, INT, INT, INT) TO marketstat_analyst;

\echo 'Function marketstat.fn_public_salary_by_education_in_industry (corrected return type for count) created/replaced.'
