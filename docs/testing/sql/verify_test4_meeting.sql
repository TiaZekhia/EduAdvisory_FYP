-- ============================================================
-- EduAdvisory — Verify Test 4: Meeting Scheduling + Google Meet
-- Run after completing Test 4 execution.
-- ============================================================

SELECT '=== TEST 4: MEETING SCHEDULING + GOOGLE MEET VERIFICATION ===' AS section;

-- 1. Verify advisor availability rules are active
SELECT
    rule_id,
    advisor_id,
    CASE day_of_week
        WHEN 0 THEN 'Sunday'    WHEN 1 THEN 'Monday'
        WHEN 2 THEN 'Tuesday'   WHEN 3 THEN 'Wednesday'
        WHEN 4 THEN 'Thursday'  WHEN 5 THEN 'Friday'
        WHEN 6 THEN 'Saturday'
    END AS day_name,
    day_of_week,
    start_time,
    end_time,
    is_active
FROM advisor_availability_rule
WHERE advisor_id = (SELECT linked_advisor_id FROM users WHERE username = 'ahmad.advisor')
ORDER BY day_of_week;
-- Expected: Monday (1) and Thursday (4) rules with is_active=true

-- 2. Verify meeting request lifecycle
SELECT
    mr.request_id,
    mr.student_id,
    mr.advisor_id,
    mr.start_at AT TIME ZONE 'Asia/Beirut' AS start_beirut,
    mr.end_at   AT TIME ZONE 'Asia/Beirut' AS end_beirut,
    mr.reason,
    mr.status,
    mr.rejection_reason,
    mr.requested_at,
    mr.responded_at
FROM meeting_request mr
WHERE mr.student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student')
ORDER BY mr.requested_at DESC
LIMIT 5;
-- Expected: latest row has status='ACCEPTED', responded_at IS NOT NULL

-- 3. Verify meeting was created with Google Meet link
SELECT
    m.meeting_id,
    m.advisor_id,
    m.student_id,
    m.title,
    m.meeting_type,
    m.start_at AT TIME ZONE 'Asia/Beirut' AS start_beirut,
    m.end_at   AT TIME ZONE 'Asia/Beirut' AS end_beirut,
    m.duration_minutes,
    m.status,
    m.meeting_link,
    m.google_space_name,
    m.request_id,
    m.created_at
FROM meeting m
WHERE m.student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student')
ORDER BY m.created_at DESC
LIMIT 5;
-- Expected:
--   status = 'UPCOMING'
--   meeting_link starts with 'https://meet.google.com/'
--   google_space_name is not null

-- 4. Verify the Google Meet link format
SELECT
    meeting_id,
    meeting_link,
    CASE
        WHEN meeting_link LIKE 'https://meet.google.com/%' THEN 'VALID MEET LINK'
        WHEN meeting_link IS NULL OR meeting_link = '' THEN 'MISSING'
        ELSE 'INVALID FORMAT'
    END AS link_status
FROM meeting
WHERE student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student')
  AND status = 'UPCOMING'
ORDER BY created_at DESC
LIMIT 3;

-- 5. Verify Google account token is still valid
SELECT
    id,
    google_email,
    LEFT(access_token, 30)  AS access_token_prefix,
    token_expiry_utc,
    updated_at,
    CASE WHEN token_expiry_utc > NOW() THEN 'VALID' ELSE 'EXPIRED' END AS token_status
FROM app_google_account;
-- Expected: token_status = 'VALID' (or EXPIRED if it auto-refreshed past the check — both OK)

-- 6. No conflicting meetings exist for the test time
SELECT
    COUNT(*) AS conflicting_upcoming_meetings
FROM meeting
WHERE (advisor_id = (SELECT linked_advisor_id FROM users WHERE username = 'ahmad.advisor')
    OR student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student'))
  AND status = 'UPCOMING'
  AND start_at > NOW();
