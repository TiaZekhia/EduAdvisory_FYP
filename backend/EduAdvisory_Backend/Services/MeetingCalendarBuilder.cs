using EduAdvisory_Backend.DTOs.Meetings;
using EduAdvisory_Backend.Models;

namespace EduAdvisory_Backend.Services;

public static class MeetingCalendarBuilder
{
    private static readonly int[] AllowedDurations = [15, 30, 45, 60];

    public static List<AdvisorCalendarStartTimeDto> BuildAvailableStartTimes(
        DateOnly targetDate,
        AdvisorAvailabilityRule rule,
        IEnumerable<Meeting> existingMeetings,
        IEnumerable<MeetingRequest> pendingRequests)
    {
        var result = new List<AdvisorCalendarStartTimeDto>();

        var localDayStart = GetBeirutDayStart(targetDate);
        var windowStartLocal = localDayStart.Add(rule.StartTime);
        var windowEndLocal = localDayStart.Add(rule.EndTime);

        var cursorLocal = windowStartLocal;

        while (cursorLocal < windowEndLocal)
        {
            var allowedDurations = new List<int>();

            foreach (var duration in AllowedDurations)
            {
                var slotEndLocal = cursorLocal.AddMinutes(duration);

                if (slotEndLocal > windowEndLocal)
                    continue;

                var slotStartUtc = cursorLocal.ToUniversalTime();
                var slotEndUtc = slotEndLocal.ToUniversalTime();

                var meetingOverlap = existingMeetings.Any(m =>
                    string.Equals(m.Status, "UPCOMING", StringComparison.OrdinalIgnoreCase) &&
                    m.StartAt < slotEndUtc &&
                    m.EndAt > slotStartUtc);

                var pendingOverlap = pendingRequests.Any(r =>
                    string.Equals(r.Status, "PENDING", StringComparison.OrdinalIgnoreCase) &&
                    r.StartAt < slotEndUtc &&
                    r.EndAt > slotStartUtc);

                if (!meetingOverlap && !pendingOverlap && cursorLocal > GetBeirutNow())
                    allowedDurations.Add(duration);
            }

            if (allowedDurations.Count > 0)
            {
                result.Add(new AdvisorCalendarStartTimeDto
                {
                    StartAt = cursorLocal,
                    AllowedDurations = allowedDurations
                });
            }

            cursorLocal = cursorLocal.AddMinutes(15);
        }

        return result;
    }

    private static DateTimeOffset GetBeirutDayStart(DateOnly date)
    {
        var tz = GetBeirutTimeZone();

        var localDateTime = new DateTime(
            date.Year,
            date.Month,
            date.Day,
            0, 0, 0,
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