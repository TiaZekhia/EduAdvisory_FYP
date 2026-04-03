using EduAdvisory_Backend.DTOs.Meetings;
using EduAdvisory_Backend.DTOs.Student;
using EduAdvisory_Backend.Models;
using EduAdvisory_Backend.Services;
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
        private static readonly int[] AllowedDurations = [15, 30, 45, 60];

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

        [HttpGet("my/advisor-calendar")]
        public async Task<IActionResult> GetMyAdvisorCalendar([FromQuery] DateOnly date)
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return Unauthorized();
            if (student.AdvisorId == null) return BadRequest("Student has no assigned advisor.");

            var dayOfWeek = (int)date.DayOfWeek;

            var rules = await _context.Set<AdvisorAvailabilityRule>()
                .Where(x =>
                    x.AdvisorId == student.AdvisorId &&
                    x.IsActive &&
                    x.DayOfWeek == dayOfWeek)
                .OrderBy(x => x.StartTime)
                .ToListAsync();

            if (!rules.Any())
                return Ok(new List<AdvisorCalendarStartTimeDto>());

            var tz = GetBeirutTimeZone();
            var localDateTime = new DateTime(
                date.Year,
                date.Month,
                date.Day,
                0, 0, 0,
                DateTimeKind.Unspecified);

            var offset = tz.GetUtcOffset(localDateTime);
            var localDayStart = new DateTimeOffset(localDateTime, offset);

            var utcDayStart = localDayStart.ToUniversalTime();
            var utcDayEnd = localDayStart.AddDays(1).ToUniversalTime();

            var meetings = await _context.Meetings
                .Where(m =>
                    m.AdvisorId == student.AdvisorId &&
                    m.Status == "UPCOMING" &&
                    m.StartAt < utcDayEnd &&
                    m.EndAt > utcDayStart)
                .ToListAsync();

            var pendingRequests = await _context.Set<MeetingRequest>()
                .Where(r =>
                    r.AdvisorId == student.AdvisorId &&
                    r.Status == "PENDING" &&
                    r.StartAt < utcDayEnd &&
                    r.EndAt > utcDayStart)
                .ToListAsync();

            var allStartTimes = new List<AdvisorCalendarStartTimeDto>();

            foreach (var rule in rules)
            {
                var ruleStartTimes = MeetingCalendarBuilder.BuildAvailableStartTimes(
                    date, rule, meetings, pendingRequests);

                allStartTimes.AddRange(ruleStartTimes);
            }

            var merged = allStartTimes
                .GroupBy(x => x.StartAt)
                .Select(g => new AdvisorCalendarStartTimeDto
                {
                    StartAt = g.Key,
                    AllowedDurations = g.SelectMany(x => x.AllowedDurations)
                        .Distinct()
                        .OrderBy(x => x)
                        .ToList()
                })
                .OrderBy(x => x.StartAt)
                .ToList();

            return Ok(merged);
        }

        [HttpPost("my/requests")]
        public async Task<IActionResult> CreateMeetingRequest([FromBody] CreateMeetingRequestDto dto)
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return Unauthorized();
            if (student.AdvisorId == null) return BadRequest("Student has no assigned advisor.");

            if (!AllowedDurations.Contains(dto.DurationMinutes))
                return BadRequest("Allowed durations are 15, 30, 45, or 60 minutes.");

            var localStart = dto.StartAt;
            var localEnd = dto.StartAt.AddMinutes(dto.DurationMinutes);

            if (localEnd <= localStart)
                return BadRequest("End time must be after start time.");

            if (localStart <= GetBeirutNow())
                return BadRequest("Meeting must be in the future.");

            var targetDate = DateOnly.FromDateTime(localStart.Date);
            var dayOfWeek = (int)targetDate.DayOfWeek;

            var rules = await _context.Set<AdvisorAvailabilityRule>()
                .Where(x =>
                    x.AdvisorId == student.AdvisorId &&
                    x.IsActive &&
                    x.DayOfWeek == dayOfWeek)
                .ToListAsync();

            if (!rules.Any())
                return BadRequest("Advisor is not available on this day.");

            var matchingRule = rules.FirstOrDefault(rule =>
            {
                var ruleStartLocal = BuildBeirutDateTime(targetDate, rule.StartTime);
                var ruleEndLocal = BuildBeirutDateTime(targetDate, rule.EndTime);

                return localStart >= ruleStartLocal && localEnd <= ruleEndLocal;
            });

            if (matchingRule == null)
                return BadRequest("Selected interval is outside advisor availability.");

            var startAtUtc = localStart.ToUniversalTime();
            var endAtUtc = localEnd.ToUniversalTime();

            var advisorMeetingConflict = await _context.Meetings.AnyAsync(m =>
                m.AdvisorId == student.AdvisorId &&
                m.Status == "UPCOMING" &&
                m.StartAt < endAtUtc &&
                m.EndAt > startAtUtc);

            if (advisorMeetingConflict)
                return BadRequest("This time is no longer available.");

            var advisorPendingConflict = await _context.Set<MeetingRequest>().AnyAsync(r =>
                r.AdvisorId == student.AdvisorId &&
                r.Status == "PENDING" &&
                r.StartAt < endAtUtc &&
                r.EndAt > startAtUtc);

            if (advisorPendingConflict)
                return BadRequest("This time is already requested by another student.");

            var studentMeetingConflict = await _context.Meetings.AnyAsync(m =>
                m.StudentId == student.StudentId &&
                m.Status == "UPCOMING" &&
                m.StartAt < endAtUtc &&
                m.EndAt > startAtUtc);

            if (studentMeetingConflict)
                return BadRequest("You already have another meeting during this time.");

            var alreadyRequested = await _context.Set<MeetingRequest>().AnyAsync(r =>
                r.StudentId == student.StudentId &&
                r.Status == "PENDING" &&
                r.StartAt < endAtUtc &&
                r.EndAt > startAtUtc);

            if (alreadyRequested)
                return BadRequest("You already requested an overlapping time.");

            var request = new MeetingRequest
            {
                StudentId = student.StudentId,
                AdvisorId = student.AdvisorId.Value,
                StartAt = startAtUtc,
                EndAt = endAtUtc,
                Reason = dto.Reason,
                Status = "PENDING",
                RequestedAt = DateTimeOffset.UtcNow
            };

            _context.Set<MeetingRequest>().Add(request);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Meeting request submitted successfully." });
        }

        [HttpGet("my/requests")]
        public async Task<IActionResult> GetMyRequests()
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return Unauthorized();

            var requests = await _context.Set<MeetingRequest>()
                .Include(r => r.Advisor)
                .Where(r => r.StudentId == student.StudentId)
                .OrderByDescending(r => r.RequestedAt)
                .Select(r => new
                {
                    r.RequestId,
                    AdvisorName = r.Advisor.Name,
                    r.StartAt,
                    r.EndAt,
                    r.Status,
                    r.Reason,
                    r.RejectionReason
                })
                .ToListAsync();

            return Ok(requests);
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

        private static DateTimeOffset BuildBeirutDateTime(DateOnly date, TimeSpan time)
        {
            var tz = GetBeirutTimeZone();

            var localDateTime = new DateTime(
                date.Year,
                date.Month,
                date.Day,
                time.Hours,
                time.Minutes,
                time.Seconds,
                DateTimeKind.Unspecified);

            var offset = tz.GetUtcOffset(localDateTime);
            return new DateTimeOffset(localDateTime, offset);
        }

        private static DateTimeOffset GetBeirutNow()
        {
            var tz = GetBeirutTimeZone();
            return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz);
        }

        private static TimeZoneInfo GetBeirutTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Beirut");
            }
            catch
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Middle East Standard Time");
            }
        }
    }
}