using EduAdvisory_Backend.DTOs.Advisor;
using EduAdvisory_Backend.Interfaces.Repositories;
using EduAdvisory_Backend.Models;

namespace EduAdvisory_Backend.Repositories
{
    public class AdvisorRepository : IAdvisorRepository
    {
        private readonly EduAdvisoryDbContext _context;

        public AdvisorRepository(EduAdvisoryDbContext context)
        {
            _context = context;
        }

        public Advisor GetById(int advisorId)
        {
            return _context.Advisors.FirstOrDefault(a => a.AdvisorId == advisorId);
        }

        public Advisor GetByUsername(string username)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (user == null || user.LinkedAdvisorId == null)
                return null;

            return _context.Advisors.FirstOrDefault(a => a.AdvisorId == user.LinkedAdvisorId);
        }

        public AdvisorSummaryDto GetAdvisorSummary(int advisorId)
        {
            var advisor = _context.Advisors.FirstOrDefault(a => a.AdvisorId == advisorId);

            if (advisor == null)
                return null;

            return new AdvisorSummaryDto
            {
                AdvisorId = advisor.AdvisorId,
                Name = advisor.Name,
                Email = advisor.Email
            };
        }

        public AdvisorDashboardSummaryDto GetDashboardSummary(int advisorId)
        {
            var now = DateTime.UtcNow;

            var studentsQuery = _context.SisStudents.Where(s => s.AdvisorId == advisorId);

            var totalStudents = studentsQuery.Count();

            var onProbation = studentsQuery.Count(s =>
                s.AcademicStatus != null &&
                s.AcademicStatus.ToUpper() == "PROBATION");

            var averageGpa = studentsQuery.Any(s => s.CurrentGpa.HasValue)
                ? Math.Round(studentsQuery
                    .Where(s => s.CurrentGpa.HasValue)
                    .Average(s => s.CurrentGpa!.Value), 2)
                : 0;

            var upcomingMeetings = _context.Meetings.Count(m =>
                m.AdvisorId == advisorId &&
                m.MeetingDate.HasValue &&
                m.MeetingDate.Value.UtcDateTime >= now);

            return new AdvisorDashboardSummaryDto
            {
                TotalStudents = totalStudents,
                OnProbation = onProbation,
                AverageGpa = averageGpa,
                UpcomingMeetings = upcomingMeetings
            };
        }
        public List<AdvisorStudentOverviewDto> GetStudentsOverview(int advisorId)
        {
            return _context.SisStudents
                .Where(s => s.AdvisorId == advisorId)
                .OrderBy(s => s.StudentId)
                .Select(s => new AdvisorStudentOverviewDto
                {
                    StudentId = s.StudentId,
                    Name = ((s.FirstName ?? "") + " " + (s.LastName ?? "")).Trim(),
                    CurrentSemester = s.CurrentSemester ?? 0,
                    Gpa = s.CurrentGpa ?? 0,
                    AcademicStatus = s.AcademicStatus ?? "GOOD",
                    ProgramCode = s.ProgramCode ?? ""
                })
                .ToList();
        }

        public List<AdvisorRecentActivityDto> GetRecentActivity(int advisorId, int limit = 10)
        {
            var now = DateTimeOffset.UtcNow;

            var meetingActivities = _context.Meetings
                .Where(m => m.AdvisorId == advisorId)
                .Join(
                    _context.SisStudents,
                    m => m.StudentId,
                    s => s.StudentId,
                    (m, s) => new AdvisorRecentActivityDto
                    {
                        Id = "meeting-" + m.MeetingId,
                        Title = "Meeting scheduled with " + (((s.FirstName ?? "") + " " + (s.LastName ?? "")).Trim()),
                        CreatedAt = m.CreatedAt ?? now,
                        TimeAgo = ""
                    })
                .ToList();

            var announcementRecipientCount = _context.SisStudents.Count(s => s.AdvisorId == advisorId);

            var announcementActivities = _context.Announcements
                .Where(a => a.AdvisorId == advisorId)
                .Select(a => new AdvisorRecentActivityDto
                {
                    Id = "announcement-" + a.AnnouncementId,
                    Title = $"Broadcast message sent to {announcementRecipientCount} students",
                    CreatedAt = a.CreatedAt.HasValue
                        ? new DateTimeOffset(DateTime.SpecifyKind(a.CreatedAt.Value, DateTimeKind.Utc))
                        : now,
                    TimeAgo = ""
                })
                .AsEnumerable()
                .ToList();

            var activities = meetingActivities
                .Concat(announcementActivities)
                .OrderByDescending(a => a.CreatedAt)
                .Take(limit)
                .ToList();

            foreach (var activity in activities)
            {
                activity.TimeAgo = ToTimeAgo(activity.CreatedAt.UtcDateTime, now.UtcDateTime);
            }

            return activities;
        }
        public List<AdvisorMeetingDto> GetUpcomingMeetings(int advisorId, int limit = 5)
        {
            var now = DateTime.UtcNow;

            return _context.Meetings
                .Where(m => m.AdvisorId == advisorId && m.MeetingDate >= now)
                .OrderBy(m => m.MeetingDate)
                .Take(limit)
                .Join(_context.SisStudents,
                    m => m.StudentId,
                    s => s.StudentId,
                    (m, s) => new { m, s })
                .AsEnumerable()
                .Select(x => new AdvisorMeetingDto
                {
                    MeetingId = x.m.MeetingId,
                    StudentId = x.s.StudentId,
                    StudentName = (x.s.FirstName ?? "") + " " + (x.s.LastName ?? ""),
                    MeetingDate = x.m.MeetingDate.HasValue ? x.m.MeetingDate.Value.UtcDateTime : DateTime.UtcNow,
                    MeetingType = x.m.MeetingType ?? "",
                    Title = ToMeetingTitle(x.m.MeetingType ?? ""),
                    Status = "scheduled",
                    Notes = x.m.Notes
                })
                .ToList();
        }

        public List<AdvisorMeetingDto> GetPastMeetings(int advisorId, int limit = 10)
        {
            var now = DateTime.UtcNow;

            return _context.Meetings
                .Where(m => m.AdvisorId == advisorId && m.MeetingDate < now)
                .OrderByDescending(m => m.MeetingDate)
                .Take(limit)
                .Join(_context.SisStudents,
                    m => m.StudentId,
                    s => s.StudentId,
                    (m, s) => new { m, s })
                .AsEnumerable()
                .Select(x => new AdvisorMeetingDto
                {
                    MeetingId = x.m.MeetingId,
                    StudentId = x.s.StudentId,
                    StudentName = (x.s.FirstName ?? "") + " " + (x.s.LastName ?? ""),
                    MeetingDate = x.m.MeetingDate.HasValue ? x.m.MeetingDate.Value.UtcDateTime : DateTime.UtcNow,
                    MeetingType = x.m.MeetingType ?? "",
                    Title = ToMeetingTitle(x.m.MeetingType ?? ""),
                    Status = "completed",
                    Notes = x.m.Notes
                })
                .ToList();
        }

        public AdvisorMessagesSummaryDto GetMessagesSummary(int advisorId)
        {
            var now = DateTime.UtcNow;
            var sevenDaysAgo = now.AddDays(-7);
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var announcements = _context.Announcements
                .Where(a => a.AdvisorId == advisorId)
                .AsEnumerable()
                .ToList();

            return new AdvisorMessagesSummaryDto
            {
                TotalMessages = announcements.Count,
                Recent7Days = announcements.Count(a => (a.CreatedAt ?? now) >= sevenDaysAgo),
                ThisMonth = announcements.Count(a => (a.CreatedAt ?? now) >= monthStart)
            };
        }

        public List<AdvisorMessageDto> GetMessages(int advisorId, int limit = 20)
        {
            var recipientsCount = _context.SisStudents.Count(s => s.AdvisorId == advisorId);
            var now = DateTime.UtcNow;

            return _context.Announcements
                .Where(a => a.AdvisorId == advisorId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(limit)
                .AsEnumerable()
                .Select(a => new AdvisorMessageDto
                {
                    AnnouncementId = a.AnnouncementId,
                    Title = a.Title ?? "",
                    Content = a.Content ?? "",
                    CreatedAt = a.CreatedAt ?? now,
                    RecipientsCount = recipientsCount
                })
                .ToList();
        }

        private static string ToMeetingTitle(string meetingType)
        {
            return meetingType switch
            {
                "MID_SEMESTER" => "Mid-Semester Progress Check",
                "COURSE_SELECTION" => "Course Selection Discussion",
                "PROGRESS_REVIEW" => "Progress Review",
                _ => meetingType.Replace("_", " ")
            };
        }
        private static string ToTimeAgo(DateTime dateTime, DateTime now)
        {
            var span = now - dateTime;

            if (span.TotalMinutes < 1)
                return "Just now";

            if (span.TotalMinutes < 60)
                return $"{(int)span.TotalMinutes} minute{((int)span.TotalMinutes == 1 ? "" : "s")} ago";

            if (span.TotalHours < 24)
                return $"{(int)span.TotalHours} hour{((int)span.TotalHours == 1 ? "" : "s")} ago";

            if (span.TotalDays < 7)
                return $"{(int)span.TotalDays} day{((int)span.TotalDays == 1 ? "" : "s")} ago";

            return dateTime.ToString("dd MMM yyyy");
        }
    }
}