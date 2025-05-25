SELECT * FROM marketstat.fn_salary_summary(
    p_source_temp_table_name := NULL,
    p_industry_field_id      := NULL,
    p_standard_job_role_id   := NULL,
    p_hierarchy_level_id     := NULL,
    p_district_id            := NULL,
    p_oblast_id              := NULL,
    p_city_id                := NULL,
    p_date_start             := NULL,
    p_date_end               := NULL
    -- p_target_percentile uses default 90
);

SELECT * FROM marketstat.fn_salary_summary(p_city_id := 10, p_target_percentile := 75);
