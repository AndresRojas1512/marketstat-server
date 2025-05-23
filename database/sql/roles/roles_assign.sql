-- Script for granting Object Privileges and Set Security Context
-- Run as 'marketstat_administrator'

\echo '--- Granting Object Privileges by marketstat_administrator ---'
\set ON_ERROR_STOP on
SET search_path = marketstat, public;



\echo 'Granting privileges to marketstat_etl_user'
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE
    dim_date, dim_federal_district, dim_oblast, dim_city,
    dim_employer, dim_industry_field, dim_hierarchy_level,
    dim_standard_job_role, dim_job_role, dim_education_level,
    dim_education, dim_employee, dim_employer_industry_field,
    dim_standard_job_role_hierarchy, dim_employee_education
TO marketstat_etl_user;

GRANT SELECT, INSERT, UPDATE, DELETE, TRUNCATE ON TABLE fact_salaries TO marketstat_etl_user;
GRANT SELECT, INSERT, UPDATE, DELETE, TRUNCATE ON TABLE failed_salary_facts_load TO marketstat_etl_user;

GRANT USAGE, SELECT, UPDATE ON ALL SEQUENCES IN SCHEMA marketstat TO marketstat_etl_user;

GRANT EXECUTE ON PROCEDURE bulk_load_salary_facts_from_staging(TEXT) TO marketstat_etl_user;
\echo 'marketstat_etl_user privileges granted.'



\echo 'Granting privileges to marketstat_analyst'
GRANT EXECUTE ON FUNCTION fn_get_benchmarking_data(TEXT, TEXT, TEXT, TEXT, TEXT, TEXT, DATE, DATE, INT, TEXT, INT) TO marketstat_analyst;
GRANT EXECUTE ON PROCEDURE marketstat.sp_save_benchmark(
    OUT BIGINT,                 -- p_new_benchmark_history_id
    IN INT,                     -- p_user_id
    IN JSONB,                   -- p_benchmark_result_json
    IN VARCHAR,                 -- p_benchmark_name
    IN TEXT,                    -- p_filter_industry_field_name
    IN TEXT,                    -- p_filter_standard_job_role_title
    IN TEXT,                    -- p_filter_hierarchy_level_name
    IN TEXT,                    -- p_filter_district_name
    IN TEXT,                    -- p_filter_oblast_name
    IN TEXT,                    -- p_filter_city_name
    IN DATE,                    -- p_filter_date_start
    IN DATE,                    -- p_filter_date_end
    IN INT,                     -- p_filter_target_percentile
    IN TEXT,                    -- p_filter_granularity
    IN INT                      -- p_filter_periods
) TO marketstat_analyst;

GRANT SELECT ON TABLE benchmark_history TO marketstat_analyst;
GRANT SELECT ON TABLE users TO marketstat_analyst;
GRANT USAGE, SELECT ON SEQUENCE benchmark_history_benchmark_history_id_seq TO marketstat_analyst;

GRANT SELECT ON TABLE
    dim_industry_field, dim_standard_job_role, dim_hierarchy_level,
    dim_federal_district, dim_oblast, dim_city,
    dim_education_level
TO marketstat_analyst;

\echo 'marketstat_analys privileges granted'



\echo 'Granting privileges to marketstat_public_guest...'
-- TODO
\echo 'marketstat_public_guest privileges granted'



\echo 'Altering key routines to SECURITY DEFINER'
ALTER FUNCTION fn_filtered_salaries(INT,INT,INT,INT,INT,INT,DATE,DATE) SECURITY DEFINER;
ALTER FUNCTION fn_salary_distribution(INT,INT,INT,INT,INT,INT,DATE,DATE) SECURITY DEFINER;
ALTER FUNCTION fn_salary_summary(INT,INT,INT,INT,INT,INT,DATE,DATE,INT) SECURITY DEFINER;
ALTER FUNCTION fn_salary_time_series(INT,INT,INT,INT,INT,INT,DATE,DATE,TEXT,INT) SECURITY DEFINER;
ALTER FUNCTION fn_get_benchmarking_data(TEXT,TEXT,TEXT,TEXT,TEXT,TEXT,DATE,DATE,INT,TEXT,INT) SECURITY DEFINER;
ALTER PROCEDURE sp_save_benchmark(OUT BIGINT,IN INT,IN JSONB,IN VARCHAR,IN TEXT,IN TEXT,IN TEXT,IN TEXT,IN TEXT,IN TEXT,IN DATE,IN DATE,IN INT,IN TEXT,IN INT) SECURITY DEFINER;
ALTER PROCEDURE bulk_load_salary_facts_from_staging(TEXT) SECURITY DEFINER;
\echo 'Key routines security context set to DEFINER.'



\echo 'Setting default privileges for future objects created by marketstat_administrator in schema marketstat'
ALTER DEFAULT PRIVILEGES FOR ROLE marketstat_administrator IN SCHEMA marketstat
   GRANT SELECT ON TABLES TO marketstat_analyst;

ALTER DEFAULT PRIVILEGES FOR ROLE marketstat_administrator IN SCHEMA marketstat
   GRANT EXECUTE ON FUNCTIONS TO marketstat_analyst;
ALTER DEFAULT PRIVILEGES FOR ROLE marketstat_administrator IN SCHEMA marketstat
   GRANT EXECUTE ON ROUTINES TO marketstat_analyst;

ALTER DEFAULT PRIVILEGES FOR ROLE marketstat_administrator IN SCHEMA marketstat
   GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO marketstat_etl_user;
ALTER DEFAULT PRIVILEGES FOR ROLE marketstat_administrator IN SCHEMA marketstat
   GRANT USAGE, SELECT, UPDATE ON SEQUENCES TO marketstat_etl_user;
\echo 'Default privileges configured.'

\echo '--- Success: privileges granted ---'