using EduAdvisory_Backend.Hubs;
using EduAdvisory_Backend.Models;
using EduAdvisory_Backend.Repositories.Messaging;
using EduAdvisory_Backend.Services.Messaging;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace EduAdvisory_Backend.Tests.Services;

public class ChatServiceTests
{
    private readonly Mock<IChatRepository> _chatRepoMock = new();
    private readonly Mock<IHubContext<ChatHub>> _hubContextMock = new();

    private static EduAdvisoryDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<EduAdvisoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableServiceProviderCaching(false)
            .Options);

    private ChatService CreateSut(EduAdvisoryDbContext context)
    {
        var clientsMock = new Mock<IHubClients>();
        var proxyMock = new Mock<IClientProxy>();
        clientsMock.Setup(c => c.User(It.IsAny<string>())).Returns(proxyMock.Object);
        proxyMock.Setup(p => p.SendCoreAsync(
            It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _hubContextMock.Setup(h => h.Clients).Returns(clientsMock.Object);
        return new ChatService(context, _chatRepoMock.Object, _hubContextMock.Object);
    }

    // ─── GetMyConversationsAsync ──────────────────────────────────────────────

    [Fact]
    public async Task GetMyConversationsAsync_WhenUserNotFound_ThrowsException()
    {
        using var context = CreateContext();
        var sut = CreateSut(context);

        await Assert.ThrowsAsync<Exception>(() =>
            sut.GetMyConversationsAsync("nonexistent"));
    }

    [Fact]
    public async Task GetMyConversationsAsync_WhenAdvisorNotLinked_ThrowsException()
    {
        using var context = CreateContext();
        context.Users.Add(new User
        {
            UserId = 1, KeycloakId = "kc-adv", Role = "advisor", LinkedAdvisorId = null
        });
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            sut.GetMyConversationsAsync("kc-adv"));
        Assert.Contains("not linked", ex.Message);
    }

    [Fact]
    public async Task GetMyConversationsAsync_WhenStudentNotLinked_ThrowsException()
    {
        using var context = CreateContext();
        context.Users.Add(new User
        {
            UserId = 2, KeycloakId = "kc-stu", Role = "student", LinkedStudentId = null
        });
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            sut.GetMyConversationsAsync("kc-stu"));
        Assert.Contains("not linked", ex.Message);
    }

    [Fact]
    public async Task GetMyConversationsAsync_WhenUnsupportedRole_ThrowsException()
    {
        using var context = CreateContext();
        context.Users.Add(new User
        {
            UserId = 3, KeycloakId = "kc-admin", Role = "admin"
        });
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            sut.GetMyConversationsAsync("kc-admin"));
        Assert.Contains("Only advisors and students", ex.Message);
    }

    // ─── StartConversationAsync ───────────────────────────────────────────────

    [Fact]
    public async Task StartConversationAsync_WhenUserIsStudent_ThrowsException()
    {
        using var context = CreateContext();
        context.Users.Add(new User
        {
            UserId = 4, KeycloakId = "kc-stu2", Role = "student"
        });
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            sut.StartConversationAsync("kc-stu2", 1));
        Assert.Contains("Only advisors", ex.Message);
    }

    [Fact]
    public async Task StartConversationAsync_WhenAdvisorNotLinked_ThrowsException()
    {
        using var context = CreateContext();
        context.Users.Add(new User
        {
            UserId = 5, KeycloakId = "kc-adv2", Role = "advisor", LinkedAdvisorId = null
        });
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            sut.StartConversationAsync("kc-adv2", 1));
        Assert.Contains("not linked", ex.Message);
    }

    [Fact]
    public async Task StartConversationAsync_WhenStudentNotFound_ThrowsException()
    {
        using var context = CreateContext();
        var advisor = new Advisor
        {
            AdvisorId = 1, Name = "Dr. A", Email = "a@a.com", Office = "B1", OfficeHours = "9-12"
        };
        context.Advisors.Add(advisor);
        context.Users.Add(new User
        {
            UserId = 6, KeycloakId = "kc-adv3", Role = "advisor", LinkedAdvisorId = 1
        });
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            sut.StartConversationAsync("kc-adv3", 999));
        Assert.Contains("Student not found", ex.Message);
    }

    [Fact]
    public async Task StartConversationAsync_WhenStudentBelongsToDifferentAdvisor_ThrowsException()
    {
        using var context = CreateContext();
        var advisor = new Advisor
        {
            AdvisorId = 10, Name = "Dr. A", Email = "a@a.com", Office = "B1", OfficeHours = "9-12"
        };
        context.Advisors.Add(advisor);
        context.Users.Add(new User
        {
            UserId = 7, KeycloakId = "kc-adv10", Role = "advisor", LinkedAdvisorId = 10
        });
        context.SisStudents.Add(new SisStudent
        {
            StudentId = 20, AdvisorId = 99 // different advisor
        });
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            sut.StartConversationAsync("kc-adv10", 20));
        Assert.Contains("assigned students", ex.Message);
    }

    // ─── GetMessagesAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetMessagesAsync_WhenConversationNotFound_ThrowsException()
    {
        using var context = CreateContext();
        context.Users.Add(new User
        {
            UserId = 8, KeycloakId = "kc-u8", Role = "student", LinkedStudentId = 1
        });
        await context.SaveChangesAsync();

        _chatRepoMock.Setup(r => r.GetConversationByIdAsync(99))
            .ReturnsAsync((Conversation)null!);

        var sut = CreateSut(context);

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            sut.GetMessagesAsync("kc-u8", 99));
        Assert.Contains("Conversation not found", ex.Message);
    }

    // ─── StartConversationWithMyAdvisorAsync ──────────────────────────────────

    [Fact]
    public async Task StartConversationWithMyAdvisorAsync_WhenUserIsAdvisor_ThrowsException()
    {
        using var context = CreateContext();
        context.Users.Add(new User
        {
            UserId = 9, KeycloakId = "kc-adv-me", Role = "advisor"
        });
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            sut.StartConversationWithMyAdvisorAsync("kc-adv-me"));
        Assert.Contains("Only students", ex.Message);
    }

    [Fact]
    public async Task StartConversationWithMyAdvisorAsync_WhenStudentHasNoAdvisor_ThrowsException()
    {
        using var context = CreateContext();
        var student = new SisStudent { StudentId = 30, AdvisorId = null };
        context.SisStudents.Add(student);
        context.Users.Add(new User
        {
            UserId = 10, KeycloakId = "kc-stu-noadv", Role = "student",
            LinkedStudentId = 30
        });
        await context.SaveChangesAsync();

        var sut = CreateSut(context);

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            sut.StartConversationWithMyAdvisorAsync("kc-stu-noadv"));
        Assert.Contains("assigned advisor", ex.Message);
    }

    // ─── GetUnreadMessagesCountAsync ──────────────────────────────────────────

    [Fact]
    public async Task GetUnreadMessagesCountAsync_WhenUserIsAdvisorWithLinkedId_ReturnsCount()
    {
        using var context = CreateContext();
        var advisor = new Advisor
        {
            AdvisorId = 20, Name = "Dr. D", Email = "d@d.com", Office = "C1", OfficeHours = "9-10"
        };
        context.Advisors.Add(advisor);
        context.Users.Add(new User
        {
            UserId = 11, KeycloakId = "kc-adv-count", Role = "advisor", LinkedAdvisorId = 20
        });
        await context.SaveChangesAsync();

        _chatRepoMock.Setup(r => r.GetUnreadMessagesCountAsync(11, 20, null))
            .ReturnsAsync(7);

        var sut = CreateSut(context);
        var count = await sut.GetUnreadMessagesCountAsync("kc-adv-count");

        Assert.Equal(7, count);
    }
}
