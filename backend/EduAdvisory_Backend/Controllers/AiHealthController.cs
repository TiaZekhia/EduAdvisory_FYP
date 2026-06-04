using EduAdvisory_Backend.Interfaces.Services.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduAdvisory_Backend.Controllers;

[ApiController]
[Route("api/ai-health")]
public class AiHealthController : ControllerBase
{
    private readonly IEmbeddingService _embeddingService;

    public AiHealthController(IEmbeddingService embeddingService)
    {
        _embeddingService = embeddingService;
    }

    [Authorize(Roles = "ADMIN")]
    [HttpGet("embedding-test")]
    public async Task<IActionResult> TestEmbedding()
    {
        var embedding = await _embeddingService.GenerateEmbeddingAsync(
            "This is a test academic advising document."
        );

        return Ok(new
        {
            Dimensions = embedding.Length,
            FirstValues = embedding.Take(5)
        });
    }
}