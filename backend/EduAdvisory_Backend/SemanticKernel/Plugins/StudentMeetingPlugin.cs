using System.ComponentModel;
using EduAdvisory_Backend.Interfaces.Services.AI;
using EduAdvisory_Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;

namespace EduAdvisory_Backend.SemanticKernel.Plugins;

public class StudentMeetingPlugin
{
    private readonly EduAdvisoryDbContext _dbContext;
    private readonly ICurrentAiStudentContext _studentContext;

    public StudentMeetingPlugin(
        EduAdvisoryDbContext dbContext,
        ICurrentAiStudentContext studentContext)
    {
        _dbContext = dbContext;
        _studentContext = studentContext;
    }

    [KernelFunction]
    [Description("Gets the current student's upcoming advisor meetings.")]
    public async Task<string> GetMyUpcomingMeetingsAsync()
    {
        var studentId = RequireStudentId();
        var now = DateTime.UtcNow;

        var meetings = await _dbContext.Meetings
            .AsNoTracking()
            .Where(m => m.StudentId == studentId && m.StartAt >= now)
            .OrderBy(m => m.StartAt)
            .Take(5)
            .Select(m => new
            {
                m.MeetingId,
                m.StartAt,
                m.EndAt,
                m.Status,
                m.MeetingType
            })
            .ToListAsync();

        if (!meetings.Any())
        {
            return "No upcoming advisor meetings were found.";
        }

        return "Upcoming advisor meetings:\n" +
               string.Join("\n", meetings.Select(m =>
                   $"- {m.StartAt:yyyy-MM-dd HH:mm} to {m.EndAt:HH:mm}, Type: {m.MeetingType}, Status: {m.Status}"));
    }

    private int RequireStudentId()
    {
        if (_studentContext.StudentId == null)
        {
            throw new InvalidOperationException("Student context is not available.");
        }

        return _studentContext.StudentId.Value;
    }
}