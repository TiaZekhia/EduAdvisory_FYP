namespace EduAdvisory_Backend.Interfaces.Services.AI;

public interface IAiDocumentProcessingService
{
    Task ProcessDocumentAsync(
        int documentId,
        CancellationToken cancellationToken = default);
}