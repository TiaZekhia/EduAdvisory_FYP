namespace EduAdvisory_Backend.DTOs.Admin;

public class AdminManagedUserDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? KeycloakId { get; set; }
    public int LinkedEntityId { get; set; }
    public string LinkedDisplayName { get; set; } = string.Empty;
    public string? LinkedEmail { get; set; }
    public string? SecondaryText { get; set; }
}