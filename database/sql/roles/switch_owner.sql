-- PHASE 1 SCRIPT: Establish marketstat_administrator as Owner
-- TO BE RUN BY A POSTGRESQL SUPERUSER
-- Ensure you are connected to a maintenance database (e.g., 'postgres' or 'template1')
-- if you encounter issues dropping 'marketstat_user' while connected to 'marketstat' database
-- due to its potential (though unlikely after ownership transfer) implicit dependencies or connections.
-- However, for ALTER SCHEMA/TABLE ... OWNER commands, you generally need to be connected to the target database ('marketstat').
-- It's a bit of a dance: best to do ownership changes connected to 'marketstat',
-- then for DROP ROLE, ensure no connections from that role and ideally connect to 'postgres' DB.
-- This script attempts to do most things while connected to 'marketstat'.

\echo '--- PHASE 1: Establishing marketstat_administrator as Owner ---'
\set ON_ERROR_STOP on

-- Step 1.1: Create the new marketstat_administrator role (if it doesn't exist)
\echo 'Creating role "marketstat_administrator"...'
DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'marketstat_administrator') THEN
        CREATE ROLE marketstat_administrator LOGIN PASSWORD 'msadmin';
        RAISE NOTICE 'Role "marketstat_administrator" created.';
    ELSE
        RAISE NOTICE 'Role "marketstat_administrator" already exists. Ensure password is set if needed.';
    END IF;
END$$;

-- Step 1.2: Transfer Database Ownership
\echo 'Transferring ownership of DATABASE "marketstat" to "marketstat_administrator"...'
ALTER DATABASE marketstat OWNER TO marketstat_administrator;

-- Connect to the 'marketstat' database to alter schema and object ownership within it.
-- If you were connected to 'postgres', you'd \c marketstat here.
-- If running this whole script via psql -f, ensure your initial connection for the whole file allows this.
-- For interactive, ensure you're in 'marketstat' DB now.
\c marketstat

\echo 'Connected to database "marketstat". Current user is ' || CURRENT_USER || '.'
\echo 'Current search_path is ' || current_setting('search_path') || '.'
-- Ensure schema is in search path for subsequent commands if not fully qualified
SET search_path = marketstat, public;


-- Step 1.3: Transfer Schema Ownership
\echo 'Transferring ownership of SCHEMA "marketstat" to "marketstat_administrator"...'
ALTER SCHEMA marketstat OWNER TO marketstat_administrator;

-- Step 1.4: Transfer Ownership of ALL objects FROM 'marketstat_user' TO 'marketstat_administrator'
\echo 'Transferring ownership of tables in schema "marketstat"...'
DO $$ DECLARE r RECORD;
BEGIN
    FOR r IN (SELECT tablename FROM pg_tables WHERE schemaname = 'marketstat' AND tableowner = 'marketstat_user') LOOP
        RAISE NOTICE 'Changing owner of table marketstat.% to marketstat_administrator', r.tablename;
        EXECUTE 'ALTER TABLE marketstat."' || r.tablename || '" OWNER TO marketstat_administrator;';
    END LOOP;
END $$;

\echo 'Transferring ownership of sequences in schema "marketstat"...'
DO $$ DECLARE r RECORD;
BEGIN
    FOR r IN (
        SELECT c.relname AS sequencename
        FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace
        WHERE c.relkind = 'S' AND n.nspname = 'marketstat' AND pg_get_userbyid(c.relowner) = 'marketstat_user'
    ) LOOP
        RAISE NOTICE 'Changing owner of sequence marketstat.% to marketstat_administrator', r.sequencename;
        EXECUTE 'ALTER SEQUENCE marketstat."' || r.sequencename || '" OWNER TO marketstat_administrator;';
    END LOOP;
END $$;

\echo 'Transferring ownership of functions, procedures, and aggregates in schema "marketstat"...'
DO $$
DECLARE
    r RECORD;
    v_alter_command TEXT;
    v_routine_type TEXT;
BEGIN
    FOR r IN (
        SELECT
            p.proname AS object_name,
            n.nspname AS schema_name,
            pg_get_function_identity_arguments(p.oid) AS object_args, -- Includes IN/OUT/etc. and names
            p.prokind AS object_kind -- 'f' for normal function, 'p' for procedure, 'a' for aggregate function, 'w' for window function
        FROM pg_proc p
        JOIN pg_namespace n ON p.pronamespace = n.oid
        WHERE n.nspname = 'marketstat' AND pg_get_userbyid(p.proowner) = 'marketstat_user'
    ) LOOP
        IF r.object_kind = 'p' THEN
            v_routine_type := 'PROCEDURE';
            v_alter_command := format('ALTER PROCEDURE %I.%I(%s) OWNER TO marketstat_administrator;',
                                      r.schema_name, r.object_name, r.object_args);
        ELSIF r.object_kind = 'f' THEN
            v_routine_type := 'FUNCTION';
            v_alter_command := format('ALTER FUNCTION %I.%I(%s) OWNER TO marketstat_administrator;',
                                      r.schema_name, r.object_name, r.object_args);
        ELSIF r.object_kind = 'a' THEN
            v_routine_type := 'AGGREGATE';
            v_alter_command := format('ALTER AGGREGATE %I.%I(%s) OWNER TO marketstat_administrator;',
                                      r.schema_name, r.object_name, r.object_args);
        -- Note: Window functions (prokind = 'w') also use ALTER FUNCTION for ownership.
        ELSIF r.object_kind = 'w' THEN
            v_routine_type := 'WINDOW FUNCTION';
            v_alter_command := format('ALTER FUNCTION %I.%I(%s) OWNER TO marketstat_administrator;',
                                      r.schema_name, r.object_name, r.object_args);
        ELSE
            RAISE NOTICE 'Skipping unknown routine kind: % for %.%(%)', r.object_kind, r.schema_name, r.object_name, r.object_args;
            CONTINUE;
        END IF;

        RAISE NOTICE 'Changing owner of % marketstat.%(%) to marketstat_administrator', v_routine_type, r.object_name, r.object_args;
        EXECUTE v_alter_command;
    END LOOP;
END $$;
\echo 'Function, procedure, and aggregate ownership transferred.'
-- End of the corrected block for routine ownership

\echo 'Transferring ownership of views in schema "marketstat" (if any)...'
DO $$ DECLARE r RECORD;
BEGIN
    FOR r IN (SELECT viewname FROM pg_views WHERE schemaname = 'marketstat' AND viewowner = 'marketstat_user') LOOP
        RAISE NOTICE 'Changing owner of view marketstat.% to marketstat_administrator', r.viewname;
        EXECUTE 'ALTER VIEW marketstat."' || r.viewname || '" OWNER TO marketstat_administrator;';
    END LOOP;
END $$;

\echo 'All object ownership within schema "marketstat" should now be transferred from "marketstat_user" to "marketstat_administrator".'

-- Step 1.5: Grant necessary schema privileges to marketstat_administrator
\echo 'Granting CREATE and USAGE on schema "marketstat" to "marketstat_administrator"...'
GRANT CREATE, USAGE ON SCHEMA marketstat TO marketstat_administrator;

-- Step 1.6: Set default search_path for marketstat_administrator for convenience
\echo 'Setting default search_path for "marketstat_administrator"...'
ALTER ROLE marketstat_administrator SET search_path = marketstat, public;

\echo 'Role "marketstat_administrator" is now configured as the primary owner and manager.'

-- Step 1.7: Handle the old 'marketstat_user'
\echo 'Handling the old "marketstat_user"...'

-- Revoke any remaining privileges from marketstat_user.
-- This is important before dropping the role.
\echo 'Revoking privileges from "marketstat_user"...'
-- Note: If marketstat_user was connected to other databases or had other global privs,
-- those would need separate handling. This focuses on the 'marketstat' database.
REVOKE ALL PRIVILEGES ON DATABASE marketstat FROM marketstat_user;
REVOKE ALL PRIVILEGES ON SCHEMA marketstat FROM marketstat_user;
-- Attempt to revoke from all objects it might still have non-owner privs on (unlikely after ownership transfer)
DO $$ BEGIN EXECUTE 'REVOKE ALL ON ALL TABLES IN SCHEMA marketstat FROM marketstat_user;'; EXCEPTION WHEN OTHERS THEN RAISE NOTICE 'No table privileges to revoke from marketstat_user or tables not found for it.'; END; $$;
DO $$ BEGIN EXECUTE 'REVOKE ALL ON ALL SEQUENCES IN SCHEMA marketstat FROM marketstat_user;'; EXCEPTION WHEN OTHERS THEN RAISE NOTICE 'No sequence privileges to revoke from marketstat_user.'; END; $$;
DO $$ BEGIN EXECUTE 'REVOKE ALL ON ALL FUNCTIONS IN SCHEMA marketstat FROM marketstat_user;'; EXCEPTION WHEN OTHERS THEN RAISE NOTICE 'No function privileges to revoke from marketstat_user.'; END; $$;
DO $$ BEGIN EXECUTE 'REVOKE ALL ON ALL ROUTINES IN SCHEMA marketstat FROM marketstat_user;'; EXCEPTION WHEN OTHERS THEN RAISE NOTICE 'No routine privileges to revoke from marketstat_user.'; END; $$;

-- Ensure marketstat_user owns no more objects in the current database that would prevent its drop.
-- The ownership transfer above should have covered objects in 'marketstat' schema.
-- If it owns objects in other schemas in this DB, those would also need reassigning.
\echo 'Reassigning any remaining owned objects by "marketstat_user" in database "marketstat" to "marketstat_administrator"...'
REASSIGN OWNED BY marketstat_user TO marketstat_administrator;

-- Attempt to drop the old role.
-- For this to succeed, marketstat_user must not have any active connections,
-- and must not own any objects in ANY database in the cluster,
-- and must not have privileges on objects that cannot be dropped along with it.
-- The REASSIGN OWNED command helps with object ownership in the CURRENT database.
\echo 'Attempting to DROP ROLE "marketstat_user"...'
\echo 'NOTE: If "marketstat_user" has active connections, this DROP ROLE will fail.'
\echo 'You may need to terminate its connections first using pg_terminate_backend(pid).'
\echo 'Example: SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE usename = ''marketstat_user'';'
DROP ROLE IF EXISTS marketstat_user;
\echo 'Old role "marketstat_user" processed (dropped if no dependencies/connections).'

\echo '--- PHASE 1 Finished: marketstat_administrator is now the primary owner. ---'