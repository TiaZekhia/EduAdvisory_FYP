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

public class StudentAnalysisControllerTests
{
    private readonly Mock<IStudentAnalysisService> _serviceMock = new();
    private readonly Mock<IStudentRepository> _repoMock = new();
    private readonly StudentAnalysisController _sut;

    public StudentAnalysisControllerTests()
    {
        _sut = new StudentAnalysisController(_serviceMock.Object, _repoMock.Object);
    }

    [Fact]
    public void AnalyzeCurrentStudent_WhenNoUsername_ReturnsUnauthorized()
    {
        _sut.ControllerContext = MakeContext(null);

        var result = _sut.AnalyzeCurrentStudent();

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public void AnalyzeCurrentStudent_WhenStudentNotLinked_ReturnsNotFound()
    {
        _sut.ControllerContext = MakeContext("user1");
        _repoMock.Setup(r => r.GetByUsername("user1")).Returns((SisStudent)null!);

        var result = _sut.AnalyzeCurrentStudent();

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void AnalyzeCurrentStudent_WhenStudentFound_ReturnsOkWithAnalysis()
    {
        _sut.ControllerContext = MakeContext("user1");
        _repoMock.Setup(r => r.GetByUsername("user1"))
            .Returns(new SisStudent { StudentId = 42 });
        var expected = new StudentAnalysisDto { StudentId = 42, IsOnTrack = true };
        _serviceMock.Setup(s => s.AnalyzeStudent(42)).Returns(expected);

        var result = _sut.AnalyzeCurrentStudent();

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
