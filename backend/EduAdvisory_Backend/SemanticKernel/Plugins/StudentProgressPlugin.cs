using System.ComponentModel;
using EduAdvisory_Backend.Interfaces.Services.AI;
using EduAdvisory_Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;

namespace EduAdvisory_Backend.SemanticKernel.Plugins;

public class StudentProgressPlugin
{
    private readonly EduAdvisoryDbContext _dbContext;
    private readonly ICurrentAiStudentContext _studentContext;

    public StudentProgressPlugin(
        EduAdvisoryDbContext dbContext,
        ICurrentAiStudentContext studentContext)
    {
        _dbContext = dbContext;
        _studentContext = studentContext;
    }

    [KernelFunction]
    [Description("Gets the current student's grades or assessment results for a specific course. Use this when the student asks about grades, marks, assessment performance, or course progress. If the student mentions a course by name (not code), first call GetMyCurrentCoursesAsync to find the exact course code, then call this function with that code.")]
    public async Task<string> GetMyCourseProgressAsync(
        [Description("The exact course code (e.g. PJMG101-EC00). If unknown, call GetMyCurrentCoursesAsync first to resolve it.")]
        string courseCode)
    {
        var studentId = RequireStudentId();

        // Try exact code match first, then fall back to name-based search
        var enrollment = await _dbContext.SisCurrentEnrollments
            .AsNoTracking()
            .Where(e => e.StudentId == studentId &&
                        (e.CourseCode == courseCode ||
                         e.CourseCodeNavigation.CourseName.ToLower().Contains(courseCode.ToLower())))
            .Select(e => new { e.CourseCode, CourseName = e.CourseCodeNavigation.CourseName })
            .FirstOrDefaultAsync();

        if (enrollment == null)
        {
            return $"The student is not enrolled in any course matching \"{courseCode}\". Use GetMyCurrentCoursesAsync to see enrolled courses.";
        }

        var resolvedCode = enrollment.CourseCode;

        var grades = await _dbContext.SisStudentGrades
            .AsNoTracking()
            .Where(g => g.StudentId == studentId && g.CourseCode == resolvedCode)
            .Select(g => new
            {
                g.GradeId,
                g.Grade
            })
            .ToListAsync();

        if (!grades.Any())
        {
            return $"No grade records were found for {enrollment.CourseName} ({resolvedCode}).";
        }

        return $"Grade/progress records for {enrollment.CourseName} ({resolvedCode}):\n" +
               string.Join("\n", grades.Select(g =>
                   $"- Grade: {g.Grade}"));
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