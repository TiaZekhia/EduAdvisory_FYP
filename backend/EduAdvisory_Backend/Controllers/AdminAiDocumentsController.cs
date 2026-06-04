using System.Security.Claims;
using EduAdvisory_Backend.DTOs.AI.AdminDocuments;
using EduAdvisory_Backend.Interfaces.Services.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduAdvisory_Backend.Controllers;

[ApiController]
[Route("api/admin/ai-documents")]
[Authorize(Roles = "ADMIN")]
public class AdminAiDocumentsController : ControllerBase
{
    private readonly IAdminAiDocumentService _documentService;

    public AdminAiDocumentsController(IAdminAiDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(
        [FromForm] UploadAiDocumentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var uploadedByUserId = GetLocalUserId();

            var result = await _documentService.UploadDocumentAsync(
                request,
                uploadedByUserId,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.DocumentId },
                result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var documents = await _documentService.GetDocumentsAsync(cancellationToken);
        return Ok(documents);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var document = await _documentService.GetDocumentByIdAsync(id, cancellationToken);

        if (document == null)
        {
            return NotFound(new { message = "Document not found." });
        }

        return Ok(document);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var deleted = await _documentService.DeleteDocumentAsync(id, cancellationToken);

        if (!deleted)
        {
            return NotFound(new { message = "Document not found." });
        }

        return NoContent();
    }

    [HttpPost("{id:int}/reprocess")]
    public async Task<IActionResult> Reprocess(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _documentService.ReprocessDocumentAsync(id, cancellationToken);

        if (!result)
        {
            return NotFound(new { message = "Document not found." });
        }

        return Ok(new
        {
            message = "Document marked for reprocessing."
        });
    }

    private int? GetLocalUserId()
    {
        var userIdClaim =
            User.FindFirst("user_id")?.Value ??
            User.FindFirst("UserId")?.Value ??
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (int.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        return null;
    }
}