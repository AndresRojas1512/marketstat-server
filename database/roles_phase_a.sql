DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'marketstat_viewer') THEN
        CREATE ROLE marketstat_viewer NOLOGIN;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'marketstat_analyst') THEN
        CREATE ROLE marketstat_analyst NOLOGIN;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'marketstat_admin') THEN
        CREATE ROLE marketstat_admin NOLOGIN;
    END IF;
END
$$;


-- VIEWER: read-only
GRANT USAGE
    ON SCHEMA marketstat
    TO marketstat_viewer;

GRANT SELECT
    ON ALL TABLES IN SCHEMA marketstat
    TO marketstat_viewer;

GRANT SELECT
    ON ALL SEQUENCES IN SCHEMA marketstat
    TO marketstat_viewer;

GRANT EXECUTE
    ON ALL FUNCTIONS IN SCHEMA marketstat
    TO marketstat_viewer;

ALTER DEFAULT PRIVILEGES IN SCHEMA marketstat
    GRANT SELECT ON TABLES    TO marketstat_viewer;
ALTER DEFAULT PRIVILEGES IN SCHEMA marketstat
    GRANT SELECT ON SEQUENCES TO marketstat_viewer;
ALTER DEFAULT PRIVILEGES IN SCHEMA marketstat
    GRANT EXECUTE ON FUNCTIONS TO marketstat_viewer;

-- ANALYST: viewer + exec rights
GRANT marketstat_viewer TO marketstat_analyst;

-- ADMIN: full control
GRANT marketstat_analyst TO marketstat_admin;

GRANT ALL PRIVILEGES 
    ON SCHEMA marketstat 
    TO marketstat_admin;

GRANT ALL PRIVILEGES 
    ON ALL TABLES IN SCHEMA marketstat 
    TO marketstat_admin;

GRANT ALL PRIVILEGES 
    ON ALL SEQUENCES IN SCHEMA marketstat 
    TO marketstat_admin;

GRANT ALL PRIVILEGES 
    ON ALL FUNCTIONS IN SCHEMA marketstat 
    TO marketstat_admin;

-- Default privileges for objects in schema
ALTER DEFAULT PRIVILEGES IN SCHEMA marketstat
    GRANT ALL ON TABLES    TO marketstat_admin;
ALTER DEFAULT PRIVILEGES IN SCHEMA marketstat
    GRANT ALL ON SEQUENCES TO marketstat_admin;
ALTER DEFAULT PRIVILEGES IN SCHEMA marketstat
    GRANT EXECUTE ON FUNCTIONS TO marketstat_admin;
