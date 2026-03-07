using EduAdvisory_Backend.DTOs.CoursePlan;

namespace EduAdvisory_Backend.Interfaces.Services
{
    public interface ICoursePlanAiService
    {
        Task<CoursePlanAiInsightsDto> RankAndExplainAsync(
            List<CoursePlanDto> plans,
            string programCode,
            int currentSemester,
            string academicStatus,
            int creditLimit,
            CancellationToken ct = default
        );
    }
}