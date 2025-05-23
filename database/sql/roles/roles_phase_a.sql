-- PHASE 2 - PART A: Create Operational Roles
-- TO BE RUN BY A POSTGRESQL SUPERUSER (e.g., 'postgres')

\echo '--- PHASE 2 - PART A: Creating Operational Roles ---'
\set ON_ERROR_STOP on

-- Step 2.1: Create the operational roles
\echo 'Creating operational roles: marketstat_etl_user, marketstat_analyst, marketstat_public_guest...'
DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'marketstat_etl_user') THEN
        CREATE ROLE marketstat_etl_user LOGIN PASSWORD 'msetl';
        RAISE NOTICE 'Role "marketstat_etl_user" created.';
    ELSE
        RAISE NOTICE 'Role "marketstat_etl_user" already exists. Consider ALTER ROLE if password needs update.';
    END IF;

    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'marketstat_analyst') THEN
        CREATE ROLE marketstat_analyst LOGIN PASSWORD 'msanalyst';
        RAISE NOTICE 'Role "marketstat_analyst" created.';
    ELSE
        RAISE NOTICE 'Role "marketstat_analyst" already exists. Consider ALTER ROLE if password needs update.';
    END IF;

    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'marketstat_public_guest') THEN
        CREATE ROLE marketstat_public_guest LOGIN PASSWORD 'msguest'; -- <<< REPLACE PASSWORD
        RAISE NOTICE 'Role "marketstat_public_guest" created.';
    ELSE
        RAISE NOTICE 'Role "marketstat_public_guest" already exists. Consider ALTER ROLE if password needs update.';
    END IF;
END$$;
\echo 'Operational roles creation process finished.'

-- Step 2.2: Grant basic database and schema privileges from superuser
\echo 'Granting CONNECT on database "marketstat" to new roles...'
GRANT CONNECT ON DATABASE marketstat TO marketstat_etl_user;
GRANT CONNECT ON DATABASE marketstat TO marketstat_analyst;
GRANT CONNECT ON DATABASE marketstat TO marketstat_public_guest;

\echo 'Granting USAGE on schema "marketstat" to new roles...'
GRANT USAGE ON SCHEMA marketstat TO marketstat_etl_user;
GRANT USAGE ON SCHEMA marketstat TO marketstat_analyst;
GRANT USAGE ON SCHEMA marketstat TO marketstat_public_guest;

-- Step 2.3: Set default search_path for the new roles for convenience
\echo 'Setting default search_path for new roles...'
ALTER ROLE marketstat_etl_user SET search_path = marketstat, public;
ALTER ROLE marketstat_analyst SET search_path = marketstat, public;
ALTER ROLE marketstat_public_guest SET search_path = marketstat, public;

\echo '--- PHASE 2 - PART A (Superuser tasks) Finished ---'