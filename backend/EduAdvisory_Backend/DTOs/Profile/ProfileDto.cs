namespace EduAdvisory_Backend.DTOs.Profile;

public class ProfileDto
{
    public int UserId { get; set; }
    public string? Username { get; set; }
    public string? Role { get; set; }

    public StudentProfileDto? StudentProfile { get; set; }
    public AdvisorProfileDto? AdvisorProfile { get; set; }
}