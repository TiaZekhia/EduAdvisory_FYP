using EduAdvisory_Backend.DTOs.Course;
using EduAdvisory_Backend.DTOs.Student;
using EduAdvisory_Backend.Interfaces.Repositories;
using EduAdvisory_Backend.Models;
using Microsoft.EntityFrameworkCore;


namespace EduAdvisory_Backend.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly EduAdvisoryDbContext _context;

        public StudentRepository(EduAdvisoryDbContext context)
        {
            _context = context;
        }

        public SisStudent GetById(int studentId)
        {
            return _context.SisStudents
                .FirstOrDefault(s => s.StudentId == studentId);
        }

        public List<SisStudentCourseHistory> GetCourseHistory(int studentId)
        {
            return _context.SisStudentCourseHistories
                .Where(h => h.StudentId == studentId)
                .ToList();
        }

        public List<SisCurrentEnrollment> GetCurrentEnrollment(int studentId)
        {
            return _context.SisCurrentEnrollments
                .Where(e => e.StudentId == studentId)
                .ToList();
        }

        public SisStudent GetByUsername(string username)
        {
            var user = _context.Users
                .FirstOrDefault(u => u.Username == username);

            if (user == null || user.LinkedStudentId == null)
                return null;

            return _context.SisStudents
                .FirstOrDefault(s => s.StudentId == user.LinkedStudentId);
        }

        public List<CurrentEnrollmentCourseDto> GetCurrentEnrollmentWithCourse(int studentId)
        {
            return _context.SisCurrentEnrollments
                .Where(e => e.StudentId == studentId)
                .Join(_context.SisCourses,
                      e => e.CourseCode,
                      c => c.CourseCode,
                      (e, c) => new CurrentEnrollmentCourseDto
                      {
                          CourseCode = c.CourseCode,
                          CourseName = c.CourseName,
                          Credits = c.Credits,
                          Semester = e.Semester
                      })
                .ToList();
        }

        public List<CurrentCoursePerformanceDto> GetCurrentCoursesPerformance(int studentId)
        {
            // 1) Current courses with course info
            var currentCourses = _context.SisCurrentEnrollments
                .Where(e => e.StudentId == studentId)
                .Join(_context.SisCourses,
                      e => e.CourseCode,
                      c => c.CourseCode,
                      (e, c) => new { c.CourseCode, c.CourseName, c.Credits })
                .ToList();

            var courseCodes = currentCourses.Select(x => x.CourseCode).ToList();

            // 2) Assessments (absences)
            var assessments = _context.SisCourseAssessments
                .Where(a => a.StudentId == studentId && courseCodes.Contains(a.CourseCode))
                .ToList();

            // 3) Grades (components)
            var grades = _context.SisStudentGrades
                .Where(g => g.StudentId == studentId && courseCodes.Contains(g.CourseCode))
                .ToList();

            // 4) Merge
            var result = currentCourses.Select(c =>
            {
                var assess = assessments.FirstOrDefault(a => a.CourseCode == c.CourseCode);

                var comps = grades
                    .Where(g => g.CourseCode == c.CourseCode)
                    .Select(g => new CourseComponentGradeDto
                    {
                        ComponentName = g.ComponentName,
                        Grade = g.Grade
                    })
                    .ToList();

                return new CurrentCoursePerformanceDto
                {
                    CourseCode = c.CourseCode,
                    CourseName = c.CourseName,
                    Credits = c.Credits,
                    AbsencesCount = assess?.AbsencesCount,
                    MaxAbsences = assess?.MaxAbsences,
                    Components = comps
                };
            }).ToList();

            return result;
        }

        public StudentStatsDto GetStudentStats(int studentId)
        {
            // Completed + Failed from history
            var history = _context.SisStudentCourseHistories
                .Where(h => h.StudentId == studentId)
                .ToList();

            var completedCodes = history
                .Where(h => h.Status == "PASSED")
                .Select(h => h.CourseCode)
                .Distinct()
                .ToList();

            var failedCodes = history
                .Where(h => h.Status == "FAILED")
                .Select(h => h.CourseCode)
                .Distinct()
                .ToList();

            // Credits earned = sum credits for passed courses
            var creditsEarned = _context.SisCourses
                .Where(c => completedCodes.Contains(c.CourseCode))
                .Sum(c => c.Credits);

            return new StudentStatsDto
            {
                CompletedCourses = completedCodes.Count,
                FailedCourses = failedCodes.Count,
                CreditsEarned = creditsEarned
            };
        }

        public DegreeProgressDto GetDegreeProgress(int studentId)
        {
            var student = GetById(studentId);
            if (student == null) return null;

            var stats = GetStudentStats(studentId);

            // Credits required = sum of all study guide course credits for this program
            var requiredCodes = _context.StudyGuides
                .Where(sg => sg.ProgramCode == student.ProgramCode)
                .Select(sg => sg.CourseCode)
                .Distinct()
                .ToList();

            var creditsRequired = _context.SisCourses
                .Where(c => requiredCodes.Contains(c.CourseCode))
                .Sum(c => c.Credits);

            decimal percent = creditsRequired > 0
                ? Math.Round(((decimal)stats.CreditsEarned / creditsRequired) * 100m, 1)
                : 0m;

            return new DegreeProgressDto
            {
                CreditsEarned = stats.CreditsEarned,
                CreditsRequired = creditsRequired,
                PercentComplete = percent
            };
        }

        public int GetUpcomingMeetingsCount(int studentId)
        {
            var now = DateTime.UtcNow; // or DateTime.Now if your DB uses local time
            return _context.Meetings
                .Count(m => m.StudentId == studentId && m.MeetingDate >= now);
        }
    }

}
