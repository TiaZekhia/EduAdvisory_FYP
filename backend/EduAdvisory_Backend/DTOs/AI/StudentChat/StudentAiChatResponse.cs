namespace EduAdvisory_Backend.DTOs.AI.StudentChat;

public class StudentAiChatResponse
{
    public int SessionId { get; set; }

    public string Answer { get; set; } = string.Empty;

    public string ResponseSource { get; set; } = "rag";

    public double? TopSimilarityScore { get; set; }

    public List<AiSourceDto> Sources { get; set; } = new();
}