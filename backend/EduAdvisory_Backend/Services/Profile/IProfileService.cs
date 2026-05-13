using EduAdvisory_Backend.DTOs.Profile;

namespace EduAdvisory_Backend.Services.Profile;

public interface IProfileService
{
    Task<ProfileDto> GetMyProfileAsync(string keycloakId);
}