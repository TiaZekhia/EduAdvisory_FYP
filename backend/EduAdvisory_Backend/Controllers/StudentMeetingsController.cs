using EduAdvisory_Backend.DTOs.Meetings;
using EduAdvisory_Backend.DTOs.Student;
using EduAdvisory_Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduAdvisory_Backend.Controllers
{
    [Authorize(Roles = "STUDENT")]
    [ApiController]
    [Route("api/student-meetings")]
    public class StudentMeetingsController : ControllerBase
    {
        private readonly EduAdvisoryDbContext _context;

        public StudentMeetingsController(EduAdvisoryDbContext context)
        {
            _context = context;
        }

        private async Task<SisStudent?> GetCurrentStudentAsync()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(username)) return null;

            return await _context.Users
                .Include(u => u.LinkedStudent)
                .Where(u => u.Username == username)
                .Select(u => u.LinkedStudent)
                .FirstOrDefaultAsync();
        }

        [HttpGet("my/advisor")]
        public async Task<IActionResult> GetMyAdvisor()
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return Unauthorized();

            var advisor = await _context.Advisors
                .Where(a => a.AdvisorId == student.AdvisorId)
                .Select(a => new
                {
                    a.AdvisorId,
                    a.Name,
                    a.Email,
                    a.Office,
                    a.OfficeHours
                })
                .FirstOrDefaultAsync();

            if (advisor == null) return NotFound("Advisor not found.");

            return Ok(advisor);
        }

        [HttpGet("my/advisor-availability")]
        public async Task<IActionResult> GetMyAdvisorAvailability()
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return Unauthorized();
            if (student.AdvisorId == null) return BadRequest("Student has no assigned advisor.");

            var now = DateTimeOffset.UtcNow;

            var slots = await _context.Set<AdvisorAvailability>()
                .Where(x =>
                    x.AdvisorId == student.AdvisorId &&
                    x.IsActive &&
                    !x.IsBooked &&
                    x.StartAt > now)
                .OrderBy(x => x.StartAt)
                .Select(x => new AdvisorAvailabilitySlotDto
                {
                    AvailabilityId = x.AvailabilityId,
                    StartAt = x.StartAt,
                    EndAt = x.EndAt
                })
                .ToListAsync();

            return Ok(slots);
        }

        [HttpGet("my/requests")]
        public async Task<IActionResult> GetMyRequests()
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return Unauthorized();

            var requests = await _context.Set<MeetingRequest>()
                .Include(r => r.Advisor)
                .Include(r => r.Availability)
                .Where(r => r.StudentId == student.StudentId)
                .OrderByDescending(r => r.RequestedAt)
                .Select(r => new StudentMeetingRequestListItemDto
                {
                    RequestId = r.RequestId,
                    AdvisorName = r.Advisor.Name,
                    StartAt = r.Availability.StartAt,
                    EndAt = r.Availability.EndAt,
                    Status = r.Status,
                    Reason = r.Reason,
                    RejectionReason = r.RejectionReason
                })
                .ToListAsync();

            return Ok(requests);
        }

        [HttpPost("my/requests")]
        public async Task<IActionResult> CreateMeetingRequest([FromBody] CreateMeetingRequestDto dto)
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return Unauthorized();
            if (student.AdvisorId == null) return BadRequest("Student has no assigned advisor.");

            var slot = await _context.Set<AdvisorAvailability>()
                .FirstOrDefaultAsync(x =>
                    x.AvailabilityId == dto.AvailabilityId &&
                    x.AdvisorId == student.AdvisorId &&
                    x.IsActive &&
                    !x.IsBooked);

            if (slot == null)
                return BadRequest("Selected slot is no longer available.");

            if (slot.StartAt <= DateTimeOffset.UtcNow)
                return BadRequest("Cannot request a past slot.");

            var alreadyRequested = await _context.Set<MeetingRequest>().AnyAsync(r =>
                r.StudentId == student.StudentId &&
                r.AvailabilityId == dto.AvailabilityId &&
                r.Status == "PENDING");

            if (alreadyRequested)
                return BadRequest("You already requested this slot.");

            var hasConflict = await _context.Meetings.AnyAsync(m =>
                m.StudentId == student.StudentId &&
                m.Status == "UPCOMING" &&
                m.StartAt < slot.EndAt &&
                m.EndAt > slot.StartAt);

            if (hasConflict)
                return BadRequest("You already have another meeting during this time.");

            var request = new MeetingRequest
            {
                StudentId = student.StudentId,
                AdvisorId = student.AdvisorId.Value,
                AvailabilityId = slot.AvailabilityId,
                Reason = dto.Reason,
                Status = "PENDING",
                RequestedAt = DateTimeOffset.UtcNow
            };

            _context.Set<MeetingRequest>().Add(request);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Meeting request submitted successfully." });
        }

        [HttpDelete("my/requests/{id:int}")]
        public async Task<IActionResult> CancelMeetingRequest(int id)
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return Unauthorized();

            var request = await _context.Set<MeetingRequest>()
                .FirstOrDefaultAsync(r =>
                    r.RequestId == id &&
                    r.StudentId == student.StudentId &&
                    r.Status == "PENDING");

            if (request == null)
                return NotFound("Pending request not found.");

            request.Status = "CANCELLED";
            request.RespondedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Meeting request cancelled." });
        }

        [HttpGet("my/upcoming")]
        public async Task<IActionResult> GetMyUpcomingMeetings()
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return Unauthorized();

            var now = DateTimeOffset.UtcNow;

            var meetings = await _context.Meetings
                .Include(m => m.Advisor)
                .Where(m =>
                    m.StudentId == student.StudentId &&
                    m.Status == "UPCOMING" &&
                    m.StartAt >= now)
                .OrderBy(m => m.StartAt)
                .Select(m => new MeetingDto
                {
                    MeetingId = m.MeetingId,
                    Title = m.Title ?? "Advising Meeting",
                    StartAt = m.StartAt,
                    EndAt = m.EndAt,
                    Status = m.Status!,
                    MeetingType = m.MeetingType,
                    MeetingLink = m.MeetingLink,
                    Notes = m.Notes,
                    AdvisorName = m.Advisor!.Name,
                    AdvisorEmail = m.Advisor.Email,
                    StudentName = $"{student.FirstName} {student.LastName}"
                })
                .ToListAsync();

            return Ok(meetings);
        }

        [HttpGet("my/history")]
        public async Task<IActionResult> GetMyMeetingHistory()
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return Unauthorized();

            var now = DateTimeOffset.UtcNow;

            var meetings = await _context.Meetings
                .Include(m => m.Advisor)
                .Where(m =>
                    m.StudentId == student.StudentId &&
                    (m.Status == "COMPLETED" || m.StartAt < now))
                .OrderByDescending(m => m.StartAt)
                .Select(m => new MeetingDto
                {
                    MeetingId = m.MeetingId,
                    Title = m.Title ?? "Advising Meeting",
                    StartAt = m.StartAt,
                    EndAt = m.EndAt,
                    Status = m.Status ?? "COMPLETED",
                    MeetingType = m.MeetingType,
                    MeetingLink = m.MeetingLink,
                    Notes = m.Notes,
                    AdvisorName = m.Advisor!.Name,
                    AdvisorEmail = m.Advisor.Email,
                    StudentName = $"{student.FirstName} {student.LastName}"
                })
                .ToListAsync();

            return Ok(meetings);
        }
    }
}