using System.Net.NetworkInformation;
using EduAdvisory_Backend.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EduAdvisory_Backend.Interfaces.Repositories;

namespace EduAdvisory_Backend.Controllers
{
    [Authorize(Roles = "STUDENT")]
    [ApiController]
    [Route("api/[controller]")]
    public class StudentAnalysisController : ControllerBase
    {
        private readonly IStudentAnalysisService _service;
        private readonly IStudentRepository _studentRepo;

        public StudentAnalysisController(
            IStudentAnalysisService service,
            IStudentRepository studentRepo)
        {
            _service = service;
            _studentRepo = studentRepo;
        }

        [HttpGet("me")]
        public IActionResult AnalyzeCurrentStudent()
        {
            var username = User.Identity?.Name;

            if (string.IsNullOrEmpty(username))
                return Unauthorized();

            var student = _studentRepo.GetByUsername(username);

            if (student == null)
                return NotFound("Student not linked to this user.");

            var result = _service.AnalyzeStudent(student.StudentId);

            return Ok(result);
        }
    }

}
