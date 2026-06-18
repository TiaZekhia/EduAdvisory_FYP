-- ============================================================
-- EduAdvisory — Verify Test 5: AI Course Planning
-- Run after completing Test 5 execution.
-- ============================================================

SELECT '=== TEST 5: COURSE PLANNING VERIFICATION ===' AS section;

-- 1. Verify generated plan rows exist
SELECT
    gsp.plan_id,
    gsp.student_id,
    gsp.course_code,
    sc.course_name,
    sc.credits,
    gsp.planned_semester,
    gsp.generation_date
FROM generated_study_plan gsp
JOIN sis_course sc ON sc.course_code = gsp.course_code
WHERE gsp.student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student')
ORDER BY gsp.planned_semester, sc.course_name;
-- Expected: rows covering all remaining program courses spread across semesters

-- 2. Count courses per planned semester
SELECT
    planned_semester,
    COUNT(*)            AS courses_in_semester,
    SUM(sc.credits)     AS total_credits
FROM generated_study_plan gsp
JOIN sis_course sc ON sc.course_code = gsp.course_code
WHERE gsp.student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student')
GROUP BY planned_semester
ORDER BY planned_semester;
-- Expected: each semester has a reasonable credit load (<= 18 for normal, <= 16 for PROBATION)

-- 3. Verify NO prerequisite violations in the plan
SELECT
    dep.course_code              AS course,
    dep.planned_semester         AS course_semester,
    cp.prerequisite_course_code  AS prerequisite,
    pre.planned_semester         AS prereq_semester,
    CASE
        WHEN pre.planned_semester IS NULL
            THEN 'PREREQ ALREADY PASSED (not in plan)'
        WHEN pre.planned_semester < dep.planned_semester
            THEN 'OK'
        WHEN pre.planned_semester = dep.planned_semester
            THEN 'SAME SEMESTER — CHECK'
        ELSE
            'VIOLATION!'
    END AS prerequisite_check
FROM generated_study_plan dep
JOIN course_prerequisite cp
    ON cp.course_code = dep.course_code
LEFT JOIN generated_study_plan pre
    ON pre.course_code = cp.prerequisite_course_code
   AND pre.student_id  = dep.student_id
WHERE dep.student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student')
ORDER BY dep.planned_semester, dep.course_code;
-- Expected: all rows show 'OK' or 'PREREQ ALREADY PASSED' — no 'VIOLATION!'

-- 4. Count of violations (should be 0)
SELECT
    COUNT(*) AS prerequisite_violations
FROM generated_study_plan dep
JOIN course_prerequisite cp ON cp.course_code = dep.course_code
JOIN generated_study_plan pre
    ON pre.course_code = cp.prerequisite_course_code
   AND pre.student_id  = dep.student_id
WHERE dep.student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student')
  AND pre.planned_semester >= dep.planned_semester;
-- Expected: 0

-- 5. Verify all remaining courses are included in the plan
WITH remaining AS (
    SELECT sg.course_code
    FROM study_guide sg
    WHERE sg.program_code = (
        SELECT program_code FROM sis_student
        WHERE student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student')
    )
    AND sg.course_code NOT IN (
        SELECT h.course_code
        FROM sis_student_course_history h
        WHERE h.student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student')
          AND h.final_grade >= 50
    )
),
planned AS (
    SELECT DISTINCT course_code
    FROM generated_study_plan
    WHERE student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student')
)
SELECT
    r.course_code,
    CASE WHEN p.course_code IS NOT NULL THEN 'IN PLAN' ELSE 'MISSING FROM PLAN' END AS plan_status
FROM remaining r
LEFT JOIN planned p ON p.course_code = r.course_code
ORDER BY plan_status, r.course_code;
-- Expected: all rows show 'IN PLAN', no 'MISSING FROM PLAN'

-- 6. Summary
SELECT
    COUNT(*) AS total_courses_in_plan,
    MIN(planned_semester) AS first_planned_semester,
    MAX(planned_semester) AS last_planned_semester,
    MIN(generation_date)  AS generated_at
FROM generated_study_plan
WHERE student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student');
