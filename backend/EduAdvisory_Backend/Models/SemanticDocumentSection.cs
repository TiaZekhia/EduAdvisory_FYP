namespace EduAdvisory_Backend.Models;

public class SemanticDocumentSection
{
    public string SectionTitle { get; set; } = "General";
    public string Text { get; set; } = string.Empty;
    public int? PageNumber { get; set; }
}