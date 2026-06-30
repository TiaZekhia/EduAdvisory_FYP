using EduAdvisory_Backend.DTOs.Broadcasts;
using EduAdvisory_Backend.Hubs;
using EduAdvisory_Backend.Models;
using EduAdvisory_Backend.Repositories.Messaging;
using EduAdvisory_Backend.Services.Messaging;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace EduAdvisory_Backend.Tests.Services;

public class BroadcastServiceTests
{
    private readonly Mock<IBroadcastRepository> _broadcastRepoMock = new();
    private readonly Mock<IHubContext<ChatHub>> _hubContextMock = new();

    private static EduAdvisoryDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<EduAdvisoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableServiceProviderCaching(false)
            .Options);

    private BroadcastService CreateSut(EduAdvisoryDbContext context)
    {
        var clientsMock = new Mock<IHubClients>();
        var proxyMock = new Mock<IClientProxy>();
        clientsMock.Setup(c => c.User(It.IsAny<string>())).Returns(proxyMock.Object);
        proxyMock.Setup(p => p.SendCoreAsync(
            It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _hubContextMock.Setup(h => h.Clients).Returns(clientsMock.Object);

        return new BroadcastService(context, _broadcastRepoMock.Object, _hubContextMock.Object);
    }

    // ─── CreateBroadcastAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task CreateBroadcastAsync_WhenUserNotFound_ThrowsException()
    {
        using var context = CreateContext();
        var sut = CreateSut(context);

        await Assert.ThrowsAsync<Exception>(() =>
            sut.CreateBroadcastAsync("nonexistent", new CreateBroadcastDto()));
    }

    [Fact]
    public async Task CreateBroadcastAsync_WhenUserIsStudent_ThrowsException()
    {
        using var context = CreateContext();
        context.Users.Add(new User { UserId = 1, KeycloakId = "kc-student", Role = "student" });
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            sut.CreateBroadcastAsync("kc-student", new CreateBroadcastDto()));
        Assert.Contains("Only advisors", ex.Message);
    }

    [Fact]
    public async Task CreateBroadcastAsync_WhenAdvisorNotLinked_ThrowsException()
    {
        using var context = CreateContext();
        context.Users.Add(new User
        {
            UserId = 2, KeycloakId = "kc-adv-nolink",
            Role = "advisor",
            LinkedAdvisorId = null
        });
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            sut.CreateBroadcastAsync("kc-adv-nolink", new CreateBroadcastDto
            {
                Title = "Test", Content = "Content"
            }));
        Assert.Contains("not linked", ex.Message);
    }

    [Fact]
    public async Task CreateBroadcastAsync_WhenTitleIsEmpty_ThrowsException()
    {
        using var context = CreateContext();
        var advisor = new Advisor
        {
            AdvisorId = 1, Name = "Dr. A", Email = "a@a.com", Office = "B1", OfficeHours = "9-12"
        };
        context.Advisors.Add(advisor);
        context.Users.Add(new User
        {
            UserId = 3, KeycloakId = "kc-adv", Role = "advisor", LinkedAdvisorId = 1
        });
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            sut.CreateBroadcastAsync("kc-adv", new CreateBroadcastDto
            {
                Title = "", Content = "Some content"
            }));
        Assert.Contains("title is required", ex.Message);
    }

    [Fact]
    public async Task CreateBroadcastAsync_WhenContentIsEmpty_ThrowsException()
    {
        using var context = CreateContext();
        var advisor = new Advisor
        {
            AdvisorId = 2, Name = "Dr. B", Email = "b@b.com", Office = "B2", OfficeHours = "10-12"
        };
        context.Advisors.Add(advisor);
        context.Users.Add(new User
        {
            UserId = 4, KeycloakId = "kc-adv2", Role = "advisor", LinkedAdvisorId = 2
        });
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            sut.CreateBroadcastAsync("kc-adv2", new CreateBroadcastDto
            {
                Title = "Valid Title", Content = ""
            }));
        Assert.Contains("content is required", ex.Message);
    }

    [Fact]
    public async Task CreateBroadcastAsync_WhenNoStudentsSelected_ThrowsException()
    {
        using var context = CreateContext();
        var advisor = new Advisor
        {
            AdvisorId = 3, Name = "Dr. C", Email = "c@c.com", Office = "B3", OfficeHours = "11-13"
        };
        context.Advisors.Add(advisor);
        context.Users.Add(new User
        {
            UserId = 5, KeycloakId = "kc-adv3", Role = "advisor", LinkedAdvisorId = 3
        });
        // No students assigned to advisor 3
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            sut.CreateBroadcastAsync("kc-adv3", new CreateBroadcastDto
            {
                Title = "Hello", Content = "World"
            }));
        Assert.Contains("No students selected", ex.Message);
    }

    // ─── GetMyBroadcastsAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task GetMyBroadcastsAsync_WhenUserNotFound_ThrowsException()
    {
        using var context = CreateContext();
        var sut = CreateSut(context);

        await Assert.ThrowsAsync<Exception>(() =>
            sut.GetMyBroadcastsAsync("nonexistent-kc"));
    }

    [Fact]
    public async Task GetMyBroadcastsAsync_WhenAdminRole_ThrowsException()
    {
        using var context = CreateContext();
        context.Users.Add(new User
        {
            UserId = 6, KeycloakId = "kc-admin", Role = "admin"
        });
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            sut.GetMyBroadcastsAsync("kc-admin"));
        Assert.Contains("Only advisors and students", ex.Message);
    }

    // ─── MarkBroadcastAsReadAsync ─────────────────────────────────────────────

    [Fact]
    public async Task MarkBroadcastAsReadAsync_WhenUserIsAdvisor_ThrowsException()
    {
        using var context = CreateContext();
        context.Users.Add(new User
        {
            UserId = 7, KeycloakId = "kc-adv-read", Role = "advisor"
        });
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            sut.MarkBroadcastAsReadAsync("kc-adv-read", 1));
        Assert.Contains("Only students", ex.Message);
    }
}
