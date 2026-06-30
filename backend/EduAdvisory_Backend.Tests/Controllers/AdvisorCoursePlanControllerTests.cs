using System.Security.Claims;
using EduAdvisory_Backend.Controllers;
using EduAdvisory_Backend.DTOs.CoursePlan;
using EduAdvisory_Backend.Interfaces.Repositories;
using EduAdvisory_Backend.Interfaces.Services;
using EduAdvisory_Backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EduAdvisory_Backend.Tests.Controllers;

public class AdvisorCoursePlanControllerTests
{
    private readonly Mock<IAdvisorRepository> _advisorRepoMock = new();
    private readonly Mock<IStudentRepository> _studentRepoMock = new();
    private readonly Mock<ICoursePlanService> _coursePlanServiceMock = new();
    private readonly Mock<ICoursePlanAiService> _coursePlanAiServiceMock = new();
    private readonly Mock<ILogger<AdvisorCoursePlanController>> _loggerMock = new();
    private readonly AdvisorCoursePlanController _sut;

    private static readonly Advisor DefaultAdvisor = new()
    {
        AdvisorId = 10, Name = "Dr. A", Email = "a@a.com", Office = "B1", OfficeHours = "9-12"
    };

    private static readonly SisStudent DefaultStudent = new()
    {
        StudentId = 5, AdvisorId = 10, ProgramCode = "CS", CurrentSemester = 2, AcademicStatus = "NORMAL"
    };

    public AdvisorCoursePlanControllerTests()
    {
        _sut = new AdvisorCoursePlanController(
            _advisorRepoMock.Object,
            _studentRepoMock.Object,
            _coursePlanServiceMock.Object,
            _coursePlanAiServiceMock.Object,
            _loggerMock.Object);
    }

    // ─── GetStudentGeneratedPlans ────────────────────────────────────────────

    [Fact]
    public void GetStudentGeneratedPlans_WhenNoUsername_ReturnsUnauthorized()
    {
        _sut.ControllerContext = MakeContext(null);

        var result = _sut.GetStudentGeneratedPlans(1);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public void GetStudentGeneratedPlans_WhenAdvisorNotFound_ReturnsNotFound()
    {
        _sut.ControllerContext = MakeContext("adv1");
        _advisorRepoMock.Setup(r => r.GetByUsername("adv1")).Returns((Advisor)null!);

        var result = _sut.GetStudentGeneratedPlans(5);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void GetStudentGeneratedPlans_WhenStudentNotFound_ReturnsNotFound()
    {
        _sut.ControllerContext = MakeContext("adv1");
        _advisorRepoMock.Setup(r => r.GetByUsername("adv1")).Returns(DefaultAdvisor);
        _studentRepoMock.Setup(r => r.GetById(5)).Returns((SisStudent)null!);

        var result = _sut.GetStudentGeneratedPlans(5);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void GetStudentGeneratedPlans_WhenStudentNotAssignedToAdvisor_ReturnsForbid()
    {
        _sut.ControllerContext = MakeContext("adv1");
        _advisorRepoMock.Setup(r => r.GetByUsername("adv1")).Returns(DefaultAdvisor);
        _studentRepoMock.Setup(r => r.GetById(5))
            .Returns(new SisStudent { StudentId = 5, AdvisorId = 99 });

        var result = _sut.GetStudentGeneratedPlans(5);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public void GetStudentGeneratedPlans_WhenAuthorized_ReturnsOkWithPlans()
    {
        _sut.ControllerContext = MakeContext("adv1");
        _advisorRepoMock.Setup(r => r.GetByUsername("adv1")).Returns(DefaultAdvisor);
        _studentRepoMock.Setup(r => r.GetById(5)).Returns(DefaultStudent);
        var plans = new List<CoursePlanDto> { new() { Strategy = "Balanced" } };
        _coursePlanServiceMock.Setup(s => s.GeneratePlansForStudent(5, 3)).Returns(plans);

        var result = _sut.GetStudentGeneratedPlans(5);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(plans, ok.Value);
    }

    // ─── GetStudentCoursePlanInsights ─────────────────────────────────────────

    [Fact]
    public async Task GetStudentCoursePlanInsights_WhenAuthorized_ReturnsOkWithInsights()
    {
        _sut.ControllerContext = MakeContext("adv1");
        _advisorRepoMock.Setup(r => r.GetByUsername("adv1")).Returns(DefaultAdvisor);
        _studentRepoMock.Setup(r => r.GetById(5)).Returns(DefaultStudent);
        var plans = new List<CoursePlanDto> { new() { Strategy = "Balanced" } };
        _coursePlanServiceMock.Setup(s => s.GeneratePlansForStudent(5, 3)).Returns(plans);
        var insights = new CoursePlanAiInsightsDto { BestPlanIndex = 0 };
        _coursePlanAiServiceMock.Setup(s => s.RankAndExplainAsync(
            plans, "CS", 2, "NORMAL", 18, It.IsAny<CancellationToken>())).ReturnsAsync(insights);

        var result = await _sut.GetStudentCoursePlanInsights(5);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<CoursePlanInsightsResponseDto>(ok.Value);
        Assert.Equal(plans, response.Plans);
        Assert.Null(response.AiError);
    }

    [Fact]
    public async Task GetStudentCoursePlanInsights_WhenAiServiceFails_ReturnsFallbackInsights()
    {
        _sut.ControllerContext = MakeContext("adv1");
        _advisorRepoMock.Setup(r => r.GetByUsername("adv1")).Returns(DefaultAdvisor);
        _studentRepoMock.Setup(r => r.GetById(5)).Returns(DefaultStudent);
        var plans = new List<CoursePlanDto> { new() { Strategy = "Balanced" } };
        _coursePlanServiceMock.Setup(s => s.GeneratePlansForStudent(5, 3)).Returns(plans);
        _coursePlanAiServiceMock.Setup(s => s.RankAndExplainAsync(
            It.IsAny<List<CoursePlanDto>>(), It.IsAny<string>(), It.IsAny<int>(),
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("AI service unavailable"));

        var result = await _sut.GetStudentCoursePlanInsights(5);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<CoursePlanInsightsResponseDto>(ok.Value);
        Assert.NotNull(response.AiError);
        Assert.Contains("AI service unavailable", response.AiError);
    }

    private static ControllerContext MakeContext(string? username)
    {
        var claims = username == null
            ? Enumerable.Empty<Claim>()
            : new[] { new Claim(ClaimTypes.Name, username) };
        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
            }
        };
    }
}
