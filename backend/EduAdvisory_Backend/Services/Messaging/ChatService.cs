using EduAdvisory_Backend.DTOs.Messages;
using EduAdvisory_Backend.Hubs;
using EduAdvisory_Backend.Models;
using EduAdvisory_Backend.Repositories.Messaging;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace EduAdvisory_Backend.Services.Messaging;

public class ChatService : IChatService
{
    private readonly EduAdvisoryDbContext _context;
    private readonly IChatRepository _chatRepository;
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatService(
        EduAdvisoryDbContext context,
        IChatRepository chatRepository,
        IHubContext<ChatHub> hubContext)
    {
        _context = context;
        _chatRepository = chatRepository;
        _hubContext = hubContext;
    }

    public async Task<List<ConversationDto>> GetMyConversationsAsync(string keycloakId)
    {
        var user = await GetCurrentUserAsync(keycloakId);

        List<Conversation> conversations;

        if (user.Role?.ToLower() == "advisor")
        {
            if (user.LinkedAdvisorId == null)
                throw new Exception("Advisor account is not linked to an advisor profile.");

            conversations = await _chatRepository
                .GetConversationsForAdvisorAsync(user.LinkedAdvisorId.Value);
        }
        else if (user.Role?.ToLower() == "student")
        {
            if (user.LinkedStudentId == null)
                throw new Exception("Student account is not linked to a student profile.");

            conversations = await _chatRepository
                .GetConversationsForStudentAsync(user.LinkedStudentId.Value);
        }
        else
        {
            throw new Exception("Only advisors and students can use chat.");
        }

        return conversations.Select(c => ToConversationDto(c, user.UserId)).ToList();
    }

    public async Task<ConversationDto> StartConversationAsync(string keycloakId, int studentId)
    {
        var user = await GetCurrentUserAsync(keycloakId);

        if (user.Role?.ToLower() != "advisor")
            throw new Exception("Only advisors can start a conversation with a student.");

        if (user.LinkedAdvisorId == null)
            throw new Exception("Advisor account is not linked to an advisor profile.");

        var advisorId = user.LinkedAdvisorId.Value;

        var student = await _context.SisStudents
            .FirstOrDefaultAsync(s => s.StudentId == studentId);

        if (student == null)
            throw new Exception("Student not found.");

        if (student.AdvisorId != advisorId)
            throw new Exception("You can only message your assigned students.");

        var conversation = await _chatRepository
            .GetConversationByAdvisorAndStudentAsync(advisorId, studentId);

        if (conversation == null)
        {
            conversation = new Conversation
            {
                AdvisorId = advisorId,
                StudentId = studentId,
                CreatedAt = DateTime.UtcNow
            };

            conversation = await _chatRepository.CreateConversationAsync(conversation);

            conversation = await _chatRepository
                .GetConversationByIdAsync(conversation.ConversationId);
        }

        return ToConversationDto(conversation!, user.UserId);
    }

    public async Task<List<MessageDto>> GetMessagesAsync(string keycloakId, int conversationId)
    {
        var user = await GetCurrentUserAsync(keycloakId);

        var conversation = await _chatRepository.GetConversationByIdAsync(conversationId);

        if (conversation == null)
            throw new Exception("Conversation not found.");

        ValidateConversationAccess(user, conversation);

        var messages = await _chatRepository.GetMessagesAsync(conversationId);

        return messages.Select(m => ToMessageDto(m, user.UserId)).ToList();
    }

    public async Task<MessageDto> SendMessageAsync(string keycloakId, SendMessageDto dto)
    {
        var user = await GetCurrentUserAsync(keycloakId);

        var conversation = await _chatRepository.GetConversationByIdAsync(dto.ConversationId);

        if (conversation == null)
            throw new Exception("Conversation not found.");

        ValidateConversationAccess(user, conversation);

        if (string.IsNullOrWhiteSpace(dto.Content))
            throw new Exception("Message content cannot be empty.");

        var message = new ChatMessage
        {
            ConversationId = dto.ConversationId,
            SenderUserId = user.UserId,
            Content = dto.Content.Trim(),
            SentAt = DateTime.UtcNow,
            IsRead = false
        };

        var savedMessage = await _chatRepository.AddMessageAsync(message);

        savedMessage = await _context.ChatMessages
            .Include(m => m.SenderUser)
            .FirstAsync(m => m.MessageId == savedMessage.MessageId);

        var senderMessageDto = ToMessageDto(savedMessage, user.UserId);

        var receiverUser = await GetReceiverUserAsync(user, conversation);

        if (receiverUser != null)
        {
            var receiverMessageDto = ToMessageDto(savedMessage, receiverUser.UserId);

            await _hubContext.Clients.User(receiverUser.KeycloakId!)
                .SendAsync("ReceiveMessage", receiverMessageDto);
        }

        await _hubContext.Clients.User(user.KeycloakId!)
    .SendAsync("MessageSent", senderMessageDto);

        return senderMessageDto;
    }

    public async Task MarkAsReadAsync(string keycloakId, int conversationId)
    {
        var user = await GetCurrentUserAsync(keycloakId);

        var conversation = await _chatRepository.GetConversationByIdAsync(conversationId);

        if (conversation == null)
            throw new Exception("Conversation not found.");

        ValidateConversationAccess(user, conversation);

        await _chatRepository.MarkMessagesAsReadAsync(conversationId, user.UserId);
    }

    private async Task<User> GetCurrentUserAsync(string keycloakId)
    {
        var user = await _context.Users
            .Include(u => u.LinkedAdvisor)
            .Include(u => u.LinkedStudent)
            .FirstOrDefaultAsync(u => u.KeycloakId == keycloakId);

        if (user == null)
            throw new Exception("User not found in local database.");

        return user;
    }

    public async Task<ConversationDto> StartConversationWithMyAdvisorAsync(string keycloakId)
    {
        var user = await GetCurrentUserAsync(keycloakId);

        if (user.Role?.ToLower() != "student")
            throw new Exception("Only students can start a conversation with their advisor.");

        if (user.LinkedStudentId == null)
            throw new Exception("Student account is not linked to a student profile.");

        var student = await _context.SisStudents
            .Include(s => s.Advisor)
            .FirstOrDefaultAsync(s => s.StudentId == user.LinkedStudentId.Value);

        if (student == null)
            throw new Exception("Student profile not found.");

        if (student.AdvisorId == null)
            throw new Exception("You do not have an assigned advisor yet.");

        var conversation = await _chatRepository
            .GetConversationByAdvisorAndStudentAsync(student.AdvisorId.Value, student.StudentId);

        if (conversation == null)
        {
            conversation = new Conversation
            {
                AdvisorId = student.AdvisorId.Value,
                StudentId = student.StudentId,
                CreatedAt = DateTime.UtcNow
            };

            conversation = await _chatRepository.CreateConversationAsync(conversation);

            conversation = await _chatRepository
                .GetConversationByIdAsync(conversation.ConversationId);
        }

        return ToConversationDto(conversation!, user.UserId);
    }

    public async Task<List<AdvisorStudentDto>> GetMyAssignedStudentsAsync(string keycloakId)
    {
        var user = await GetCurrentUserAsync(keycloakId);

        if (user.Role?.ToLower() != "advisor")
            throw new Exception("Only advisors can view assigned students.");

        if (user.LinkedAdvisorId == null)
            throw new Exception("Advisor account is not linked to an advisor profile.");

        return await _context.SisStudents
            .Where(s => s.AdvisorId == user.LinkedAdvisorId.Value)
            .OrderBy(s => s.FirstName)
            .ThenBy(s => s.LastName)
            .Select(s => new AdvisorStudentDto
            {
                StudentId = s.StudentId,
                FullName = ((s.FirstName ?? "") + " " + (s.LastName ?? "")).Trim(),
                Email = s.Email,
                ProgramCode = s.ProgramCode,
                CurrentSemester = s.CurrentSemester
            })
            .ToListAsync();
    }


    private void ValidateConversationAccess(User user, Conversation conversation)
    {
        var role = user.Role?.ToLower();

        if (role == "advisor")
        {
            if (user.LinkedAdvisorId != conversation.AdvisorId)
                throw new Exception("You do not have access to this conversation.");
        }
        else if (role == "student")
        {
            if (user.LinkedStudentId != conversation.StudentId)
                throw new Exception("You do not have access to this conversation.");
        }
        else
        {
            throw new Exception("Only advisors and students can access conversations.");
        }
    }

    private async Task<User?> GetReceiverUserAsync(User sender, Conversation conversation)
    {
        var role = sender.Role?.ToLower();

        if (role == "advisor")
        {
            return await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.LinkedStudentId == conversation.StudentId &&
                    u.KeycloakId != null);
        }

        if (role == "student")
        {
            return await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.LinkedAdvisorId == conversation.AdvisorId &&
                    u.KeycloakId != null);
        }

        return null;
    }

    private ConversationDto ToConversationDto(Conversation conversation, int currentUserId)
    {
        var lastMessage = conversation.Messages
            .OrderByDescending(m => m.SentAt)
            .FirstOrDefault();

        return new ConversationDto
        {
            ConversationId = conversation.ConversationId,
            AdvisorId = conversation.AdvisorId,
            AdvisorName = conversation.Advisor?.Name ?? "Advisor",
            StudentId = conversation.StudentId,
            StudentName = $"{conversation.Student?.FirstName} {conversation.Student?.LastName}".Trim(),
            CreatedAt = conversation.CreatedAt,
            LastMessage = lastMessage?.Content,
            LastMessageAt = lastMessage?.SentAt,
            UnreadCount = conversation.Messages.Count(m =>
                m.SenderUserId != currentUserId &&
                !m.IsRead)
        };
    }

    private MessageDto ToMessageDto(ChatMessage message, int currentUserId)
    {
        return new MessageDto
        {
            MessageId = message.MessageId,
            ConversationId = message.ConversationId,
            SenderUserId = message.SenderUserId,
            SenderRole = message.SenderUser?.Role ?? "unknown",
            Content = message.Content,
            SentAt = message.SentAt,
            IsRead = message.IsRead,
            IsMine = message.SenderUserId == currentUserId
        };
    }
}