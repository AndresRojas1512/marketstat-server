-- Test script for marketstat.fn_filtered_salaries
-- =================================================
-- IMPORTANT: Before running, replace placeholder IDs (e.g., 101, 201)
-- with actual IDs from your dimension tables that match the descriptions.
-- Use your sample data to guide which records to expect.

SET search_path = marketstat, public;

\echo '--- Test 1: No filters (should return all 18 records from your sample) ---'
-- Expected: All 18 records from your sample data, if all FKs resolve correctly.
SELECT * FROM marketstat.fn_filtered_salaries(
    p_industry_field_id    := NULL,
    p_standard_job_role_id := NULL,
    p_hierarchy_level_id   := NULL,
    p_district_id          := NULL,
    p_oblast_id            := NULL,
    p_city_id              := NULL,
    p_date_start           := NULL,
    p_date_end             := NULL
) ORDER BY salary_fact_id;

\echo '--- Test 2: Filter by a specific City ID ---'
-- Example: Filter for 'Сергиев Посад'. Find its actual city_id.
-- Expected: Record(s) for 'Сергиев Посад' (e.g., salary_fact_id = 1 from your sample).
-- REPLACE 101 with the actual city_id for 'Сергиев Посад'.
SELECT * FROM marketstat.fn_filtered_salaries(p_city_id := 10) ORDER BY salary_fact_id;

\echo '--- Test 3: Filter by a specific Oblast ID ---'
-- Example: Filter for 'Московская область'. Find its actual oblast_id.
-- Expected: Records from 'Московская область' (e.g., salary_fact_id = 1, 3, 10).
-- REPLACE 201 with the actual oblast_id for 'Московская область'.
SELECT * FROM marketstat.fn_filtered_salaries(p_oblast_id := 201) ORDER BY salary_fact_id;

\echo '--- Test 4: Filter by a specific Federal District ID ---'
-- Example: Filter for 'Центральный федеральный округ'. Find its actual district_id.
-- Expected: Records from 'Центральный федеральный округ' (e.g., salary_fact_id = 1, 3, 4, 5, 6, 9, 10, 11, 12, 13, 14, 15).
-- REPLACE 301 with the actual district_id.
SELECT * FROM marketstat.fn_filtered_salaries(p_district_id := 301) ORDER BY salary_fact_id;

\echo '--- Test 5: Filter by a specific Standard Job Role ID ---'
-- Example: Filter for 'Цементатор'. Find its actual standard_job_role_id.
-- Expected: Records for 'Цементатор' (e.g., salary_fact_id = 1).
-- REPLACE 401 with the actual standard_job_role_id.
SELECT * FROM marketstat.fn_filtered_salaries(p_standard_job_role_id := 401) ORDER BY salary_fact_id;

\echo '--- Test 6: Filter by a specific Hierarchy Level ID ---'
-- Example: Filter for 'Специалист'. Find its actual hierarchy_level_id.
-- Expected: Records for 'Специалист' (e.g., salary_fact_id = 1, 10).
-- REPLACE 501 with the actual hierarchy_level_id.
SELECT * FROM marketstat.fn_filtered_salaries(p_hierarchy_level_id := 501) ORDER BY salary_fact_id;

\echo '--- Test 7: Filter by a specific Industry Field ID ---'
-- Example: Filter for 'Добыча металлических руд'. Find its actual industry_field_id.
-- Expected: Records for 'Добыча металлических руд' (e.g., salary_fact_id = 1).
-- REPLACE 601 with the actual industry_field_id.
SELECT * FROM marketstat.fn_filtered_salaries(p_industry_field_id := 601) ORDER BY salary_fact_id;

\echo '--- Test 8: Filter by Date Range (p_date_start and p_date_end) ---'
-- Example: Records from 2021.
-- Expected: Records where recorded_date is in 2021 (e.g., salary_fact_id = 1, 6, 7, 15, 16).
SELECT * FROM marketstat.fn_filtered_salaries(
    p_date_start := '2021-01-01',
    p_date_end   := '2021-12-31'
) ORDER BY salary_fact_id;

\echo '--- Test 9: Filter by p_date_start only ---'
-- Example: Records from 2023 onwards.
-- Expected: Records where recorded_date >= 2023-01-01 (e.g., 2, 4, 5, 11, 12, 18).
SELECT * FROM marketstat.fn_filtered_salaries(p_date_start := '2023-01-01') ORDER BY salary_fact_id;

\echo '--- Test 10: Filter by p_date_end only ---'
-- Example: Records up to end of 2020.
-- Expected: Records where recorded_date <= 2020-12-31 (e.g., 3, 9, 10, 13, 14).
SELECT * FROM marketstat.fn_filtered_salaries(p_date_end := '2020-12-31') ORDER BY salary_fact_id;

\echo '--- Test 11: Combination of City ID and Date Range ---'
-- Example: 'Люберцы' (find its city_id) during August 2020.
-- Expected: Records for 'Люберцы' in Aug 2020 (e.g., salary_fact_id = 3, 10).
-- REPLACE 102 with actual city_id for 'Люберцы'.
SELECT * FROM marketstat.fn_filtered_salaries(
    p_city_id    := 102,
    p_date_start := '2020-08-01',
    p_date_end   := '2020-08-31'
) ORDER BY salary_fact_id;

\echo '--- Test 12: Combination of Oblast ID, Standard Job Role ID, and Hierarchy Level ID ---'
-- Example: 'Московская область' (find oblast_id), 'Инженер по инструменту' (find standard_job_role_id), 'Старший специалист' (find hierarchy_level_id).
-- Expected: Record for 'Инженер по инструменту', 'Старший специалист' in 'Московская область' (e.g., salary_fact_id = 3).
-- REPLACE 201, 402, 502 with actual IDs.
SELECT * FROM marketstat.fn_filtered_salaries(
    p_oblast_id            := 201,
    p_standard_job_role_id := 402,
    p_hierarchy_level_id   := 502
) ORDER BY salary_fact_id;

\echo '--- Test 13: Highly specific filter aiming for a single known record (e.g., salary_fact_id = 1) ---'
-- Use all known dimension IDs for salary_fact_id = 1.
-- City: 'Сергиев Посад' (e.g., ID 101)
-- Oblast: 'Московская область' (e.g., ID 201)
-- District: 'Центральный федеральный округ' (e.g., ID 301)
-- Standard Job Role: 'Цементатор' (e.g., ID 401)
-- Hierarchy Level: 'Специалист' (e.g., ID 501)
-- Industry Field: 'Добыча металлических руд' (e.g., ID 601)
-- Date: 2021-10-04
-- REPLACE ALL IDs with actual values corresponding to salary_fact_id = 1.
SELECT * FROM marketstat.fn_filtered_salaries(
    p_industry_field_id    := 601,
    p_standard_job_role_id := 401,
    p_hierarchy_level_id   := 501,
    p_district_id          := 301,
    p_oblast_id            := 201,
    p_city_id              := 101,
    p_date_start           := '2021-10-04',
    p_date_end             := '2021-10-04'
) ORDER BY salary_fact_id;

\echo '--- Test 14: Filter that should return no records (non-existent City ID) ---'
SELECT * FROM marketstat.fn_filtered_salaries(p_city_id := 999999) ORDER BY salary_fact_id;

\echo '--- Test 15: Filter that should return no records (date range with no data) ---'
SELECT * FROM marketstat.fn_filtered_salaries(
    p_date_start := '1901-01-01',
    p_date_end   := '1901-12-31'
) ORDER BY salary_fact_id;

\echo '--- Test 16: Filter with conflicting dimension criteria (if possible to set up) ---'
-- Example: City X (which is in Oblast A) and Oblast B (where City X is not).
-- Let's assume City 'Сергиев Посад' (ID 101, in Oblast 'Московская область' ID 201)
-- And Oblast 'Тульская область' (ID 202 - placeholder). This should return 0 rows.
-- REPLACE 101 with ID for 'Сергиев Посад', REPLACE 202 with ID for 'Тульская область'.
SELECT * FROM marketstat.fn_filtered_salaries(
    p_city_id   := 101,
    p_oblast_id := 202
) ORDER BY salary_fact_id;

\echo '--- End of fn_filtered_salaries Test Script ---'