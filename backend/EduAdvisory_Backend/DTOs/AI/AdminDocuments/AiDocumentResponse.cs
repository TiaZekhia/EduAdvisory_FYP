namespace EduAdvisory_Backend.DTOs.AI.AdminDocuments;

public class AiDocumentResponse
{
    public int DocumentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string Scope { get; set; } = "course";
    public string? CourseCode { get; set; }
    public string? ProgramCode { get; set; }
    public string? AcademicYear { get; set; }
    public string? Semester { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int ChunkCount { get; set; }
}