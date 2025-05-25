DROP FUNCTION IF EXISTS marketstat.fn_filtered_salaries(INT,INT,INT,INT,INT,INT,DATE,DATE);

CREATE OR REPLACE FUNCTION marketstat.fn_filtered_salaries(
    p_industry_field_id     INT     DEFAULT NULL,
    p_standard_job_role_id  INT     DEFAULT NULL,
    p_hierarchy_level_id    INT     DEFAULT NULL,
    p_district_id           INT     DEFAULT NULL,
    p_oblast_id             INT     DEFAULT NULL,
    p_city_id               INT     DEFAULT NULL,
    p_date_start            DATE    DEFAULT NULL,
    p_date_end              DATE    DEFAULT NULL
)
RETURNS TABLE(
    salary_fact_id          BIGINT,
    date_id                 INT,
    city_id_from_fact       INT,
    employer_id             INT,
    job_role_id_from_fact   INT,
    employee_id             INT,
    salary_amount           NUMERIC,
    bonus_amount            NUMERIC,

    full_date               DATE,

    resolved_city_id        INT,
    resolved_oblast_id      INT,
    resolved_district_id    INT,
    resolved_standard_job_role_id INT,
    resolved_hierarchy_level_id INT,
    resolved_industry_field_id  INT
)
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
BEGIN
  RETURN QUERY
    SELECT
      fs.salary_fact_id,
      fs.date_id,
      fs.city_id AS city_id_from_fact,
      fs.employer_id,
      fs.job_role_id AS job_role_id_from_fact,
      fs.employee_id,
      fs.salary_amount,
      fs.bonus_amount,
      d.full_date,
      c.city_id AS resolved_city_id,
      o.oblast_id AS resolved_oblast_id,
      di.district_id AS resolved_district_id,
      jr.standard_job_role_id AS resolved_standard_job_role_id,
      jr.hierarchy_level_id AS resolved_hierarchy_level_id,
      sjr.industry_field_id AS resolved_industry_field_id
    FROM marketstat.fact_salaries fs
    JOIN marketstat.dim_date             d   ON fs.date_id     = d.date_id
    JOIN marketstat.dim_city             c   ON fs.city_id     = c.city_id
    JOIN marketstat.dim_oblast           o   ON c.oblast_id    = o.oblast_id
    JOIN marketstat.dim_federal_district di  ON o.district_id  = di.district_id
    JOIN marketstat.dim_job_role         jr  ON fs.job_role_id = jr.job_role_id
    JOIN marketstat.dim_standard_job_role sjr ON jr.standard_job_role_id = sjr.standard_job_role_id
    WHERE
      (p_city_id                 IS NULL OR c.city_id               = p_city_id)
      AND (p_oblast_id            IS NULL OR o.oblast_id             = p_oblast_id)
      AND (p_district_id          IS NULL OR di.district_id          = p_district_id)
      AND (p_standard_job_role_id IS NULL OR jr.standard_job_role_id = p_standard_job_role_id)
      AND (p_hierarchy_level_id   IS NULL OR jr.hierarchy_level_id   = p_hierarchy_level_id)
      AND (p_industry_field_id    IS NULL OR sjr.industry_field_id   = p_industry_field_id)
      AND (p_date_start           IS NULL OR d.full_date            >= p_date_start)
      AND (p_date_end             IS NULL OR d.full_date            <= p_date_end);
END;
$$;

ALTER FUNCTION marketstat.fn_filtered_salaries(INT,INT,INT,INT,INT,INT,DATE,DATE) OWNER TO marketstat_administrator;
GRANT EXECUTE ON FUNCTION marketstat.fn_filtered_salaries(INT,INT,INT,INT,INT,INT,DATE,DATE) TO marketstat_analyst;
\echo 'Function marketstat.fn_filtered_salaries (hybrid output structure) created/replaced.'