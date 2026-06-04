namespace EduAdvisory_Backend.Models;

public class AiOptions
{
    public string UploadRootPath { get; set; } =
        "wwwroot/uploads/ai-documents";

    public int MaxRetrievedChunks { get; set; } = 5;

    public double MinSimilarityThreshold { get; set; } = 0.75;
}