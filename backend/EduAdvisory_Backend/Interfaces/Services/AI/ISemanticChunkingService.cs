using EduAdvisory_Backend.Models;

namespace EduAdvisory_Backend.Interfaces.Services.AI;

public interface ISemanticChunkingService
{
    List<DocumentChunkCandidate> CreateChunks(
        List<ExtractedPdfPage> pages,
        string documentType);
}