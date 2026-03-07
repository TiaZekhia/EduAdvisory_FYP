using EduAdvisory_Backend.DTOs.Course;
using EduAdvisory_Backend.DTOs.Messages;
using EduAdvisory_Backend.DTOs.Student;
using EduAdvisory_Backend.Interfaces.Repositories;
using EduAdvisory_Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;


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

        private static string SeverityFrom(double score)
        {
            if (score >= 0.7) return "HIGH";
            if (score >= 0.5) return "MEDIUM";
            return "LOW";
        }

        public List<StudentAlertDto> GetStudentAlerts(int studentId, int limit = 5)
        {
            var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

            // Current enrolled courses
            var currentCourses = _context.SisCurrentEnrollments
                .Where(e => e.StudentId == studentId)
                .Join(_context.SisCourses,
                      e => e.CourseCode,
                      c => c.CourseCode,
                      (e, c) => new { c.CourseCode, c.CourseName, c.Credits })
                .ToList();

            var codes = currentCourses.Select(x => x.CourseCode).ToList();

            // Assessments (absences)
            var assessments = _context.SisCourseAssessments
                .Where(a => a.StudentId == studentId && codes.Contains(a.CourseCode))
                .ToList();

            // Grades
            var grades = _context.SisStudentGrades
                .Where(g => g.StudentId == studentId && codes.Contains(g.CourseCode))
                .ToList();

            var alerts = new List<StudentAlertDto>();

            // A) Absence alerts
            foreach (var a in assessments)
            {
                if (a.MaxAbsences == null || a.MaxAbsences == 0) continue;

                var ratio = (double)(a.AbsencesCount ?? 0) / (double)a.MaxAbsences;
                if (ratio < 0.5) continue; // no alert

                var sev = SeverityFrom(ratio);
                alerts.Add(new StudentAlertDto
                {
                    Severity = sev,
                    Title = "Attendance risk",
                    Message = $"You have {a.AbsencesCount}/{a.MaxAbsences} absences. You may be at risk in this course.",
                    CourseCode = a.CourseCode,
                    CreatedAt = now
                });
            }

            // B) Midterm alerts (if MIDTERM exists)
            foreach (var code in codes)
            {
                var mid = grades
                    .FirstOrDefault(g => g.CourseCode == code && g.ComponentName == "MIDTERM");

                if (mid?.Grade == null) continue;

                if (mid.Grade < 50)
                {
                    alerts.Add(new StudentAlertDto
                    {
                        Severity = "HIGH",
                        Title = "Low midterm grade",
                        Message = $"Your MIDTERM grade is {mid.Grade}/100. You may need immediate action.",
                        CourseCode = code,
                        CreatedAt = now
                    });
                }
                else if (mid.Grade < 60)
                {
                    alerts.Add(new StudentAlertDto
                    {
                        Severity = "MEDIUM",
                        Title = "Midterm needs improvement",
                        Message = $"Your MIDTERM grade is {mid.Grade}/100. Consider extra study to improve.",
                        CourseCode = code,
                        CreatedAt = now
                    });
                }
            }

            // C) Analysis-based alerts (optional but valuable)
            // If you want to keep repository clean, you can compute it in service instead.
            // For simplicity, do minimal compute here:

            var student = GetById(studentId);
            if (student != null)
            {
                // Missing current semester guide courses
                var expected = _context.StudyGuides
                    .Where(sg => sg.ProgramCode == student.ProgramCode && sg.RecommendedSemester == student.CurrentSemester)
                    .Select(sg => sg.CourseCode)
                    .ToList();

                var enrolled = codes.ToHashSet();
                var missing = expected.Where(x => !enrolled.Contains(x)).ToList();

                if (missing.Any())
                {
                    alerts.Add(new StudentAlertDto
                    {
                        Severity = "MEDIUM",
                        Title = "Missing recommended courses",
                        Message = $"You are missing {missing.Count} recommended course(s) for this semester.",
                        CourseCode = null,
                        CreatedAt = now
                    });
                }

                // Failed but not retaken
                var history = _context.SisStudentCourseHistories
                    .Where(h => h.StudentId == studentId)
                    .ToList();

                var passed = history.Where(h => h.Status == "PASSED").Select(h => h.CourseCode).ToHashSet();
                var failed = history.Where(h => h.Status == "FAILED").Select(h => h.CourseCode).ToHashSet();

                var failedNotRetaken = failed.Where(fc => !passed.Contains(fc)).ToList();
                if (failedNotRetaken.Any())
                {
                    alerts.Add(new StudentAlertDto
                    {
                        Severity = "HIGH",
                        Title = "Failed course not retaken",
                        Message = $"You have {failedNotRetaken.Count} failed course(s) not retaken yet.",
                        CourseCode = null,
                        CreatedAt = now
                    });
                }
            }

            // Order by severity then return top N
            int severityRank(string s) => s == "HIGH" ? 3 : s == "MEDIUM" ? 2 : 1;

            return alerts
                .OrderByDescending(a => severityRank(a.Severity))
                .ThenByDescending(a => a.CreatedAt)
                .Take(limit)
                .ToList();
        }

        public StudentAlertsCountDto GetStudentAlertsCount(int studentId)
        {
            var alerts = GetStudentAlerts(studentId, limit: 1000);

            return new StudentAlertsCountDto
            {
                Count = alerts.Count,
                High = alerts.Count(a => a.Severity == "HIGH"),
                Medium = alerts.Count(a => a.Severity == "MEDIUM"),
                Low = alerts.Count(a => a.Severity == "LOW"),
            };
        }

        public ProgressSummaryDto GetProgressSummary(int studentId)
        {
            var dp = GetDegreeProgress(studentId);
            var stats = GetStudentStats(studentId);

            var remaining = (dp?.CreditsRequired ?? 0) - (dp?.CreditsEarned ?? 0);
            if (remaining < 0) remaining = 0;

            return new ProgressSummaryDto
            {
                CreditsEarned = dp?.CreditsEarned ?? 0,
                CreditsRequired = dp?.CreditsRequired ?? 0,
                CreditsRemaining = remaining,
                PercentComplete = dp?.PercentComplete ?? 0,

                CoursesPassed = stats?.CompletedCourses ?? 0,
                CoursesFailed = stats?.FailedCourses ?? 0
            };
        }

        private static string ExtractDepartmentPrefix(string courseCode)
        {
            if (string.IsNullOrWhiteSpace(courseCode)) return "UNKNOWN";

            // Take leading letters until digit or '-'
            var match = Regex.Match(courseCode, @"^[A-Za-z]+");
            return match.Success ? match.Value.ToUpper() : "UNKNOWN";
        }

        public List<DepartmentCreditsDto> GetProgressDepartments(int studentId)
        {
            var historyCodes = _context.SisStudentCourseHistories
                .Where(h => h.StudentId == studentId)
                .Select(h => h.CourseCode)
                .Distinct()
                .ToList();

            return historyCodes
                .GroupBy(code => ExtractDepartmentPrefix(code))
                .Select(g => new DepartmentCreditsDto
                {
                    Department = g.Key,     // ✅ raw prefix like PROG, MATH, ENGI...
                    CoursesCount = g.Count()
                })
                .OrderByDescending(x => x.CoursesCount)
                .ThenBy(x => x.Department)
                .ToList();
        }

        public List<SemesterHistoryDto> GetProgressHistory(int studentId)
        {
            var history = _context.SisStudentCourseHistories
                .Where(h => h.StudentId == studentId && h.Semester != null)
                .ToList();

            if (!history.Any())
                return new List<SemesterHistoryDto>();

            var allCodes = history.Select(h => h.CourseCode).Distinct().ToList();

            var courseMap = _context.SisCourses
                .Where(c => allCodes.Contains(c.CourseCode))
                .ToDictionary(c => c.CourseCode, c => c);

            var prereqs = _context.CoursePrerequisites
                .Where(p => allCodes.Contains(p.CourseCode))
                .ToList();

            var result = history
                .GroupBy(h => h.Semester!)
                .Select(g =>
                {
                    var courses = g.Select(h =>
                    {
                        courseMap.TryGetValue(h.CourseCode, out var course);

                        var prereqCodes = prereqs
                            .Where(p => p.CourseCode == h.CourseCode)
                            .Select(p => p.PrerequisiteCourseCode)
                            .Distinct()
                            .ToList();

                        return new CourseHistoryItemDto
                        {
                            CourseCode = h.CourseCode,
                            CourseName = course?.CourseName ?? h.CourseCode,
                            Credits = course?.Credits ?? 0,
                            FinalGrade = Math.Round(h.FinalGrade ?? 0m, 2), // ✅ use stored FinalGrade
                            Status = h.Status ?? "",
                            Prerequisites = prereqCodes
                        };
                    }).ToList();

                    var totalCredits = courses.Sum(c => c.Credits);

                    decimal semesterGpa = 0m;
                    if (totalCredits > 0)
                    {
                        // ✅ GPA = Σ(finalGrade * credits) / Σ(credits)
                        semesterGpa = courses.Sum(c => c.FinalGrade * c.Credits) / totalCredits;
                        semesterGpa = Math.Round(semesterGpa, 2);
                    }

                    return new SemesterHistoryDto
                    {
                        Semester = g.Key,
                        SemesterGpa = semesterGpa,
                        Credits = totalCredits,
                        CoursesCount = courses.Count,
                        Courses = courses
                            .OrderByDescending(c => c.Credits)
                            .ThenBy(c => c.CourseCode)
                            .ToList()
                    };
                })
                .OrderBy(x => x.Semester)
                .ToList();

            return result;
        }

        public List<StudyGuideComparisonDto> GetStudyGuideComparison(int studentId)
        {
            var student = GetById(studentId);
            if (student == null) return new List<StudyGuideComparisonDto>();

            var program = student.ProgramCode;
            var currentSemester = student.CurrentSemester ?? 0;

            var study = _context.StudyGuides
                .Where(sg => sg.ProgramCode == program && sg.RecommendedSemester <= currentSemester)
                .ToList();

            var passed = _context.SisStudentCourseHistories
                .Where(h => h.StudentId == studentId && h.Status == "PASSED")
                .Select(h => h.CourseCode)
                .ToHashSet();

            var result = study
                .GroupBy(sg => sg.RecommendedSemester)
                .Select(g =>
                {
                    var total = g.Select(x => x.CourseCode).Distinct().Count();
                    var completed = g.Select(x => x.CourseCode).Distinct().Count(code => passed.Contains(code));

                    return new StudyGuideComparisonDto
                    {
                        RecommendedSemester = g.Key ?? 0,
                        Total = total,
                        Completed = completed
                    };
                })
                .OrderBy(x => x.RecommendedSemester)
                .ToList();

            return result;
        }

        public StudentMessagesSummaryDto GetStudentMessagesSummary(int studentId)
        {
            var student = GetById(studentId);
            if (student == null || student.AdvisorId == null)
                return new StudentMessagesSummaryDto();

            var advisorId = student.AdvisorId.Value;
            var now = DateTime.Now;
            var sevenDaysAgo = now.AddDays(-7);
            var monthStart = new DateTime(now.Year, now.Month, 1);

            var announcements = _context.Announcements
                .Where(a => a.AdvisorId == advisorId)
                .ToList();

            return new StudentMessagesSummaryDto
            {
                TotalMessages = announcements.Count,
                Recent7Days = announcements.Count(a => a.CreatedAt >= sevenDaysAgo),
                ThisMonth = announcements.Count(a => a.CreatedAt >= monthStart)
            };
        }

        public List<StudentMessageDto> GetStudentMessages(int studentId, int limit = 20)
        {
            var student = GetById(studentId);
            if (student == null || student.AdvisorId == null)
                return new List<StudentMessageDto>();

            var advisorId = student.AdvisorId.Value;

            // number of students assigned to same advisor
            var recipientsCount = _context.SisStudents
                .Count(s => s.AdvisorId == advisorId);

            var messages = _context.Announcements
                .Where(a => a.AdvisorId == advisorId)
                .Join(_context.Advisors,
                    ann => ann.AdvisorId,
                    adv => adv.AdvisorId,
                    (ann, adv) => new StudentMessageDto
                    {
                        AnnouncementId = ann.AnnouncementId,
                        Title = ann.Title ?? "",
                        Content = ann.Content ?? "",
                        CreatedAt = ann.CreatedAt ?? DateTime.Now,
                        AdvisorId = adv.AdvisorId,
                        AdvisorName = adv.Name,
                        RecipientsCount = recipientsCount
                    })
                .OrderByDescending(x => x.CreatedAt)
                .Take(limit)
                .ToList();

            return messages;
        }

        public StudentMessagesAdvisorDto? GetStudentMessagesAdvisor(int studentId)
        {
            var advisor = _context.SisStudents
                .Where(s => s.StudentId == studentId)
                .Join(_context.Advisors,
                    s => s.AdvisorId,
                    a => a.AdvisorId,
                    (s, a) => new StudentMessagesAdvisorDto
                    {
                        AdvisorId = a.AdvisorId,
                        Name = a.Name,
                        Email = a.Email,
                        Office = a.Office,
                        OfficeHours = a.OfficeHours
                    })
                .FirstOrDefault();

            return advisor;
        }

    }

}

