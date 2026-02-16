using System.Net.NetworkInformation;
using EduAdvisory_Backend.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace EduAdvisory_Backend.Controllers
{
    [Authorize(Roles = "STUDENT")]
    [ApiController]
    [Route("api/[controller]")]
    public class StudentAnalysisController : ControllerBase
    {
        private readonly IStudentAnalysisService _service;

        public StudentAnalysisController(IStudentAnalysisService service)
        {
            _service = service;
        }

        [HttpGet("student/{studentId}")]
        public IActionResult Analyze(int studentId)
        {
            return Ok(_service.AnalyzeStudent(studentId));
        }
    }

}
