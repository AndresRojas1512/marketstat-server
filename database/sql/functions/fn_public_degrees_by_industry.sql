SET search_path = marketstat, public;

DROP FUNCTION IF EXISTS marketstat.fn_public_degrees_by_industry(INT, INT, INT);

CREATE OR REPLACE FUNCTION marketstat.fn_public_degrees_by_industry(
    p_industry_field_id            INT,
    p_top_n_degrees                INT DEFAULT 5,
    p_min_employee_count_for_degree INT DEFAULT 3
)
RETURNS TABLE (
    education_specialty         VARCHAR(255),
    employee_with_degree_count  BIGINT
)
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
BEGIN
    IF p_industry_field_id IS NULL THEN
        RAISE EXCEPTION 'Industry Field ID (p_industry_field_id) is a mandatory parameter.';
    END IF;

    RETURN QUERY
    WITH employees_in_industry_jobs AS (
        SELECT DISTINCT fs.employee_id
        FROM marketstat.fact_salaries fs
        JOIN marketstat.dim_job_role jr ON fs.job_role_id = jr.job_role_id
        JOIN marketstat.dim_standard_job_role sjr ON jr.standard_job_role_id = sjr.standard_job_role_id
        WHERE sjr.industry_field_id = p_industry_field_id
    )
    SELECT
        de.specialty AS education_specialty,
        COUNT(DISTINCT eij.employee_id) AS employee_with_degree_count
    FROM
        marketstat.dim_education de
    JOIN
        marketstat.dim_employee_education dee ON de.education_id = dee.education_id
    JOIN
        employees_in_industry_jobs eij ON dee.employee_id = eij.employee_id
    GROUP BY
        de.specialty
    HAVING
        COUNT(DISTINCT eij.employee_id) >= p_min_employee_count_for_degree
    ORDER BY
        employee_with_degree_count DESC, de.specialty ASC
    LIMIT
        p_top_n_degrees;
END;
$$;

ALTER FUNCTION marketstat.fn_public_degrees_by_industry(INT, INT, INT) OWNER TO marketstat_administrator;
GRANT EXECUTE ON FUNCTION marketstat.fn_public_degrees_by_industry(INT, INT, INT) TO marketstat_public_guest;
GRANT EXECUTE ON FUNCTION marketstat.fn_public_degrees_by_industry(INT, INT, INT) TO marketstat_analyst;
\echo 'Function marketstat.fn_public_degrees_by_industry created/replaced.'