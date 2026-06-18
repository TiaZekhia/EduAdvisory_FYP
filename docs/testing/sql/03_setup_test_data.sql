-- ============================================================
-- EduAdvisory Testing — Step 3: Prepare All Test Data
-- Run AFTER backup (01_backup.sql).
-- This script prepares data for Tests 2, 3, 4, and 5.
-- ============================================================

DO $$
DECLARE
    v_student_id   INT;
    v_advisor_id   INT;
    v_program_code TEXT;
    v_test_day_of_week INT;
BEGIN
    -- Get test IDs
    SELECT linked_student_id INTO v_student_id FROM users WHERE username = 'tia.student';
    SELECT linked_advisor_id INTO v_advisor_id FROM users WHERE username = 'ahmad.advisor';
    SELECT program_code INTO v_program_code FROM sis_student WHERE student_id = v_student_id;

    IF v_student_id IS NULL THEN
        RAISE EXCEPTION 'tia.student not found in users table. Check account setup.';
    END IF;
    IF v_advisor_id IS NULL THEN
        RAISE EXCEPTION 'ahmad.advisor not found in users table. Check account setup.';
    END IF;

    RAISE NOTICE 'Test setup: student_id=%, advisor_id=%, program=%',
        v_student_id, v_advisor_id, v_program_code;

    -- ============================================================
    -- TEST 2 PREP — Ensure student is enrolled in an AI-documented course
    -- Run ONLY if ai_document_chunks has at least 1 embedded chunk.
    -- ============================================================

    -- (No SQL needed — enrollment is checked via 00_identify_accounts.sql)
    -- The AI Assistant test uses the student's existing enrollment.
    -- If you need to add an enrollment for a specific PDF course:
    -- INSERT INTO sis_current_enrollment (student_id, course_code, semester)
    -- VALUES (v_student_id, '<YOUR_PDF_COURSE_CODE>', '2025-2026 Spring')
    -- ON CONFLICT DO NOTHING;

    -- ============================================================
    -- TEST 3 PREP — HIGH-risk student dataset
    -- ============================================================

    -- 3a. Set student GPA to HIGH-risk level (< 60) and PROBATION status
    UPDATE sis_student
    SET current_gpa     = 52.00,
        academic_status = 'PROBATION'
    WHERE student_id = v_student_id;

    -- 3b. Maximize absences for all enrolled courses
    UPDATE sis_course_assessment
    SET absences_count = max_absences,
        last_updated   = NOW()
    WHERE student_id = v_student_id;

    -- If no assessment rows exist yet, insert them from enrollment
    INSERT INTO sis_course_assessment
        (student_id, course_code, course_credits, absences_count, max_absences,
         semester_start_date, semester_end_date, last_updated)
    SELECT
        v_student_id,
        e.course_code,
        COALESCE(sc.credits, 3),
        CASE WHEN COALESCE(sc.credits, 3) = 1 THEN 3 ELSE 9 END,
        CASE WHEN COALESCE(sc.credits, 3) = 1 THEN 3 ELSE 9 END,
        '2025-09-01',
        '2026-01-31',
        NOW()
    FROM sis_current_enrollment e
    JOIN sis_course sc ON sc.course_code = e.course_code
    WHERE e.student_id = v_student_id
      AND NOT EXISTS (
          SELECT 1 FROM sis_course_assessment
          WHERE student_id = v_student_id
            AND course_code = e.course_code
      );

    -- 3c. Set all component grades very low (< 60 = HIGH risk per component)
    UPDATE sis_student_grades
    SET grade = 28.00
    WHERE student_id = v_student_id;

    -- If no grade rows yet, insert failing grades from enrollment + grading schema
    INSERT INTO sis_student_grades (student_id, course_code, component_name, grade)
    SELECT
        v_student_id,
        e.course_code,
        gs.component_name,
        28.00
    FROM sis_current_enrollment e
    JOIN course_grading_schema gs ON gs.course_code = e.course_code
    WHERE e.student_id = v_student_id
      AND NOT EXISTS (
          SELECT 1 FROM sis_student_grades
          WHERE student_id = v_student_id
            AND course_code = e.course_code
      );

    -- 3d. Clear cooldown so automation runs fresh
    DELETE FROM risk_intervention_log
    WHERE student_id = v_student_id;

    -- 3e. Ensure conversation exists between student and advisor (required for message delivery)
    INSERT INTO conversation (advisor_id, student_id, created_at)
    VALUES (v_advisor_id, v_student_id, NOW())
    ON CONFLICT ON CONSTRAINT conversation_advisor_student_unique DO NOTHING;

    -- ============================================================
    -- TEST 4 PREP — Advisor availability rule for meeting requests
    -- Pick the next weekday that is a Monday (day_of_week = 1)
    -- ============================================================

    -- Remove any old test rules to start clean
    -- (does NOT remove original rules — those are in backup)
    -- We add a fresh Monday rule
    DELETE FROM advisor_availability_rule
    WHERE advisor_id = v_advisor_id AND day_of_week = 1;

    INSERT INTO advisor_availability_rule
        (advisor_id, day_of_week, start_time, end_time, is_active, created_at)
    VALUES
        (v_advisor_id, 1, '08:00:00', '18:00:00', true, NOW());
    -- day_of_week: 0=Sunday, 1=Monday, 2=Tuesday, 3=Wednesday,
    --              4=Thursday, 5=Friday, 6=Saturday

    -- Also add Thursday rule as backup (common working day in Lebanon)
    DELETE FROM advisor_availability_rule
    WHERE advisor_id = v_advisor_id AND day_of_week = 4;

    INSERT INTO advisor_availability_rule
        (advisor_id, day_of_week, start_time, end_time, is_active, created_at)
    VALUES
        (v_advisor_id, 4, '08:00:00', '18:00:00', true, NOW());

    -- 4b. Cancel any existing PENDING requests from this student
    UPDATE meeting_request
    SET status = 'CANCELLED'
    WHERE student_id = v_student_id AND status = 'PENDING';

    -- ============================================================
    -- TEST 5 PREP — Verify course plan prerequisites exist
    -- No data injection needed; plans are generated from existing
    -- study_guide, course_prerequisite, and course history.
    -- Just ensure student has a valid program_code and semester.
    -- ============================================================

    -- Set a specific semester for predictable plan generation
    UPDATE sis_student
    SET current_semester = 5
    WHERE student_id = v_student_id
      AND (current_semester IS NULL OR current_semester < 1);

    -- Clear old generated plans for a clean test
    DELETE FROM generated_study_plan WHERE student_id = v_student_id;

    RAISE NOTICE 'Test data setup complete.';
END $$;

-- ============================================================
-- Verify setup results
-- ============================================================
SELECT 'SETUP VERIFICATION' AS section;

-- Test 3 risk data
SELECT
    'Risk data' AS check_name,
    s.current_gpa,
    s.academic_status,
    COUNT(ca.assessment_id)                                          AS assessment_rows,
    COUNT(ca.assessment_id) FILTER (WHERE ca.absences_count >= ca.max_absences) AS maxed_absences,
    COUNT(sg.grade_id)                                               AS grade_rows
FROM sis_student s
LEFT JOIN sis_course_assessment ca ON ca.student_id = s.student_id
LEFT JOIN sis_student_grades sg    ON sg.student_id = s.student_id
WHERE s.student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student')
GROUP BY s.current_gpa, s.academic_status;

-- Test 4 availability rules
SELECT 'Availability rules' AS check_name, day_of_week, start_time, end_time, is_active
FROM advisor_availability_rule
WHERE advisor_id = (SELECT linked_advisor_id FROM users WHERE username = 'ahmad.advisor');

-- Test 5 course plan readiness
SELECT
    'Course plan data' AS check_name,
    sg.program_code,
    COUNT(sg.study_guide_id)  AS program_courses,
    COUNT(h.history_id)       AS passed_courses,
    COUNT(sg.study_guide_id) - COUNT(h.history_id) AS remaining_courses
FROM sis_student st
JOIN study_guide sg ON sg.program_code = st.program_code
LEFT JOIN sis_student_course_history h
    ON h.student_id = st.student_id
    AND h.course_code = sg.course_code
    AND h.final_grade >= 50
WHERE st.student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student')
GROUP BY sg.program_code;
