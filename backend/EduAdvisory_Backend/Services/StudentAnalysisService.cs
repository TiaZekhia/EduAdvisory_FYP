using EduAdvisory_Backend.DTOs.Student;
using EduAdvisory_Backend.Interfaces.Repositories;
using EduAdvisory_Backend.Interfaces.Services;

namespace EduAdvisory_Backend.Services
{
    public class StudentAnalysisService : IStudentAnalysisService
    {
        private readonly IStudentRepository _studentRepo;
        private readonly IStudyGuideRepository _studyGuideRepo;
        private readonly ICoursePrerequisiteRepository _prereqRepo;

        public StudentAnalysisService(
            IStudentRepository studentRepo,
            IStudyGuideRepository studyGuideRepo,
            ICoursePrerequisiteRepository prereqRepo)
        {
            _studentRepo = studentRepo;
            _studyGuideRepo = studyGuideRepo;
            _prereqRepo = prereqRepo;
        }

        public StudentAnalysisDto AnalyzeStudent(int studentId)
        {
            var student = _studentRepo.GetById(studentId);

            var studyGuide = _studyGuideRepo
                .GetByProgram(student.ProgramCode);

            var history = _studentRepo.GetCourseHistory(studentId);
            var current = _studentRepo.GetCurrentEnrollment(studentId);

            var passedCourses = history
                .Where(h => h.Status == "PASSED")
                .Select(h => h.CourseCode)
                .ToHashSet();

            var failedCourses = history
                .Where(h => h.Status == "FAILED")
                .Select(h => h.CourseCode)
                .ToHashSet();

            // 1️⃣ Missing current semester courses
            var expectedCurrent = studyGuide
                .Where(sg => sg.RecommendedSemester == student.CurrentSemester)
                .Select(sg => sg.CourseCode)
                .ToList();

            var currentEnrolled = current
                .Select(c => c.CourseCode)
                .ToHashSet();

            var missingCurrent = expectedCurrent
                .Where(c => !currentEnrolled.Contains(c))
                .ToList();

            // 2️⃣ Failed but not retaken
            var failedNotRetaken = failedCourses
                .Where(fc => !passedCourses.Contains(fc))
                .ToList();

            // 3️⃣ Blocking prerequisite courses
            var prereqs = _prereqRepo.GetAll();

            var blocking = prereqs
                .Where(p =>
                    !passedCourses.Contains(p.PrerequisiteCourseCode) &&
                    studyGuide.Any(sg => sg.CourseCode == p.CourseCode))
                .Select(p => p.PrerequisiteCourseCode)
                .Distinct()
                .ToList();

            return new StudentAnalysisDto
            {
                StudentId = studentId,
                IsOnTrack = !missingCurrent.Any() && !failedNotRetaken.Any(),
                MissingCurrentSemesterCourses = missingCurrent,
                FailedNotRetakenCourses = failedNotRetaken,
                BlockingCourses = blocking
            };
        }
    }

}
