-- ============================================================
-- EduAdvisory Testing — Step 2: Restore Original Data
-- Run AFTER tests to revert to the backed-up state.
-- Requires 01_backup.sql to have been run first.
-- ============================================================

DO $$
DECLARE
    v_student_id INT;
    v_advisor_id INT;
BEGIN
    SELECT linked_student_id INTO v_student_id FROM users WHERE username = 'tia.student';
    SELECT linked_advisor_id INTO v_advisor_id FROM users WHERE username = 'ahmad.advisor';

    -- --------------------------------------------------------
    -- 1. Restore student profile (GPA, status, semester)
    -- --------------------------------------------------------
    DELETE FROM sis_student WHERE student_id = v_student_id;
    INSERT INTO sis_student SELECT * FROM testing_backup.sis_student_snap;

    -- --------------------------------------------------------
    -- 2. Restore course assessments
    -- --------------------------------------------------------
    DELETE FROM sis_course_assessment WHERE student_id = v_student_id;
    INSERT INTO sis_course_assessment SELECT * FROM testing_backup.sis_course_assessment_snap;

    -- --------------------------------------------------------
    -- 3. Restore student grades
    -- --------------------------------------------------------
    DELETE FROM sis_student_grades WHERE student_id = v_student_id;
    INSERT INTO sis_student_grades SELECT * FROM testing_backup.sis_student_grades_snap;

    -- --------------------------------------------------------
    -- 4. Remove test risk records (only rows created AFTER backup)
    -- --------------------------------------------------------
    DELETE FROM student_risk
    WHERE student_id = v_student_id
      AND risk_id NOT IN (SELECT risk_id FROM testing_backup.student_risk_snap);

    -- --------------------------------------------------------
    -- 5. Remove test risk intervention logs (created during tests)
    -- --------------------------------------------------------
    DELETE FROM risk_intervention_log
    WHERE student_id = v_student_id
      AND id NOT IN (SELECT id FROM testing_backup.risk_intervention_log_snap);

    -- --------------------------------------------------------
    -- 6. Remove test meetings (created during tests)
    -- --------------------------------------------------------
    DELETE FROM meeting
    WHERE student_id = v_student_id
      AND meeting_id NOT IN (SELECT meeting_id FROM testing_backup.meeting_snap);

    -- --------------------------------------------------------
    -- 7. Remove test meeting requests (created during tests)
    -- --------------------------------------------------------
    DELETE FROM meeting_request
    WHERE student_id = v_student_id
      AND request_id NOT IN (SELECT request_id FROM testing_backup.meeting_request_snap);

    -- --------------------------------------------------------
    -- 8. Remove AI chat sessions and messages created during tests
    -- --------------------------------------------------------
    DELETE FROM ai_chat_messages
    WHERE session_id IN (
        SELECT session_id FROM ai_chat_sessions
        WHERE student_id = v_student_id
          AND session_id NOT IN (SELECT session_id FROM testing_backup.ai_chat_sessions_snap)
    );
    DELETE FROM ai_chat_sessions
    WHERE student_id = v_student_id
      AND session_id NOT IN (SELECT session_id FROM testing_backup.ai_chat_sessions_snap);

    -- --------------------------------------------------------
    -- 9. Remove AI retrieval logs created during tests
    -- --------------------------------------------------------
    DELETE FROM ai_retrieval_logs
    WHERE student_id = v_student_id
      AND retrieval_log_id NOT IN (SELECT retrieval_log_id FROM testing_backup.ai_retrieval_logs_snap);

    -- --------------------------------------------------------
    -- 10. Remove generated study plans created during tests
    -- --------------------------------------------------------
    DELETE FROM generated_study_plan
    WHERE student_id = v_student_id
      AND plan_id NOT IN (SELECT plan_id FROM testing_backup.generated_study_plan_snap);

    -- --------------------------------------------------------
    -- 11. Restore advisor availability rules to original state
    -- --------------------------------------------------------
    DELETE FROM advisor_availability_rule WHERE advisor_id = v_advisor_id;
    INSERT INTO advisor_availability_rule SELECT * FROM testing_backup.advisor_availability_rule_snap;

    RAISE NOTICE 'Restore completed for student_id=% and advisor_id=%', v_student_id, v_advisor_id;
END $$;

-- Verify restore
SELECT 'After restore:' AS status;
SELECT current_gpa, academic_status FROM sis_student
WHERE student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student');
