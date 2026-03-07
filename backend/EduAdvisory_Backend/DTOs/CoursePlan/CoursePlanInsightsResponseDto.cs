namespace EduAdvisory_Backend.DTOs.CoursePlan
{
    public class CoursePlanInsightsResponseDto
    {
        public List<CoursePlanDto> Plans { get; set; } = new();
        public CoursePlanAiInsightsDto Insights { get; set; } = new();

        // Optional but VERY helpful for debugging during dev:
        public string? AiError { get; set; }
    }
}