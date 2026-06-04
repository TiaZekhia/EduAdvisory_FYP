using System.ComponentModel;
using EduAdvisory_Backend.Interfaces.Services.AI;
using EduAdvisory_Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;

namespace EduAdvisory_Backend.SemanticKernel.Plugins;

public class StudentInfoPlugin
{
    private readonly EduAdvisoryDbContext _dbContext;
    private readonly ICurrentAiStudentContext _studentContext;

    public StudentInfoPlugin(
        EduAdvisoryDbContext dbContext,
        ICurrentAiStudentContext studentContext)
    {
        _dbContext = dbContext;
        _studentContext = studentContext;
    }

    [KernelFunction]
    [Description("Gets the current student's basic academic profile, including program, semester, GPA, and academic status.")]
    public async Task<string> GetMyAcademicProfileAsync()
    {
        var studentId = RequireStudentId();

        var student = await _dbContext.SisStudents
            .AsNoTracking()
            .Where(s => s.StudentId == studentId)
            .Select(s => new
            {
                s.StudentId,
                s.FirstName,
                s.LastName,
                s.ProgramCode,
                s.CurrentSemester,
                s.CurrentGpa,
                s.AcademicStatus,
                s.Email
            })
            .FirstOrDefaultAsync();

        if (student == null)
        {
            return "No student profile was found for the current user.";
        }

        return $"""
Student profile:
- Name: {student.FirstName} {student.LastName}
- Program: {student.ProgramCode}
- Current semester: {student.CurrentSemester}
- Current GPA: {student.CurrentGpa}
- Academic status: {student.AcademicStatus}
""";
    }

    [KernelFunction]
    [Description("Gets the current student's enrolled courses for the current semester.")]
    public async Task<string> GetMyCurrentCoursesAsync()
    {
        var studentId = RequireStudentId();

        var courses = await _dbContext.SisCurrentEnrollments
            .AsNoTracking()
            .Where(e => e.StudentId == studentId)
            .Select(e => new
            {
                e.CourseCode,
                CourseName = e.CourseCodeNavigation.CourseName,
                e.Semester
            })
            .ToListAsync();

        if (!courses.Any())
        {
            return "No current course enrollments were found.";
        }

        return "Current enrolled courses:\n" +
               string.Join("\n", courses.Select(c =>
                   $"- {c.CourseCode}: {c.CourseName} ({c.Semester})"));
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