namespace EduAdvisory_Backend.Models;

public class OpenAiOptions
{
    public string ApiKey { get; set; } =
        Environment.GetEnvironmentVariable("OPENAI_API_KEY")
        ?? string.Empty;

    public string ChatModel { get; set; } =
        Environment.GetEnvironmentVariable("OPENAI_MODEL")
        ?? "gpt-4o-mini";

    public string EmbeddingModel { get; set; } =
        Environment.GetEnvironmentVariable("OPENAI_EMBEDDING_MODEL")
        ?? "text-embedding-3-small";

    public int EmbeddingDimensions { get; set; } = 1536;
}