using EduAdvisory_Backend.DTOs.Advisor;
using EduAdvisory_Backend.Models;

namespace EduAdvisory_Backend.Interfaces.Repositories
{
    public interface IAdvisorRepository
    {
        Advisor GetById(int advisorId);
        Advisor GetByUsername(string username);

        AdvisorSummaryDto GetAdvisorSummary(int advisorId);
        AdvisorDashboardSummaryDto GetDashboardSummary(int advisorId);

        List<AdvisorMeetingDto> GetUpcomingMeetings(int advisorId, int limit = 5);
        List<AdvisorMeetingDto> GetPastMeetings(int advisorId, int limit = 10);

        AdvisorMessagesSummaryDto GetMessagesSummary(int advisorId);
        List<AdvisorMessageDto> GetMessages(int advisorId, int limit = 20);
    }
}