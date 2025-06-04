SET search_path = marketstat, public;

\echo '--- Starting script to populate dim_employee_education ---'

DO $$
DECLARE
    emp_record RECORD;
    edu_ids INT[];
    selected_education_id INT;
    education_count INT;
    employee_count INT;
    inserted_count INT := 0;

    -- Variables for graduation year calculation
    birth_year INT;
    career_start_year INT;
    min_possible_grad_year INT;
    max_possible_grad_year INT;
    calculated_grad_year SMALLINT;

    min_age_at_graduation INT := 21; -- earliest for a Bachelor's
    max_age_at_graduation INT := 30; -- covering Master's or slightly later starts
BEGIN
    -- Get all education IDs into an array
    SELECT array_agg(education_id) INTO edu_ids FROM marketstat.dim_education;
    SELECT COUNT(*) INTO education_count FROM marketstat.dim_education;

    IF education_count = 0 THEN
        RAISE NOTICE 'No educations found in dim_education. Cannot populate dim_employee_education.';
        RETURN;
    END IF;
    RAISE NOTICE 'Found % education records to choose from.', education_count;

    -- Get count of employees to process
    SELECT COUNT(*) INTO employee_count FROM marketstat.dim_employee;
    IF employee_count = 0 THEN
        RAISE NOTICE 'No employees found in dim_employee. Cannot populate dim_employee_education.';
        RETURN;
    END IF;
    RAISE NOTICE 'Processing % employees...', employee_count;

    -- Loop through each employee
    FOR emp_record IN SELECT employee_id, birth_date, career_start_date FROM marketstat.dim_employee LOOP
        -- Randomly select an education_id from the array
        selected_education_id := edu_ids[floor(random() * array_length(edu_ids, 1) + 1)];

        -- Calculate a plausible graduation year
        birth_year := EXTRACT(YEAR FROM emp_record.birth_date);
        career_start_year := EXTRACT(YEAR FROM emp_record.career_start_date);

        -- Min graduation year: e.g., birth year + 21 (for a Bachelor's)
        min_possible_grad_year := birth_year + min_age_at_graduation;

        -- Max graduation year: should ideally be before or at career start.
        -- Let's say they graduate and then start a career, or graduate in the same year.
        max_possible_grad_year := career_start_year;

        -- Ensure graduation is not after career start and not too early
        IF min_possible_grad_year > max_possible_grad_year THEN
            -- This can happen if career started very early (e.g., before age 21)
            -- Or if min_age_at_graduation is too high for the data.
            -- Default to career_start_year or a year before if career started "too early".
            calculated_grad_year := GREATEST(1900, career_start_year - floor(random() * 2))::SMALLINT; -- Graduated 0-1 years before career
             RAISE NOTICE 'Adjusting grad year for employee % due to early career start. Birth: %, Career: %, MinGrad: %, MaxGrad: %, Chosen: %',
                emp_record.employee_id, birth_year, career_start_year, min_possible_grad_year, max_possible_grad_year, calculated_grad_year;
        ELSE
            -- Generate a random graduation year within the plausible range
            calculated_grad_year := (floor(random() * (max_possible_grad_year - min_possible_grad_year + 1)) + min_possible_grad_year)::SMALLINT;
        END IF;

        -- Ensure graduation_year is within table constraints (1900 to current year)
        calculated_grad_year := GREATEST(1900, LEAST(calculated_grad_year, EXTRACT(YEAR FROM CURRENT_DATE)::SMALLINT));

        -- Insert into the bridge table
        BEGIN
            INSERT INTO marketstat.dim_employee_education (employee_id, education_id, graduation_year)
            VALUES (emp_record.employee_id, selected_education_id, calculated_grad_year);

            inserted_count := inserted_count + 1;
        EXCEPTION
            WHEN unique_violation THEN
                RAISE NOTICE 'Employee ID % already has education ID %. Skipping duplicate.', emp_record.employee_id, selected_education_id;
            WHEN OTHERS THEN
                RAISE WARNING 'Error inserting for employee ID % and education ID %: %', emp_record.employee_id, selected_education_id, SQLERRM;
        END;

    END LOOP;

    RAISE NOTICE 'Finished populating dim_employee_education. Inserted % new links.', inserted_count;

END $$;

\echo '--- Script to populate dim_employee_education finished ---'
