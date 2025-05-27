TRUNCATE TABLE
    marketstat.fact_salaries,
    marketstat.dim_employee_education,
    marketstat.dim_job_role,
    marketstat.dim_standard_job_role_hierarchy,
    marketstat.dim_standard_job_role,
    marketstat.dim_hierarchy_level,
    marketstat.dim_employer_industry_field,
    marketstat.dim_employer,
    marketstat.dim_industry_field,
    marketstat.dim_city,
    marketstat.dim_oblast,
    marketstat.dim_federal_district,
    marketstat.dim_education,
    marketstat.dim_education_level,
    marketstat.dim_date,
    marketstat.users,
    marketstat.benchmark_history
RESTART IDENTITY
CASCADE;
