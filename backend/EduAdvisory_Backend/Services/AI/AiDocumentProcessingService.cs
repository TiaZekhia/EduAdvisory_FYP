using EduAdvisory_Backend.Interfaces.Services.AI;
using EduAdvisory_Backend.Models;
using Microsoft.EntityFrameworkCore;
using Pgvector;

namespace EduAdvisory_Backend.Services.AI;

public class AiDocumentProcessingService : IAiDocumentProcessingService
{
    private readonly EduAdvisoryDbContext _dbContext;
    private readonly IPdfTextExtractionService _pdfTextExtractionService;
    private readonly ISemanticChunkingService _semanticChunkingService;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger<AiDocumentProcessingService> _logger;

    public AiDocumentProcessingService(
        EduAdvisoryDbContext dbContext,
        IPdfTextExtractionService pdfTextExtractionService,
        ISemanticChunkingService semanticChunkingService,
        IEmbeddingService embeddingService,
        ILogger<AiDocumentProcessingService> logger)
    {
        _dbContext = dbContext;
        _pdfTextExtractionService = pdfTextExtractionService;
        _semanticChunkingService = semanticChunkingService;
        _embeddingService = embeddingService;
        _logger = logger;
    }

    public async Task ProcessDocumentAsync(
        int documentId,
        CancellationToken cancellationToken = default)
    {
        var document = await _dbContext.AiDocuments
            .FirstOrDefaultAsync(d => d.DocumentId == documentId, cancellationToken);

        if (document == null)
        {
            throw new InvalidOperationException($"AI document {documentId} was not found.");
        }

        try
        {
            document.Status = "Processing";
            document.ErrorMessage = null;
            await _dbContext.SaveChangesAsync(cancellationToken);

            var oldChunks = await _dbContext.AiDocumentChunks
                .Where(c => c.DocumentId == documentId)
                .ToListAsync(cancellationToken);

            if (oldChunks.Any())
            {
                _dbContext.AiDocumentChunks.RemoveRange(oldChunks);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            var pages = await _pdfTextExtractionService.ExtractPagesAsync(
                document.FilePath,
                cancellationToken);

            if (!pages.Any())
            {
                throw new InvalidOperationException("No readable text was extracted from the PDF.");
            }

            var chunkCandidates = _semanticChunkingService.CreateChunks(
                pages,
                document.DocumentType);

            if (!chunkCandidates.Any())
            {
                throw new InvalidOperationException("No semantic chunks were created from the document.");
            }

            foreach (var candidate in chunkCandidates)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var textForEmbedding = $"""
Document title: {document.Title}
Document type: {document.DocumentType}
Scope: {document.Scope}
Program: {document.ProgramCode}
Course: {document.CourseCode}
Academic year: {document.AcademicYear}
Section: {candidate.SectionTitle}
Page: {candidate.PageNumber}

{candidate.ChunkText}
""";

                var embedding = await _embeddingService.GenerateEmbeddingAsync(
                    textForEmbedding,
                    cancellationToken);

                var chunk = new AiDocumentChunk
                {
                    DocumentId = document.DocumentId,
                    Scope = document.Scope,
                    CourseCode = document.CourseCode,
                    ProgramCode = document.ProgramCode,
                    AcademicYear = document.AcademicYear,
                    DocumentType = document.DocumentType,
                    SectionTitle = candidate.SectionTitle?.Length > 255
                        ? candidate.SectionTitle.Substring(0, 255)
                        : candidate.SectionTitle,
                    ChunkText = candidate.ChunkText,
                    Embedding = new Vector(embedding),
                    ChunkIndex = candidate.ChunkIndex,
                    PageNumber = candidate.PageNumber,
                    TokenCount = candidate.TokenCount,
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.AiDocumentChunks.Add(chunk);
            }

            document.Status = "Processed";
            document.ProcessedAt = DateTime.UtcNow;
            document.ErrorMessage = null;

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Processed AI document {DocumentId}. Created {ChunkCount} chunks.",
                documentId,
                chunkCandidates.Count);
        }
        catch (Exception ex)
        {
            document.Status = "Failed";
            document.ErrorMessage = ex.Message;
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogError(
                ex,
                "Failed processing AI document {DocumentId}",
                documentId);

            throw;
        }
    }
}