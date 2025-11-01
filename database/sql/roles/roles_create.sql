-- run as postgres

\set ON_ERROR_STOP on

\echo 'Creating operational roles: marketstat_analyst, marketstat_public_guest'
DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'marketstat_analyst') THEN
        CREATE ROLE marketstat_analyst LOGIN PASSWORD 'msanalyst';
    END IF;

    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'marketstat_public_guest') THEN
        CREATE ROLE marketstat_public_guest LOGIN PASSWORD 'msguest';
    END IF;
END$$;

\echo 'Granting CONNECT on database "marketstat" to new roles'
GRANT CONNECT ON DATABASE marketstat TO marketstat_analyst;
GRANT CONNECT ON DATABASE marketstat TO marketstat_public_guest;

\echo 'Granting USAGE on schema "marketstat" to new roles'
GRANT USAGE ON SCHEMA marketstat TO marketstat_analyst;
GRANT USAGE ON SCHEMA marketstat TO marketstat_public_guest;

\echo 'Setting default search_path for new roles'
ALTER ROLE marketstat_analyst SET search_path = marketstat, public;
ALTER ROLE marketstat_public_guest SET search_path = marketstat, public;
