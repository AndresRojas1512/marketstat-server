\set ON_ERROR_STOP on

\echo 'Creating schema "marketstat"'
CREATE SCHEMA IF NOT EXISTS marketstat;

\echo 'Creating roles: marketstat_administrator, marketstat_analyst, marketstat_public_guest'
DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'marketstat_administrator') THEN
        CREATE ROLE marketstat_administrator LOGIN PASSWORD 'andresrmlnx15';
    END IF;

    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname= 'marketstat_analyst') THEN
        CREATE ROLE marketstat_analyst LOGIN PASSWORD 'msanalyst';
    END IF;
    
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'marketstat_public_guest') THEN
        CREATE ROLE marketstat_public_guest LOGIN PASSWORD 'msguest';
    END IF;
END$$;

\echo 'Granting privileges on database "marketstat" and schema "marketstat"'
GRANT CONNECT ON DATABASE marketstat TO marketstat_administrator;
GRANT CONNECT ON DATABASE marketstat TO marketstat_analyst;
GRANT CONNECT ON DATABASE marketstat TO marketstat_public_guest;

GRANT USAGE, CREATE ON SCHEMA marketstat TO marketstat_administrator;

GRANT USAGE, CREATE ON SCHEMA marketstat TO marketstat_analyst;
GRANT USAGE, CREATE ON SCHEMA marketstat TO marketstat_public_guest;

\echo 'Setting default privileges for tables and sequences'

ALTER DEFAULT PRIVILEGES FOR ROLE marketstat_administrator IN SCHEMA marketstat
    GRANT SELECT ON TABLES TO marketstat_analyst;
    
ALTER DEFAULT PRIVILEGES FOR ROLE marketstat_administrator IN SCHEMA marketstat
    GRANT SELECT ON TABLES TO marketstat_public_guest; -- Added this for consistency

ALTER DEFAULT PRIVILEGES FOR ROLE marketstat_administrator IN SCHEMA marketstat
    GRANT USAGE, SELECT ON SEQUENCES TO marketstat_analyst;

-- You had a duplicate line here, I'm keeping one:
ALTER DEFAULT PRIVILEGES FOR ROLE marketstat_administrator IN SCHEMA marketstat
    GRANT USAGE, SELECT ON SEQUENCES TO marketstat_public_guest;

-- This line was the error and has been REMOVED:
-- GRANT INSERT, UPDATE, DELETE ON TABLE marketstat.users TO marketstat_analyst;

\echo 'PostgreSQL role setup complete.'