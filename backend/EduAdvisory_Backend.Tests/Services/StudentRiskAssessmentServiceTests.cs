using EduAdvisory_Backend.DTOs.Course;
using EduAdvisory_Backend.DTOs.Student;
using EduAdvisory_Backend.Interfaces.Repositories;
using EduAdvisory_Backend.Models;
using EduAdvisory_Backend.Services;
using Moq;
using Xunit;

namespace EduAdvisory_Backend.Tests.Services;

public class StudentRiskAssessmentServiceTests
{
    private readonly Mock<IStudentRepository> _studentRepoMock = new();
    private readonly StudentRiskAssessmentService _sut;

    public StudentRiskAssessmentServiceTests()
    {
        _sut = new StudentRiskAssessmentService(_studentRepoMock.Object);
    }

    // ─── AssessStudent – exception ────────────────────────────────────────────

    [Fact]
    public void AssessStudent_WhenStudentNotFound_ThrowsException()
    {
        _studentRepoMock.Setup(r => r.GetById(It.IsAny<int>())).Returns((SisStudent)null!);

        Assert.Throws<Exception>(() => _sut.AssessStudent(99));
    }

    // ─── AssessStudent – GPA alerts ───────────────────────────────────────────

    [Fact]
    public void AssessStudent_WhenGpaBelow60_GeneratesHighGpaAlert()
    {
        var student = MakeStudent(1, gpa: 55m);
        SetupMinimalStudent(student);

        var result = _sut.AssessStudent(1);

        var alert = result.GlobalAlertsSummary.Alerts
            .Single(a => a.Title == "Low cumulative GPA");
        Assert.Equal("HIGH", alert.Severity);
        Assert.Equal(100, result.Factors.GpaRiskScore);
    }

    [Fact]
    public void AssessStudent_WhenGpaBetween60And70_GeneratesMediumGpaAlert()
    {
        var student = MakeStudent(2, gpa: 65m);
        SetupMinimalStudent(student);

        var result = _sut.AssessStudent(2);

        var alert = result.GlobalAlertsSummary.Alerts
            .Single(a => a.Title == "GPA needs improvement");
        Assert.Equal("MEDIUM", alert.Severity);
        Assert.Equal(60, result.Factors.GpaRiskScore);
    }

    [Fact]
    public void AssessStudent_WhenGpaAbove80_NoGpaAlert()
    {
        var student = MakeStudent(3, gpa: 90m);
        SetupMinimalStudent(student);

        var result = _sut.AssessStudent(3);

        Assert.DoesNotContain(result.GlobalAlertsSummary.Alerts,
            a => a.Title is "Low cumulative GPA" or "GPA needs improvement");
        Assert.Equal(0, result.Factors.GpaRiskScore);
    }

    // ─── AssessStudent – missing current semester courses ─────────────────────

    [Fact]
    public void AssessStudent_WhenTwoMissingCurrentSemesterCourses_GeneratesHighAlert()
    {
        var student = MakeStudent(4, semester: 2);
        SetupMinimalStudent(student, studyMap: new() { { "CS201", 2 }, { "CS202", 2 } });

        var result = _sut.AssessStudent(4);

        var alert = result.GlobalAlertsSummary.Alerts
            .Single(a => a.Title == "Missing current semester course");
        Assert.Equal("HIGH", alert.Severity);
        Assert.Equal(2, result.Factors.CurrentSemesterMissingCoursesCount);
    }

    [Fact]
    public void AssessStudent_WhenOneMissingCurrentSemesterCourse_GeneratesMediumAlert()
    {
        var student = MakeStudent(5, semester: 2);
        SetupMinimalStudent(student, studyMap: new() { { "CS201", 2 } });

        var result = _sut.AssessStudent(5);

        var alert = result.GlobalAlertsSummary.Alerts
            .Single(a => a.Title == "Missing current semester course");
        Assert.Equal("MEDIUM", alert.Severity);
    }

    [Fact]
    public void AssessStudent_WhenExpectedCourseAlreadyPassed_NoMissingAlert()
    {
        var student = MakeStudent(6, semester: 2);
        SetupMinimalStudent(
            student,
            studyMap: new() { { "CS201", 2 } },
            passedCourses: ["CS201"]);

        var result = _sut.AssessStudent(6);

        Assert.DoesNotContain(result.GlobalAlertsSummary.Alerts,
            a => a.Title == "Missing current semester course");
    }

    // ─── AssessStudent – credit delay ─────────────────────────────────────────

    [Fact]
    public void AssessStudent_WhenDelayedCreditsAbove12_GeneratesHighCreditDelayAlert()
    {
        var student = MakeStudent(7, semester: 3);
        var studyMap = new Dictionary<string, int>
        {
            { "CS101", 1 }, { "CS102", 1 }, { "CS103", 1 },
            { "CS201", 2 }, { "CS202", 2 }, { "CS301", 3 }
        };
        var coursesMeta = studyMap.Keys.ToDictionary(
            k => k, k => (name: k, credits: 3));

        SetupMinimalStudent(student, studyMap: studyMap, coursesMeta: coursesMeta);

        var result = _sut.AssessStudent(7);

        var alert = result.GlobalAlertsSummary.Alerts
            .Single(a => a.Title == "Credit completion delay");
        Assert.Equal("HIGH", alert.Severity);
        Assert.True(result.Factors.DelayedCredits >= 12);
    }

    [Fact]
    public void AssessStudent_WhenDelayedCreditsBetween6And12_GeneratesMediumCreditDelayAlert()
    {
        var student = MakeStudent(8, semester: 2);
        var studyMap = new Dictionary<string, int>
        {
            { "CS101", 1 }, { "CS102", 1 }, { "CS201", 2 }
        };
        var coursesMeta = studyMap.Keys.ToDictionary(
            k => k, k => (name: k, credits: 3)); // 9 expected, 0 earned → delayed = 9

        SetupMinimalStudent(student, studyMap: studyMap, coursesMeta: coursesMeta);

        var result = _sut.AssessStudent(8);

        var alert = result.GlobalAlertsSummary.Alerts
            .FirstOrDefault(a => a.Title == "Credit completion delay");
        Assert.NotNull(alert);
        Assert.Equal("MEDIUM", alert.Severity);
        Assert.True(result.Factors.DelayedCredits is >= 6 and < 12);
    }

    // ─── AssessStudent – academic status ──────────────────────────────────────

    [Fact]
    public void AssessStudent_WhenOnProbation_AcademicStatusRiskScoreIs100()
    {
        var student = MakeStudent(9, status: "PROBATION");
        SetupMinimalStudent(student);

        var result = _sut.AssessStudent(9);

        Assert.Equal(100, result.Factors.AcademicStatusRiskScore);
    }

    [Fact]
    public void AssessStudent_WhenOnWarning_AcademicStatusRiskScoreIs60()
    {
        var student = MakeStudent(10, status: "WARNING");
        SetupMinimalStudent(student);

        var result = _sut.AssessStudent(10);

        Assert.Equal(60, result.Factors.AcademicStatusRiskScore);
    }

    [Fact]
    public void AssessStudent_WhenNormalStatus_AcademicStatusRiskScoreIsZero()
    {
        var student = MakeStudent(11, status: "NORMAL");
        SetupMinimalStudent(student);

        var result = _sut.AssessStudent(11);

        Assert.Equal(0, result.Factors.AcademicStatusRiskScore);
    }

    // ─── AssessStudent – course-level absence risk ────────────────────────────

    [Fact]
    public void AssessStudent_WhenAbsenceRatioAbove80Pct_AddsHighAbsenceFactor()
    {
        var student = MakeStudent(12);
        SetupStudentWithCourse(student, absencesCount: 9, maxAbsences: 10);

        var result = _sut.AssessStudent(12);

        var courseAssessment = result.CourseAssessments.Single(c => c.CourseCode == "CS101");
        var factor = courseAssessment.RiskFactors.Single(f => f.Title == "Critical Absences");
        Assert.Equal("HIGH", factor.Severity);
    }

    [Fact]
    public void AssessStudent_WhenAbsenceRatioBetween50And80Pct_AddsMediumAbsenceFactor()
    {
        var student = MakeStudent(13);
        SetupStudentWithCourse(student, absencesCount: 6, maxAbsences: 10);

        var result = _sut.AssessStudent(13);

        var courseAssessment = result.CourseAssessments.Single(c => c.CourseCode == "CS101");
        var factor = courseAssessment.RiskFactors.Single(f => f.Title == "High Absences");
        Assert.Equal("MEDIUM", factor.Severity);
    }

    [Fact]
    public void AssessStudent_WhenAbsenceRatioBelow30Pct_NoAbsenceFactor()
    {
        var student = MakeStudent(14);
        SetupStudentWithCourse(student, absencesCount: 2, maxAbsences: 10);

        var result = _sut.AssessStudent(14);

        var courseAssessment = result.CourseAssessments.Single(c => c.CourseCode == "CS101");
        Assert.DoesNotContain(courseAssessment.RiskFactors,
            f => f.Title is "Critical Absences" or "High Absences" or "Moderate Absences");
    }

    // ─── AssessStudent – course-level component risk ──────────────────────────

    [Fact]
    public void AssessStudent_WhenComponentGradeBelow60_AddsHighComponentRiskFactor()
    {
        var student = MakeStudent(15);
        SetupStudentWithCourse(student, components: [new() { ComponentName = "Midterm", Grade = 40m }],
            schemaItems: [new() { ComponentName = "Midterm", WeightPercentage = 30 }]);

        var result = _sut.AssessStudent(15);

        var courseAssessment = result.CourseAssessments.Single(c => c.CourseCode == "CS101");
        var factor = courseAssessment.RiskFactors.Single(f => f.Title.Contains("Midterm"));
        Assert.Equal("HIGH", factor.Severity);
    }

    [Fact]
    public void AssessStudent_WhenComponentGradeAbove80_NoComponentRiskFactor()
    {
        var student = MakeStudent(16);
        SetupStudentWithCourse(student, components: [new() { ComponentName = "Midterm", Grade = 85m }],
            schemaItems: [new() { ComponentName = "Midterm", WeightPercentage = 30 }]);

        var result = _sut.AssessStudent(16);

        var courseAssessment = result.CourseAssessments.Single(c => c.CourseCode == "CS101");
        Assert.Empty(courseAssessment.RiskFactors);
    }

    // ─── AssessStudent – overall risk level ───────────────────────────────────

    [Fact]
    public void AssessStudent_WhenNoRisks_ReturnsLowRiskAndDefaultReason()
    {
        var student = MakeStudent(17, gpa: 90m, status: "NORMAL");
        SetupMinimalStudent(student);

        var result = _sut.AssessStudent(17);

        Assert.Equal("LOW", result.RiskLevel);
        Assert.Contains("No major risk indicators detected.", result.MainReasons);
    }

    [Fact]
    public void AssessStudent_WhenLowGpaAndProbation_RiskScoreReflectsBothFactors()
    {
        // Course portfolio has 55% weight, so with no courses the max achievable
        // score from GPA (15%) + probation (10%) is 25 — which is still LOW.
        // This test verifies the individual factor scores are correctly set.
        var student = MakeStudent(18, gpa: 55m, status: "PROBATION");
        SetupMinimalStudent(student);

        var result = _sut.AssessStudent(18);

        Assert.Equal(100, result.Factors.GpaRiskScore);
        Assert.Equal(100, result.Factors.AcademicStatusRiskScore);
        Assert.Equal(25, result.RiskScore); // 0.15*100 + 0.10*100 = 25
    }

    // ─── GetStudentAlerts ─────────────────────────────────────────────────────

    [Fact]
    public void GetStudentAlerts_WhenHighGpaRisk_ReturnsNonZeroHighCount()
    {
        var student = MakeStudent(19, gpa: 50m);
        SetupMinimalStudent(student);

        var alerts = _sut.GetStudentAlerts(19);

        Assert.True(alerts.High > 0);
        Assert.Equal(alerts.High + alerts.Medium + alerts.Low, alerts.Count);
    }

    // ─── GetStudentAlertsCount ────────────────────────────────────────────────

    [Fact]
    public void GetStudentAlertsCount_TotalEqualsHighPlusMediumPlusLow()
    {
        var student = MakeStudent(20, gpa: 65m);
        SetupMinimalStudent(student);

        var count = _sut.GetStudentAlertsCount(20);

        Assert.Equal(count.High + count.Medium + count.Low, count.Count);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static SisStudent MakeStudent(
        int id,
        decimal? gpa = 80m,
        string? status = "NORMAL",
        int semester = 1,
        string programCode = "CS") =>
        new()
        {
            StudentId = id,
            FirstName = "Test",
            LastName = $"Student{id}",
            ProgramCode = programCode,
            CurrentSemester = semester,
            CurrentGpa = gpa,
            AcademicStatus = status
        };

    private void SetupMinimalStudent(
        SisStudent student,
        Dictionary<string, int>? studyMap = null,
        List<string>? passedCourses = null,
        Dictionary<string, (string name, int credits)>? coursesMeta = null)
    {
        _studentRepoMock.Setup(r => r.GetById(student.StudentId)).Returns(student);
        _studentRepoMock.Setup(r => r.GetCurrentEnrollmentWithCourse(student.StudentId))
            .Returns([]);
        _studentRepoMock.Setup(r => r.GetCurrentCoursesPerformance(student.StudentId))
            .Returns([]);
        _studentRepoMock.Setup(r => r.GetPassedCourses(student.StudentId))
            .Returns(passedCourses ?? []);
        _studentRepoMock.Setup(r => r.GetStudyGuideRecommendedSemester(It.IsAny<string>()))
            .Returns(studyMap ?? new Dictionary<string, int>());
        _studentRepoMock.Setup(r => r.GetCoursesMeta(It.IsAny<List<string>>()))
            .Returns(coursesMeta ?? new Dictionary<string, (string, int)>());
        _studentRepoMock.Setup(r => r.GetCourseGradingSchema(It.IsAny<List<string>>()))
            .Returns(new Dictionary<string, List<CourseGradingSchemaDto>>());
    }

    private void SetupStudentWithCourse(
        SisStudent student,
        int absencesCount = 0,
        int maxAbsences = 10,
        List<CourseComponentGradeDto>? components = null,
        List<CourseGradingSchemaDto>? schemaItems = null)
    {
        _studentRepoMock.Setup(r => r.GetById(student.StudentId)).Returns(student);
        _studentRepoMock.Setup(r => r.GetCurrentEnrollmentWithCourse(student.StudentId))
            .Returns([new() { CourseCode = "CS101", CourseName = "Intro to CS", Credits = 3 }]);
        _studentRepoMock.Setup(r => r.GetCurrentCoursesPerformance(student.StudentId))
            .Returns(
            [
                new()
                {
                    CourseCode = "CS101",
                    CourseName = "Intro to CS",
                    Credits = 3,
                    AbsencesCount = absencesCount,
                    MaxAbsences = maxAbsences,
                    Components = components ?? []
                }
            ]);
        _studentRepoMock.Setup(r => r.GetPassedCourses(student.StudentId)).Returns([]);
        _studentRepoMock.Setup(r => r.GetStudyGuideRecommendedSemester(It.IsAny<string>()))
            .Returns(new Dictionary<string, int> { { "CS101", 1 } });
        _studentRepoMock.Setup(r => r.GetCoursesMeta(It.IsAny<List<string>>()))
            .Returns(new Dictionary<string, (string, int)> { { "CS101", ("Intro to CS", 3) } });
        _studentRepoMock.Setup(r => r.GetCourseGradingSchema(It.IsAny<List<string>>()))
            .Returns(new Dictionary<string, List<CourseGradingSchemaDto>>
            {
                { "CS101", schemaItems ?? [] }
            });
    }
}
