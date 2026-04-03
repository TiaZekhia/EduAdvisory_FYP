using EduAdvisory_Backend.Interfaces.Repositories;
using EduAdvisory_Backend.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduAdvisory_Backend.Controllers
{
    [Authorize(Roles = "ADVISOR")]
    [ApiController]
    [Route("api/advisors/me/students")]
    public class AdvisorStudentAnalysisController : ControllerBase
    {
        private readonly IAdvisorRepository _advisorRepo;
        private readonly IStudentRepository _studentRepo;
        private readonly IStudentAnalysisService _studentAnalysisService;

        public AdvisorStudentAnalysisController(
            IAdvisorRepository advisorRepo,
            IStudentRepository studentRepo,
            IStudentAnalysisService studentAnalysisService)
        {
            _advisorRepo = advisorRepo;
            _studentRepo = studentRepo;
            _studentAnalysisService = studentAnalysisService;
        }

        [HttpGet("{studentId:int}/analysis")]
        public IActionResult GetStudentAnalysis(int studentId)
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

            var result = _studentAnalysisService.AnalyzeStudentForAdvisor(studentId);
            return Ok(result);
        }
    }
}