using EduAdvisory_Backend.Models;

namespace EduAdvisory_Backend.Interfaces.Services.AI;

public interface IVectorSearchService
{
    Task<List<RetrievedChunk>> SearchRelevantChunksAsync(
        string query,
        int studentId,
        string? courseCode = null,
        CancellationToken cancellationToken = default);
}