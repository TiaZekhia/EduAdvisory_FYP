-- =====================================================
-- EduAdvisory FYP
-- Database Schema (Updated: course_code as PK)
-- =====================================================

-- =========================
-- ADVISOR
-- =========================
CREATE TABLE advisor (
    advisor_id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL
);

-- =========================
-- COURSE
-- =========================
CREATE TABLE sis_course (
    course_code VARCHAR(20) PRIMARY KEY,
    course_name VARCHAR(150) NOT NULL,
    credits INT NOT NULL CHECK (credits IN (1, 3))
);

-- =========================
-- COURSE PREREQUISITE
-- (Self-referencing Many-to-Many)
-- =========================
CREATE TABLE course_prerequisite (
    course_code VARCHAR(20),
    prerequisite_course_code VARCHAR(20),
    PRIMARY KEY (course_code, prerequisite_course_code),
    FOREIGN KEY (course_code)
        REFERENCES sis_course(course_code)
        ON DELETE CASCADE,
    FOREIGN KEY (prerequisite_course_code)
        REFERENCES sis_course(course_code)
        ON DELETE CASCADE
);

-- =========================
-- STUDENT (SIS)
-- =========================
CREATE TABLE sis_student (
    student_id SERIAL PRIMARY KEY,
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    program_code VARCHAR(50),
    current_semester INT CHECK (current_semester BETWEEN 1 AND 10),
    current_gpa DECIMAL(3,2),
    academic_status VARCHAR(20) CHECK (academic_status IN ('NORMAL', 'PROBATION')),
    advisor_id INT,
    FOREIGN KEY (advisor_id)
        REFERENCES advisor(advisor_id)
);

-- =========================
-- STUDENT COURSE HISTORY (SIS)
-- =========================
CREATE TABLE sis_student_course_history (
    history_id SERIAL PRIMARY KEY,
    student_id INT NOT NULL,
    course_code VARCHAR(20) NOT NULL,
    semester VARCHAR(20),
    final_grade DECIMAL(5,2),
    status VARCHAR(20) CHECK (status IN ('PASSED', 'FAILED')),
    FOREIGN KEY (student_id)
        REFERENCES sis_student(student_id),
    FOREIGN KEY (course_code)
        REFERENCES sis_course(course_code)
);

-- =========================
-- CURRENT ENROLLMENT (SIS)
-- =========================
CREATE TABLE sis_current_enrollment (
    enrollment_id SERIAL PRIMARY KEY,
    student_id INT NOT NULL,
    course_code VARCHAR(20) NOT NULL,
    semester VARCHAR(20),
    FOREIGN KEY (student_id)
        REFERENCES sis_student(student_id),
    FOREIGN KEY (course_code)
        REFERENCES sis_course(course_code)
);

-- =========================
-- COURSE ASSESSMENT (ATTENDANCE)
-- =========================
CREATE TABLE sis_course_assessment (
    assessment_id SERIAL PRIMARY KEY,
    student_id INT NOT NULL,
    course_code VARCHAR(20) NOT NULL,
    course_credits INT,
    absences_count INT,
    max_absences INT,
    semester_start_date DATE,
    semester_end_date DATE,
    last_updated TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (student_id)
        REFERENCES sis_student(student_id),
    FOREIGN KEY (course_code)
        REFERENCES sis_course(course_code)
);

-- =========================
-- COURSE GRADING SCHEMA
-- =========================
CREATE TABLE course_grading_schema (
    grading_id SERIAL PRIMARY KEY,
    course_code VARCHAR(20) NOT NULL,
    component_name VARCHAR(50),
    weight_percentage INT CHECK (weight_percentage BETWEEN 0 AND 100),
    FOREIGN KEY (course_code)
        REFERENCES sis_course(course_code)
);

-- =========================
-- STUDENT GRADES (SIS)
-- =========================
CREATE TABLE sis_student_grades (
    grade_id SERIAL PRIMARY KEY,
    student_id INT NOT NULL,
    course_code VARCHAR(20) NOT NULL,
    component_name VARCHAR(50),
    grade DECIMAL(5,2),
    FOREIGN KEY (student_id)
        REFERENCES sis_student(student_id),
    FOREIGN KEY (course_code)
        REFERENCES sis_course(course_code)
);

-- =========================
-- STUDY GUIDE
-- =========================
CREATE TABLE study_guide (
    study_guide_id SERIAL PRIMARY KEY,
    program_code VARCHAR(50),
    course_code VARCHAR(20),
    recommended_semester INT CHECK (recommended_semester BETWEEN 1 AND 10),
    FOREIGN KEY (course_code)
        REFERENCES sis_course(course_code)
);

-- =========================
-- GENERATED STUDY PLAN
-- =========================
CREATE TABLE generated_study_plan (
    plan_id SERIAL PRIMARY KEY,
    student_id INT,
    course_code VARCHAR(20),
    planned_semester INT,
    generation_date DATE,
    FOREIGN KEY (student_id)
        REFERENCES sis_student(student_id),
    FOREIGN KEY (course_code)
        REFERENCES sis_course(course_code)
);

-- =========================
-- STUDENT RISK
-- =========================
CREATE TABLE student_risk (
    risk_id SERIAL PRIMARY KEY,
    student_id INT,
    course_code VARCHAR(20),
    risk_level VARCHAR(20) CHECK (risk_level IN ('LOW', 'MEDIUM', 'HIGH')),
    risk_score DECIMAL(5,2),
    calculated_at TIMESTAMP,
    FOREIGN KEY (student_id)
        REFERENCES sis_student(student_id),
    FOREIGN KEY (course_code)
        REFERENCES sis_course(course_code)
);

-- =========================
-- USERS
-- =========================
CREATE TABLE users (
    user_id SERIAL PRIMARY KEY,
    username VARCHAR(100) UNIQUE,
    password TEXT,
    role VARCHAR(20) CHECK (role IN ('ADMIN', 'ADVISOR', 'STUDENT')),
    linked_student_id INT,
    linked_advisor_id INT,
    FOREIGN KEY (linked_student_id)
        REFERENCES sis_student(student_id),
    FOREIGN KEY (linked_advisor_id)
        REFERENCES advisor(advisor_id)
);

-- =========================
-- MEETING
-- =========================
CREATE TABLE meeting (
    meeting_id SERIAL PRIMARY KEY,
    advisor_id INT,
    student_id INT,
    meeting_date TIMESTAMP,
    meeting_type VARCHAR(20),
    notes TEXT,
    FOREIGN KEY (advisor_id)
        REFERENCES advisor(advisor_id),
    FOREIGN KEY (student_id)
        REFERENCES sis_student(student_id)
);

-- =========================
-- ANNOUNCEMENT
-- =========================
CREATE TABLE announcement (
    announcement_id SERIAL PRIMARY KEY,
    advisor_id INT,
    title VARCHAR(150),
    content TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (advisor_id)
        REFERENCES advisor(advisor_id)
);

-- =========================
-- CHATBOT HISTORY
-- =========================
CREATE TABLE chatbot_history (
    chatbot_id SERIAL PRIMARY KEY,
    student_id INT,
    question TEXT,
    answer TEXT,
    timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (student_id)
        REFERENCES sis_student(student_id)
);
