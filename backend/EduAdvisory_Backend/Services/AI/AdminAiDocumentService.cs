using EduAdvisory_Backend.DTOs.AI.AdminDocuments;
using EduAdvisory_Backend.Interfaces.Services.AI;
using EduAdvisory_Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EduAdvisory_Backend.Services.AI;

public class AdminAiDocumentService : IAdminAiDocumentService
{
    private readonly EduAdvisoryDbContext _dbContext;
    private readonly AiOptions _aiOptions;
    private readonly IAiDocumentProcessingService _processingService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<AdminAiDocumentService> _logger;

    private static readonly HashSet<string> AllowedDocumentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "study_guide",
        "course_syllabus"
    };

    private static readonly HashSet<string> AllowedScopes = new(StringComparer.OrdinalIgnoreCase)
    {
        "course",
        "program",
        "general"
    };

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf"
    };

    public AdminAiDocumentService(
     EduAdvisoryDbContext dbContext,
     IOptions<AiOptions> aiOptions,
     IWebHostEnvironment environment,
     ILogger<AdminAiDocumentService> logger,
     IAiDocumentProcessingService processingService)
    {
        _dbContext = dbContext;
        _aiOptions = aiOptions.Value;
        _environment = environment;
        _logger = logger;
        _processingService = processingService;
    }

    public async Task<AiDocumentResponse> UploadDocumentAsync(
        UploadAiDocumentRequest request,
        int? uploadedByUserId,
        CancellationToken cancellationToken = default)
    {
        ValidateUploadRequest(request);

        // Validate scope-specific requirements
        var scope = request.Scope.ToLowerInvariant();

        if (scope == "course")
        {
            if (string.IsNullOrWhiteSpace(request.CourseCode))
            {
                throw new InvalidOperationException("CourseCode is required for course-scoped documents.");
            }

            var courseExists = await _dbContext.SisCourses
                .AnyAsync(c => c.CourseCode == request.CourseCode, cancellationToken);

            if (!courseExists)
            {
                throw new InvalidOperationException($"Course '{request.CourseCode}' does not exist.");
            }
        }
        else if (scope == "program")
        {
            if (string.IsNullOrWhiteSpace(request.ProgramCode))
            {
                throw new InvalidOperationException("ProgramCode is required for program-scoped documents.");
            }
        }
        else if (scope == "general")
        {
            // General documents don't require course or program code
        }
        else
        {
            throw new InvalidOperationException("Invalid scope. Allowed values are course, program, and general.");
        }

        var uploadsRoot = GetUploadRootPath();
        Directory.CreateDirectory(uploadsRoot);

        var originalFileName = Path.GetFileName(request.File.FileName);
        var extension = Path.GetExtension(originalFileName);
        var safeFileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsRoot, safeFileName);

        await using (var stream = new FileStream(filePath, FileMode.CreateNew))
        {
            await request.File.CopyToAsync(stream, cancellationToken);
        }

        var document = new AiDocument
        {
            Title = request.Title.Trim(),
            DocumentType = request.DocumentType.Trim().ToLowerInvariant(),
            Scope = scope,
            CourseCode = scope == "course" ? request.CourseCode?.Trim() : null,
            ProgramCode = scope == "program" ? request.ProgramCode?.Trim() : null,
            AcademicYear = request.AcademicYear?.Trim(),
            Semester = request.Semester?.Trim(),
            FileName = originalFileName,
            FilePath = filePath,
            Status = "Uploaded",
            UploadedByUserId = uploadedByUserId,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AiDocuments.Add(document);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _processingService.ProcessDocumentAsync(document.DocumentId, cancellationToken);

        await _dbContext.Entry(document)
            .Collection(d => d.Chunks)
            .LoadAsync(cancellationToken);

        return MapToResponse(document, document.Chunks.Count);
    }

    public async Task<List<AiDocumentResponse>> GetDocumentsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.AiDocuments
            .AsNoTracking()
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new AiDocumentResponse
            {
                DocumentId = d.DocumentId,
                Title = d.Title,
                DocumentType = d.DocumentType,
                Scope = d.Scope,
                CourseCode = d.CourseCode,
                ProgramCode = d.ProgramCode,
                AcademicYear = d.AcademicYear,
                Semester = d.Semester,
                FileName = d.FileName,
                Status = d.Status,
                CreatedAt = d.CreatedAt,
                ProcessedAt = d.ProcessedAt,
                ErrorMessage = d.ErrorMessage,
                ChunkCount = d.Chunks.Count
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<AiDocumentResponse?> GetDocumentByIdAsync(
        int documentId,
        CancellationToken cancellationToken = default)
    {
        var document = await _dbContext.AiDocuments
            .AsNoTracking()
            .Include(d => d.Chunks)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId, cancellationToken);

        if (document == null)
        {
            return null;
        }

        return MapToResponse(document, document.Chunks.Count);
    }

    public async Task<bool> DeleteDocumentAsync(
        int documentId,
        CancellationToken cancellationToken = default)
    {
        var document = await _dbContext.AiDocuments
            .Include(d => d.Chunks)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId, cancellationToken);

        if (document == null)
        {
            return false;
        }

        _dbContext.AiDocumentChunks.RemoveRange(document.Chunks);
        _dbContext.AiDocuments.Remove(document);

        await _dbContext.SaveChangesAsync(cancellationToken);

        TryDeletePhysicalFile(document.FilePath);

        return true;
    }

    public async Task<bool> ReprocessDocumentAsync(
        int documentId,
        CancellationToken cancellationToken = default)
    {
        var document = await _dbContext.AiDocuments
            .FirstOrDefaultAsync(d => d.DocumentId == documentId, cancellationToken);

        if (document == null)
        {
            return false;
        }

        document.Status = "Uploaded";
        document.ErrorMessage = null;
        document.ProcessedAt = null;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _processingService.ProcessDocumentAsync(
            documentId,
            cancellationToken);

        return true;
    }

    private void ValidateUploadRequest(UploadAiDocumentRequest request)
    {
        if (!AllowedDocumentTypes.Contains(request.DocumentType))
        {
            throw new InvalidOperationException(
                "Invalid document type. Allowed values are study_guide and course_syllabus.");
        }

        var extension = Path.GetExtension(request.File.FileName);

        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Only PDF files are currently supported.");
        }

        if (request.File.Length == 0)
        {
            throw new InvalidOperationException("Uploaded file is empty.");
        }

        if (request.File.Length > 20 * 1024 * 1024)
        {
            throw new InvalidOperationException("File size cannot exceed 20MB.");
        }
    }

    private string GetUploadRootPath()
    {
        if (Path.IsPathRooted(_aiOptions.UploadRootPath))
        {
            return _aiOptions.UploadRootPath;
        }

        return Path.Combine(_environment.ContentRootPath, _aiOptions.UploadRootPath);
    }

    private void TryDeletePhysicalFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete AI document file {FilePath}", filePath);
        }
    }

    private static AiDocumentResponse MapToResponse(AiDocument document, int chunkCount)
    {
        return new AiDocumentResponse
        {
            DocumentId = document.DocumentId,
            Title = document.Title,
            DocumentType = document.DocumentType,
            Scope = document.Scope,
            CourseCode = document.CourseCode,
            ProgramCode = document.ProgramCode,
            AcademicYear = document.AcademicYear,
            Semester = document.Semester,
            FileName = document.FileName,
            Status = document.Status,
            CreatedAt = document.CreatedAt,
            ProcessedAt = document.ProcessedAt,
            ErrorMessage = document.ErrorMessage,
            ChunkCount = chunkCount
        };
    }
}