-- ============================================================
-- EduAdvisory — Verify Test 1: Authentication & RBAC
-- Run after completing Test 1 execution.
-- ============================================================

SELECT '=== TEST 1: AUTH & RBAC VERIFICATION ===' AS section;

-- 1. Confirm all 4 roles exist in the users table
SELECT
    role,
    COUNT(*)     AS account_count,
    BOOL_AND(is_active) AS all_active,
    BOOL_AND(keycloak_id IS NOT NULL) AS all_have_keycloak_id
FROM users
WHERE role IN ('STUDENT', 'ADVISOR', 'ADMIN', 'AutomationAdmin')
GROUP BY role
ORDER BY role;

-- Expected: 4 rows, each with all_active=true, all_have_keycloak_id=true

-- 2. Confirm student and advisor are properly linked
SELECT
    u.username,
    u.role,
    CASE
        WHEN u.role = 'STUDENT'  AND u.linked_student_id IS NOT NULL THEN 'LINKED'
        WHEN u.role = 'ADVISOR'  AND u.linked_advisor_id IS NOT NULL THEN 'LINKED'
        WHEN u.role IN ('ADMIN','AutomationAdmin')                   THEN 'N/A'
        ELSE 'MISSING LINK'
    END AS link_status
FROM users u
WHERE u.role IN ('STUDENT', 'ADVISOR', 'ADMIN', 'AutomationAdmin')
ORDER BY u.role, u.username;

-- 3. Confirm student links to the expected student row
SELECT
    u.username,
    u.role,
    s.student_id,
    s.first_name || ' ' || s.last_name AS student_name,
    s.program_code,
    s.advisor_id
FROM users u
JOIN sis_student s ON u.linked_student_id = s.student_id
WHERE u.username = 'tia.student';

-- 4. Confirm advisor links to the expected advisor row
SELECT
    u.username,
    u.role,
    a.advisor_id,
    a.name AS advisor_name,
    a.email
FROM users u
JOIN advisor a ON u.linked_advisor_id = a.advisor_id
WHERE u.username = 'ahmad.advisor';
