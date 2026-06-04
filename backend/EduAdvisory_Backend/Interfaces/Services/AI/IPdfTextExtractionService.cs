using EduAdvisory_Backend.Models;

namespace EduAdvisory_Backend.Interfaces.Services.AI;

public interface IPdfTextExtractionService
{
    Task<List<ExtractedPdfPage>> ExtractPagesAsync(
        string filePath,
        CancellationToken cancellationToken = default);
}