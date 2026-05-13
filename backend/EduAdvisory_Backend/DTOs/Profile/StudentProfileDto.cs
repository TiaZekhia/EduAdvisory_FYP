namespace EduAdvisory_Backend.DTOs.Profile;

public class StudentProfileDto
{
    public string FullName { get; set; } = null!;
    public int StudentId { get; set; }
    public string? Email { get; set; }

    public string? ProgramCode { get; set; }
    public int? CurrentSemester { get; set; }
    public decimal? CurrentGpa { get; set; }
    public string? AcademicStatus { get; set; }

    public string? AdvisorName { get; set; }
    public string? AdvisorEmail { get; set; }
    public string? AdvisorOffice { get; set; }
    public string? AdvisorOfficeHours { get; set; }
}