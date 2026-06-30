using System.Security.Claims;
using EduAdvisory_Backend.Controllers;
using EduAdvisory_Backend.DTOs.Profile;
using EduAdvisory_Backend.Services.Profile;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace EduAdvisory_Backend.Tests.Controllers;

public class ProfileControllerTests
{
    private readonly Mock<IProfileService> _profileServiceMock = new();
    private readonly ProfileController _sut;

    public ProfileControllerTests()
    {
        _sut = new ProfileController(_profileServiceMock.Object);
        _sut.ControllerContext = MakeContext("kc-user");
    }

    [Fact]
    public async Task GetMyProfile_ReturnsOkWithProfile()
    {
        var profile = new ProfileDto { UserId = 1, Role = "student" };
        _profileServiceMock.Setup(s => s.GetMyProfileAsync("kc-user")).ReturnsAsync(profile);

        var result = await _sut.GetMyProfile();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(profile, ok.Value);
    }

    [Fact]
    public async Task GetMyProfile_DelegatesToServiceWithKeycloakId()
    {
        _profileServiceMock.Setup(s => s.GetMyProfileAsync("kc-user"))
            .ReturnsAsync(new ProfileDto());

        await _sut.GetMyProfile();

        _profileServiceMock.Verify(s => s.GetMyProfileAsync("kc-user"), Times.Once);
    }

    private static ControllerContext MakeContext(string keycloakId) =>
        new()
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new[] { new Claim("sub", keycloakId) }, "Test"))
            }
        };
}
