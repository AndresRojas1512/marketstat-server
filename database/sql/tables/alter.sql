ALTER TABLE marketstat.dim_job_role
ADD CONSTRAINT uq_dim_job_role_natural_key UNIQUE (job_role_title, standard_job_role_id, hierarchy_level_id);

ALTER TABLE marketstat.dim_employee
ADD CONSTRAINT uq_dim_employee_natural_key UNIQUE (birth_date, career_start_date);

ALTER TABLE marketstat.users
    ALTER COLUMN email SET NOT NULL,
    ALTER COLUMN full_name SET NOT NULL;