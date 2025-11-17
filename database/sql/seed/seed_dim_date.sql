\set ON_ERROR_STOP ON
SET search_path = marketstat, public;

BEGIN;

INSERT INTO marketstat.dim_date (full_date, year, month, quarter)
SELECT
    d.date_val AS full_date,
    EXTRACT(YEAR FROM d.date_val)::smallint AS year,
    EXTRACT(MONTH FROM d.date_val)::smallint AS month,
    CEIL(EXTRACT(MONTH FROM d.date_val) / 3.0)::smallint AS quarter
FROM
    generate_series(
        '2010-01-01'::date,
        '2030-12-31'::date,
        '1 day'::interval
    ) AS d(date_val)
WHERE NOT EXISTS (
    SELECT 1
    FROM marketstat.dim_date dd
    WHERE dd.full_date = d.date_val
);

COMMIT;
