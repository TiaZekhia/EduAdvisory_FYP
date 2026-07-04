using System.ComponentModel;
using EduAdvisory_Backend.Interfaces.Services.AI;
using EduAdvisory_Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;

namespace EduAdvisory_Backend.SemanticKernel.Plugins;

public class AdvisorAnnouncementPlugin
{
    private readonly EduAdvisoryDbContext _dbContext;
    private readonly ICurrentAiStudentContext _studentContext;

    public AdvisorAnnouncementPlugin(
        EduAdvisoryDbContext dbContext,
        ICurrentAiStudentContext studentContext)
    {
        _dbContext = dbContext;
        _studentContext = studentContext;
    }

    [KernelFunction]
    [Description("Gets the latest announcements posted by the student's assigned advisor. Use this when the student asks about announcements, advisor news, updates, or notices from their advisor.")]
    public async Task<string> GetMyAdvisorAnnouncementsAsync()
    {
        var studentId = RequireStudentId();

        var advisorId = await _dbContext.SisStudents
            .AsNoTracking()
            .Where(s => s.StudentId == studentId)
            .Select(s => s.AdvisorId)
            .FirstOrDefaultAsync();

        if (advisorId == null)
            return "No advisor is assigned to you. Please contact administration.";

        var announcements = await _dbContext.Announcements
            .AsNoTracking()
            .Where(a => a.AdvisorId == advisorId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(5)
            .Select(a => new
            {
                a.Title,
                a.Content,
                a.CreatedAt,
                AdvisorName = a.Advisor != null ? a.Advisor.Name : "Your Advisor"
            })
            .ToListAsync();

        if (!announcements.Any())
            return "Your advisor has no recent announcements.";

        return $"Latest announcements from your advisor:\n" +
               string.Join("\n\n", announcements.Select(a =>
                   $"[{a.CreatedAt:MMM dd, yyyy}] {a.Title}\n{a.Content}"));
    }

    private int RequireStudentId()
    {
        if (_studentContext.StudentId == null)
            throw new InvalidOperationException("Student context is not available.");
        return _studentContext.StudentId.Value;
    }
}
