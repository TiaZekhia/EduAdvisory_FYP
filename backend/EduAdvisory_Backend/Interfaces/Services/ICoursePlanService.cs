using EduAdvisory_Backend.DTOs.CoursePlan;

namespace EduAdvisory_Backend.Interfaces.Services
{
    public interface ICoursePlanService
    {
        List<CoursePlanDto> GeneratePlansForStudent(int studentId, int plansCount = 3);
    }
}