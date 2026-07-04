using System.ComponentModel;
using EduAdvisory_Backend.Interfaces.Services.AI;
using EduAdvisory_Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;

namespace EduAdvisory_Backend.SemanticKernel.Plugins;

public class StudentAttendancePlugin
{
    private readonly EduAdvisoryDbContext _dbContext;
    private readonly ICurrentAiStudentContext _studentContext;

    public StudentAttendancePlugin(
        EduAdvisoryDbContext dbContext,
        ICurrentAiStudentContext studentContext)
    {
        _dbContext = dbContext;
        _studentContext = studentContext;
    }

    [KernelFunction]
    [Description("Gets the current student's attendance and absence records for all enrolled courses or a specific course. Use this when the student asks about absences, attendance, how many classes they missed, or whether they are at risk of being barred.")]
    public async Task<string> GetMyAttendanceAsync(
        [Description("Optional course code to filter by a specific course. Leave empty to get attendance for all courses.")]
        string? courseCode = null)
    {
        var studentId = RequireStudentId();

        var query = _dbContext.SisCourseAssessments
            .AsNoTracking()
            .Where(a => a.StudentId == studentId);

        if (!string.IsNullOrWhiteSpace(courseCode))
            query = query.Where(a =>
                a.CourseCode == courseCode ||
                a.CourseCodeNavigation.CourseName.ToLower().Contains(courseCode.ToLower()));

        var records = await query
            .Select(a => new
            {
                a.CourseCode,
                CourseName = a.CourseCodeNavigation.CourseName,
                a.AbsencesCount,
                a.MaxAbsences,
                a.LastUpdated
            })
            .ToListAsync();

        if (!records.Any())
            return courseCode is null
                ? "No attendance records were found."
                : $"No attendance records were found for \"{courseCode}\".";

        return "Attendance records:\n" +
               string.Join("\n", records.Select(r =>
               {
                   var remaining = (r.MaxAbsences ?? 0) - (r.AbsencesCount ?? 0);
                   var warning = remaining <= 1 ? " ⚠️ At risk of being barred!" : "";
                   return $"- {r.CourseName} ({r.CourseCode}): {r.AbsencesCount ?? 0} absences out of {r.MaxAbsences ?? 0} allowed ({remaining} remaining){warning}";
               }));
    }

    private int RequireStudentId()
    {
        if (_studentContext.StudentId == null)
            throw new InvalidOperationException("Student context is not available.");
        return _studentContext.StudentId.Value;
    }
}
