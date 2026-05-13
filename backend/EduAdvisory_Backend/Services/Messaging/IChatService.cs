using EduAdvisory_Backend.DTOs.Messages;

namespace EduAdvisory_Backend.Services.Messaging;

public interface IChatService
{
    Task<List<ConversationDto>> GetMyConversationsAsync(string keycloakId);

    Task<ConversationDto> StartConversationAsync(string keycloakId, int studentId);

    Task<List<MessageDto>> GetMessagesAsync(string keycloakId, int conversationId);

    Task<MessageDto> SendMessageAsync(string keycloakId, SendMessageDto dto);

    Task<ConversationDto> StartConversationWithMyAdvisorAsync(string keycloakId);

    Task<List<AdvisorStudentDto>> GetMyAssignedStudentsAsync(string keycloakId);

    Task<MessageDto> SendMessageWithFilesAsync(string keycloakId, SendMessageWithFileDto dto);

    Task MarkAsReadAsync(string keycloakId, int conversationId);

    Task<MessageDto> EditMessageAsync(string keycloakId, int messageId, EditMessageDto dto);

    Task DeleteMessageAsync(string keycloakId, int messageId);

    Task<int> GetUnreadMessagesCountAsync(string keycloakId);
}