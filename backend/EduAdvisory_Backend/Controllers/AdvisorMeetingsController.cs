using EduAdvisory_Backend.DTOs.Advisor;
using EduAdvisory_Backend.DTOs.Meetings;
using EduAdvisory_Backend.Interfaces.Services;
using EduAdvisory_Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduAdvisory_Backend.Controllers
{
    [Authorize(Roles = "ADVISOR")]
    [ApiController]
    [Route("api/advisor-meetings")]
    public class AdvisorMeetingsController : ControllerBase
    {
        private readonly EduAdvisoryDbContext _context;

        public AdvisorMeetingsController(EduAdvisoryDbContext context)
        {
            _context = context;
        }

        private async Task<Advisor?> GetCurrentAdvisorAsync()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(username)) return null;

            return await _context.Users
                .Include(u => u.LinkedAdvisor)
                .Where(u => u.Username == username)
                .Select(u => u.LinkedAdvisor)
                .FirstOrDefaultAsync();
        }

        [HttpGet("availability")]
        public async Task<IActionResult> GetMyAvailability()
        {
            var advisor = await GetCurrentAdvisorAsync();
            if (advisor == null) return Unauthorized();

            var slots = await _context.Set<AdvisorAvailability>()
                .Where(x => x.AdvisorId == advisor.AdvisorId && x.IsActive)
                .OrderBy(x => x.StartAt)
                .Select(x => new
                {
                    x.AvailabilityId,
                    x.StartAt,
                    x.EndAt,
                    x.IsBooked,
                    x.IsActive
                })
                .ToListAsync();

            return Ok(slots);
        }

        [HttpPost("availability")]
        public async Task<IActionResult> CreateAvailability([FromBody] CreateAdvisorAvailabilityDto dto)
        {
            var advisor = await GetCurrentAdvisorAsync();
            if (advisor == null) return Unauthorized();

            if (dto.EndAt <= dto.StartAt)
                return BadRequest("End time must be after start time.");

            if (dto.StartAt <= DateTimeOffset.UtcNow)
                return BadRequest("Availability must be in the future.");

            var overlap = await _context.Set<AdvisorAvailability>().AnyAsync(x =>
                x.AdvisorId == advisor.AdvisorId &&
                x.IsActive &&
                x.StartAt < dto.EndAt &&
                x.EndAt > dto.StartAt);

            if (overlap)
                return BadRequest("This slot overlaps with another availability.");

            var meetingOverlap = await _context.Meetings.AnyAsync(m =>
                m.AdvisorId == advisor.AdvisorId &&
                m.Status == "UPCOMING" &&
                m.StartAt < dto.EndAt &&
                m.EndAt > dto.StartAt);

            if (meetingOverlap)
                return BadRequest("This slot overlaps with an existing meeting.");

            var slot = new AdvisorAvailability
            {
                AdvisorId = advisor.AdvisorId,
                StartAt = dto.StartAt,
                EndAt = dto.EndAt,
                IsBooked = false,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Set<AdvisorAvailability>().Add(slot);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Availability slot created successfully." });
        }

        [HttpDelete("availability/{id:int}")]
        public async Task<IActionResult> DeleteAvailability(int id)
        {
            var advisor = await GetCurrentAdvisorAsync();
            if (advisor == null) return Unauthorized();

            var slot = await _context.Set<AdvisorAvailability>()
                .FirstOrDefaultAsync(x =>
                    x.AvailabilityId == id &&
                    x.AdvisorId == advisor.AdvisorId);

            if (slot == null)
                return NotFound("Availability slot not found.");

            if (slot.IsBooked)
                return BadRequest("Booked slots cannot be deleted.");

            slot.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Availability slot removed." });
        }

        [HttpGet("requests/pending")]
        public async Task<IActionResult> GetPendingRequests()
        {
            var advisor = await GetCurrentAdvisorAsync();
            if (advisor == null) return Unauthorized();

            var requests = await _context.Set<MeetingRequest>()
                .Include(r => r.Student)
                .Include(r => r.Availability)
                .Where(r => r.AdvisorId == advisor.AdvisorId && r.Status == "PENDING")
                .OrderBy(r => r.Availability.StartAt)
                .Select(r => new AdvisorMeetingRequestDto
                {
                    RequestId = r.RequestId,
                    StudentId = r.StudentId,
                    StudentName = $"{r.Student.FirstName} {r.Student.LastName}",
                    StartAt = r.Availability.StartAt,
                    EndAt = r.Availability.EndAt,
                    Reason = r.Reason,
                    Status = r.Status,
                    RequestedAt = r.RequestedAt
                })
                .ToListAsync();

            return Ok(requests);
        }

        [HttpPost("requests/{id:int}/respond")]
        public async Task<IActionResult> RespondToRequest(
    int id,
    [FromBody] RespondMeetingRequestDto dto,
    [FromServices] ISharedGoogleMeetService googleMeetService)
        {
            var advisor = await GetCurrentAdvisorAsync();
            if (advisor == null) return Unauthorized();

            var request = await _context.Set<MeetingRequest>()
                .Include(r => r.Student)
                .Include(r => r.Availability)
                .FirstOrDefaultAsync(r =>
                    r.RequestId == id &&
                    r.AdvisorId == advisor.AdvisorId);

            if (request == null)
                return NotFound("Request not found.");

            if (request.Status != "PENDING")
                return BadRequest("This request has already been processed.");

            var decision = dto.Decision?.Trim().ToUpperInvariant();

            if (decision == "REJECTED")
            {
                request.Status = "REJECTED";
                request.RejectionReason = dto.RejectionReason;
                request.RespondedAt = DateTimeOffset.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Meeting request rejected." });
            }

            if (decision != "ACCEPTED")
                return BadRequest("Decision must be ACCEPTED or REJECTED.");

            if (request.Availability.IsBooked || !request.Availability.IsActive)
                return BadRequest("This slot is no longer available.");

            var advisorConflict = await _context.Meetings.AnyAsync(m =>
                m.AdvisorId == advisor.AdvisorId &&
                m.Status == "UPCOMING" &&
                m.StartAt < request.Availability.EndAt &&
                m.EndAt > request.Availability.StartAt);

            if (advisorConflict)
                return BadRequest("You already have another meeting during this time.");

            var studentConflict = await _context.Meetings.AnyAsync(m =>
                m.StudentId == request.StudentId &&
                m.Status == "UPCOMING" &&
                m.StartAt < request.Availability.EndAt &&
                m.EndAt > request.Availability.StartAt);

            if (studentConflict)
                return BadRequest("Student already has another meeting during this time.");

            var googleMeet = await googleMeetService.CreateMeetingSpaceAsync();

            request.Status = "ACCEPTED";
            request.RespondedAt = DateTimeOffset.UtcNow;
            request.Availability.IsBooked = true;

            var startAt = request.Availability.StartAt;
            var endAt = request.Availability.EndAt;
            var durationMinutes = (int)(endAt - startAt).TotalMinutes;

            var meeting = new Meeting
            {
                AdvisorId = advisor.AdvisorId,
                StudentId = request.StudentId,
                RequestId = request.RequestId,
                Title = "Academic Advising Meeting",
                MeetingType = "ONLINE",

                StartAt = startAt,
                EndAt = endAt,

                MeetingDate = startAt,
                DurationMinutes = durationMinutes,

                MeetingLink = googleMeet.MeetingUri,
                GoogleSpaceName = googleMeet.SpaceName,
                Status = "UPCOMING",
                Notes = request.Reason,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _context.Meetings.Add(meeting);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Meeting request accepted and Google Meet link generated.",
                meetLink = googleMeet.MeetingUri
            });
        }

        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingMeetings()
        {
            var advisor = await GetCurrentAdvisorAsync();
            if (advisor == null) return Unauthorized();

            var now = DateTimeOffset.UtcNow;

            var meetings = await _context.Meetings
                .Include(m => m.Student)
                .Where(m =>
                    m.AdvisorId == advisor.AdvisorId &&
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
                    AdvisorName = advisor.Name,
                    AdvisorEmail = advisor.Email,
                    StudentName = $"{m.Student!.FirstName} {m.Student.LastName}"
                })
                .ToListAsync();

            return Ok(meetings);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetMeetingHistory()
        {
            var advisor = await GetCurrentAdvisorAsync();
            if (advisor == null) return Unauthorized();

            var now = DateTimeOffset.UtcNow;

            var meetings = await _context.Meetings
                .Include(m => m.Student)
                .Where(m =>
                    m.AdvisorId == advisor.AdvisorId &&
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
                    AdvisorName = advisor.Name,
                    AdvisorEmail = advisor.Email,
                    StudentName = $"{m.Student!.FirstName} {m.Student.LastName}"
                })
                .ToListAsync();

            return Ok(meetings);
        }
    }
}