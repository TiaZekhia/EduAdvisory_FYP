using System.ComponentModel;
using EduAdvisory_Backend.Interfaces.Services.AI;
using EduAdvisory_Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;

namespace EduAdvisory_Backend.SemanticKernel.Plugins;

public class StudentCourseHistoryPlugin
{
    private readonly EduAdvisoryDbContext _dbContext;
    private readonly ICurrentAiStudentContext _studentContext;

    public StudentCourseHistoryPlugin(
        EduAdvisoryDbContext dbContext,
        ICurrentAiStudentContext studentContext)
    {
        _dbContext = dbContext;
        _studentContext = studentContext;
    }

    [KernelFunction]
    [Description("Gets the current student's full academic course history — past courses taken, final grades, and statuses across all semesters. Use this when the student asks about their transcript, previously completed courses, past grades, or academic history.")]
    public async Task<string> GetMyCourseHistoryAsync()
    {
        var studentId = RequireStudentId();

        var history = await _dbContext.SisStudentCourseHistories
            .AsNoTracking()
            .Where(h => h.StudentId == studentId)
            .OrderBy(h => h.Semester)
            .Select(h => new
            {
                h.CourseCode,
                CourseName = h.CourseCodeNavigation.CourseName,
                h.Semester,
                h.FinalGrade,
                h.Status
            })
            .ToListAsync();

        if (!history.Any())
            return "No course history was found.";

        var grouped = history
            .GroupBy(h => h.Semester ?? "Unknown")
            .OrderBy(g => g.Key);

        var lines = new List<string>();
        foreach (var semester in grouped)
        {
            lines.Add($"Semester {semester.Key}:");
            foreach (var c in semester)
                lines.Add($"  - {c.CourseName} ({c.CourseCode}): Grade {c.FinalGrade?.ToString("0.00") ?? "N/A"}, Status: {c.Status ?? "N/A"}");
        }

        return "Academic course history:\n" + string.Join("\n", lines);
    }

    private int RequireStudentId()
    {
        if (_studentContext.StudentId == null)
            throw new InvalidOperationException("Student context is not available.");
        return _studentContext.StudentId.Value;
    }
}
