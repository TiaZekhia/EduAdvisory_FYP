using EduAdvisory_Backend.DTOs.Automation;

namespace EduAdvisory_Backend.Interfaces.Services;

public interface IRiskAutomationService
{
    Task<RiskAutomationSummaryDto> RunRiskInterventionsAsync();
}
