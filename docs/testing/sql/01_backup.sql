-- ============================================================
-- EduAdvisory Testing — Step 1: Backup Test Data
-- Run BEFORE any test to create a recoverable snapshot.
-- Replace :STUDENT_ID and :ADVISOR_ID with values from Step 0.
-- ============================================================

-- Create backup schema (run once)
CREATE SCHEMA IF NOT EXISTS testing_backup;

-- Drop old backups if they exist
DROP TABLE IF EXISTS testing_backup.sis_student_snap;
DROP TABLE IF EXISTS testing_backup.sis_course_assessment_snap;
DROP TABLE IF EXISTS testing_backup.sis_student_grades_snap;
DROP TABLE IF EXISTS testing_backup.student_risk_snap;
DROP TABLE IF EXISTS testing_backup.risk_intervention_log_snap;
DROP TABLE IF EXISTS testing_backup.meeting_snap;
DROP TABLE IF EXISTS testing_backup.meeting_request_snap;
DROP TABLE IF EXISTS testing_backup.conversation_snap;
DROP TABLE IF EXISTS testing_backup.chat_message_snap;
DROP TABLE IF EXISTS testing_backup.ai_chat_sessions_snap;
DROP TABLE IF EXISTS testing_backup.ai_chat_messages_snap;
DROP TABLE IF EXISTS testing_backup.ai_retrieval_logs_snap;
DROP TABLE IF EXISTS testing_backup.generated_study_plan_snap;
DROP TABLE IF EXISTS testing_backup.advisor_availability_rule_snap;

-- ============================================================
-- Snapshot tables relevant to the test student and advisor
-- ============================================================

-- Student profile snapshot
CREATE TABLE testing_backup.sis_student_snap AS
SELECT * FROM sis_student
WHERE student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student');

-- Course assessment snapshot
CREATE TABLE testing_backup.sis_course_assessment_snap AS
SELECT * FROM sis_course_assessment
WHERE student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student');

-- Student grades snapshot
CREATE TABLE testing_backup.sis_student_grades_snap AS
SELECT * FROM sis_student_grades
WHERE student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student');

-- Risk records snapshot
CREATE TABLE testing_backup.student_risk_snap AS
SELECT * FROM student_risk
WHERE student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student');

-- Risk intervention log snapshot
CREATE TABLE testing_backup.risk_intervention_log_snap AS
SELECT * FROM risk_intervention_log
WHERE student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student');

-- Meetings snapshot
CREATE TABLE testing_backup.meeting_snap AS
SELECT * FROM meeting
WHERE student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student')
   OR advisor_id = (SELECT linked_advisor_id FROM users WHERE username = 'ahmad.advisor');

-- Meeting requests snapshot
CREATE TABLE testing_backup.meeting_request_snap AS
SELECT * FROM meeting_request
WHERE student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student')
   OR advisor_id = (SELECT linked_advisor_id FROM users WHERE username = 'ahmad.advisor');

-- Conversation snapshot
CREATE TABLE testing_backup.conversation_snap AS
SELECT * FROM conversation
WHERE student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student')
   OR advisor_id = (SELECT linked_advisor_id FROM users WHERE username = 'ahmad.advisor');

-- Chat messages snapshot
CREATE TABLE testing_backup.chat_message_snap AS
SELECT cm.* FROM chat_message cm
JOIN conversation c ON cm.conversation_id = c.conversation_id
WHERE c.student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student');

-- AI chat sessions snapshot
CREATE TABLE testing_backup.ai_chat_sessions_snap AS
SELECT * FROM ai_chat_sessions
WHERE student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student');

-- AI chat messages snapshot
CREATE TABLE testing_backup.ai_chat_messages_snap AS
SELECT acm.* FROM ai_chat_messages acm
JOIN ai_chat_sessions acs ON acm.session_id = acs.session_id
WHERE acs.student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student');

-- AI retrieval logs snapshot
CREATE TABLE testing_backup.ai_retrieval_logs_snap AS
SELECT * FROM ai_retrieval_logs
WHERE student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student');

-- Generated study plan snapshot
CREATE TABLE testing_backup.generated_study_plan_snap AS
SELECT * FROM generated_study_plan
WHERE student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student');

-- Advisor availability rules snapshot
CREATE TABLE testing_backup.advisor_availability_rule_snap AS
SELECT * FROM advisor_availability_rule
WHERE advisor_id = (SELECT linked_advisor_id FROM users WHERE username = 'ahmad.advisor');

-- ============================================================
-- Verify backup was created
-- ============================================================
SELECT
    'sis_student'              AS table_name, COUNT(*) AS rows FROM testing_backup.sis_student_snap
UNION ALL SELECT 'sis_course_assessment',  COUNT(*) FROM testing_backup.sis_course_assessment_snap
UNION ALL SELECT 'sis_student_grades',     COUNT(*) FROM testing_backup.sis_student_grades_snap
UNION ALL SELECT 'student_risk',           COUNT(*) FROM testing_backup.student_risk_snap
UNION ALL SELECT 'risk_intervention_log',  COUNT(*) FROM testing_backup.risk_intervention_log_snap
UNION ALL SELECT 'meeting',                COUNT(*) FROM testing_backup.meeting_snap
UNION ALL SELECT 'meeting_request',        COUNT(*) FROM testing_backup.meeting_request_snap
UNION ALL SELECT 'conversation',           COUNT(*) FROM testing_backup.conversation_snap
UNION ALL SELECT 'chat_message',           COUNT(*) FROM testing_backup.chat_message_snap
UNION ALL SELECT 'ai_chat_sessions',       COUNT(*) FROM testing_backup.ai_chat_sessions_snap
UNION ALL SELECT 'ai_retrieval_logs',      COUNT(*) FROM testing_backup.ai_retrieval_logs_snap
UNION ALL SELECT 'generated_study_plan',   COUNT(*) FROM testing_backup.generated_study_plan_snap
UNION ALL SELECT 'advisor_availability_rule', COUNT(*) FROM testing_backup.advisor_availability_rule_snap
ORDER BY table_name;
