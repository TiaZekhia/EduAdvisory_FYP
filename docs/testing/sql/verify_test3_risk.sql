-- ============================================================
-- EduAdvisory — Verify Test 3: Risk Assessment + n8n Automation
-- Run after completing Test 3 execution.
-- ============================================================

SELECT '=== TEST 3: RISK ASSESSMENT + AUTOMATION VERIFICATION ===' AS section;

-- 1. Verify the student's HIGH-risk input data
SELECT
    'Risk inputs' AS check_name,
    s.student_id,
    s.current_gpa,
    s.academic_status,
    s.current_semester
FROM sis_student s
WHERE s.student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student');
-- Expected: current_gpa=52.00, academic_status='PROBATION'

-- 2. Verify absences are at maximum (absenceRisk = 1.0)
SELECT
    ca.course_code,
    ca.absences_count,
    ca.max_absences,
    CASE WHEN ca.absences_count >= ca.max_absences THEN 'HIGH RISK' ELSE 'OK' END AS absence_status,
    ROUND((ca.absences_count::numeric / NULLIF(ca.max_absences,0)) * 100, 1) AS absence_pct
FROM sis_course_assessment ca
WHERE ca.student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student')
ORDER BY ca.course_code;
-- Expected: all rows show absence_pct = 100.0

-- 3. Verify component grades are failing (componentRisk = 1.0)
SELECT
    sg.course_code,
    sg.component_name,
    sg.grade,
    CASE
        WHEN sg.grade < 60 THEN 'HIGH RISK'
        WHEN sg.grade < 70 THEN 'MEDIUM RISK'
        WHEN sg.grade < 80 THEN 'LOW RISK'
        ELSE 'NO RISK'
    END AS grade_risk
FROM sis_student_grades sg
WHERE sg.student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student')
ORDER BY sg.course_code, sg.component_name;
-- Expected: all grades = 28.00, grade_risk = 'HIGH RISK'

-- 4. Verify risk score was computed and stored
SELECT
    r.risk_id,
    r.student_id,
    r.course_code,
    r.risk_level,
    r.risk_score,
    r.calculated_at
FROM student_risk r
WHERE r.student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student')
ORDER BY r.calculated_at DESC;
-- Expected: at least 1 row per enrolled course, risk_level='HIGH', risk_score >= 70

-- 5. Verify risk intervention log (both COMPLETED + SKIPPED_DUPLICATE)
SELECT
    l.id,
    l.student_id,
    l.advisor_id,
    l.risk_level,
    l.risk_score,
    l.action_type,
    l.status,
    LEFT(l.notes, 100) AS notes_preview,
    l.created_at
FROM risk_intervention_log l
WHERE l.student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student')
ORDER BY l.created_at;
-- Expected:
--   Row 1: action_type='HIGH_RISK_MEETING_RECOMMENDATION', status='COMPLETED'
--   Row 2: action_type='HIGH_RISK_MEETING_RECOMMENDATION', status='SKIPPED_DUPLICATE'

-- 6. Verify the [HIGH RISK ALERT] message was sent to advisor conversation
SELECT
    m.message_id,
    m.sender_user_id,
    LEFT(m.content, 150) AS message_content,
    m.sent_at,
    m.is_read,
    c.advisor_id,
    c.student_id
FROM chat_message m
JOIN conversation c ON c.conversation_id = m.conversation_id
WHERE c.student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student')
ORDER BY m.sent_at DESC
LIMIT 5;
-- Expected: message content starts with '[HIGH RISK ALERT]' or '[MEDIUM RISK' or '[LOW RISK'

-- 7. Summary of intervention types processed
SELECT
    action_type,
    status,
    COUNT(*) AS count
FROM risk_intervention_log
WHERE student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student')
GROUP BY action_type, status
ORDER BY action_type, status;
