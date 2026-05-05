using EduAdvisory_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace EduAdvisory_Backend.Repositories.Messaging;

public class BroadcastRepository : IBroadcastRepository
{
    private readonly EduAdvisoryDbContext _context;

    public BroadcastRepository(EduAdvisoryDbContext context)
    {
        _context = context;
    }

    public async Task<BroadcastMessage> CreateBroadcastAsync(BroadcastMessage broadcast)
    {
        _context.BroadcastMessages.Add(broadcast);
        await _context.SaveChangesAsync();

        return broadcast;
    }

    public async Task<List<BroadcastMessage>> GetBroadcastsForAdvisorAsync(int advisorId)
    {
        return await _context.BroadcastMessages
            .Include(b => b.Advisor)
            .Include(b => b.Recipients)
            .Include(b => b.Attachments)
            .Where(b => b.AdvisorId == advisorId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<BroadcastRecipient>> GetBroadcastsForStudentAsync(int studentId)
    {
        return await _context.BroadcastRecipients
            .Include(r => r.BroadcastMessage)
                .ThenInclude(b => b.Advisor)
            .Include(r => r.BroadcastMessage)
                .ThenInclude(b => b.Attachments)
            .Where(r => r.StudentId == studentId)
            .OrderByDescending(r => r.BroadcastMessage.CreatedAt)
            .ToListAsync();
    }

    public async Task<BroadcastRecipient?> GetRecipientAsync(int broadcastMessageId, int studentId)
    {
        return await _context.BroadcastRecipients
            .Include(r => r.BroadcastMessage)
            .FirstOrDefaultAsync(r =>
                r.BroadcastMessageId == broadcastMessageId &&
                r.StudentId == studentId);
    }

    public async Task MarkAsReadAsync(BroadcastRecipient recipient)
    {
        recipient.IsRead = true;
        recipient.ReadAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }
}