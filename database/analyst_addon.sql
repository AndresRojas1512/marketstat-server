-- SCRIPT FOR USER AND BENCHMARK HISTORY SETUP
-- Run as marketstat_administrator

\echo '--- Starting User and Benchmark History Setup ---'
\set ON_ERROR_STOP on
SET search_path = marketstat, public;

-- Step 1: Create the "users" table
\echo 'Creating table "users"...'
CREATE TABLE IF NOT EXISTS marketstat.users (
    user_id                 INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    username                VARCHAR(100) NOT NULL UNIQUE,
    password_hash           TEXT NOT NULL, -- Store securely hashed passwords only!
    email                   VARCHAR(255) NOT NULL UNIQUE,
    full_name               VARCHAR(255) NOT NULL,
    is_active               BOOLEAN NOT NULL DEFAULT TRUE,
    created_at              TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    last_login_at           TIMESTAMPTZ NULL,
    saved_benchmarks_count  INT NOT NULL DEFAULT 0 -- For the trigger idea we discussed
);
\echo 'Table "users" created.'

-- Step 2: Create the "benchmark_history" table
\echo 'Creating table "benchmark_history"...'
CREATE TABLE IF NOT EXISTS marketstat.benchmark_history (
    benchmark_history_id    BIGSERIAL PRIMARY KEY,
    user_id                 INT NOT NULL,
    benchmark_name          VARCHAR(255) NULL,
    saved_at                TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,

    -- Filter parameters as they were effectively used
    filter_industry_field_name   TEXT NULL,
    filter_standard_job_role_title TEXT NULL,
    filter_hierarchy_level_name  TEXT NULL,
    filter_district_name         TEXT NULL,
    filter_oblast_name           TEXT NULL,
    filter_city_name             TEXT NULL,
    filter_date_start            DATE NULL,
    filter_date_end              DATE NULL,
    filter_target_percentile     INT NULL,
    filter_granularity           TEXT NULL,
    filter_periods               INT NULL,

    benchmark_result_json      JSONB NOT NULL,

    CONSTRAINT fk_benchmark_history_user FOREIGN KEY (user_id) REFERENCES marketstat.users(user_id) ON DELETE CASCADE -- Or SET NULL, depending on desired behavior
);
CREATE INDEX IF NOT EXISTS idx_benchmark_history_user_id ON marketstat.benchmark_history(user_id);
CREATE INDEX IF NOT EXISTS idx_benchmark_history_saved_at ON marketstat.benchmark_history(saved_at DESC);
\echo 'Table "benchmark_history" created.'

-- Step 3: Create/Recreate the stored procedure "sp_save_benchmark"
-- This procedure will be owned by marketstat_administrator and set to SECURITY DEFINER
\echo 'Creating procedure "sp_save_benchmark"...'
CREATE OR REPLACE PROCEDURE marketstat.sp_save_benchmark(
    -- OUT parameter first
    OUT p_new_benchmark_history_id  BIGINT,

    -- Required IN parameters (no defaults)
    IN p_user_id INT,
    IN p_benchmark_result_json      JSONB,

    -- Optional IN parameters (with defaults)
    IN p_benchmark_name VARCHAR(255) DEFAULT NULL,
    IN p_filter_industry_field_name   TEXT    DEFAULT NULL,
    IN p_filter_standard_job_role_title TEXT  DEFAULT NULL,
    IN p_filter_hierarchy_level_name  TEXT    DEFAULT NULL,
    IN p_filter_district_name         TEXT    DEFAULT NULL,
    IN p_filter_oblast_name           TEXT    DEFAULT NULL,
    IN p_filter_city_name             TEXT    DEFAULT NULL,
    IN p_filter_date_start            DATE    DEFAULT NULL,
    IN p_filter_date_end              DATE    DEFAULT NULL,
    IN p_filter_target_percentile     INT     DEFAULT NULL,
    IN p_filter_granularity           TEXT    DEFAULT NULL,
    IN p_filter_periods               INT     DEFAULT NULL
)
LANGUAGE plpgsql
SECURITY DEFINER
-- SET search_path = marketstat, public; -- Usually inherited or not strictly needed if objects inside are schema-qualified
AS $$
BEGIN
    INSERT INTO marketstat.benchmark_history (
        user_id,
        benchmark_name,
        filter_industry_field_name,
        filter_standard_job_role_title,
        filter_hierarchy_level_name,
        filter_district_name,
        filter_oblast_name,
        filter_city_name,
        filter_date_start,
        filter_date_end,
        filter_target_percentile,
        filter_granularity,
        filter_periods,
        benchmark_result_json
        -- saved_at uses its DEFAULT CURRENT_TIMESTAMP
    ) VALUES (
        p_user_id,
        p_benchmark_name,
        p_filter_industry_field_name,
        p_filter_standard_job_role_title,
        p_filter_hierarchy_level_name,
        p_filter_district_name,
        p_filter_oblast_name,
        p_filter_city_name,
        p_filter_date_start,
        p_filter_date_end,
        p_filter_target_percentile,
        p_filter_granularity,
        p_filter_periods,
        p_benchmark_result_json
    )
    RETURNING benchmark_history_id INTO p_new_benchmark_history_id;

    RAISE NOTICE 'Saved benchmark with ID: % for user ID: %', p_new_benchmark_history_id, p_user_id;
END;
$$;
\echo 'Procedure "sp_save_benchmark" created/replaced and set to SECURITY DEFINER.'

-- Step 4: Grant necessary privileges to the 'marketstat_analyst' role
\echo 'Granting privileges to "marketstat_analyst" for new objects...'
-- marketstat_analyst will call sp_save_benchmark
GRANT EXECUTE ON PROCEDURE marketstat.sp_save_benchmark(
    OUT BIGINT,                 -- p_new_benchmark_history_id
    IN INT,                     -- p_user_id
    IN JSONB,                   -- p_benchmark_result_json
    IN VARCHAR,                 -- p_benchmark_name (VARCHAR(255) is fine as VARCHAR)
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

-- marketstat_analyst needs to read benchmark_history (application will filter by user_id)
GRANT SELECT ON TABLE marketstat.benchmark_history TO marketstat_analyst;
GRANT SELECT ON TABLE marketstat.users TO marketstat_analyst; -- To read basic user info, NOT password_hash. API should control field exposure.

-- The sequence for benchmark_history_id will be used by sp_save_benchmark under SECURITY DEFINER context (owner's rights).
-- Direct grant to marketstat_analyst on sequence is not strictly needed for this SP but can be added for other direct uses if any.
-- GRANT USAGE, SELECT ON SEQUENCE marketstat.benchmark_history_benchmark_history_id_seq TO marketstat_analyst;

\echo 'Analyst privileges for history feature granted.'

-- Step 5: (Optional) Implement the trigger for saved_benchmarks_count on users table
\echo 'Creating trigger for "users.saved_benchmarks_count"...'

CREATE OR REPLACE FUNCTION marketstat.fn_update_user_benchmark_count()
RETURNS TRIGGER AS $$
BEGIN
    IF (TG_OP = 'INSERT') THEN
        UPDATE marketstat.users
        SET saved_benchmarks_count = saved_benchmarks_count + 1
        WHERE user_id = NEW.user_id;
        RETURN NEW;
    ELSIF (TG_OP = 'DELETE') THEN
        UPDATE marketstat.users
        SET saved_benchmarks_count = GREATEST(0, saved_benchmarks_count - 1) -- Ensure count doesn't go below 0
        WHERE user_id = OLD.user_id;
        RETURN OLD;
    END IF;
    RETURN NULL; -- Should not happen
END;
$$ LANGUAGE plpgsql SECURITY DEFINER; -- Define with security of owner

DROP TRIGGER IF EXISTS trg_update_user_benchmark_count ON marketstat.benchmark_history;
CREATE TRIGGER trg_update_user_benchmark_count
AFTER INSERT OR DELETE ON marketstat.benchmark_history
FOR EACH ROW
EXECUTE FUNCTION marketstat.fn_update_user_benchmark_count();
\echo 'Trigger for "users.saved_benchmarks_count" created.'


-- Step 6: Update Default Privileges if you want marketstat_analyst to SELECT from future tables automatically
-- This was in your Phase 2 Part B, just confirming it's good to have.
-- This command is run by marketstat_administrator (current user)
\echo 'Reviewing/Confirming default privileges for marketstat_administrator...'
ALTER DEFAULT PRIVILEGES FOR ROLE marketstat_administrator IN SCHEMA marketstat
   GRANT SELECT ON TABLES TO marketstat_analyst;
-- Any other default privileges established earlier should remain.
\echo 'Default privileges confirmed/updated.'


\echo '--- User and Benchmark History Setup Finished ---'