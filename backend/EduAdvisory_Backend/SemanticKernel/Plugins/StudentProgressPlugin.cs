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
    [Description("Gets the current student's grades or assessment results for a specific course. Use this when the student asks about grades, marks, assessment performance, or course progress.")]
    public async Task<string> GetMyCourseProgressAsync(
        [Description("The course code, for example CS201.")]
        string courseCode)
    {
        var studentId = RequireStudentId();

        var isEnrolled = await _dbContext.SisCurrentEnrollments
            .AsNoTracking()
            .AnyAsync(e => e.StudentId == studentId && e.CourseCode == courseCode);

        if (!isEnrolled)
        {
            return $"The student is not enrolled in {courseCode}.";
        }

        var grades = await _dbContext.SisStudentGrades
            .AsNoTracking()
            .Where(g => g.StudentId == studentId && g.CourseCode == courseCode)
            .Select(g => new
            {
                g.GradeId,
                g.Grade
            })
            .ToListAsync();

        if (!grades.Any())
        {
            return $"No grade records were found for {courseCode}.";
        }

        return $"Grade/progress records for {courseCode}:\n" +
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