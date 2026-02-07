using EduAdvisory_Backend.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace EduAdvisory_Backend.Controllers
{
    [ApiController]
    [Route("api/analysis")]
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
