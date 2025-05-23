-- PHASE 2 - PART B: Grant Object Privileges and Set Security Context
-- TO BE RUN AS marketstat_administrator (the owner of the objects in the marketstat schema)

\echo '--- PHASE 2 - PART B: Granting Object Privileges by marketstat_administrator ---'
\set ON_ERROR_STOP on
SET search_path = marketstat, public;

-- Step 2.4: Privileges for marketstat_etl_user
\echo 'Granting privileges to marketstat_etl_user...'
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
\echo 'ETL user privileges granted.'

-- Step 2.5: Privileges for marketstat_analyst (Authorized Application User)
\echo 'Granting privileges to marketstat_analyst...'
GRANT EXECUTE ON FUNCTION fn_get_benchmarking_data(TEXT, TEXT, TEXT, TEXT, TEXT, TEXT, DATE, DATE, INT, TEXT, INT) TO marketstat_analyst;
-- GRANT EXECUTE ON PROCEDURE sp_save_benchmark(...) TO marketstat_analyst; -- Correctly commented
-- GRANT SELECT ON TABLE benchmark_history TO marketstat_analyst; -- Correctly commented
-- GRANT USAGE, SELECT ON SEQUENCE benchmark_history_benchmark_history_id_seq TO marketstat_analyst; -- Correctly commented

GRANT SELECT ON TABLE
    dim_industry_field, dim_standard_job_role, dim_hierarchy_level,
    dim_federal_district, dim_oblast, dim_city,
    dim_education_level
TO marketstat_analyst;
\echo 'Analyst (application user) privileges granted.'

-- Step 2.6: Privileges for marketstat_public_guest
\echo 'Granting privileges to marketstat_public_guest...'
-- ... (placeholders are fine for now) ...
\echo 'Public guest privileges configured (remember to grant EXECUTE on specific public functions/SELECT on public views as needed).'

-- Step 2.7: Ensure Key Routines are SECURITY DEFINER
\echo 'Altering key routines to SECURITY DEFINER...'
ALTER FUNCTION fn_filtered_salaries(INT,INT,INT,INT,INT,INT,DATE,DATE) SECURITY DEFINER;
ALTER FUNCTION fn_salary_distribution(INT,INT,INT,INT,INT,INT,DATE,DATE) SECURITY DEFINER;
ALTER FUNCTION fn_salary_summary(INT,INT,INT,INT,INT,INT,DATE,DATE,INT) SECURITY DEFINER;
ALTER FUNCTION fn_salary_time_series(INT,INT,INT,INT,INT,INT,DATE,DATE,TEXT,INT) SECURITY DEFINER;
ALTER FUNCTION fn_get_benchmarking_data(TEXT,TEXT,TEXT,TEXT,TEXT,TEXT,DATE,DATE,INT,TEXT,INT) SECURITY DEFINER;
-- ALTER PROCEDURE sp_save_benchmark(...) SECURITY DEFINER; -- Correctly commented
-- ALTER PROCEDURE marketstat.bulk_load_salary_facts_from_staging(TEXT) SECURITY DEFINER; -- Still optional, fine to leave commented if etl_user has direct rights
\echo 'Key routines security context set to DEFINER.'

-- Step 2.8: (Recommended) Default Privileges for Future Objects created by marketstat_administrator
\echo 'Setting default privileges for future objects created by marketstat_administrator in schema marketstat...'
ALTER DEFAULT PRIVILEGES FOR ROLE marketstat_administrator IN SCHEMA marketstat
   GRANT SELECT ON TABLES TO marketstat_analyst;
-- ... other default privileges ...
ALTER DEFAULT PRIVILEGES FOR ROLE marketstat_administrator IN SCHEMA marketstat
   GRANT EXECUTE ON FUNCTIONS TO marketstat_analyst;
ALTER DEFAULT PRIVILEGES FOR ROLE marketstat_administrator IN SCHEMA marketstat
   GRANT EXECUTE ON ROUTINES TO marketstat_analyst;

ALTER DEFAULT PRIVILEGES FOR ROLE marketstat_administrator IN SCHEMA marketstat
   GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO marketstat_etl_user;
ALTER DEFAULT PRIVILEGES FOR ROLE marketstat_administrator IN SCHEMA marketstat
   GRANT USAGE, SELECT, UPDATE ON SEQUENCES TO marketstat_etl_user;
\echo 'Default privileges configured.'

\echo '--- PHASE 2 - PART B (marketstat_administrator tasks) Finished ---'