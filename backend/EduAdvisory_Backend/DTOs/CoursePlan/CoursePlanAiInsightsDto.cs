namespace EduAdvisory_Backend.DTOs.CoursePlan
{
    public class CoursePlanAiInsightsDto
    {
        public int BestPlanIndex { get; set; } // 0-based
        public string BestPlanSummary { get; set; } = "";
        public List<CoursePlanAiPlanInsightDto> PlanInsights { get; set; } = new();
    }

    public class CoursePlanAiPlanInsightDto
    {
        public int PlanIndex { get; set; }
        public int Score { get; set; } // 0-100
        public string Explanation { get; set; } = "";
        public List<string> Pros { get; set; } = new();
        public List<string> Cons { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }
}