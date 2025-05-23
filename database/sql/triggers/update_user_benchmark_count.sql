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
    RETURN NULL;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

DROP TRIGGER IF EXISTS trg_update_user_benchmark_count ON marketstat.benchmark_history;
CREATE TRIGGER trg_update_user_benchmark_count
AFTER INSERT OR DELETE ON marketstat.benchmark_history
FOR EACH ROW
EXECUTE FUNCTION marketstat.fn_update_user_benchmark_count();
