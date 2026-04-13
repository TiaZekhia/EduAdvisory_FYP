using EduAdvisory_Backend.Interfaces.Repositories;
using EduAdvisory_Backend.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduAdvisory_Backend.Controllers
{
    [Authorize(Roles = "ADVISOR")]
    [ApiController]
    [Route("api/advisors/me/students")]
    public class AdvisorRiskAssessmentController : ControllerBase
    {
        private readonly IAdvisorRepository _advisorRepo;
        private readonly IStudentRepository _studentRepo;
        private readonly IStudentRiskAssessmentService _riskService;

        public AdvisorRiskAssessmentController(
            IAdvisorRepository advisorRepo,
            IStudentRepository studentRepo,
            IStudentRiskAssessmentService riskService)
        {
            _advisorRepo = advisorRepo;
            _studentRepo = studentRepo;
            _riskService = riskService;
        }

        [HttpGet("{studentId:int}/risk-assessment")]
        public IActionResult GetStudentRiskAssessment(int studentId)
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

            var result = _riskService.AssessStudent(studentId);
            return Ok(result);
        }
    }
}