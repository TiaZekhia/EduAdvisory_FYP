namespace EduAdvisory_Backend.DTOs.Messages;

public class AdvisorStudentDto
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = null!;
    public string? Email { get; set; }
    public string? ProgramCode { get; set; }
    public int? CurrentSemester { get; set; }
    public int UnreadCount { get; set; }

}