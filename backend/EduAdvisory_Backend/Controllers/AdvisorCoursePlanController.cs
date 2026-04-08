using EduAdvisory_Backend.DTOs.CoursePlan;
using EduAdvisory_Backend.Interfaces.Repositories;
using EduAdvisory_Backend.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduAdvisory_Backend.Controllers
{
    [Authorize(Roles = "ADVISOR")]
    [ApiController]
    [Route("api/advisors/me/students")]
    public class AdvisorCoursePlanController : ControllerBase
    {
        private readonly IAdvisorRepository _advisorRepo;
        private readonly IStudentRepository _studentRepo;
        private readonly ICoursePlanService _coursePlanService;
        private readonly ICoursePlanAiService _coursePlanAiService;
        private readonly ILogger<AdvisorCoursePlanController> _logger;

        public AdvisorCoursePlanController(
            IAdvisorRepository advisorRepo,
            IStudentRepository studentRepo,
            ICoursePlanService coursePlanService,
            ICoursePlanAiService coursePlanAiService,
            ILogger<AdvisorCoursePlanController> logger)
        {
            _advisorRepo = advisorRepo;
            _studentRepo = studentRepo;
            _coursePlanService = coursePlanService;
            _coursePlanAiService = coursePlanAiService;
            _logger = logger;
        }

        [HttpGet("{studentId:int}/course-plan/plans")]
        public IActionResult GetStudentGeneratedPlans(int studentId, [FromQuery] int count = 3)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized();

            var advisor = _advisorRepo.GetByUsername(username);
            if (advisor == null)
                return NotFound("Advisor not linked to this user.");

            var student = _studentRepo.GetById(studentId);
            if (student == null)
                return NotFound("Student not found.");

            if (student.AdvisorId != advisor.AdvisorId)
                return Forbid();

            var plans = _coursePlanService.GeneratePlansForStudent(studentId, count);
            return Ok(plans);
        }

        [HttpGet("{studentId:int}/course-plan/insights")]
        public async Task<IActionResult> GetStudentCoursePlanInsights(int studentId, [FromQuery] int count = 3)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized();

            var advisor = _advisorRepo.GetByUsername(username);
            if (advisor == null)
                return NotFound("Advisor not linked to this user.");

            var student = _studentRepo.GetById(studentId);
            if (student == null)
                return NotFound("Student not found.");

            if (student.AdvisorId != advisor.AdvisorId)
                return Forbid();

            var plans = _coursePlanService.GeneratePlansForStudent(studentId, count);
            var creditLimit = student.AcademicStatus == "PROBATION" ? 16 : 18;

            CoursePlanAiInsightsDto insights;
            string? aiError = null;

            try
            {
                insights = await _coursePlanAiService.RankAndExplainAsync(
                    plans,
                    student.ProgramCode ?? "",
                    student.CurrentSemester ?? 0,
                    student.AcademicStatus ?? "NORMAL",
                    creditLimit
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CoursePlan AI failed for advisor view. StudentId: {StudentId}", studentId);
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
    }
}