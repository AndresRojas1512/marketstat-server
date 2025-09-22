SET search_path = marketstat, public;

DO $$
DECLARE
    emp_record RECORD;
    edu_ids INT[];
    selected_education_id INT;
    education_count INT;
    employee_count INT;
    inserted_count INT := 0;

    birth_year INT;
    career_start_year INT;
    min_possible_grad_year INT;
    max_possible_grad_year INT;
    calculated_grad_year SMALLINT;

    min_age_at_graduation INT := 21;
    max_age_at_graduation INT := 30;
BEGIN
    SELECT array_agg(education_id) INTO edu_ids FROM marketstat.dim_education;
    SELECT COUNT(*) INTO education_count FROM marketstat.dim_education;

    IF education_count = 0 THEN
        RAISE NOTICE 'No educations found in dim_education. Cannot populate dim_employee_education.';
        RETURN;
    END IF;
    RAISE NOTICE 'Found % education records to choose from.', education_count;

    SELECT COUNT(*) INTO employee_count FROM marketstat.dim_employee;
    IF employee_count = 0 THEN
        RAISE NOTICE 'No employees found in dim_employee. Cannot populate dim_employee_education.';
        RETURN;
    END IF;
    RAISE NOTICE 'Processing % employees...', employee_count;

    FOR emp_record IN SELECT employee_id, birth_date, career_start_date FROM marketstat.dim_employee LOOP
        selected_education_id := edu_ids[floor(random() * array_length(edu_ids, 1) + 1)];

        birth_year := EXTRACT(YEAR FROM emp_record.birth_date);
        career_start_year := EXTRACT(YEAR FROM emp_record.career_start_date);

        min_possible_grad_year := birth_year + min_age_at_graduation;

        max_possible_grad_year := career_start_year;

        IF min_possible_grad_year > max_possible_grad_year THEN
            calculated_grad_year := GREATEST(1900, career_start_year - floor(random() * 2))::SMALLINT;
             RAISE NOTICE 'Adjusting grad year for employee % due to early career start. Birth: %, Career: %, MinGrad: %, MaxGrad: %, Chosen: %',
                emp_record.employee_id, birth_year, career_start_year, min_possible_grad_year, max_possible_grad_year, calculated_grad_year;
        ELSE
            calculated_grad_year := (floor(random() * (max_possible_grad_year - min_possible_grad_year + 1)) + min_possible_grad_year)::SMALLINT;
        END IF;

        calculated_grad_year := GREATEST(1900, LEAST(calculated_grad_year, EXTRACT(YEAR FROM CURRENT_DATE)::SMALLINT));

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

