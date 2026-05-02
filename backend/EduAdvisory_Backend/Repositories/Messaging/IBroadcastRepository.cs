using EduAdvisory_Backend.Models;

namespace EduAdvisory_Backend.Repositories.Messaging;

public interface IBroadcastRepository
{
    Task<BroadcastMessage> CreateBroadcastAsync(BroadcastMessage broadcast);

    Task<List<BroadcastMessage>> GetBroadcastsForAdvisorAsync(int advisorId);

    Task<List<BroadcastRecipient>> GetBroadcastsForStudentAsync(int studentId);

    Task<BroadcastRecipient?> GetRecipientAsync(int broadcastMessageId, int studentId);

    Task MarkAsReadAsync(BroadcastRecipient recipient);
}