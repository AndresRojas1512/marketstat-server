-- run as marketstat_administrator

\set ON_ERROR_STOP on
SET search_path = marketstat, public;

ALTER DEFAULT PRIVILEGES FOR ROLE marketstat_administrator IN SCHEMA marketstat
	GRANT SELECT ON TABLES TO marketstat_analyst;

ALTER DEFAULT PRIVILEGES FOR ROLE marketstat_administrator IN SCHEMA marketstat
	GRANT USAGE, SELECT ON SEQUENCES TO marketstat_analyst;

GRANT INSERT, UPDATE, DELETE ON TABLE users, benchmark_history TO marketstat_analyst;
