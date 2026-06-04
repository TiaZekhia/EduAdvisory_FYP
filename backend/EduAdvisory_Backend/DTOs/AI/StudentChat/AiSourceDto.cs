namespace EduAdvisory_Backend.DTOs.AI.StudentChat;

public class AiSourceDto
{
    public int ChunkId { get; set; }

    public int DocumentId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string DocumentType { get; set; } = string.Empty;

    public string Scope { get; set; } = "course";

    public string? CourseCode { get; set; }

    public string? ProgramCode { get; set; }

    public string? AcademicYear { get; set; }

    public string? SectionTitle { get; set; }

    public int? PageNumber { get; set; }

    public double SimilarityScore { get; set; }
}