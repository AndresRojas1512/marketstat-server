TRUNCATE TABLE
    market.fact_salaries,
    market.dim_employee_education,
    market.dim_job_role,
    market.dim_standard_job_role_hierarchy,
    market.dim_standard_job_role,
    market.dim_hierarchy_level,
    market.dim_employer_industry_field,
    market.dim_employer,
    market.dim_industry_field,
    market.dim_city,
    market.dim_oblast,
    market.dim_federal_district,
    market.dim_education,
    market.dim_education_level,
    market.dim_date
RESTART IDENTITY
CASCADE;
