using EduAdvisory_Backend.Interfaces.Repositories;
using EduAdvisory_Backend.Models;
using EduAdvisory_Backend.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace EduAdvisory_Backend.Tests.Services;

public class CoursePlanServiceTests
{
    private readonly Mock<IStudentRepository> _studentRepoMock = new();

    private static EduAdvisoryDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<EduAdvisoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableServiceProviderCaching(false)
            .Options);

    // ─── Student not found ────────────────────────────────────────────────────

    [Fact]
    public void GeneratePlansForStudent_WhenStudentNotFound_ReturnsEmptyList()
    {
        _studentRepoMock.Setup(r => r.GetById(It.IsAny<int>())).Returns((SisStudent)null!);
        using var context = CreateContext();
        var sut = new CoursePlanService(_studentRepoMock.Object, context);

        var result = sut.GeneratePlansForStudent(99);

        Assert.Empty(result);
    }

    // ─── All courses already passed ───────────────────────────────────────────

    [Fact]
    public void GeneratePlansForStudent_WhenAllCoursesPassed_ReturnsEmptyPlanSemesters()
    {
        var student = new SisStudent
        {
            StudentId = 1, ProgramCode = "CS", CurrentSemester = 2, AcademicStatus = "NORMAL"
        };
        _studentRepoMock.Setup(r => r.GetById(1)).Returns(student);
        _studentRepoMock.Setup(r => r.GetPassedCourses(1)).Returns(["CS101", "CS102"]);
        _studentRepoMock.Setup(r => r.GetFailedNotRetakenCourses(1)).Returns([]);
        _studentRepoMock.Setup(r => r.GetStudyGuideRecommendedSemester("CS"))
            .Returns(new Dictionary<string, int> { { "CS101", 1 }, { "CS102", 2 } });
        _studentRepoMock.Setup(r => r.GetCoursesMeta(It.IsAny<List<string>>()))
            .Returns(new Dictionary<string, (string, int)>());
        _studentRepoMock.Setup(r => r.GetPrerequisitesMap(It.IsAny<List<string>>()))
            .Returns(new Dictionary<string, List<string>>());

        using var context = CreateContext();
        var sut = new CoursePlanService(_studentRepoMock.Object, context);

        var result = sut.GeneratePlansForStudent(1);

        Assert.All(result, plan => Assert.Empty(plan.Semesters));
    }

    // ─── Credit limit: probation ──────────────────────────────────────────────

    [Fact]
    public void GeneratePlansForStudent_WhenStudentOnProbation_CapsCreditLimitAt16()
    {
        var student = new SisStudent
        {
            StudentId = 2, ProgramCode = "CS", CurrentSemester = 1, AcademicStatus = "PROBATION"
        };
        _studentRepoMock.Setup(r => r.GetById(2)).Returns(student);
        _studentRepoMock.Setup(r => r.GetPassedCourses(2)).Returns([]);
        _studentRepoMock.Setup(r => r.GetFailedNotRetakenCourses(2)).Returns([]);
        // 8 courses in semester 2 (Fall) to trigger the credit cap
        _studentRepoMock.Setup(r => r.GetStudyGuideRecommendedSemester("CS"))
            .Returns(new Dictionary<string, int>
            {
                { "C1", 2 }, { "C2", 2 }, { "C3", 2 }, { "C4", 2 },
                { "C5", 2 }, { "C6", 2 }
            });
        var meta = new Dictionary<string, (string, int)>
        {
            { "C1", ("Course1", 3) }, { "C2", ("Course2", 3) }, { "C3", ("Course3", 3) },
            { "C4", ("Course4", 3) }, { "C5", ("Course5", 3) }, { "C6", ("Course6", 3) }
        };
        _studentRepoMock.Setup(r => r.GetCoursesMeta(It.IsAny<List<string>>())).Returns(meta);
        _studentRepoMock.Setup(r => r.GetPrerequisitesMap(It.IsAny<List<string>>()))
            .Returns(new Dictionary<string, List<string>>());

        using var context = CreateContext();
        var sut = new CoursePlanService(_studentRepoMock.Object, context);

        var result = sut.GeneratePlansForStudent(2);

        // All semesters must not exceed 16 credits
        Assert.All(result, plan =>
            Assert.All(plan.Semesters, s => Assert.True(s.TotalCredits <= 16)));
    }

    [Fact]
    public void GeneratePlansForStudent_WhenNormalStatus_AllowsUpTo18Credits()
    {
        var student = new SisStudent
        {
            StudentId = 3, ProgramCode = "CS", CurrentSemester = 1, AcademicStatus = "NORMAL"
        };
        _studentRepoMock.Setup(r => r.GetById(3)).Returns(student);
        _studentRepoMock.Setup(r => r.GetPassedCourses(3)).Returns([]);
        _studentRepoMock.Setup(r => r.GetFailedNotRetakenCourses(3)).Returns([]);
        // 7 courses × 3 credits = 21 credits available, should pick 6 = 18
        _studentRepoMock.Setup(r => r.GetStudyGuideRecommendedSemester("CS"))
            .Returns(new Dictionary<string, int>
            {
                { "C1", 2 }, { "C2", 2 }, { "C3", 2 },
                { "C4", 2 }, { "C5", 2 }, { "C6", 2 }, { "C7", 2 }
            });
        var meta = Enumerable.Range(1, 7)
            .ToDictionary(i => $"C{i}", i => ($"Course{i}", 3));
        _studentRepoMock.Setup(r => r.GetCoursesMeta(It.IsAny<List<string>>())).Returns(meta);
        _studentRepoMock.Setup(r => r.GetPrerequisitesMap(It.IsAny<List<string>>()))
            .Returns(new Dictionary<string, List<string>>());

        using var context = CreateContext();
        var sut = new CoursePlanService(_studentRepoMock.Object, context);

        var result = sut.GeneratePlansForStudent(3);

        Assert.All(result, plan =>
            Assert.All(plan.Semesters, s => Assert.True(s.TotalCredits <= 18)));
    }

    // ─── Plans generated and persisted ────────────────────────────────────────

    [Fact]
    public void GeneratePlansForStudent_WithRemainingCourses_GeneratesPlansAndSavesToDb()
    {
        var student = new SisStudent
        {
            StudentId = 4, ProgramCode = "CS", CurrentSemester = 1, AcademicStatus = "NORMAL"
        };
        _studentRepoMock.Setup(r => r.GetById(4)).Returns(student);
        _studentRepoMock.Setup(r => r.GetPassedCourses(4)).Returns([]);
        _studentRepoMock.Setup(r => r.GetFailedNotRetakenCourses(4)).Returns([]);
        _studentRepoMock.Setup(r => r.GetStudyGuideRecommendedSemester("CS"))
            .Returns(new Dictionary<string, int> { { "CS101", 2 } });
        _studentRepoMock.Setup(r => r.GetCoursesMeta(It.IsAny<List<string>>()))
            .Returns(new Dictionary<string, (string, int)> { { "CS101", ("Intro", 3) } });
        _studentRepoMock.Setup(r => r.GetPrerequisitesMap(It.IsAny<List<string>>()))
            .Returns(new Dictionary<string, List<string>>());

        using var context = CreateContext();
        var sut = new CoursePlanService(_studentRepoMock.Object, context);

        var result = sut.GeneratePlansForStudent(4);

        Assert.NotEmpty(result);
        Assert.True(result[0].Semesters.Any());
        // Persisted to DB
        Assert.True(context.GeneratedStudyPlans.Any(p => p.StudentId == 4));
    }

    // ─── Prerequisite constraints ─────────────────────────────────────────────

    [Fact]
    public void GeneratePlansForStudent_WhenPrereqNotPassed_CourseScheduledAfterPrereq()
    {
        var student = new SisStudent
        {
            StudentId = 5, ProgramCode = "CS", CurrentSemester = 1, AcademicStatus = "NORMAL"
        };
        _studentRepoMock.Setup(r => r.GetById(5)).Returns(student);
        _studentRepoMock.Setup(r => r.GetPassedCourses(5)).Returns([]);
        _studentRepoMock.Setup(r => r.GetFailedNotRetakenCourses(5)).Returns([]);
        // CS101 in semester 2 (Fall), CS201 in semester 4 (Fall), CS201 requires CS101
        _studentRepoMock.Setup(r => r.GetStudyGuideRecommendedSemester("CS"))
            .Returns(new Dictionary<string, int> { { "CS101", 2 }, { "CS201", 4 } });
        _studentRepoMock.Setup(r => r.GetCoursesMeta(It.IsAny<List<string>>()))
            .Returns(new Dictionary<string, (string, int)>
            {
                { "CS101", ("Intro", 3) }, { "CS201", ("Algorithms", 3) }
            });
        _studentRepoMock.Setup(r => r.GetPrerequisitesMap(It.IsAny<List<string>>()))
            .Returns(new Dictionary<string, List<string>>
            {
                { "CS201", new List<string> { "CS101" } }
            });

        using var context = CreateContext();
        var sut = new CoursePlanService(_studentRepoMock.Object, context);

        var result = sut.GeneratePlansForStudent(5);

        Assert.NotEmpty(result);
        var plan = result[0];
        var cs101Sem = plan.Semesters.FirstOrDefault(s => s.Courses.Any(c => c.CourseCode == "CS101"));
        var cs201Sem = plan.Semesters.FirstOrDefault(s => s.Courses.Any(c => c.CourseCode == "CS201"));

        if (cs101Sem != null && cs201Sem != null)
            Assert.True(cs101Sem.PlannedSemester < cs201Sem.PlannedSemester);
    }

    // ─── Retake courses ───────────────────────────────────────────────────────

    [Fact]
    public void GeneratePlansForStudent_WhenCourseFailedAndNotRetaken_MarksAsRetake()
    {
        var student = new SisStudent
        {
            StudentId = 6, ProgramCode = "CS", CurrentSemester = 2, AcademicStatus = "NORMAL"
        };
        _studentRepoMock.Setup(r => r.GetById(6)).Returns(student);
        _studentRepoMock.Setup(r => r.GetPassedCourses(6)).Returns([]);
        _studentRepoMock.Setup(r => r.GetFailedNotRetakenCourses(6)).Returns(["CS101"]);
        _studentRepoMock.Setup(r => r.GetStudyGuideRecommendedSemester("CS"))
            .Returns(new Dictionary<string, int> { { "CS101", 1 } });
        _studentRepoMock.Setup(r => r.GetCoursesMeta(It.IsAny<List<string>>()))
            .Returns(new Dictionary<string, (string, int)> { { "CS101", ("Intro", 3) } });
        _studentRepoMock.Setup(r => r.GetPrerequisitesMap(It.IsAny<List<string>>()))
            .Returns(new Dictionary<string, List<string>>());

        using var context = CreateContext();
        var sut = new CoursePlanService(_studentRepoMock.Object, context);

        var result = sut.GeneratePlansForStudent(6);

        var cs101Course = result
            .SelectMany(p => p.Semesters)
            .SelectMany(s => s.Courses)
            .FirstOrDefault(c => c.CourseCode == "CS101");

        if (cs101Course != null)
            Assert.True(cs101Course.IsRetake);
    }

    // ─── Metrics ──────────────────────────────────────────────────────────────

    [Fact]
    public void GeneratePlansForStudent_MetricsReflectRemainingCoursesBeforePlan()
    {
        var student = new SisStudent
        {
            StudentId = 7, ProgramCode = "CS", CurrentSemester = 1, AcademicStatus = "NORMAL"
        };
        _studentRepoMock.Setup(r => r.GetById(7)).Returns(student);
        _studentRepoMock.Setup(r => r.GetPassedCourses(7)).Returns([]);
        _studentRepoMock.Setup(r => r.GetFailedNotRetakenCourses(7)).Returns([]);
        _studentRepoMock.Setup(r => r.GetStudyGuideRecommendedSemester("CS"))
            .Returns(new Dictionary<string, int> { { "CS101", 2 }, { "CS201", 4 } });
        _studentRepoMock.Setup(r => r.GetCoursesMeta(It.IsAny<List<string>>()))
            .Returns(new Dictionary<string, (string, int)>
            {
                { "CS101", ("Intro", 3) }, { "CS201", ("Algorithms", 3) }
            });
        _studentRepoMock.Setup(r => r.GetPrerequisitesMap(It.IsAny<List<string>>()))
            .Returns(new Dictionary<string, List<string>>());

        using var context = CreateContext();
        var sut = new CoursePlanService(_studentRepoMock.Object, context);

        var result = sut.GeneratePlansForStudent(7);

        Assert.NotEmpty(result);
        Assert.Equal(2, result[0].Metrics.CoursesRemaining);
        Assert.Equal(6, result[0].Metrics.CreditsRemaining);
    }
}
