using EduAdvisory_Backend.DTOs.Student;
using EduAdvisory_Backend.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EduAdvisory_Backend.DTOs.Meetings;


namespace EduAdvisory_Backend.Controllers
{
    [Authorize(Roles = "STUDENT")]
    [ApiController]
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentRepository _studentRepo;

        public StudentsController(IStudentRepository studentRepo)
        {
            _studentRepo = studentRepo;
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
            if (string.IsNullOrEmpty(username)) return Unauthorized();

            var student = _studentRepo.GetByUsername(username);
            if (student == null) return NotFound("Student not linked to this user.");

            return Ok(_studentRepo.GetProgressSummary(student.StudentId));
        }

        [HttpGet("me/progress/departments")]
        public IActionResult GetMyProgressDepartments()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return Unauthorized();

            var student = _studentRepo.GetByUsername(username);
            if (student == null) return NotFound("Student not linked to this user.");

            return Ok(_studentRepo.GetProgressDepartments(student.StudentId));
        }

        [HttpGet("me/progress/history")]
        public IActionResult GetMyProgressHistory()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return Unauthorized();

            var student = _studentRepo.GetByUsername(username);
            if (student == null) return NotFound("Student not linked to this user.");

            return Ok(_studentRepo.GetProgressHistory(student.StudentId));
        }

        [HttpGet("me/progress/study-guide-comparison")]
        public IActionResult GetMyStudyGuideComparison()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return Unauthorized();

            var student = _studentRepo.GetByUsername(username);
            if (student == null) return NotFound("Student not linked to this user.");

            return Ok(_studentRepo.GetStudyGuideComparison(student.StudentId));
        }

        [HttpGet("me/meetings/summary")]
        public IActionResult GetMyMeetingsSummary()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return Unauthorized();

            var student = _studentRepo.GetByUsername(username);
            if (student == null) return NotFound("Student not linked to this user.");

            var summary = _studentRepo.GetStudentMeetingsSummary(student.StudentId);
            return Ok(summary);
        }

        [HttpGet("me/meetings/upcoming")]
        public IActionResult GetMyUpcomingMeetings([FromQuery] int limit = 3)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return Unauthorized();

            var student = _studentRepo.GetByUsername(username);
            if (student == null) return NotFound("Student not linked to this user.");

            var data = _studentRepo.GetUpcomingMeetings(student.StudentId, limit);
            return Ok(data);
        }

        [HttpGet("me/meetings/past")]
        public IActionResult GetMyPastMeetings([FromQuery] int limit = 10)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return Unauthorized();

            var student = _studentRepo.GetByUsername(username);
            if (student == null) return NotFound("Student not linked to this user.");

            var data = _studentRepo.GetPastMeetings(student.StudentId, limit);
            return Ok(data);
        }

        [HttpGet("me/advisor")]
        public IActionResult GetMyAdvisor()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return Unauthorized();

            var student = _studentRepo.GetByUsername(username);
            if (student == null) return NotFound("Student not linked to this user.");

            var advisor = _studentRepo.GetStudentAdvisor(student.StudentId);
            return Ok(advisor);
        }
    }
}