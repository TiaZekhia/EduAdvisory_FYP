using System.Security.Claims;
using EduAdvisory_Backend.Controllers;
using EduAdvisory_Backend.DTOs.Messages;
using EduAdvisory_Backend.Services.Messaging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace EduAdvisory_Backend.Tests.Controllers;

public class ChatControllerTests
{
    private readonly Mock<IChatService> _chatServiceMock = new();
    private readonly ChatController _sut;

    public ChatControllerTests()
    {
        _sut = new ChatController(_chatServiceMock.Object);
        _sut.ControllerContext = MakeContext("kc-123");
    }

    [Fact]
    public async Task GetMyConversations_ReturnsOkWithConversations()
    {
        var conversations = new List<ConversationDto> { new() { ConversationId = 1 } };
        _chatServiceMock.Setup(s => s.GetMyConversationsAsync("kc-123")).ReturnsAsync(conversations);

        var result = await _sut.GetMyConversations();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(conversations, ok.Value);
    }

    [Fact]
    public async Task StartConversation_ReturnsOkWithConversation()
    {
        var dto = new StartConversationDto { StudentId = 5 };
        var conversation = new ConversationDto { ConversationId = 1, StudentId = 5 };
        _chatServiceMock.Setup(s => s.StartConversationAsync("kc-123", 5)).ReturnsAsync(conversation);

        var result = await _sut.StartConversation(dto);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(conversation, ok.Value);
    }

    [Fact]
    public async Task GetMessages_ReturnsOkWithMessages()
    {
        var messages = new List<MessageDto> { new() { MessageId = 1 } };
        _chatServiceMock.Setup(s => s.GetMessagesAsync("kc-123", 1)).ReturnsAsync(messages);

        var result = await _sut.GetMessages(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(messages, ok.Value);
    }

    [Fact]
    public async Task SendMessage_ReturnsOkWithMessage()
    {
        var dto = new SendMessageDto { ConversationId = 1, Content = "Hello" };
        var message = new MessageDto { MessageId = 10 };
        _chatServiceMock.Setup(s => s.SendMessageAsync("kc-123", dto)).ReturnsAsync(message);

        var result = await _sut.SendMessage(dto);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(message, ok.Value);
    }

    [Fact]
    public async Task MarkAsRead_ReturnsNoContent()
    {
        _chatServiceMock.Setup(s => s.MarkAsReadAsync("kc-123", 1)).Returns(Task.CompletedTask);

        var result = await _sut.MarkAsRead(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task StartConversationWithMyAdvisor_ReturnsOkWithConversation()
    {
        var conversation = new ConversationDto { ConversationId = 2 };
        _chatServiceMock.Setup(s => s.StartConversationWithMyAdvisorAsync("kc-123")).ReturnsAsync(conversation);

        var result = await _sut.StartConversationWithMyAdvisor();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(conversation, ok.Value);
    }

    [Fact]
    public async Task GetMyAssignedStudents_ReturnsOkWithStudents()
    {
        var students = new List<AdvisorStudentDto> { new() { StudentId = 1 } };
        _chatServiceMock.Setup(s => s.GetMyAssignedStudentsAsync("kc-123")).ReturnsAsync(students);

        var result = await _sut.GetMyAssignedStudents();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(students, ok.Value);
    }

    [Fact]
    public async Task GetUnreadMessagesCount_ReturnsOkWithCount()
    {
        _chatServiceMock.Setup(s => s.GetUnreadMessagesCountAsync("kc-123")).ReturnsAsync(5);

        var result = await _sut.GetUnreadMessagesCount();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task DeleteMessage_ReturnsNoContent()
    {
        _chatServiceMock.Setup(s => s.DeleteMessageAsync("kc-123", 10)).Returns(Task.CompletedTask);

        var result = await _sut.DeleteMessage(10);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task EditMessage_ReturnsOkWithUpdatedMessage()
    {
        var dto = new EditMessageDto { Content = "Updated" };
        var message = new MessageDto { MessageId = 10, Content = "Updated" };
        _chatServiceMock.Setup(s => s.EditMessageAsync("kc-123", 10, dto)).ReturnsAsync(message);

        var result = await _sut.EditMessage(10, dto);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(message, ok.Value);
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
