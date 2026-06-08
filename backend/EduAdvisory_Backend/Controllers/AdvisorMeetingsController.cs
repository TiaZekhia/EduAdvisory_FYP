using EduAdvisory_Backend.DTOs.Advisor;
using EduAdvisory_Backend.DTOs.Meetings;
using System.Globalization;
using EduAdvisory_Backend.Interfaces.Services;
using EduAdvisory_Backend.Models;
using EduAdvisory_Backend.Services;
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

        [HttpGet("weekly-availability")]
        public async Task<IActionResult> GetWeeklyAvailability()
        {
            var advisor = await GetCurrentAdvisorAsync();
            if (advisor == null) return Unauthorized();

            var rules = await _context.Set<AdvisorAvailabilityRule>()
                .Where(x => x.AdvisorId == advisor.AdvisorId && x.IsActive)
                .OrderBy(x => x.DayOfWeek)
                .ThenBy(x => x.StartTime)
                .Select(x => new AdvisorAvailabilityRuleDto
                {
                    RuleId = x.RuleId,
                    DayOfWeek = x.DayOfWeek,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    IsActive = x.IsActive
                })
                .ToListAsync();

            return Ok(rules);
        }

        [HttpPost("weekly-availability")]
        public async Task<IActionResult> CreateWeeklyAvailability([FromBody] CreateAdvisorAvailabilityRuleDto dto)
        {
            var advisor = await GetCurrentAdvisorAsync();
            if (advisor == null) return Unauthorized();

            if (dto.DayOfWeek < 0 || dto.DayOfWeek > 6)
                return BadRequest("DayOfWeek must be between 0 and 6.");

            if (dto.EndTime <= dto.StartTime)
                return BadRequest("End time must be after start time.");

            var overlap = await _context.Set<AdvisorAvailabilityRule>().AnyAsync(x =>
                x.AdvisorId == advisor.AdvisorId &&
                x.IsActive &&
                x.DayOfWeek == dto.DayOfWeek &&
                x.StartTime < dto.EndTime &&
                x.EndTime > dto.StartTime);

            if (overlap)
                return BadRequest("This availability overlaps with another rule.");

            var rule = new AdvisorAvailabilityRule
            {
                AdvisorId = advisor.AdvisorId,
                DayOfWeek = dto.DayOfWeek,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Set<AdvisorAvailabilityRule>().Add(rule);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Weekly availability created successfully." });
        }

        [HttpDelete("weekly-availability/{id:int}")]
        public async Task<IActionResult> DeleteWeeklyAvailability(int id)
        {
            var advisor = await GetCurrentAdvisorAsync();
            if (advisor == null) return Unauthorized();

            var rule = await _context.Set<AdvisorAvailabilityRule>()
                .FirstOrDefaultAsync(x => x.RuleId == id && x.AdvisorId == advisor.AdvisorId && x.IsActive);

            if (rule == null)
                return NotFound("Availability rule not found.");

            rule.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Weekly availability removed." });
        }

        [HttpGet("requests/pending")]
        public async Task<IActionResult> GetPendingRequests()
        {
            var advisor = await GetCurrentAdvisorAsync();
            if (advisor == null) return Unauthorized();

            var requests = await _context.Set<MeetingRequest>()
                .Include(r => r.Student)
                .Where(r => r.AdvisorId == advisor.AdvisorId && r.Status == "PENDING")
                .OrderBy(r => r.StartAt)
                .Select(r => new
                {
                    r.RequestId,
                    r.StudentId,
                    StudentName = $"{r.Student.FirstName} {r.Student.LastName}",
                    r.StartAt,
                    r.EndAt,
                    r.Reason,
                    r.Status,
                    r.RequestedAt
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

            var advisorConflict = await _context.Meetings.AnyAsync(m =>
                m.AdvisorId == advisor.AdvisorId &&
                m.Status == "UPCOMING" &&
                m.StartAt < request.EndAt &&
                m.EndAt > request.StartAt);

            if (advisorConflict)
                return BadRequest("You already have another meeting during this time.");

            var studentConflict = await _context.Meetings.AnyAsync(m =>
                m.StudentId == request.StudentId &&
                m.Status == "UPCOMING" &&
                m.StartAt < request.EndAt &&
                m.EndAt > request.StartAt);

            if (studentConflict)
                return BadRequest("Student already has another meeting during this time.");

            SharedGoogleMeetCreateResult googleMeet;
            try
            {
                googleMeet = await googleMeetService.CreateMeetingSpaceAsync();
            }
            catch (SharedGoogleAuthException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    reconnectRequired = ex.ReconnectRequired
                });
            }

            request.Status = "ACCEPTED";
            request.RespondedAt = DateTimeOffset.UtcNow;

            var durationMinutes = (int)(request.EndAt - request.StartAt).TotalMinutes;

            var meeting = new Meeting
            {
                AdvisorId = advisor.AdvisorId,
                StudentId = request.StudentId,
                RequestId = request.RequestId,
                Title = "Academic Advising Meeting",
                MeetingType = "ONLINE",
                StartAt = request.StartAt,
                EndAt = request.EndAt,
                MeetingDate = request.StartAt,
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

        [HttpGet("exceptions")]
        public async Task<IActionResult> GetExceptions()
        {
            var advisor = await GetCurrentAdvisorAsync();
            if (advisor == null) return Unauthorized();

            var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

            var exceptions = await _context.Set<AdvisorAvailabilityException>()
                .Where(x => x.AdvisorId == advisor.AdvisorId && x.ExceptionDate >= today)
                .OrderBy(x => x.ExceptionDate)
                .ThenBy(x => x.StartTime)
                .Select(x => new
                {
                    date = x.ExceptionDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    startTime = x.StartTime != null
                        ? x.StartTime.Value.Hours.ToString("D2") + ":" + x.StartTime.Value.Minutes.ToString("D2")
                        : (string?)null,
                    endTime = x.EndTime != null
                        ? x.EndTime.Value.Hours.ToString("D2") + ":" + x.EndTime.Value.Minutes.ToString("D2")
                        : (string?)null,
                    exceptionId = x.ExceptionId
                })
                .ToListAsync();

            return Ok(exceptions);
        }

        [HttpPost("exceptions")]
        public async Task<IActionResult> AddException([FromBody] AddExceptionDto dto)
        {
            var advisor = await GetCurrentAdvisorAsync();
            if (advisor == null) return Unauthorized();

            var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

            if (dto.Date < today)
                return BadRequest(new { message = "Cannot block a date in the past." });

            bool isPartial = dto.StartTime.HasValue || dto.EndTime.HasValue;

            if (isPartial)
            {
                if (!dto.StartTime.HasValue || !dto.EndTime.HasValue)
                    return BadRequest(new { message = "Both start time and end time are required for a partial block." });

                if (dto.EndTime <= dto.StartTime)
                    return BadRequest(new { message = "End time must be after start time." });

                // Check for overlap with existing exceptions on this date
                var hasOverlap = await _context.Set<AdvisorAvailabilityException>().AnyAsync(x =>
                    x.AdvisorId == advisor.AdvisorId &&
                    x.ExceptionDate == dto.Date &&
                    (
                        x.StartTime == null || // existing full-day block
                        (x.StartTime < dto.EndTime && x.EndTime > dto.StartTime) // time overlap
                    ));

                if (hasOverlap)
                    return BadRequest(new { message = "This time range overlaps with an existing blocked period." });
            }
            else
            {
                // Full-day block: must not have any existing exception on this date
                var exists = await _context.Set<AdvisorAvailabilityException>()
                    .AnyAsync(x => x.AdvisorId == advisor.AdvisorId && x.ExceptionDate == dto.Date);

                if (exists)
                    return BadRequest(new { message = "This date already has a blocked period. Remove it first or add a time range." });
            }

            var exception = new AdvisorAvailabilityException
            {
                AdvisorId = advisor.AdvisorId,
                ExceptionDate = dto.Date,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Set<AdvisorAvailabilityException>().Add(exception);
            await _context.SaveChangesAsync();

            return Ok(new { message = isPartial ? "Time range blocked successfully." : "Date blocked successfully." });
        }

        [HttpDelete("exceptions/{id:int}")]
        public async Task<IActionResult> RemoveException(int id)
        {
            var advisor = await GetCurrentAdvisorAsync();
            if (advisor == null) return Unauthorized();

            var exception = await _context.Set<AdvisorAvailabilityException>()
                .FirstOrDefaultAsync(x => x.ExceptionId == id && x.AdvisorId == advisor.AdvisorId);

            if (exception == null)
                return NotFound(new { message = "Blocked period not found." });

            _context.Set<AdvisorAvailabilityException>().Remove(exception);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Period unblocked successfully." });
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