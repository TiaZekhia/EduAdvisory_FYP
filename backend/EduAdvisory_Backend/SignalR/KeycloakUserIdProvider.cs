using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace EduAdvisory_Backend.SignalR;

public class KeycloakUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User?.FindFirst("sub")?.Value
            ?? connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}