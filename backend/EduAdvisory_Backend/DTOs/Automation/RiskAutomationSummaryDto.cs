using System.Collections.Generic;

namespace EduAdvisory_Backend.DTOs.Automation;

public class RiskAutomationSummaryDto
{
    public int ProcessedStudents { get; set; }
    public int LowRiskMessagesSent { get; set; }
    public int MediumRiskAdvisorNotifications { get; set; }
    public int HighRiskMeetingRecommendations { get; set; }
    public int SkippedDuplicates { get; set; }
    public int FailedActions { get; set; }

    public List<RiskAutomationItemDto> Items { get; set; } = new();
}
