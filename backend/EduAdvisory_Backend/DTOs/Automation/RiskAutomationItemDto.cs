namespace EduAdvisory_Backend.DTOs.Automation;

public class RiskAutomationItemDto
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int AdvisorId { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public int RiskScore { get; set; }
    public string ActionTaken { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}
