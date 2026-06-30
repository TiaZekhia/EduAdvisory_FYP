using System.Security.Claims;
using EduAdvisory_Backend.Controllers;
using EduAdvisory_Backend.DTOs.Student;
using EduAdvisory_Backend.Interfaces.Repositories;
using EduAdvisory_Backend.Interfaces.Services;
using EduAdvisory_Backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace EduAdvisory_Backend.Tests.Controllers;

public class AdvisorStudentAnalysisControllerTests
{
    private readonly Mock<IAdvisorRepository> _advisorRepoMock = new();
    private readonly Mock<IStudentRepository> _studentRepoMock = new();
    private readonly Mock<IStudentAnalysisService> _serviceMock = new();
    private readonly AdvisorStudentAnalysisController _sut;

    public AdvisorStudentAnalysisControllerTests()
    {
        _sut = new AdvisorStudentAnalysisController(
            _advisorRepoMock.Object,
            _studentRepoMock.Object,
            _serviceMock.Object);
    }

    [Fact]
    public void GetStudentAnalysis_WhenNoUsername_ReturnsUnauthorized()
    {
        _sut.ControllerContext = MakeContext(null);

        var result = _sut.GetStudentAnalysis(1);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public void GetStudentAnalysis_WhenAdvisorNotFound_ReturnsNotFound()
    {
        _sut.ControllerContext = MakeContext("adv1");
        _advisorRepoMock.Setup(r => r.GetByUsername("adv1")).Returns((Advisor)null!);

        var result = _sut.GetStudentAnalysis(1);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void GetStudentAnalysis_WhenStudentNotFound_ReturnsNotFound()
    {
        _sut.ControllerContext = MakeContext("adv1");
        _advisorRepoMock.Setup(r => r.GetByUsername("adv1"))
            .Returns(new Advisor { AdvisorId = 10, Name = "Dr. A", Email = "a@a.com", Office = "B1", OfficeHours = "9-12" });
        _studentRepoMock.Setup(r => r.GetById(5)).Returns((SisStudent)null!);

        var result = _sut.GetStudentAnalysis(5);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void GetStudentAnalysis_WhenStudentBelongsToDifferentAdvisor_ReturnsForbid()
    {
        _sut.ControllerContext = MakeContext("adv1");
        _advisorRepoMock.Setup(r => r.GetByUsername("adv1"))
            .Returns(new Advisor { AdvisorId = 10, Name = "Dr. A", Email = "a@a.com", Office = "B1", OfficeHours = "9-12" });
        _studentRepoMock.Setup(r => r.GetById(5))
            .Returns(new SisStudent { StudentId = 5, AdvisorId = 99 });

        var result = _sut.GetStudentAnalysis(5);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public void GetStudentAnalysis_WhenAuthorized_ReturnsOkWithAnalysis()
    {
        _sut.ControllerContext = MakeContext("adv1");
        _advisorRepoMock.Setup(r => r.GetByUsername("adv1"))
            .Returns(new Advisor { AdvisorId = 10, Name = "Dr. A", Email = "a@a.com", Office = "B1", OfficeHours = "9-12" });
        _studentRepoMock.Setup(r => r.GetById(5))
            .Returns(new SisStudent { StudentId = 5, AdvisorId = 10 });
        var expected = new AdvisorStudentAnalysisDto { StudentId = 5 };
        _serviceMock.Setup(s => s.AnalyzeStudentForAdvisor(5)).Returns(expected);

        var result = _sut.GetStudentAnalysis(5);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expected, ok.Value);
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
