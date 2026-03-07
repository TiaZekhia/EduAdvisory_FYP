using EduAdvisory_Backend.DTOs.Student;
using EduAdvisory_Backend.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EduAdvisory_Backend.DTOs.Course;
using EduAdvisory_Backend.Interfaces.Services;
using EduAdvisory_Backend.DTOs.CoursePlan;

namespace EduAdvisory_Backend.Controllers
{
    [Authorize(Roles = "STUDENT")]
    [ApiController]
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentRepository _studentRepo;
        private readonly ICoursePlanService _coursePlanService;
        private readonly ILogger<StudentsController> _logger;

        public StudentsController(
            IStudentRepository studentRepo,
            ICoursePlanService coursePlanService,
            ILogger<StudentsController> logger)
        {
            _studentRepo = studentRepo;
            _coursePlanService = coursePlanService;
            _logger = logger;
        }

        [HttpGet("me/summary")]
        public IActionResult GetMySummary()
        {
            var username = User.Identity?.Name;

            if (string.IsNullOrEmpty(username))
                return Unauthorized();

            var student = _studentRepo.GetByUsername(username);

            if (student == null)
                return NotFound("Student not linked to this user.");

            var dto = new StudentMeSummaryDto
            {
                StudentId = student.StudentId,
                FullName = $"{student.FirstName} {student.LastName}",
                ProgramCode = student.ProgramCode,
                CurrentSemester = student.CurrentSemester ?? 0,
                CurrentGpa = student.CurrentGpa,
                AcademicStatus = student.AcademicStatus
            };

            return Ok(dto);
        }

        [HttpGet("me/current-enrollment")]
        public IActionResult GetMyCurrentEnrollment()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized();

            var student = _studentRepo.GetByUsername(username);
            if (student == null)
                return NotFound("Student not linked to this user.");

            var courses = _studentRepo.GetCurrentEnrollmentWithCourse(student.StudentId);

            return Ok(courses);
        }

        [HttpGet("me/current-courses/performance")]
        public IActionResult GetMyCurrentCoursesPerformance()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized();

            var student = _studentRepo.GetByUsername(username);
            if (student == null)
                return NotFound("Student not linked to this user.");

            var data = _studentRepo.GetCurrentCoursesPerformance(student.StudentId);

            return Ok(data);
        }

        [HttpGet("me/stats")]
        public IActionResult GetMyStats()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized();

            var student = _studentRepo.GetByUsername(username);
            if (student == null)
                return NotFound("Student not linked to this user.");

            var stats = _studentRepo.GetStudentStats(student.StudentId);
            return Ok(stats);
        }

        [HttpGet("me/degree-progress")]
        public IActionResult GetMyDegreeProgress()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized();

            var student = _studentRepo.GetByUsername(username);
            if (student == null)
                return NotFound("Student not linked to this user.");

            var progress = _studentRepo.GetDegreeProgress(student.StudentId);
            return Ok(progress);
        }

        [HttpGet("me/meetings/upcoming/count")]
        public IActionResult GetMyUpcomingMeetingsCount()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized();

            var student = _studentRepo.GetByUsername(username);
            if (student == null)
                return NotFound("Student not linked to this user.");

            var count = _studentRepo.GetUpcomingMeetingsCount(student.StudentId);
            return Ok(new UpcomingMeetingsCountDto { UpcomingMeetingsCount = count });
        }

        [HttpGet("me/alerts")]
        public IActionResult GetMyAlerts([FromQuery] int limit = 5)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized();

            var student = _studentRepo.GetByUsername(username);
            if (student == null)
                return NotFound("Student not linked to this user.");

            var alerts = _studentRepo.GetStudentAlerts(student.StudentId, limit);
            return Ok(alerts);
        }

        [HttpGet("me/alerts/count")]
        public IActionResult GetMyAlertsCount()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized();

            var student = _studentRepo.GetByUsername(username);
            if (student == null)
                return NotFound("Student not linked to this user.");

            var count = _studentRepo.GetStudentAlertsCount(student.StudentId);
            return Ok(count);
        }

        [HttpGet("me/progress/summary")]
        public IActionResult GetMyProgressSummary()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized();

            var student = _studentRepo.GetByUsername(username);
            if (student == null)
                return NotFound("Student not linked to this user.");

            return Ok(_studentRepo.GetProgressSummary(student.StudentId));
        }

        [HttpGet("me/progress/departments")]
        public IActionResult GetMyProgressDepartments()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized();

            var student = _studentRepo.GetByUsername(username);
            if (student == null)
                return NotFound("Student not linked to this user.");

            return Ok(_studentRepo.GetProgressDepartments(student.StudentId));
        }

        [HttpGet("me/progress/history")]
        public IActionResult GetMyProgressHistory()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized();

            var student = _studentRepo.GetByUsername(username);
            if (student == null)
                return NotFound("Student not linked to this user.");

            return Ok(_studentRepo.GetProgressHistory(student.StudentId));
        }

        [HttpGet("me/progress/study-guide-comparison")]
        public IActionResult GetMyStudyGuideComparison()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized();

            var student = _studentRepo.GetByUsername(username);
            if (student == null)
                return NotFound("Student not linked to this user.");

            return Ok(_studentRepo.GetStudyGuideComparison(student.StudentId));
        }

        [HttpGet("me/course-plan/plans")]
        public IActionResult GetMyGeneratedPlans([FromQuery] int count = 3)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized();

            var student = _studentRepo.GetByUsername(username);
            if (student == null)
                return NotFound("Student not linked to this user.");

            var plans = _coursePlanService.GeneratePlansForStudent(student.StudentId, count);
            return Ok(plans);
        }

        [HttpGet("me/course-plan/insights")]
        public async Task<IActionResult> GetMyCoursePlanInsights(
            [FromServices] ICoursePlanService planService,
            [FromServices] ICoursePlanAiService aiService,
            [FromQuery] int count = 3)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized();

            var student = _studentRepo.GetByUsername(username);
            if (student == null)
                return NotFound("Student not linked to this user.");

            var plans = planService.GeneratePlansForStudent(student.StudentId, count);
            var creditLimit = student.AcademicStatus == "PROBATION" ? 16 : 18;

            CoursePlanAiInsightsDto insights;
            string? aiError = null;

            try
            {
                insights = await aiService.RankAndExplainAsync(
                    plans,
                    student.ProgramCode ?? "",
                    student.CurrentSemester ?? 0,
                    student.AcademicStatus ?? "NORMAL",
                    creditLimit
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CoursePlan AI failed");
                aiError = ex.Message;

                insights = new CoursePlanAiInsightsDto
                {
                    BestPlanIndex = 0,
                    BestPlanSummary = "AI insights unavailable. Showing generated plans without AI ranking.",
                    PlanInsights = plans.Select((p, i) => new CoursePlanAiPlanInsightDto
                    {
                        PlanIndex = i,
                        Score = 50,
                        Explanation = $"Strategy: {p.Strategy}. (AI explanation unavailable.)",
                        Pros = new List<string>(),
                        Cons = new List<string>(),
                        Warnings = new List<string>()
                    }).ToList()
                };
            }

            return Ok(new CoursePlanInsightsResponseDto
            {
                Plans = plans,
                Insights = insights,
                AiError = aiError
            });
        }

        [HttpGet("me/messages/summary")]
        public IActionResult GetMyMessagesSummary()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized();

            var student = _studentRepo.GetByUsername(username);
            if (student == null)
                return NotFound("Student not linked to this user.");

            var summary = _studentRepo.GetStudentMessagesSummary(student.StudentId);
            return Ok(summary);
        }

        [HttpGet("me/messages")]
        public IActionResult GetMyMessages([FromQuery] int limit = 20)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized();

            var student = _studentRepo.GetByUsername(username);
            if (student == null)
                return NotFound("Student not linked to this user.");

            var messages = _studentRepo.GetStudentMessages(student.StudentId, limit);
            return Ok(messages);
        }

        [HttpGet("me/messages/advisor")]
        public IActionResult GetMyMessagesAdvisor()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized();

            var student = _studentRepo.GetByUsername(username);
            if (student == null)
                return NotFound("Student not linked to this user.");

            var advisor = _studentRepo.GetStudentMessagesAdvisor(student.StudentId);
            return Ok(advisor);
        }
    }
}