-- Keep dim_date in sync
CREATE OR REPLACE FUNCTION marketstat.set_date_parts()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $$
BEGIN
    NEW.year := EXTRACT(YEAR FROM NEW.full_date)::smallint;
    NEW.quarter := EXTRACT(QUARTER FROM NEW.full_date)::smallint;
    NEW.month := EXTRACT(MONTH FROM NEW.full_date)::smallint;
    RETURN NEW;
END;
$$;

CREATE TRIGGER trg_dim_date_set_parts
BEFORE INSERT OR UPDATE
ON marketstat.dim_date
FOR EACH ROW
EXECUTE FUNCTION marketstat.set_date_parts();