using EduAdvisory_Backend.DTOs.AI.AdminDocuments;

namespace EduAdvisory_Backend.Interfaces.Services.AI;

public interface IAdminAiDocumentService
{
    Task<AiDocumentResponse> UploadDocumentAsync(
        UploadAiDocumentRequest request,
        int? uploadedByUserId,
        CancellationToken cancellationToken = default);

    Task<List<AiDocumentResponse>> GetDocumentsAsync(
        CancellationToken cancellationToken = default);

    Task<AiDocumentResponse?> GetDocumentByIdAsync(
        int documentId,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteDocumentAsync(
        int documentId,
        CancellationToken cancellationToken = default);

    Task<bool> ReprocessDocumentAsync(
        int documentId,
        CancellationToken cancellationToken = default);
}