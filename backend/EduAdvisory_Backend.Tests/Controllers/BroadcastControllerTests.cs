using System.Security.Claims;
using EduAdvisory_Backend.Controllers;
using EduAdvisory_Backend.DTOs.Broadcasts;
using EduAdvisory_Backend.Services.Messaging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace EduAdvisory_Backend.Tests.Controllers;

public class BroadcastControllerTests
{
    private readonly Mock<IBroadcastService> _broadcastServiceMock = new();
    private readonly BroadcastController _sut;

    public BroadcastControllerTests()
    {
        _sut = new BroadcastController(_broadcastServiceMock.Object);
        _sut.ControllerContext = MakeContext("kc-adv");
    }

    [Fact]
    public async Task GetMyBroadcasts_ReturnsOkWithBroadcasts()
    {
        var broadcasts = new List<BroadcastDto> { new() { BroadcastMessageId = 1 } };
        _broadcastServiceMock.Setup(s => s.GetMyBroadcastsAsync("kc-adv")).ReturnsAsync(broadcasts);

        var result = await _sut.GetMyBroadcasts();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(broadcasts, ok.Value);
    }

    [Fact]
    public async Task MarkAsRead_ReturnsNoContent()
    {
        _broadcastServiceMock.Setup(s => s.MarkBroadcastAsReadAsync("kc-adv", 1))
            .Returns(Task.CompletedTask);

        var result = await _sut.MarkAsRead(1);

        Assert.IsType<NoContentResult>(result);
        _broadcastServiceMock.Verify(s => s.MarkBroadcastAsReadAsync("kc-adv", 1), Times.Once);
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
