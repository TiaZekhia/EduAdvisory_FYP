namespace EduAdvisory_Backend.DTOs.Admin;

public class AdminAvailableUserOptionDto
{
    public int LinkedEntityId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? SecondaryText { get; set; }
}