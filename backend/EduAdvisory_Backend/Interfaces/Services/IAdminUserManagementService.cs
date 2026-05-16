using EduAdvisory_Backend.DTOs.Admin;

namespace EduAdvisory_Backend.Interfaces.Services;

public interface IAdminUserManagementService
{
    Task<List<AdminManagedUserDto>> GetUsersAsync(CancellationToken ct = default);
    Task<List<AdminAvailableUserOptionDto>> GetAvailableLinksAsync(string role, CancellationToken ct = default);
    Task<AdminManagedUserDto> CreateUserAsync(CreateManagedUserDto dto, CancellationToken ct = default);
    Task<AdminManagedUserDto> UpdateUserAsync(int userId, UpdateManagedUserDto dto, CancellationToken ct = default);
    Task<AdminManagedUserDto> DeactivateUserAsync(int userId, CancellationToken ct = default);
    Task<AdminManagedUserDto> ReactivateUserAsync(int userId, CancellationToken ct = default);
}