SET search_path = marketstat, public;

DROP TRIGGER IF EXISTS trg_update_user_benchmark_count ON marketstat.benchmark_history;

CREATE TRIGGER trg_update_user_benchmark_count
AFTER INSERT OR DELETE ON marketstat.benchmark_history
FOR EACH ROW
EXECUTE FUNCTION marketstat.fn_update_user_benchmark_count();
