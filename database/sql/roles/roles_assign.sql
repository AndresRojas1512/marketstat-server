-- run as marketstat_administrator

\set ON_ERROR_STOP on
SET search_path = marketstat, public;

GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE
    dim_date, dim_federal_district, dim_oblast, dim_city,
    dim_employer, dim_industry_field, dim_hierarchy_level,
    dim_standard_job_role, dim_job_role, dim_education_level,
    dim_education, dim_employee, dim_employer_industry_field,
    dim_standard_job_role_hierarchy, dim_employee_education,
    users, benchmark_history
TO marketstat_etl_user;

GRANT SELECT, INSERT, UPDATE, DELETE, TRUNCATE ON TABLE fact_salaries TO marketstat_etl_user;
GRANT USAGE, SELECT, UPDATE ON ALL SEQUENCES IN SCHEMA marketstat TO marketstat_etl_user;
GRANT SELECT, INSERT, DELETE, TRUNCATE ON TABLE marketstat.failed_salary_facts_load TO marketstat_etl_user;
GRANT USAGE, SELECT ON SEQUENCE marketstat.failed_salary_facts_load_failed_load_id_seq TO marketstat_etl_user;
GRANT EXECUTE ON PROCEDURE marketstat.bulk_load_salary_facts_from_staging(TEXT) TO marketstat_etl_user;
\echo 'marketstat_etl_user privileges granted.'




GRANT EXECUTE ON FUNCTION fn_compute_benchmark_data(INT, INT, INT, INT, INT, INT, DATE, DATE, INT, TEXT, INT) TO marketstat_analyst;
GRANT EXECUTE ON PROCEDURE sp_save_benchmark(
    OUT BIGINT,  IN INT, IN JSONB,   IN VARCHAR,
    IN INT,      IN INT, IN INT,      IN INT,
    IN INT,      IN INT, IN DATE,     IN DATE,
    IN INT,      IN TEXT,IN INT
) TO marketstat_analyst;

GRANT SELECT ON TABLE benchmark_history TO marketstat_analyst;
GRANT SELECT ON TABLE users TO marketstat_analyst;
GRANT USAGE, SELECT ON SEQUENCE benchmark_history_benchmark_history_id_seq TO marketstat_analyst;
GRANT USAGE, SELECT ON SEQUENCE users_user_id_seq TO marketstat_analyst;

GRANT SELECT ON TABLE
    dim_industry_field, dim_standard_job_role, dim_hierarchy_level,
    dim_federal_district, dim_oblast, dim_city,
    dim_education_level
TO marketstat_analyst;

GRANT EXECUTE ON FUNCTION fn_filtered_salaries(INT,INT,INT,INT,INT,INT,DATE,DATE) TO marketstat_analyst;
GRANT EXECUTE ON FUNCTION fn_salary_distribution(TEXT,INT,INT,INT,INT,INT,INT,DATE,DATE) TO marketstat_analyst;
GRANT EXECUTE ON FUNCTION fn_salary_summary(TEXT,INT,INT,INT,INT,INT,INT,DATE,DATE,INT) TO marketstat_analyst;
GRANT EXECUTE ON FUNCTION fn_salary_time_series(TEXT,INT,INT,INT,INT,INT,INT,DATE,DATE,TEXT,INT) TO marketstat_analyst;

\echo 'marketstat_analyst privileges granted'




GRANT EXECUTE ON FUNCTION fn_public_get_roles_by_location_industry(INT, INT, INT, INT, INT) TO marketstat_public_guest;
GRANT EXECUTE ON FUNCTION fn_public_degrees_by_industry(INT, INT, INT) TO marketstat_public_guest;
\echo 'marketstat_public_guest privileges granted'




ALTER FUNCTION fn_filtered_salaries(INT,INT,INT,INT,INT,INT,DATE,DATE) SECURITY DEFINER;

ALTER FUNCTION fn_salary_distribution(TEXT,INT,INT,INT,INT,INT,INT,DATE,DATE) SECURITY DEFINER;
ALTER FUNCTION fn_salary_summary(TEXT,INT,INT,INT,INT,INT,INT,DATE,DATE,INT) SECURITY DEFINER;
ALTER FUNCTION fn_salary_time_series(TEXT,INT,INT,INT,INT,INT,INT,DATE,DATE,TEXT,INT) SECURITY DEFINER;

ALTER FUNCTION fn_compute_benchmark_data(INT,INT,INT,INT,INT,INT,DATE,DATE,INT,TEXT,INT) SECURITY DEFINER;

ALTER PROCEDURE sp_save_benchmark(OUT BIGINT,IN INT,IN JSONB,IN VARCHAR,IN INT,IN INT,IN INT,IN INT,IN INT,IN INT,IN DATE,IN DATE,IN INT,IN TEXT,IN INT) SECURITY DEFINER;

ALTER PROCEDURE marketstat.bulk_load_salary_facts_from_staging(TEXT) OWNER TO marketstat_administrator;
\echo 'Key routines security context set to DEFINER.'




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

