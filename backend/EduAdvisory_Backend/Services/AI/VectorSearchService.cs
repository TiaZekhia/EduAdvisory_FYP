using EduAdvisory_Backend.Interfaces.Services.AI;
using EduAdvisory_Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace EduAdvisory_Backend.Services.AI;

public class VectorSearchService : IVectorSearchService
{
    private readonly EduAdvisoryDbContext _dbContext;
    private readonly IEmbeddingService _embeddingService;
    private readonly AiOptions _aiOptions;
    private readonly ILogger<VectorSearchService> _logger;

    public VectorSearchService(
        EduAdvisoryDbContext dbContext,
        IEmbeddingService embeddingService,
        IOptions<AiOptions> aiOptions,
        ILogger<VectorSearchService> logger)
    {
        _dbContext = dbContext;
        _embeddingService = embeddingService;
        _aiOptions = aiOptions.Value;
        _logger = logger;
    }

    public async Task<List<RetrievedChunk>> SearchRelevantChunksAsync(
        string query,
        int studentId,
        string? courseCode = null,
        CancellationToken cancellationToken = default)
    {
        // Get student's program code
        var student = await _dbContext.SisStudents
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.StudentId == studentId, cancellationToken);

        var studentProgramCode = student?.ProgramCode;

        // Build comprehensive accessible course codes from academic history.
        // This includes current enrollments, historical enrollments, completed courses, and assessed courses.
        // Rationale: Students should be able to retrieve course materials for any course in their academic journey,
        // not only currently enrolled courses. This enables them to review past courses, understand failed courses,
        // or reference completed prerequisites while maintaining security by restricting to their own academic history.
        var accessibleCourseCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Load current enrollments
        var currentEnrollments = await _dbContext.SisCurrentEnrollments
            .AsNoTracking()
            .Where(e => e.StudentId == studentId)
            .Select(e => e.CourseCode)
            .Distinct()
            .ToListAsync(cancellationToken);
        foreach (var courseCodeItem in currentEnrollments)
        {
            accessibleCourseCodes.Add(courseCodeItem);
        }

        // Load historical course enrollments (completed, failed, withdrawn, repeated, etc.)
        var historicalCourses = await _dbContext.SisStudentCourseHistories
            .AsNoTracking()
            .Where(h => h.StudentId == studentId)
            .Select(h => h.CourseCode)
            .Distinct()
            .ToListAsync(cancellationToken);
        foreach (var courseCodeItem in historicalCourses)
        {
            accessibleCourseCodes.Add(courseCodeItem);
        }

        // Load courses with grades (completed assessments)
        var gradedCourses = await _dbContext.SisStudentGrades
            .AsNoTracking()
            .Where(g => g.StudentId == studentId)
            .Select(g => g.CourseCode)
            .Distinct()
            .ToListAsync(cancellationToken);
        foreach (var courseCodeItem in gradedCourses)
        {
            accessibleCourseCodes.Add(courseCodeItem);
        }

        // Load courses with assessments
        var assessedCourses = await _dbContext.SisCourseAssessments
            .AsNoTracking()
            .Where(a => a.StudentId == studentId)
            .Select(a => a.CourseCode)
            .Distinct()
            .ToListAsync(cancellationToken);
        foreach (var courseCodeItem in assessedCourses)
        {
            accessibleCourseCodes.Add(courseCodeItem);
        }

        _logger.LogInformation(
            "Student {StudentId}: Building RAG context with {AccessibleCourseCount} courses from academic history",
            studentId,
            accessibleCourseCodes.Count);

        // If no academic history and no program, can only access general documents
        if (accessibleCourseCodes.Count == 0 && string.IsNullOrEmpty(studentProgramCode))
        {
            // Return only general scope documents
            return await GetGeneralDocumentsAsync(query, cancellationToken);
        }

        // If courseCode is provided, verify it exists in accessible course codes
        if (!string.IsNullOrWhiteSpace(courseCode))
        {
            if (!accessibleCourseCodes.Contains(courseCode))
            {
                _logger.LogWarning(
                    "Student {StudentId}: Requested course {CourseCode} not found in academic history. Request denied.",
                    studentId,
                    courseCode);
                return new List<RetrievedChunk>();
            }
        }

        var queryEmbeddingArray = await _embeddingService.GenerateEmbeddingAsync(
            query,
            cancellationToken);

        var queryVector = new Vector(queryEmbeddingArray);
        var maxResults = _aiOptions.MaxRetrievedChunks;

        var chunks = await _dbContext.AiDocumentChunks
            .AsNoTracking()
            .Include(c => c.Document)
            .Where(c =>
                c.Embedding != null &&
                c.Document != null &&
                c.Document.Status == "Processed" &&
                (
                    // Course scope: student must have course in their academic history
                    (c.Scope == "course" &&
                        c.CourseCode != null &&
                        accessibleCourseCodes.Contains(c.CourseCode) &&
                        (string.IsNullOrWhiteSpace(courseCode) || c.CourseCode == courseCode))
                    ||
                    // Program scope: student must match program code
                    (c.Scope == "program" && c.ProgramCode == studentProgramCode)
                    ||
                    // General scope: accessible to all students
                    (c.Scope == "general")
                ))
            .OrderBy(c => c.Embedding!.CosineDistance(queryVector))
            .Take(maxResults)
            .Select(c => new
            {
                c.ChunkId,
                c.DocumentId,
                DocumentTitle = c.Document!.Title,
                c.DocumentType,
                c.Scope,
                c.CourseCode,
                c.ProgramCode,
                c.AcademicYear,
                c.SectionTitle,
                c.ChunkText,
                c.PageNumber,
                Distance = c.Embedding!.CosineDistance(queryVector)
            })
            .ToListAsync(cancellationToken);

        return chunks.Select(c => new RetrievedChunk
        {
            ChunkId = c.ChunkId,
            DocumentId = c.DocumentId,
            DocumentTitle = c.DocumentTitle,
            DocumentType = c.DocumentType,
            Scope = c.Scope,
            CourseCode = c.CourseCode,
            ProgramCode = c.ProgramCode,
            AcademicYear = c.AcademicYear,
            SectionTitle = c.SectionTitle,
            ChunkText = c.ChunkText,
            PageNumber = c.PageNumber,
            SimilarityScore = 1 - c.Distance
        }).ToList();
    }

    private async Task<List<RetrievedChunk>> GetGeneralDocumentsAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        var queryEmbeddingArray = await _embeddingService.GenerateEmbeddingAsync(
            query,
            cancellationToken);

        var queryVector = new Vector(queryEmbeddingArray);
        var maxResults = _aiOptions.MaxRetrievedChunks;

        var chunks = await _dbContext.AiDocumentChunks
            .AsNoTracking()
            .Include(c => c.Document)
            .Where(c =>
                c.Scope == "general" &&
                c.Embedding != null &&
                c.Document != null &&
                c.Document.Status == "Processed")
            .OrderBy(c => c.Embedding!.CosineDistance(queryVector))
            .Take(maxResults)
            .Select(c => new
            {
                c.ChunkId,
                c.DocumentId,
                DocumentTitle = c.Document!.Title,
                c.DocumentType,
                c.Scope,
                c.CourseCode,
                c.ProgramCode,
                c.AcademicYear,
                c.SectionTitle,
                c.ChunkText,
                c.PageNumber,
                Distance = c.Embedding!.CosineDistance(queryVector)
            })
            .ToListAsync(cancellationToken);

        return chunks.Select(c => new RetrievedChunk
        {
            ChunkId = c.ChunkId,
            DocumentId = c.DocumentId,
            DocumentTitle = c.DocumentTitle,
            DocumentType = c.DocumentType,
            Scope = c.Scope,
            CourseCode = c.CourseCode,
            ProgramCode = c.ProgramCode,
            AcademicYear = c.AcademicYear,
            SectionTitle = c.SectionTitle,
            ChunkText = c.ChunkText,
            PageNumber = c.PageNumber,
            SimilarityScore = 1 - c.Distance
        }).ToList();
    }
}