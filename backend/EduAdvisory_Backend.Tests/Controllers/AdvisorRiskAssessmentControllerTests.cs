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

public class AdvisorRiskAssessmentControllerTests
{
    private readonly Mock<IAdvisorRepository> _advisorRepoMock = new();
    private readonly Mock<IStudentRepository> _studentRepoMock = new();
    private readonly Mock<IStudentRiskAssessmentService> _riskServiceMock = new();
    private readonly AdvisorRiskAssessmentController _sut;

    public AdvisorRiskAssessmentControllerTests()
    {
        _sut = new AdvisorRiskAssessmentController(
            _advisorRepoMock.Object,
            _studentRepoMock.Object,
            _riskServiceMock.Object);
    }

    [Fact]
    public void GetStudentRiskAssessment_WhenNoUsername_ReturnsUnauthorized()
    {
        _sut.ControllerContext = MakeContext(null);

        var result = _sut.GetStudentRiskAssessment(1);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public void GetStudentRiskAssessment_WhenAdvisorNotFound_ReturnsNotFound()
    {
        _sut.ControllerContext = MakeContext("adv1");
        _advisorRepoMock.Setup(r => r.GetByUsername("adv1")).Returns((Advisor)null!);

        var result = _sut.GetStudentRiskAssessment(1);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void GetStudentRiskAssessment_WhenStudentNotFound_ReturnsNotFound()
    {
        _sut.ControllerContext = MakeContext("adv1");
        _advisorRepoMock.Setup(r => r.GetByUsername("adv1"))
            .Returns(new Advisor { AdvisorId = 10, Name = "Dr. A", Email = "a@a.com", Office = "B1", OfficeHours = "9-12" });
        _studentRepoMock.Setup(r => r.GetById(7)).Returns((SisStudent)null!);

        var result = _sut.GetStudentRiskAssessment(7);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void GetStudentRiskAssessment_WhenStudentNotAssignedToAdvisor_ReturnsForbid()
    {
        _sut.ControllerContext = MakeContext("adv1");
        _advisorRepoMock.Setup(r => r.GetByUsername("adv1"))
            .Returns(new Advisor { AdvisorId = 10, Name = "Dr. A", Email = "a@a.com", Office = "B1", OfficeHours = "9-12" });
        _studentRepoMock.Setup(r => r.GetById(7))
            .Returns(new SisStudent { StudentId = 7, AdvisorId = 99 });

        var result = _sut.GetStudentRiskAssessment(7);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public void GetStudentRiskAssessment_WhenAuthorized_ReturnsOkWithAssessment()
    {
        _sut.ControllerContext = MakeContext("adv1");
        _advisorRepoMock.Setup(r => r.GetByUsername("adv1"))
            .Returns(new Advisor { AdvisorId = 10, Name = "Dr. A", Email = "a@a.com", Office = "B1", OfficeHours = "9-12" });
        _studentRepoMock.Setup(r => r.GetById(7))
            .Returns(new SisStudent { StudentId = 7, AdvisorId = 10 });
        var expected = new StudentRiskAssessmentDto { StudentId = 7, RiskLevel = "LOW" };
        _riskServiceMock.Setup(s => s.AssessStudent(7)).Returns(expected);

        var result = _sut.GetStudentRiskAssessment(7);

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
