\echo '--- Test 1: Distribution for ALL salaries (no filters) ---'
-- This will use all 18 records from your sample.
SELECT * FROM marketstat.fn_salary_distribution(
    p_source_temp_table_name := NULL,
    p_industry_field_id      := NULL,
    p_standard_job_role_id   := NULL,
    p_hierarchy_level_id     := NULL,
    p_district_id            := NULL,
    p_oblast_id              := NULL,
    p_city_id                := NULL,
    p_date_start             := NULL,
    p_date_end               := NULL
) ORDER BY lower_bound;

SELECT * FROM marketstat.fn_salary_distribution(p_city_id := 10) ORDER BY lower_bound;

SELECT * FROM marketstat.fn_salary_distribution(p_oblast_id := 6) ORDER BY lower_bound; -- REPLACE 6

SELECT * FROM marketstat.fn_salary_distribution(p_industry_field_id := 67) ORDER BY lower_bound;

SELECT * FROM marketstat.fn_salary_distribution(p_standard_job_role_id := 30) ORDER BY lower_bound;
