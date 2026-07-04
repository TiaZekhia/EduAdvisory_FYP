using System.ComponentModel;
using EduAdvisory_Backend.Interfaces.Services.AI;
using EduAdvisory_Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;

namespace EduAdvisory_Backend.SemanticKernel.Plugins;

public class StudentStudyPlanPlugin
{
    private readonly EduAdvisoryDbContext _dbContext;
    private readonly ICurrentAiStudentContext _studentContext;

    public StudentStudyPlanPlugin(
        EduAdvisoryDbContext dbContext,
        ICurrentAiStudentContext studentContext)
    {
        _dbContext = dbContext;
        _studentContext = studentContext;
    }

    [KernelFunction]
    [Description("Gets the current student's saved study plan (Balanced strategy) — the courses planned for each upcoming semester. Use this when the student asks about their study plan, future courses, graduation plan, or what courses they should take next. Note: only the Balanced plan is saved; other generated variants are not stored.")]
    public async Task<string> GetMyStudyPlanAsync()
    {
        var studentId = RequireStudentId();

        var plan = await _dbContext.GeneratedStudyPlans
            .AsNoTracking()
            .Where(p => p.StudentId == studentId)
            .OrderBy(p => p.PlannedSemester)
            .Select(p => new
            {
                p.PlannedSemester,
                p.CourseCode,
                CourseName = p.CourseCodeNavigation != null ? p.CourseCodeNavigation.CourseName : p.CourseCode,
                Credits = p.CourseCodeNavigation != null ? (int?)p.CourseCodeNavigation.Credits : null,
                p.GenerationDate
            })
            .ToListAsync();

        if (!plan.Any())
            return "No study plan has been generated for you yet. Contact your advisor to create one.";

        var grouped = plan.GroupBy(p => p.PlannedSemester ?? 0).OrderBy(g => g.Key);

        var lines = new List<string>();
        foreach (var semester in grouped)
        {
            var totalCredits = semester.Sum(c => c.Credits ?? 0);
            lines.Add($"Semester {semester.Key} ({totalCredits} credits):");
            foreach (var c in semester)
                lines.Add($"  - {c.CourseName} ({c.CourseCode}){(c.Credits.HasValue ? $" — {c.Credits} credits" : "")}");
        }

        var generatedOn = plan.FirstOrDefault()?.GenerationDate;
        var header = generatedOn.HasValue ? $"Study plan (generated {generatedOn:MMM dd, yyyy}):\n" : "Study plan:\n";
        return header + string.Join("\n", lines);
    }

    [KernelFunction]
    [Description("Gets the prerequisites for a specific course. Use this when the student asks what courses they need to complete before taking a certain course.")]
    public async Task<string> GetCoursePrerequisitesAsync(
        [Description("The course code to look up prerequisites for, e.g. CS301. If the student gives a course name, pass the name and it will be matched.")]
        string courseCode)
    {
        var course = await _dbContext.SisCourses
            .AsNoTracking()
            .Where(c => c.CourseCode == courseCode ||
                        c.CourseName.ToLower().Contains(courseCode.ToLower()))
            .Select(c => new
            {
                c.CourseCode,
                c.CourseName,
                Prerequisites = c.Prerequisites.Select(p => new
                {
                    p.PrerequisiteCourseCode,
                    PrerequisiteName = p.PrerequisiteCourse.CourseName
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (course == null)
            return $"No course found matching \"{courseCode}\".";

        if (!course.Prerequisites.Any())
            return $"{course.CourseName} ({course.CourseCode}) has no prerequisites.";

        return $"Prerequisites for {course.CourseName} ({course.CourseCode}):\n" +
               string.Join("\n", course.Prerequisites.Select(p =>
                   $"- {p.PrerequisiteName} ({p.PrerequisiteCourseCode})"));
    }

    private int RequireStudentId()
    {
        if (_studentContext.StudentId == null)
            throw new InvalidOperationException("Student context is not available.");
        return _studentContext.StudentId.Value;
    }
}
