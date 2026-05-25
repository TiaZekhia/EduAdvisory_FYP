using EduAdvisory_Backend.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduAdvisory_Backend.Controllers;

[ApiController]
[Route("api/automation")]
public class AutomationController : ControllerBase
{
    private readonly IRiskAutomationService _riskAutomationService;

    public AutomationController(IRiskAutomationService riskAutomationService)
    {
        _riskAutomationService = riskAutomationService;
    }

    /// <summary>
    /// Run risk intervention automation for all active students.
    /// Calculates risk for each student and takes appropriate action:
    /// - LOW risk: sends advisor message
    /// - MEDIUM risk: creates advisor notification
    /// - HIGH risk: creates meeting recommendation
    /// 
    /// Skips duplicate interventions within 14-day cooldown period.
    /// This endpoint is designed to be called by n8n every 2 weeks.
    /// Requires AutomationAdmin role (service account via Keycloak client credentials flow).
    /// </summary>
    /// <returns>Summary of automation run with details on each action taken</returns>
    [Authorize(Roles = "AutomationAdmin")]
    [HttpPost("risk-interventions/run")]
    public async Task<IActionResult> RunRiskInterventions()
    {
        try
        {
            var result = await _riskAutomationService.RunRiskInterventionsAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
