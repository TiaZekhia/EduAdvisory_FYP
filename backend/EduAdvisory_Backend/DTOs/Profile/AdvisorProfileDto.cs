namespace EduAdvisory_Backend.DTOs.Profile;

public class AdvisorProfileDto
{
    public string Name { get; set; } = null!;
    public int AdvisorId { get; set; }
    public string Email { get; set; } = null!;

    public string? Office { get; set; }
    public string? OfficeHours { get; set; }

    public int AssignedStudentsCount { get; set; }
    public List<string> ProgramsSupervised { get; set; } = new();
}