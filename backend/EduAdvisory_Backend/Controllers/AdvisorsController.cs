using EduAdvisory_Backend.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduAdvisory_Backend.Controllers
{
    [Authorize(Roles = "ADVISOR")]
    [ApiController]
    [Route("api/[controller]")]
    public class AdvisorsController : ControllerBase
    {
        private readonly IAdvisorRepository _advisorRepo;

        public AdvisorsController(IAdvisorRepository advisorRepo)
        {
            _advisorRepo = advisorRepo;
        }

        [HttpGet("me/summary")]
        public IActionResult GetMySummary()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return Unauthorized();

            var advisor = _advisorRepo.GetByUsername(username);
            if (advisor == null) return NotFound("Advisor not linked to this user.");

            var dto = _advisorRepo.GetAdvisorSummary(advisor.AdvisorId);
            return Ok(dto);
        }

        [HttpGet("me/dashboard/summary")]
        public IActionResult GetMyDashboardSummary()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return Unauthorized();

            var advisor = _advisorRepo.GetByUsername(username);
            if (advisor == null) return NotFound("Advisor not linked to this user.");

            var dto = _advisorRepo.GetDashboardSummary(advisor.AdvisorId);
            return Ok(dto);
        }

        [HttpGet("me/meetings/upcoming")]
        public IActionResult GetMyUpcomingMeetings([FromQuery] int limit = 5)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return Unauthorized();

            var advisor = _advisorRepo.GetByUsername(username);
            if (advisor == null) return NotFound("Advisor not linked to this user.");

            var data = _advisorRepo.GetUpcomingMeetings(advisor.AdvisorId, limit);
            return Ok(data);
        }

        [HttpGet("me/meetings/past")]
        public IActionResult GetMyPastMeetings([FromQuery] int limit = 10)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return Unauthorized();

            var advisor = _advisorRepo.GetByUsername(username);
            if (advisor == null) return NotFound("Advisor not linked to this user.");

            var data = _advisorRepo.GetPastMeetings(advisor.AdvisorId, limit);
            return Ok(data);
        }

        [HttpGet("me/messages/summary")]
        public IActionResult GetMyMessagesSummary()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return Unauthorized();

            var advisor = _advisorRepo.GetByUsername(username);
            if (advisor == null) return NotFound("Advisor not linked to this user.");

            var dto = _advisorRepo.GetMessagesSummary(advisor.AdvisorId);
            return Ok(dto);
        }

        [HttpGet("me/messages")]
        public IActionResult GetMyMessages([FromQuery] int limit = 20)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return Unauthorized();

            var advisor = _advisorRepo.GetByUsername(username);
            if (advisor == null) return NotFound("Advisor not linked to this user.");

            var data = _advisorRepo.GetMessages(advisor.AdvisorId, limit);
            return Ok(data);
        }
    }
}