using EduAdvisory_Backend.DTOs.Course;
using EduAdvisory_Backend.DTOs.Student;
using EduAdvisory_Backend.Interfaces.Repositories;
using EduAdvisory_Backend.Models;
using EduAdvisory_Backend.Services;
using Moq;
using Xunit;

namespace EduAdvisory_Backend.Tests.Services;

public class StudentAnalysisServiceTests
{
    private readonly Mock<IStudentRepository> _studentRepoMock = new();
    private readonly Mock<IStudyGuideRepository> _studyGuideRepoMock = new();
    private readonly Mock<ICoursePrerequisiteRepository> _prereqRepoMock = new();
    private readonly StudentAnalysisService _sut;

    public StudentAnalysisServiceTests()
    {
        _sut = new StudentAnalysisService(
            _studentRepoMock.Object,
            _studyGuideRepoMock.Object,
            _prereqRepoMock.Object);
    }

    // ─── AnalyzeStudent ───────────────────────────────────────────────────────

    [Fact]
    public void AnalyzeStudent_WhenStudentIsFullyEnrolledAndNoFails_ReturnsIsOnTrackTrue()
    {
        const int studentId = 1;
        SetupAnalyzeStudent(
            studentId,
            programCode: "CS",
            currentSemester: 1,
            studyGuide: [new() { CourseCode = "CS101", RecommendedSemester = 1 }],
            history: [],
            enrolled: [new() { CourseCode = "CS101" }],
            prerequisites: []);

        var result = _sut.AnalyzeStudent(studentId);

        Assert.True(result.IsOnTrack);
        Assert.Equal(studentId, result.StudentId);
        Assert.Empty(result.MissingCurrentSemesterCourses);
        Assert.Empty(result.FailedNotRetakenCourses);
        Assert.Empty(result.BlockingCourses);
    }

    [Fact]
    public void AnalyzeStudent_WhenExpectedCourseNotEnrolled_SetsIsOnTrackFalseAndListsMissingCourse()
    {
        const int studentId = 2;
        SetupAnalyzeStudent(
            studentId,
            programCode: "CS",
            currentSemester: 1,
            studyGuide:
            [
                new() { CourseCode = "CS101", RecommendedSemester = 1 },
                new() { CourseCode = "CS102", RecommendedSemester = 1 }
            ],
            history: [],
            enrolled: [new() { CourseCode = "CS101" }], // CS102 missing
            prerequisites: []);

        var result = _sut.AnalyzeStudent(studentId);

        Assert.False(result.IsOnTrack);
        Assert.Contains("CS102", result.MissingCurrentSemesterCourses);
        Assert.DoesNotContain("CS101", result.MissingCurrentSemesterCourses);
    }

    [Fact]
    public void AnalyzeStudent_WhenStudentHasFailedCourseNotRetaken_SetsIsOnTrackFalseAndListsCourse()
    {
        const int studentId = 3;
        SetupAnalyzeStudent(
            studentId,
            programCode: "CS",
            currentSemester: 2,
            studyGuide:
            [
                new() { CourseCode = "CS101", RecommendedSemester = 1 },
                new() { CourseCode = "CS201", RecommendedSemester = 2 }
            ],
            history: [new() { CourseCode = "CS101", Status = "FAILED" }],
            enrolled: [new() { CourseCode = "CS201" }],
            prerequisites: []);

        var result = _sut.AnalyzeStudent(studentId);

        Assert.False(result.IsOnTrack);
        Assert.Contains("CS101", result.FailedNotRetakenCourses);
    }

    [Fact]
    public void AnalyzeStudent_WhenFailedCourseIsLaterPassed_DoesNotListInFailedNotRetaken()
    {
        const int studentId = 4;
        SetupAnalyzeStudent(
            studentId,
            programCode: "CS",
            currentSemester: 2,
            studyGuide:
            [
                new() { CourseCode = "CS101", RecommendedSemester = 1 },
                new() { CourseCode = "CS201", RecommendedSemester = 2 }
            ],
            history:
            [
                new() { CourseCode = "CS101", Status = "FAILED" },
                new() { CourseCode = "CS101", Status = "PASSED" }
            ],
            enrolled: [new() { CourseCode = "CS201" }],
            prerequisites: []);

        var result = _sut.AnalyzeStudent(studentId);

        Assert.DoesNotContain("CS101", result.FailedNotRetakenCourses);
    }

    [Fact]
    public void AnalyzeStudent_WhenPrerequisiteNotPassed_ListsPrerequisiteInBlockingCourses()
    {
        const int studentId = 5;
        SetupAnalyzeStudent(
            studentId,
            programCode: "CS",
            currentSemester: 2,
            studyGuide: [new() { CourseCode = "CS201", RecommendedSemester = 2 }],
            history: [],
            enrolled: [new() { CourseCode = "CS201" }],
            prerequisites: [new() { CourseCode = "CS201", PrerequisiteCourseCode = "CS101" }]);

        var result = _sut.AnalyzeStudent(studentId);

        Assert.Contains("CS101", result.BlockingCourses);
    }

    [Fact]
    public void AnalyzeStudent_WhenPrerequisiteAlreadyPassed_DoesNotListInBlockingCourses()
    {
        const int studentId = 6;
        SetupAnalyzeStudent(
            studentId,
            programCode: "CS",
            currentSemester: 2,
            studyGuide: [new() { CourseCode = "CS201", RecommendedSemester = 2 }],
            history: [new() { CourseCode = "CS101", Status = "PASSED" }],
            enrolled: [new() { CourseCode = "CS201" }],
            prerequisites: [new() { CourseCode = "CS201", PrerequisiteCourseCode = "CS101" }]);

        var result = _sut.AnalyzeStudent(studentId);

        Assert.DoesNotContain("CS101", result.BlockingCourses);
    }

    // ─── AnalyzeStudentForAdvisor ─────────────────────────────────────────────

    [Fact]
    public void AnalyzeStudentForAdvisor_WhenStudentNotFound_ThrowsException()
    {
        _studentRepoMock.Setup(r => r.GetById(It.IsAny<int>())).Returns((SisStudent)null!);

        Assert.Throws<Exception>(() => _sut.AnalyzeStudentForAdvisor(99));
    }

    [Fact]
    public void AnalyzeStudentForAdvisor_WhenAllCoursesPassedOrEnrolled_ReturnsIsOnTrackTrue()
    {
        const int studentId = 10;
        var student = new SisStudent
        {
            StudentId = studentId,
            FirstName = "John",
            LastName = "Doe",
            ProgramCode = "CS",
            CurrentSemester = 1,
            CurrentGpa = 85m,
            AcademicStatus = "NORMAL"
        };

        _studentRepoMock.Setup(r => r.GetById(studentId)).Returns(student);
        _studentRepoMock.Setup(r => r.GetPassedCourses(studentId)).Returns(["CS101"]);
        _studentRepoMock.Setup(r => r.GetFailedNotRetakenCourses(studentId)).Returns([]);
        _studentRepoMock.Setup(r => r.GetCurrentEnrollmentWithCourse(studentId))
            .Returns([new() { CourseCode = "CS102", CourseName = "Data Structures", Credits = 3 }]);
        _studentRepoMock.Setup(r => r.GetStudyGuideRecommendedSemester("CS"))
            .Returns(new Dictionary<string, int> { { "CS101", 1 }, { "CS102", 1 } });
        _studentRepoMock.Setup(r => r.GetCoursesMeta(It.IsAny<List<string>>()))
            .Returns(new Dictionary<string, (string name, int credits)>
            {
                { "CS101", ("Intro to CS", 3) },
                { "CS102", ("Data Structures", 3) }
            });
        _studentRepoMock.Setup(r => r.GetPrerequisitesMap(It.IsAny<List<string>>()))
            .Returns(new Dictionary<string, List<string>>());
        _studentRepoMock.Setup(r => r.GetProgressSummary(studentId))
            .Returns(new ProgressSummaryDto { CreditsEarned = 3 });
        _studentRepoMock.Setup(r => r.GetCourseHistory(studentId))
            .Returns([]);
        _studyGuideRepoMock.Setup(r => r.GetByProgram("CS"))
            .Returns(
            [
                new() { CourseCode = "CS101", RecommendedSemester = 1 },
                new() { CourseCode = "CS102", RecommendedSemester = 1 }
            ]);

        var result = _sut.AnalyzeStudentForAdvisor(studentId);

        Assert.True(result.IsOnTrack);
        Assert.Equal("John Doe", result.StudentName);
        Assert.Equal("CS", result.ProgramCode);
        Assert.Equal(1, result.CurrentSemester);
    }

    [Fact]
    public void AnalyzeStudentForAdvisor_WhenMissingCoursesBehindSchedule_ListsThemWithMediumOrHighPriority()
    {
        const int studentId = 11;
        var student = new SisStudent
        {
            StudentId = studentId,
            FirstName = "Jane",
            LastName = "Smith",
            ProgramCode = "CS",
            CurrentSemester = 3,
            CurrentGpa = 70m,
            AcademicStatus = "NORMAL"
        };

        _studentRepoMock.Setup(r => r.GetById(studentId)).Returns(student);
        _studentRepoMock.Setup(r => r.GetPassedCourses(studentId)).Returns([]);
        _studentRepoMock.Setup(r => r.GetFailedNotRetakenCourses(studentId)).Returns([]);
        _studentRepoMock.Setup(r => r.GetCurrentEnrollmentWithCourse(studentId)).Returns([]);
        _studentRepoMock.Setup(r => r.GetStudyGuideRecommendedSemester("CS"))
            .Returns(new Dictionary<string, int>
            {
                { "CS101", 1 }, // should have been done in sem 1, now in sem 3 → MEDIUM
                { "CS201", 2 }  // should have been done in sem 2, now in sem 3 → MEDIUM
            });
        _studentRepoMock.Setup(r => r.GetCoursesMeta(It.IsAny<List<string>>()))
            .Returns(new Dictionary<string, (string name, int credits)>
            {
                { "CS101", ("Intro to CS", 3) },
                { "CS201", ("Algorithms", 3) }
            });
        _studentRepoMock.Setup(r => r.GetPrerequisitesMap(It.IsAny<List<string>>()))
            .Returns(new Dictionary<string, List<string>>());
        _studentRepoMock.Setup(r => r.GetProgressSummary(studentId))
            .Returns(new ProgressSummaryDto { CreditsEarned = 0 });
        _studentRepoMock.Setup(r => r.GetCourseHistory(studentId)).Returns([]);
        _studyGuideRepoMock.Setup(r => r.GetByProgram("CS"))
            .Returns(
            [
                new() { CourseCode = "CS101", RecommendedSemester = 1 },
                new() { CourseCode = "CS201", RecommendedSemester = 2 }
            ]);

        var result = _sut.AnalyzeStudentForAdvisor(studentId);

        Assert.False(result.IsOnTrack);
        Assert.NotEmpty(result.MissingCourses);
        Assert.All(result.MissingCourses, c => Assert.NotEqual("LOW", c.Priority));
    }

    [Fact]
    public void AnalyzeStudentForAdvisor_WhenCourseBlockedByTwoCourses_AssignsHighPriority()
    {
        const int studentId = 12;
        var student = new SisStudent
        {
            StudentId = studentId,
            FirstName = "High",
            LastName = "Priority",
            ProgramCode = "CS",
            CurrentSemester = 3,
            CurrentGpa = 75m,
            AcademicStatus = "NORMAL"
        };

        _studentRepoMock.Setup(r => r.GetById(studentId)).Returns(student);
        _studentRepoMock.Setup(r => r.GetPassedCourses(studentId)).Returns([]);
        _studentRepoMock.Setup(r => r.GetFailedNotRetakenCourses(studentId)).Returns([]);
        _studentRepoMock.Setup(r => r.GetCurrentEnrollmentWithCourse(studentId)).Returns([]);
        _studentRepoMock.Setup(r => r.GetStudyGuideRecommendedSemester("CS"))
            .Returns(new Dictionary<string, int>
            {
                { "CS101", 1 },
                { "CS201", 2 },
                { "CS301", 3 }
            });
        // CS101 is a prerequisite of both CS201 and CS301 → blockedCount = 2 → HIGH priority
        _studentRepoMock.Setup(r => r.GetPrerequisitesMap(It.IsAny<List<string>>()))
            .Returns(new Dictionary<string, List<string>>
            {
                { "CS201", new List<string> { "CS101" } },
                { "CS301", new List<string> { "CS101" } }
            });
        _studentRepoMock.Setup(r => r.GetCoursesMeta(It.IsAny<List<string>>()))
            .Returns(new Dictionary<string, (string name, int credits)>
            {
                { "CS101", ("Intro", 3) },
                { "CS201", ("Algorithms", 3) },
                { "CS301", ("OS", 3) }
            });
        _studentRepoMock.Setup(r => r.GetProgressSummary(studentId))
            .Returns(new ProgressSummaryDto { CreditsEarned = 0 });
        _studentRepoMock.Setup(r => r.GetCourseHistory(studentId)).Returns([]);
        _studyGuideRepoMock.Setup(r => r.GetByProgram("CS"))
            .Returns(
            [
                new() { CourseCode = "CS101", RecommendedSemester = 1 },
                new() { CourseCode = "CS201", RecommendedSemester = 2 },
                new() { CourseCode = "CS301", RecommendedSemester = 3 }
            ]);

        var result = _sut.AnalyzeStudentForAdvisor(studentId);

        var cs101 = result.MissingCourses.FirstOrDefault(c => c.CourseCode == "CS101");
        Assert.NotNull(cs101);
        Assert.Equal("HIGH", cs101.Priority);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private void SetupAnalyzeStudent(
        int studentId,
        string programCode,
        int currentSemester,
        List<StudyGuide> studyGuide,
        List<SisStudentCourseHistory> history,
        List<SisCurrentEnrollment> enrolled,
        List<CoursePrerequisite> prerequisites)
    {
        _studentRepoMock.Setup(r => r.GetById(studentId)).Returns(new SisStudent
        {
            StudentId = studentId,
            ProgramCode = programCode,
            CurrentSemester = currentSemester
        });
        _studyGuideRepoMock.Setup(r => r.GetByProgram(programCode)).Returns(studyGuide);
        _studentRepoMock.Setup(r => r.GetCourseHistory(studentId)).Returns(history);
        _studentRepoMock.Setup(r => r.GetCurrentEnrollment(studentId)).Returns(enrolled);
        _prereqRepoMock.Setup(r => r.GetAll()).Returns(prerequisites);
    }
}
