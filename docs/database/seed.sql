-- =====================================================
-- EduAdvisory FYP
-- Seed Data (Study Guide + Dummy SIS Data)
-- =====================================================

-- =====================================================
-- 1. ADVISORS (REAL ENTITIES)
-- =====================================================
INSERT INTO advisor (advisor_id, name, email) VALUES
(1,'Dr. Advisor 1','advisor1@ua.edu'),
(2,'Dr. Advisor 2','advisor2@ua.edu'),
(3,'Dr. Advisor 3','advisor3@ua.edu'),
(4,'Dr. Advisor 4','advisor4@ua.edu'),
(5,'Dr. Advisor 5','advisor5@ua.edu'),
(6,'Dr. Advisor 6','advisor6@ua.edu'),
(7,'Dr. Advisor 7','advisor7@ua.edu'),
(8,'Dr. Advisor 8','advisor8@ua.edu'),
(9,'Dr. Advisor 9','advisor9@ua.edu'),
(10,'Dr. Advisor 10','advisor10@ua.edu');


-- =====================================================
-- 2. COURSES (FROM STUDY GUIDE – CORE CCE)
-- =====================================================
INSERT INTO sis_course (course_code, course_name, credits) VALUES
-- Semester 1
('ENGI102-EC00', 'Introduction to Engineering', 3),
('PELE111-EC01', 'Circuit Analysis', 3),
('PELE111-EP01', 'Lab Circuit Analysis', 1),
('PROG111-EC01', 'Programming I', 3),
('PROG111-EP01', 'Lab Programming I', 1),
('MATH111-EC00', 'Algebra I', 3),
('MATH211-EC00', 'Calculus I', 3),
('NETW101-EP00', 'Lab. Computers and Networks', 1),

-- Semester 2
('SCOP202-AC00', 'Citizenship and Society', 3),
('PELE112-EC00', 'Electrostatics', 3),
('PROG112-EC10', 'Programming II', 3),
('MATH112-EC00', 'Algebra II', 3),
('MATH212-EC10', 'Calculus II', 3),
('NETW205-EC00', 'Introduction to Networks', 3),
('ENGI101-EP00', 'Lab. CAD & GIS', 1),

-- Semester 3
('PELE113-EC11', 'Electricity and Magnetism', 3),
('PELE113-EP01', 'Lab Electricity and Magnetism', 1),
('PROG113-EC10', 'Data Structures', 3),
('MATH213-EC10', 'Calculus III', 3),
('NETW206-EC10', 'Routing and Switching Essentials', 3),
('SYST107-EC00', 'Open Source Systems - UNIX', 3),
('MLTM102-EC10', 'Web Design', 3),

-- Semester 4
('COMM300-EC00', 'Oral and Writing Communication', 3),
('PROG211-EC10', 'Object Oriented Programming I', 3),
('MATH402-EC00', 'Operations Research', 3),
('MATH302-EC00', 'Probability and Statistics', 3),
('NETW207-EC10', 'Scaling and Connecting Networks', 3),
('DBMG105-EC00', 'Database Design', 3),

-- Semester 5
('ELEC101-EC11', 'Fundamental Electronics', 3),
('ELEC102-EP01', 'Lab. Fundamental Electronics', 1),
('SYST101-EC10', 'Theory of Operating Systems', 3),
('DBMG106-EC11', 'Database Programming', 3),
('TLCM111-EC11', 'Electromagnetics and Transmission Lines', 3),
('TLCM111-EP11', 'Lab. Electromagnetics and Transmission Lines', 1),
('SIGN111-EC10', 'Signals and Systems', 3),
('PROG212-EC10', 'Object Oriented Programming II', 3),
('PROG302-EC10', 'Web Programming I', 3),
('SOFT103-EP10', 'Lab. Software Design', 1),
('MLTM302-EP10', 'Lab. Sound Engineering', 1),

-- Semester 6
('COMM402-EC10', 'Communication Skills for Engineers', 3),
('SYST202-EC00', 'Computer Architecture', 3),
('MATH403-EP20', 'Lab. Numerical Analysis', 1),
('NETW208-EC10', 'Networks Architecture', 3),
('SYST108-EC00', 'Proprietary Systems', 3),
('STAP303-EC10', 'Methodology and Internship report', 1),
('ELEC212-EC11', 'Digital Logic Design', 3),
('PROG214-EC11', 'Artificial Intelligence', 3),
('PROG214-EP01', 'Lab. Artificial Intelligence', 1),
('MLTM303-EP10', 'Lab. Audiovisual Production', 1),
('INFG203-EC10', 'Infographics', 3),

-- Semester 7
('ENGI103-EC10', 'Entrepreneurship and Innovation', 3),
('TLCM112-EC10', 'Microwave Circuits', 3),
('SIGN113-EC11', 'Multimedia Signal Processing', 3),
('SIGN113-EP11', 'Lab. Multimedia Signal Processing', 1),
('ELEC211-EC11', 'Electronic Circuits', 3),
('ELEC211-EP11', 'Lab. Electronics', 1),
('SYST304-EC20', 'Information Systems Security', 3),
('SYST402-EC20', 'Development of Open Source Systems', 3),
('SYST403-EC10', 'Interconnection of Open Source Systems', 3),
('DBMG107-EC10', 'Database Administration', 3),
('PROG303-EC10', 'Web Programming II', 3),
('DBMG201-EC10', 'Multimedia Databases and Image Processing', 3),
('PROG213-EC10', 'Advanced Programming', 3),
('SOFT102-EC20', 'UX/UI Design', 3),
('MLTM202-EC10', '3D Modeling', 3),

-- Semester 8
('PJMG101-EC00', 'Project Management', 3),
('MRCH501-ES00', 'Research Methodology Seminar', 0),
('TLCM113-EC10', 'Antennas and Satellites', 3),
('SIGN211-EC11', 'Communications Systems', 3),
('SIGN211-EP11', 'Lab. Communications Systems', 1),
('SEMB111-EC11', 'Microcontroller', 3),
('SEMB111-EP11', 'Lab Microcontroller', 1),
('NETW301-EC20', 'Network Design and Optimization', 3),
('SYST405-EC10', 'Applications on Operating Systems', 3),
('SYST404-EP10', 'Lab. Systems Integration', 1),
('PROG304-EC10', 'Internet Of Things and Big Data', 3),
('PROG401-EC20', 'Mobile Development', 3),
('SOFT101-EC20', 'Human Computer Interaction', 3),
('PROG305-EC10', 'Web Multimedia Technologies', 3),
('MLTM203-EC10', 'Game Programming', 3),

-- Semester 9
('ECON302-EC00', 'Economics for Engineers', 3),
('LEGL302-AC00', 'Law For Engineers', 2),
('PRFE302-EI00', 'Final Year Project Proposal', 1),
('SEMR102-EC00', 'Engineering ethics and professional practice', 0),
('TLCM211-EC10', 'Mobile Communication Networks', 3),
('SEMB211-EC11', 'Microprocessor and Embedded Systems Design', 3),
('SEMB211-EP11', 'Lab. Embedded Systems', 1),
('SYST406-EC10', 'Cloud Computing and Storage', 3),
('SYST505-EC21', 'Advanced Security', 3),
('SYST505-EP01', 'Lab. Security', 1),
('NETW401-EP10', 'Lab. Network Programming', 1),
('PROG501-EC10', 'Distributed Systems', 3),
('SOFT201-EC10', 'Software Engineering and Quality Assurance', 3),
('MLTM304-EP10', 'Lab. Editing and Special Effects', 1),
('PROG502-EC10', 'Computer Vision and Graphics', 3),
('PROG503-EC10', 'Data Analysis and Visualization', 3),

-- Semester 10
('STAP304-ES10', 'Engineer Internship', 1),
('PRFE303-EM10', 'Final Year Project', 3);


-- =====================================================
-- 3. COURSE PREREQUISITES (FROM STUDY GUIDE LOGIC)
-- =====================================================
INSERT INTO course_prerequisite (course_code, prerequisite_course_code) VALUES
-- Programming sequence
('MATH212-EC10', 'MATH211-EC00'),
('MATH213-EC10', 'MATH211-EC00'),
('MATH211-EC00', 'MATH112-EC00'),
('PELE113-EC11', 'PELE111-EC01'),
('PROG112-EC10', 'PROG111-EC01'),
('PROG113-EC10', 'PROG112-EC10'),
('PROG211-EC10', 'PROG112-EC10'),
('SYST101-EC10', 'PROG111-EC01'),
('DBMG106-EC11', 'DBMG105-EC00'),
('NETW206-EC10', 'NETW205-EC00'),
('NETW207-EC10', 'NETW206-EC10'),
('NETW208-EC10', 'NETW206-EC10'),
('MLTM102-EC10', 'PROG111-EC01'),
('ELEC101-EC11', 'PELE111-EC01'),
('ELEC211-EC11', 'PELE111-EC01'),
('ELEC212-EC11', 'ELEC101-EC11'),
('SEMB111-EC11', 'ELEC211-EC11'),
('SEMB111-EC11', 'ELEC212-EC11'),
('SEMB211-EC11', 'ELEC212-EC11'),
('SIGN111-EC10', 'MATH213-EC10'),
('SIGN113-EC11', 'SIGN111-EC10'),
('SIGN211-EC11', 'ELEC211-EC11'),
('SIGN211-EC11', 'ELEC212-EC11'),
('TLCM111-EC11', 'PELE113-EC11'),
('TLCM112-EC10', 'TLCM111-EC11'),
('TLCM113-EC10', 'TLCM111-EC11'),
('TLCM211-EC10', 'NETW208-EC10'),
('DBMG107-EC10', 'DBMG106-EC11'),
('NETW301-EC20', 'NETW208-EC10'),
('NETW301-EC20', 'PROG211-EC10'),
('PROG212-EC10', 'PROG211-EC10'),
('PROG214-EC11', 'PROG112-EC10'),
('PROG302-EC10', 'MLTM102-EC10'),
('PROG304-EC10', 'PROG302-EC10'),
('SYST304-EC20', 'PROG302-EC10'),
('SYST304-EC20', 'PROG212-EC10'),
('SYST402-EC20', 'PROG211-EC10'),
('SYST402-EC20', 'SYST101-EC10'),
('SYST403-EC10', 'SYST107-EC00'),
('SYST404-EP10', 'SYST403-EC10'),
('SYST405-EC10', 'SYST108-EC00'),
('SYST406-EC10', 'SYST405-EC10'),
('SYST505-EC21', 'SYST405-EC10'),
('SYST505-EC21', 'SYST304-EC20'),
('SYST505-EP01', 'SYST107-EC00'),
('DBMG201-EC10', 'PROG211-EC10'),
('NETW401-EP10', 'PROG211-EC10'),
('PROG213-EC10', 'PROG212-EC10'),
('PROG303-EC10', 'PROG302-EC10'),
('PROG401-EC20', 'PROG302-EC10'),
('PROG401-EC20', 'PROG211-EC10'),
('PROG501-EC10', 'PROG213-EC10'),
('SOFT101-EC20', 'PROG212-EC10'),
('SOFT101-EC20', 'PROG302-EC10'),
('SOFT103-EP10', 'PROG211-EC10'),
('SOFT201-EC10', 'PROG212-EC10'),
('INFG203-EC10', 'MLTM102-EC10'),
('MLTM203-EC10', 'MLTM202-EC10'),
('MLTM303-EP10', 'MLTM302-EP10'),
('MLTM304-EP10', 'MLTM303-EP10'),
('PROG305-EC10', 'PROG302-EC10'),
('PROG502-EC10', 'MLTM202-EC10'),
('SOFT102-EC20', 'PROG212-EC10'),
('SOFT102-EC20', 'PROG302-EC10'),
('PROG503-EC10', 'PROG302-EC10'),
('COMM402-EC10', 'COMM300-EC00'),
('ENGI103-EC10', 'COMM402-EC10'),
('PRFE302-EI00', 'STAP303-EC10'),
('PRFE302-EI00', 'PJMG101-EC00'),
('PRFE303-EM10', 'PRFE302-EI00'),
('STAP304-ES10', 'STAP303-EC10');

-- =====================================================
-- 4. STUDY GUIDE (PROGRAM STRUCTURE)
-- =====================================================
INSERT INTO study_guide (program_code, course_code, recommended_semester) VALUES
-- Semester 1
('CCE_SYST', 'ENGI102-EC00', 1),
('CCE_SOFT', 'ENGI102-EC00', 1),
('CCE_MLTM', 'ENGI102-EC00', 1),
('CCE_TLCM', 'ENGI102-EC00', 1),
('CCE_SYST', 'PELE111-EC01', 1),
('CCE_SOFT', 'PELE111-EC01', 1),
('CCE_MLTM', 'PELE111-EC01', 1),
('CCE_TLCM', 'PELE111-EC01', 1),
('CCE_SYST', 'PELE111-EP01', 1),
('CCE_SOFT', 'PELE111-EP01', 1),
('CCE_MLTM', 'PELE111-EP01', 1),
('CCE_TLCM', 'PELE111-EP01', 1),
('CCE_SYST', 'PROG111-EC01', 1),
('CCE_SOFT', 'PROG111-EC01', 1),
('CCE_MLTM', 'PROG111-EC01', 1),
('CCE_TLCM', 'PROG111-EC01', 1),
('CCE_SYST', 'PROG111-EP01', 1),
('CCE_SOFT', 'PROG111-EP01', 1),
('CCE_MLTM', 'PROG111-EP01', 1),
('CCE_TLCM', 'PROG111-EP01', 1),
('CCE_SYST', 'MATH111-EC00', 1),
('CCE_SOFT', 'MATH111-EC00', 1),
('CCE_MLTM', 'MATH111-EC00', 1),
('CCE_TLCM', 'MATH111-EC00', 1),
('CCE_SYST', 'MATH211-EC00', 1),
('CCE_SOFT', 'MATH211-EC00', 1),
('CCE_MLTM', 'MATH211-EC00', 1),
('CCE_TLCM', 'MATH211-EC00', 1),
('CCE_SYST', 'NETW101-EP00', 1),
('CCE_SOFT', 'NETW101-EP00', 1),
('CCE_MLTM', 'NETW101-EP00', 1),
('CCE_TLCM', 'NETW101-EP00', 1),


-- Semester 2
('CCE_SYST', 'SCOP202-AC00', 2),
('CCE_SOFT', 'SCOP202-AC00', 2),
('CCE_MLTM', 'SCOP202-AC00', 2),
('CCE_TLCM', 'SCOP202-AC00', 2),
('CCE_SYST', 'PELE112-EC00', 2),
('CCE_SOFT', 'PELE112-EC00', 2),
('CCE_MLTM', 'PELE112-EC00', 2),
('CCE_TLCM', 'PELE112-EC00', 2),
('CCE_SYST', 'PROG112-EC10', 2),
('CCE_SOFT', 'PROG112-EC10', 2),
('CCE_MLTM', 'PROG112-EC10', 2),
('CCE_TLCM', 'PROG112-EC10', 2),
('CCE_SYST', 'MATH112-EC00', 2),
('CCE_SOFT', 'MATH112-EC00', 2),
('CCE_MLTM', 'MATH112-EC00', 2),
('CCE_TLCM', 'MATH112-EC00', 2),
('CCE_SYST', 'MATH212-EC10', 2),
('CCE_SOFT', 'MATH212-EC10', 2),
('CCE_MLTM', 'MATH212-EC10', 2),
('CCE_TLCM', 'MATH212-EC10', 2),
('CCE_SYST', 'NETW205-EC00', 2),
('CCE_SOFT', 'NETW205-EC00', 2),
('CCE_MLTM', 'NETW205-EC00', 2),
('CCE_TLCM', 'NETW205-EC00', 2),
('CCE_SYST', 'ENGI101-EP00', 2),
('CCE_SOFT', 'ENGI101-EP00', 2),
('CCE_MLTM', 'ENGI101-EP00', 2),
('CCE_TLCM', 'ENGI101-EP00', 2),

-- Semester 3
('CCE_SYST', 'PELE113-EC11', 3),
('CCE_SOFT', 'PELE113-EC11', 3),
('CCE_MLTM', 'PELE113-EC11', 3),
('CCE_TLCM', 'PELE113-EC11', 3),
('CCE_SYST', 'PELE113-EP01', 3),
('CCE_SOFT', 'PELE113-EP01', 3),
('CCE_MLTM', 'PELE113-EP01', 3),
('CCE_TLCM', 'PELE113-EP01', 3),
('CCE_SYST', 'PROG113-EC10', 3),
('CCE_SOFT', 'PROG113-EC10', 3),
('CCE_MLTM', 'PROG113-EC10', 3),
('CCE_TLCM', 'PROG113-EC10', 3),
('CCE_SYST', 'MATH213-EC10', 3),
('CCE_SOFT', 'MATH213-EC10', 3),
('CCE_MLTM', 'MATH213-EC10', 3),
('CCE_TLCM', 'MATH213-EC10', 3),
('CCE_SYST', 'NETW206-EC10', 3),
('CCE_SOFT', 'NETW206-EC10', 3),
('CCE_MLTM', 'NETW206-EC10', 3),
('CCE_TLCM', 'NETW206-EC10', 3),
('CCE_SYST', 'SYST107-EC00', 3),
('CCE_SOFT', 'SYST107-EC00', 3),
('CCE_MLTM', 'SYST107-EC00', 3),
('CCE_TLCM', 'SYST107-EC00', 3),
('CCE_SYST', 'MLTM102-EC10', 3),
('CCE_SOFT', 'MLTM102-EC10', 3),
('CCE_MLTM', 'MLTM102-EC10', 3),
('CCE_TLCM', 'MLTM102-EC10', 3),

-- Semester 4
('CCE_SYST', 'COMM300-EC00', 4),
('CCE_SOFT', 'COMM300-EC00', 4),
('CCE_MLTM', 'COMM300-EC00', 4),
('CCE_TLCM', 'COMM300-EC00', 4),
('CCE_SYST', 'PROG211-EC10', 4),
('CCE_SOFT', 'PROG211-EC10', 4),
('CCE_MLTM', 'PROG211-EC10', 4),
('CCE_TLCM', 'PROG211-EC10', 4),
('CCE_SYST', 'MATH402-EC00', 4),
('CCE_SOFT', 'MATH402-EC00', 4),
('CCE_MLTM', 'MATH402-EC00', 4),
('CCE_TLCM', 'MATH402-EC00', 4),
('CCE_SYST', 'MATH302-EC00', 4),
('CCE_SOFT', 'MATH302-EC00', 4),
('CCE_MLTM', 'MATH302-EC00', 4),
('CCE_TLCM', 'MATH302-EC00', 4),
('CCE_SYST', 'NETW207-EC10', 4),
('CCE_SOFT', 'NETW207-EC10', 4),
('CCE_MLTM', 'NETW207-EC10', 4),
('CCE_TLCM', 'NETW207-EC10', 4),
('CCE_SYST', 'DBMG105-EC00', 4),
('CCE_SOFT', 'DBMG105-EC00', 4),
('CCE_MLTM', 'DBMG105-EC00', 4),
('CCE_TLCM', 'DBMG105-EC00', 4),

-- Semester 5
('CCE_SYST', 'ELEC101-EC11', 5),
('CCE_SOFT', 'ELEC101-EC11', 5),
('CCE_MLTM', 'ELEC101-EC11', 5),
('CCE_TLCM', 'ELEC101-EC11', 5),
('CCE_SYST', 'SYST101-EC10', 5),
('CCE_SOFT', 'SYST101-EC10', 5),
('CCE_MLTM', 'SYST101-EC10', 5),
('CCE_TLCM', 'SYST101-EC10', 5),
('CCE_SYST', 'ELEC102-EP01', 5),
('CCE_SOFT', 'ELEC102-EP01', 5),
('CCE_MLTM', 'ELEC102-EP01', 5),
('CCE_TLCM', 'ELEC102-EP01', 5),
('CCE_SYST', 'DBMG106-EC11', 5),
('CCE_SOFT', 'DBMG106-EC11', 5),
('CCE_MLTM', 'DBMG106-EC11', 5),
('CCE_TLCM', 'DBMG106-EC11', 5),
('CCE_TLCM', 'TLCM111-EC11', 5),
('CCE_TLCM', 'TLCM111-EP11', 5),
('CCE_TLCM', 'SIGN111-EC10', 5),
('CCE_SYST', 'PROG212-EC10', 5),
('CCE_SYST', 'PROG302-EC10', 5),
('CCE_SOFT', 'PROG212-EC10', 5),
('CCE_SOFT', 'PROG302-EC10', 5),
('CCE_SOFT', 'SOFT103-EP10', 5),
('CCE_MLTM', 'PROG212-EC10', 5),
('CCE_MLTM', 'PROG302-EC10', 5),
('CCE_MLTM', 'MLTM302-EP10', 5),

-- Semester 6
('CCE_SYST', 'COMM402-EC10', 6),
('CCE_SOFT', 'COMM402-EC10', 6),
('CCE_MLTM', 'COMM402-EC10', 6),
('CCE_TLCM', 'COMM402-EC10', 6),
('CCE_SYST', 'SYST202-EC00', 6),
('CCE_SOFT', 'SYST202-EC00', 6),
('CCE_MLTM', 'SYST202-EC00', 6),
('CCE_TLCM', 'SYST202-EC00', 6),
('CCE_SYST', 'MATH403-EP20', 6),
('CCE_SOFT', 'MATH403-EP20', 6),
('CCE_MLTM', 'MATH403-EP20', 6),
('CCE_TLCM', 'MATH403-EP20', 6),
('CCE_SYST', 'NETW208-EC10', 6),
('CCE_SOFT', 'NETW208-EC10', 6),
('CCE_MLTM', 'NETW208-EC10', 6),
('CCE_TLCM', 'NETW208-EC10', 6),
('CCE_SYST', 'SYST108-EC00', 6),
('CCE_SOFT', 'SYST108-EC00', 6),
('CCE_MLTM', 'SYST108-EC00', 6),
('CCE_TLCM', 'SYST108-EC00', 6),
('CCE_SYST', 'STAP303-EC10', 6),
('CCE_SOFT', 'STAP303-EC10', 6),
('CCE_MLTM', 'STAP303-EC10', 6),
('CCE_TLCM', 'STAP303-EC10', 6),
('CCE_TLCM', 'ELEC212-EC11', 6),
('CCE_SYST', 'PROG214-EC11', 6),
('CCE_SYST', 'PROG214-EP01', 6),
('CCE_SOFT', 'PROG214-EC11', 6),
('CCE_SOFT', 'PROG214-EP01', 6),
('CCE_MLTM', 'MLTM303-EP10', 6),
('CCE_MLTM', 'INFG203-EC10', 6),

-- Semester 7
('CCE_SYST', 'ENGI103-EC10', 7),
('CCE_SOFT', 'ENGI103-EC10', 7),
('CCE_MLTM', 'ENGI103-EC10', 7),
('CCE_TLCM', 'ENGI103-EC10', 7),
('CCE_TLCM', 'TLCM112-EC10', 7),
('CCE_TLCM', 'SIGN113-EC11', 7),
('CCE_TLCM', 'SIGN113-EP11', 7),
('CCE_TLCM', 'ELEC211-EC11', 7),
('CCE_TLCM', 'ELEC211-EP11', 7),
('CCE_SYST', 'SYST304-EC20', 7),
('CCE_SYST', 'SYST402-EC20', 7),
('CCE_SYST', 'SYST403-EC10', 7),
('CCE_SYST', 'DBMG107-EC10', 7),
('CCE_SOFT', 'SYST304-EC20', 7),
('CCE_SOFT', 'PROG303-EC10', 7),
('CCE_SOFT', 'DBMG201-EC10', 7),
('CCE_SOFT', 'PROG213-EC10', 7),
('CCE_MLTM', 'SYST304-EC20', 7),
('CCE_MLTM', 'DBMG201-EC10', 7),
('CCE_MLTM', 'SOFT102-EC20', 7),
('CCE_MLTM', 'MLTM202-EC10', 7),

-- Semester 8
('CCE_SYST', 'PJMG101-EC00', 8),
('CCE_SOFT', 'PJMG101-EC00', 8),
('CCE_MLTM', 'PJMG101-EC00', 8),
('CCE_TLCM', 'PJMG101-EC00', 8),
('CCE_SYST', 'MRCH501-ES00', 8),
('CCE_SOFT', 'MRCH501-ES00', 8),
('CCE_MLTM', 'MRCH501-ES00', 8),
('CCE_TLCM', 'MRCH501-ES00', 8),
('CCE_TLCM', 'TLCM113-EC10', 8),
('CCE_TLCM', 'SIGN211-EC11', 8),
('CCE_TLCM', 'SIGN211-EP11', 8),
('CCE_TLCM', 'SEMB111-EC11', 8),
('CCE_TLCM', 'SEMB111-EP11', 8),
('CCE_SYST', 'NETW301-EC20', 8),
('CCE_SYST', 'SYST405-EC10', 8),
('CCE_SYST', 'SYST404-EP10', 8),
('CCE_SYST', 'PROG304-EC10', 8),
('CCE_SOFT', 'PROG304-EC10', 8),
('CCE_SOFT', 'PROG401-EC20', 8),
('CCE_SOFT', 'SOFT101-EC20', 8),
('CCE_MLTM', 'PROG401-EC20', 8),
('CCE_MLTM', 'PROG305-EC10', 8),
('CCE_MLTM', 'MLTM203-EC10', 8),

-- Semester 9
('CCE_SYST', 'ECON302-EC00', 9),
('CCE_SOFT', 'ECON302-EC00', 9),
('CCE_MLTM', 'ECON302-EC00', 9),
('CCE_TLCM', 'ECON302-EC00', 9),
('CCE_SYST', 'LEGL302-AC00', 9),
('CCE_SOFT', 'LEGL302-AC00', 9),
('CCE_MLTM', 'LEGL302-AC00', 9),
('CCE_TLCM', 'LEGL302-AC00', 9),
('CCE_SYST', 'PRFE302-EI00', 9),
('CCE_SOFT', 'PRFE302-EI00', 9),
('CCE_MLTM', 'PRFE302-EI00', 9),
('CCE_TLCM', 'PRFE302-EI00', 9),
('CCE_SYST', 'SEMR102-EC00', 9),
('CCE_SOFT', 'SEMR102-EC00', 9),
('CCE_MLTM', 'SEMR102-EC00', 9),
('CCE_TLCM', 'SEMR102-EC00', 9),
('CCE_TLCM', 'TLCM211-EC10', 9),
('CCE_TLCM', 'SEMB211-EC11', 9),
('CCE_TLCM', 'SEMB211-EP11', 9),
('CCE_SYST', 'SYST406-EC10', 9),
('CCE_SYST', 'SYST505-EC21', 9),
('CCE_SYST', 'SYST505-EP01', 9),
('CCE_SOFT', 'NETW401-EP10', 9),
('CCE_SOFT', 'PROG501-EC10', 9),
('CCE_SOFT', 'SOFT201-EC10', 9),
('CCE_MLTM', 'MLTM304-EP10', 9),
('CCE_MLTM', 'PROG502-EC10', 9),
('CCE_MLTM', 'PROG503-EC10', 9),

-- Semester 10
('CCE_SYST', 'STAP304-ES10', 10),
('CCE_SOFT', 'STAP304-ES10', 10),
('CCE_MLTM', 'STAP304-ES10', 10),
('CCE_TLCM', 'STAP304-ES10', 10),
('CCE_SYST', 'PRFE303-EM10', 10),
('CCE_SOFT', 'PRFE303-EM10', 10),
('CCE_MLTM', 'PRFE303-EM10', 10),
('CCE_TLCM', 'PRFE303-EM10', 10);

-- =====================================================
-- 5. COURSE GRADING SCHEMA (ACADEMIC DEFAULT)
-- =====================================================
INSERT INTO course_grading_schema (course_code, component_name, weight_percentage)
SELECT course_code,'MIDTERM',30 FROM sis_course WHERE credits >= 3
UNION ALL
SELECT course_code,'PROJECT',10 FROM sis_course WHERE credits >= 3
UNION ALL
SELECT course_code,'FINAL',40 FROM sis_course WHERE credits >= 3
UNION ALL
SELECT course_code,'LAB',20 FROM sis_course WHERE credits = 3;

-- Labs / 1-credit courses
INSERT INTO course_grading_schema (course_code, component_name, weight_percentage)
SELECT course_code,'LAB',100 FROM sis_course WHERE credits = 1;


-- =====================================================
-- 6. STUDENTS (DUMMY SIS DATA)
-- =====================================================
INSERT INTO sis_student (
    student_id, first_name, last_name,
    program_code, current_semester,
    current_gpa, academic_status, advisor_id
)
SELECT
    (year * 100000 + seq) AS student_id,
    'Student' || seq,
    'Year' || year,
    CASE (seq % 4)
        WHEN 0 THEN 'CCE_SYST'
        WHEN 1 THEN 'CCE_SOFT'
        WHEN 2 THEN 'CCE_MLTM'
        ELSE 'CCE_TLCM'
    END,
    CASE
        WHEN year = 2021 THEN 8
        WHEN year = 2022 THEN 6
        WHEN year = 2023 THEN 4
        WHEN year = 2024 THEN 2
        ELSE 1
    END,
    ROUND((RANDOM() * 2 + 2)::numeric,2),
    CASE WHEN RANDOM() > 0.8 THEN 'PROBATION' ELSE 'NORMAL' END,
    (seq % 10) + 1
FROM generate_series(2021,2026) AS year,
     generate_series(10001,10010) AS seq;


-- =====================================================
-- 7. STUDENT COURSE HISTORY (DUMMY SIS)
-- =====================================================
INSERT INTO sis_student_course_history
(student_id, course_code, semester, final_grade, status)
SELECT
    s.student_id,
    sg.course_code,
    'Semester ' || sg.recommended_semester,
    (RANDOM()*40+50)::INT,
    CASE WHEN RANDOM() > 0.25 THEN 'PASSED' ELSE 'FAILED' END
FROM sis_student s
JOIN study_guide sg
  ON sg.program_code = s.program_code
WHERE sg.recommended_semester < s.current_semester;


-- =====================================================
-- 8. CURRENT ENROLLMENT (DUMMY SIS)
-- =====================================================
INSERT INTO sis_current_enrollment
(student_id, course_code, semester)
SELECT
    s.student_id,
    sg.course_code,
    'Semester ' || s.current_semester
FROM sis_student s
JOIN study_guide sg
  ON sg.program_code = s.program_code
 AND sg.recommended_semester = s.current_semester;

-- =====================================================
-- 9. COURSE ASSESSMENT (ATTENDANCE – DUMMY SIS)
-- =====================================================
INSERT INTO sis_course_assessment
(student_id, course_code, course_credits,
 absences_count, max_absences,
 semester_start_date, semester_end_date)
SELECT
    e.student_id,
    e.course_code,
    c.credits,
    (RANDOM()*5)::INT,
    CASE WHEN c.credits = 3 THEN 9 ELSE 3 END,
    '2025-02-01',
    '2025-06-01'
FROM sis_current_enrollment e
JOIN sis_course c ON c.course_code = e.course_code;


-- =====================================================
-- 10. STUDENT GRADES (DUMMY SIS)
-- =====================================================
INSERT INTO sis_student_grades
(student_id, course_code, component_name, grade)
SELECT
    e.student_id,
    e.course_code,
    g.component_name,
    (RANDOM()*20+5)::INT
FROM sis_current_enrollment e
JOIN course_grading_schema g
  ON g.course_code = e.course_code;



