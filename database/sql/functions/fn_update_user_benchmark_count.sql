SET search_path = marketstat, public;

CREATE OR REPLACE FUNCTION marketstat.fn_update_user_benchmark_count()
RETURNS TRIGGER AS $$
BEGIN
    IF (TG_OP = 'INSERT') THEN
        UPDATE marketstat.users
        SET saved_benchmarks_count = saved_benchmarks_count + 1
        WHERE user_id = NEW.user_id;
        RAISE NOTICE 'Incremented benchmark count for user_id: % due to INSERT on benchmark_history', NEW.user_id;
        RETURN NEW;
    ELSIF (TG_OP = 'DELETE') THEN
        UPDATE marketstat.users
        SET saved_benchmarks_count = GREATEST(0, saved_benchmarks_count - 1)
        WHERE user_id = OLD.user_id;
        RAISE NOTICE 'Decremented benchmark count for user_id: % due to DELETE on benchmark_history', OLD.user_id;
        RETURN OLD;
    END IF;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

ALTER FUNCTION marketstat.fn_update_user_benchmark_count() OWNER TO marketstat_administrator;
