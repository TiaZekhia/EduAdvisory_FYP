using EduAdvisory_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace EduAdvisory_Backend.Repositories.Messaging;

public class ChatRepository : IChatRepository
{
    private readonly EduAdvisoryDbContext _context;

    public ChatRepository(EduAdvisoryDbContext context)
    {
        _context = context;
    }

    public async Task<Conversation?> GetConversationByIdAsync(int conversationId)
    {
        return await _context.Conversations
            .Include(c => c.Advisor)
            .Include(c => c.Student)
            .Include(c => c.Messages)
                 .ThenInclude(m => m.SenderUser)
            .Include(c => c.Messages)
                 .ThenInclude(m => m.Attachments)
            .FirstOrDefaultAsync(c => c.ConversationId == conversationId);
    }

    public async Task<Conversation?> GetConversationByAdvisorAndStudentAsync(int advisorId, int studentId)
    {
        return await _context.Conversations
            .Include(c => c.Advisor)
            .Include(c => c.Student)
            .FirstOrDefaultAsync(c =>
                c.AdvisorId == advisorId &&
                c.StudentId == studentId);
    }

    public async Task<Conversation> CreateConversationAsync(Conversation conversation)
    {
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        return conversation;
    }

    public async Task<List<Conversation>> GetConversationsForAdvisorAsync(int advisorId)
    {
        return await _context.Conversations
            .Include(c => c.Advisor)
            .Include(c => c.Student)
            .Include(c => c.Messages)
            .Where(c => c.AdvisorId == advisorId)
            .OrderByDescending(c =>
                c.Messages
                    .OrderByDescending(m => m.SentAt)
                    .Select(m => m.SentAt)
                    .FirstOrDefault())
            .ToListAsync();
    }

    public async Task<List<Conversation>> GetConversationsForStudentAsync(int studentId)
    {
        return await _context.Conversations
            .Include(c => c.Advisor)
            .Include(c => c.Student)
            .Include(c => c.Messages)
            .Where(c => c.StudentId == studentId)
            .OrderByDescending(c =>
                c.Messages
                    .OrderByDescending(m => m.SentAt)
                    .Select(m => m.SentAt)
                    .FirstOrDefault())
            .ToListAsync();
    }

    public async Task<List<ChatMessage>> GetMessagesAsync(int conversationId)
    {
        return await _context.ChatMessages
            .Include(m => m.Attachments)
            .Include(m => m.SenderUser)
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.SentAt)
            .ToListAsync();
    }

    public async Task<ChatMessage> AddMessageAsync(ChatMessage message)
    {
        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();

        return message;
    }

    public async Task MarkMessagesAsReadAsync(int conversationId, int readerUserId)
    {
        var unreadMessages = await _context.ChatMessages
            .Where(m =>
                m.ConversationId == conversationId &&
                m.SenderUserId != readerUserId &&
                !m.IsRead)
            .ToListAsync();

        foreach (var message in unreadMessages)
        {
            message.IsRead = true;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<int> GetUnreadMessagesCountAsync(
     int currentUserId,
     int? advisorId = null,
     int? studentId = null)
    {
        var query = _context.ChatMessages
            .Where(m =>
                !m.IsRead &&
                !m.IsDeleted &&
                m.SenderUserId != currentUserId);

        if (advisorId.HasValue)
        {
            query = query.Where(m => m.Conversation.AdvisorId == advisorId.Value);
        }

        if (studentId.HasValue)
        {
            query = query.Where(m => m.Conversation.StudentId == studentId.Value);
        }

        return await query.CountAsync();
    }
}