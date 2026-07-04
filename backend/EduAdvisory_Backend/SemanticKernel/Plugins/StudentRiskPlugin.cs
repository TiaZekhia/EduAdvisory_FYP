using System.ComponentModel;
using EduAdvisory_Backend.Interfaces.Services.AI;
using EduAdvisory_Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;

namespace EduAdvisory_Backend.SemanticKernel.Plugins;

public class StudentRiskPlugin
{
    private readonly EduAdvisoryDbContext _dbContext;
    private readonly ICurrentAiStudentContext _studentContext;

    public StudentRiskPlugin(
        EduAdvisoryDbContext dbContext,
        ICurrentAiStudentContext studentContext)
    {
        _dbContext = dbContext;
        _studentContext = studentContext;
    }

    [KernelFunction]
    [Description("Gets the current student's academic risk assessment across their enrolled courses. Use this when the student asks about their risk level, whether they are at risk of failing, or how they are performing academically overall.")]
    public async Task<string> GetMyRiskStatusAsync()
    {
        var studentId = RequireStudentId();

        var risks = await _dbContext.StudentRisks
            .AsNoTracking()
            .Where(r => r.StudentId == studentId)
            .OrderByDescending(r => r.RiskScore)
            .Select(r => new
            {
                r.CourseCode,
                CourseName = r.CourseCodeNavigation != null ? r.CourseCodeNavigation.CourseName : r.CourseCode,
                r.RiskLevel,
                r.RiskScore,
                r.CalculatedAt
            })
            .ToListAsync();

        if (!risks.Any())
            return "No risk assessment records were found for your courses.";

        var highRisk = risks.Where(r => r.RiskLevel == "HIGH").ToList();
        var summary = highRisk.Any()
            ? $"⚠️ You have {highRisk.Count} high-risk course(s) that need immediate attention.\n"
            : "No high-risk courses detected.\n";

        return summary + "Risk assessment by course:\n" +
               string.Join("\n", risks.Select(r =>
                   $"- {r.CourseName} ({r.CourseCode}): Risk level = {r.RiskLevel ?? "N/A"}, Score = {r.RiskScore?.ToString("0.00") ?? "N/A"}"));
    }

    private int RequireStudentId()
    {
        if (_studentContext.StudentId == null)
            throw new InvalidOperationException("Student context is not available.");
        return _studentContext.StudentId.Value;
    }
}
