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

            var failedNotRetaken = failedCourses
                .Where(fc => !passedCourses.Contains(fc))
                .ToList();

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

        public AdvisorStudentAnalysisDto AnalyzeStudentForAdvisor(int studentId)
        {
            var student = _studentRepo.GetById(studentId);
            if (student == null)
                throw new Exception("Student not found.");

            var currentSemester = student.CurrentSemester ?? 0;
            var programCode = student.ProgramCode ?? "";

            var passedCourses = _studentRepo.GetPassedCourses(studentId).ToHashSet();
            var failedNotRetaken = _studentRepo.GetFailedNotRetakenCourses(studentId).ToHashSet();
            var currentEnrollment = _studentRepo.GetCurrentEnrollmentWithCourse(studentId);
            var currentEnrollmentCodes = currentEnrollment.Select(c => c.CourseCode).ToHashSet();

            var recommendedMap = _studentRepo.GetStudyGuideRecommendedSemester(programCode);

            var allRelevantCodes = recommendedMap.Keys
                .Union(failedNotRetaken)
                .Union(currentEnrollmentCodes)
                .ToList();

            var coursesMeta = _studentRepo.GetCoursesMeta(allRelevantCodes);
            var prereqMap = _studentRepo.GetPrerequisitesMap(recommendedMap.Keys.ToList());

            var progress = _studentRepo.GetProgressSummary(studentId);

            var missingCourses = recommendedMap
                .Where(kvp =>
                    kvp.Value <= currentSemester &&
                    !passedCourses.Contains(kvp.Key) &&
                    !currentEnrollmentCodes.Contains(kvp.Key))
                .Select(kvp =>
                {
                    var courseCode = kvp.Key;
                    var recommendedSemester = kvp.Value;

                    var meta = coursesMeta.ContainsKey(courseCode)
                        ? coursesMeta[courseCode]
                        : (courseCode, 0);

                    var prerequisites = prereqMap.ContainsKey(courseCode)
                        ? prereqMap[courseCode]
                        : new List<string>();

                    var blockedCount = prereqMap.Count(x => x.Value.Contains(courseCode));

                    var priority =
                        blockedCount >= 2 ? "HIGH" :
                        recommendedSemester < currentSemester ? "MEDIUM" :
                        "LOW";

                    var reason =
                        recommendedSemester < currentSemester
                            ? $"Behind schedule (expected in semester {recommendedSemester})"
                            : "Expected this semester";

                    return new AdvisorMissingCourseDto
                    {
                        CourseCode = courseCode,
                        CourseName = meta.Item1,
                        RecommendedSemester = recommendedSemester,
                        Reason = reason,
                        Priority = priority,
                        Prerequisites = prerequisites
                    };
                })
                .OrderByDescending(x => x.Priority == "HIGH")
                .ThenByDescending(x => x.Priority == "MEDIUM")
                .ThenBy(x => x.RecommendedSemester)
                .ToList();

            var history = _studentRepo.GetCourseHistory(studentId);

            var failedCourses = history
                .Where(h => h.Status == "FAILED")
                .GroupBy(h => h.CourseCode)
                .Select(g =>
                {
                    var latest = g
                        .OrderByDescending(x => x.Semester)
                        .First();

                    var code = latest.CourseCode;

                    var meta = coursesMeta.ContainsKey(code)
                        ? coursesMeta[code]
                        : (code, 0);

                    var isRetaken = passedCourses.Contains(code) || currentEnrollmentCodes.Contains(code);

                    var retakeStatus =
                        passedCourses.Contains(code) ? "PASSED_LATER" :
                        currentEnrollmentCodes.Contains(code) ? "RETAKEN" :
                        "NOT_RETAKEN";

                    return new AdvisorFailedCourseDto
                    {
                        CourseCode = code,
                        CourseName = meta.Item1,
                        Semester = latest.Semester ?? "",
                        IsRetaken = isRetaken,
                        RetakeStatus = retakeStatus
                    };
                })
                .ToList();

            var currentEnrollmentDtos = currentEnrollment
                .Select(c => new AdvisorCurrentEnrollmentCourseDto
                {
                    CourseCode = c.CourseCode,
                    CourseName = c.CourseName,
                    Credits = c.Credits,
                    Status = "ENROLLED"
                })
                .ToList();

            var studyGuide = _studyGuideRepo.GetByProgram(programCode);
            var totalProgramCredits = studyGuide
                .Select(sg => sg.CourseCode)
                .Distinct()
                .ToList();

            var totalProgramCreditsValue = _studentRepo
                .GetCoursesMeta(totalProgramCredits)
                .Values
                .Sum(x => x.credits);

            return new AdvisorStudentAnalysisDto
            {
                StudentId = student.StudentId,
                StudentName = $"{student.FirstName} {student.LastName}".Trim(),
                ProgramCode = programCode,
                CurrentSemester = currentSemester,
                CurrentGpa = student.CurrentGpa ?? 0,
                AcademicStatus = student.AcademicStatus ?? "NORMAL",
                IsOnTrack = !missingCourses.Any() && !failedCourses.Any(f => f.RetakeStatus == "NOT_RETAKEN"),
                CompletedCredits = progress?.CreditsEarned ?? 0,
                TotalProgramCredits = totalProgramCreditsValue,
                CurrentEnrollment = currentEnrollmentDtos,
                FailedCourses = failedCourses,
                MissingCourses = missingCourses
            };
        }
    }
}