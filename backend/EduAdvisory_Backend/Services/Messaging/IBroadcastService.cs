using EduAdvisory_Backend.DTOs.Broadcasts;

namespace EduAdvisory_Backend.Services.Messaging;

public interface IBroadcastService
{
    Task<BroadcastDto> CreateBroadcastAsync(string keycloakId, CreateBroadcastDto dto);

    Task<List<BroadcastDto>> GetMyBroadcastsAsync(string keycloakId);

    Task MarkBroadcastAsReadAsync(string keycloakId, int broadcastMessageId);
}