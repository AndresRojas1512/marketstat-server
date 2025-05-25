SET search_path = marketstat, public;

CREATE OR REPLACE FUNCTION marketstat.fn_update_user_benchmark_count()
RETURNS TRIGGER AS $$
BEGIN
    IF (TG_OP = 'INSERT') THEN
        UPDATE marketstat.users
        SET saved_benchmarks_count = saved_benchmarks_count + 1
        WHERE user_id = NEW.user_id;
        RAISE NOTICE 'Incremented benchmark count for user_id: % due to INSERT on benchmark_history', NEW.user_id;
        RETURN NEW; -- For AFTER INSERT, return value is usually ignored but NEW is conventional
    ELSIF (TG_OP = 'DELETE') THEN
        UPDATE marketstat.users
        SET saved_benchmarks_count = GREATEST(0, saved_benchmarks_count - 1) -- Ensure count doesn't go below 0
        WHERE user_id = OLD.user_id;
        RAISE NOTICE 'Decremented benchmark count for user_id: % due to DELETE on benchmark_history', OLD.user_id;
        RETURN OLD; -- For AFTER DELETE, return value is usually ignored but OLD is conventional
    END IF;
    RETURN NULL; -- Should not be reached if trigger is only for INSERT/DELETE
END;
$$ LANGUAGE plpgsql SECURITY DEFINER; -- Run with owner's (marketstat_administrator) privileges

ALTER FUNCTION marketstat.fn_update_user_benchmark_count() OWNER TO marketstat_administrator;
-- No direct EXECUTE grant needed for application roles on trigger functions.
\echo 'Trigger function marketstat.fn_update_user_benchmark_count created/replaced.'