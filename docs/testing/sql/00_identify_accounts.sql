-- ============================================================
-- EduAdvisory Testing — Step 0: Identify Test Account IDs
-- Run this FIRST. Note the IDs for use in all other scripts.
-- ============================================================

-- 1. Find the student account linked to 'tia.student'
SELECT
    u.user_id,
    u.username,
    u.role,
    u.is_active,
    u.keycloak_id,
    u.linked_student_id      AS student_id,
    s.first_name,
    s.last_name,
    s.program_code,
    s.current_semester,
    s.current_gpa,
    s.academic_status,
    s.advisor_id             AS assigned_advisor_id
FROM users u
JOIN sis_student s ON u.linked_student_id = s.student_id
WHERE u.username = 'tia.student';

-- 2. Find the advisor account linked to 'ahmad.advisor'
SELECT
    u.user_id,
    u.username,
    u.role,
    u.is_active,
    u.keycloak_id,
    u.linked_advisor_id      AS advisor_id,
    a.name                   AS advisor_name,
    a.email                  AS advisor_email
FROM users u
JOIN advisor a ON u.linked_advisor_id = a.advisor_id
WHERE u.username = 'ahmad.advisor';

-- 3. Find the admin account
SELECT user_id, username, role, is_active, keycloak_id
FROM users
WHERE role = 'ADMIN';

-- 4. Find the AutomationAdmin account
SELECT user_id, username, role, is_active, keycloak_id
FROM users
WHERE role = 'AutomationAdmin';

-- 5. Verify the student-advisor assignment is correct
SELECT
    s.student_id,
    s.first_name || ' ' || s.last_name AS student_name,
    s.program_code,
    s.current_semester,
    a.advisor_id,
    a.name AS advisor_name
FROM sis_student s
JOIN advisor a ON s.advisor_id = a.advisor_id
WHERE s.student_id = (
    SELECT linked_student_id FROM users WHERE username = 'tia.student'
);

-- 6. Check current academic data for the test student
SELECT
    e.course_code,
    c.course_name,
    e.semester,
    ca.absences_count,
    ca.max_absences,
    ROUND((ca.absences_count::numeric / NULLIF(ca.max_absences,0)) * 100, 1) AS absence_pct
FROM sis_current_enrollment e
JOIN sis_course c ON c.course_code = e.course_code
LEFT JOIN sis_course_assessment ca ON ca.student_id = e.student_id AND ca.course_code = e.course_code
WHERE e.student_id = (
    SELECT linked_student_id FROM users WHERE username = 'tia.student'
)
ORDER BY e.course_code;

-- 7. Check completed course history
SELECT COUNT(*) AS completed_courses
FROM sis_student_course_history
WHERE student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student')
  AND final_grade >= 50;

-- 8. Check study guide course count for this student's program
SELECT COUNT(*) AS total_program_courses
FROM study_guide
WHERE program_code = (
    SELECT program_code FROM sis_student
    WHERE student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student')
);

-- 9. Check Google account status
SELECT
    id,
    google_email,
    refresh_token IS NOT NULL AS has_refresh_token,
    token_expiry_utc,
    CASE WHEN token_expiry_utc > NOW() THEN 'VALID' ELSE 'EXPIRED_OR_NULL' END AS token_status
FROM app_google_account;

-- 10. Check pgvector extension
SELECT extname, extversion FROM pg_extension WHERE extname = 'vector';

-- 11. Check processed AI documents
SELECT
    d.document_id, d.title, d.course_code, d.document_type, d.status,
    COUNT(c.chunk_id)                                         AS chunks,
    COUNT(c.chunk_id) FILTER (WHERE c.embedding IS NOT NULL) AS embedded
FROM ai_documents d
LEFT JOIN ai_document_chunks c ON c.document_id = d.document_id
GROUP BY d.document_id, d.title, d.course_code, d.document_type, d.status;
