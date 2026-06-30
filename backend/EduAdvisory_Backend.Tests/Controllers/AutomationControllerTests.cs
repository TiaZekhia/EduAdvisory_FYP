using EduAdvisory_Backend.Controllers;
using EduAdvisory_Backend.DTOs.Automation;
using EduAdvisory_Backend.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace EduAdvisory_Backend.Tests.Controllers;

public class AutomationControllerTests
{
    private readonly Mock<IRiskAutomationService> _automationServiceMock = new();
    private readonly AutomationController _sut;

    public AutomationControllerTests()
    {
        _sut = new AutomationController(_automationServiceMock.Object);
    }

    [Fact]
    public async Task RunRiskInterventions_WhenSuccessful_ReturnsOkWithSummary()
    {
        var summary = new RiskAutomationSummaryDto
        {
            ProcessedStudents = 10,
            LowRiskMessagesSent = 5,
            HighRiskMeetingRecommendations = 2
        };
        _automationServiceMock.Setup(s => s.RunRiskInterventionsAsync()).ReturnsAsync(summary);

        var result = await _sut.RunRiskInterventions();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(summary, ok.Value);
    }

    [Fact]
    public async Task RunRiskInterventions_WhenServiceThrows_Returns500()
    {
        _automationServiceMock.Setup(s => s.RunRiskInterventionsAsync())
            .ThrowsAsync(new Exception("Database connection failed"));

        var result = await _sut.RunRiskInterventions();

        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }
}
