namespace EduAdvisory_Backend.DTOs.CoursePlan
{
    public class CoursePlanDto
    {
        public string PlanId { get; set; } = "";
        public string Strategy { get; set; } = ""; // e.g. "Balanced", "Fast blockers", etc.
        public List<PlannedSemesterDto> Semesters { get; set; } = new();
        public CoursePlanMetricsDto Metrics { get; set; } = new();
    }
}
