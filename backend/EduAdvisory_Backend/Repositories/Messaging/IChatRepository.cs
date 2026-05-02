using EduAdvisory_Backend.Models;

namespace EduAdvisory_Backend.Repositories.Messaging;

public interface IChatRepository
{
    Task<Conversation?> GetConversationByIdAsync(int conversationId);

    Task<Conversation?> GetConversationByAdvisorAndStudentAsync(int advisorId, int studentId);

    Task<Conversation> CreateConversationAsync(Conversation conversation);

    Task<List<Conversation>> GetConversationsForAdvisorAsync(int advisorId);

    Task<List<Conversation>> GetConversationsForStudentAsync(int studentId);

    Task<List<ChatMessage>> GetMessagesAsync(int conversationId);

    Task<ChatMessage> AddMessageAsync(ChatMessage message);

    Task MarkMessagesAsReadAsync(int conversationId, int readerUserId);
}