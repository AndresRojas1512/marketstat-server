\set ON_ERROR_STOP on
SET search_path = marketstat, public;


SELECT
    (SELECT COUNT(*) FROM marketstat.dim_standard_job_role) AS standard_job_roles_count,
    (SELECT COUNT(*) FROM marketstat.dim_hierarchy_level) AS hierarchy_levels_count,
    (SELECT COUNT(*) FROM marketstat.dim_standard_job_role) * (SELECT COUNT(*) FROM marketstat.dim_hierarchy_level) AS expected_combinations;

BEGIN;

INSERT INTO marketstat.dim_standard_job_role_hierarchy (standard_job_role_id, hierarchy_level_id)
SELECT
    sjr.standard_job_role_id,
    hl.hierarchy_level_id
FROM
    marketstat.dim_standard_job_role sjr,
    marketstat.dim_hierarchy_level hl
ON CONFLICT (standard_job_role_id, hierarchy_level_id) DO NOTHING;
COMMIT;

SELECT COUNT(*) AS total_rows_in_dim_standard_job_role_hierarchy
  FROM marketstat.dim_standard_job_role_hierarchy;

\echo 'Successfully populated dim_standard_job_role_hierarchy table.'
\echo 'Verify that the total_rows_in_dim_standard_job_role_hierarchy matches the expected_combinations.'