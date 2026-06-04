namespace EduAdvisory_Backend.Models;

public class DocumentChunkCandidate
{
    public string SectionTitle { get; set; } = "General";
    public string ChunkText { get; set; } = string.Empty;
    public int? PageNumber { get; set; }
    public int ChunkIndex { get; set; }
    public int TokenCount { get; set; }
}