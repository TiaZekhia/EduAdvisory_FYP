using EduAdvisory_Backend.DTOs.Student;

namespace EduAdvisory_Backend.Interfaces.Services
{
    public interface IStudentRiskAssessmentService
    {
        StudentRiskAssessmentDto AssessStudent(int studentId);
        StudentAlertsResponseDto GetStudentAlerts(int studentId);
        StudentAlertsCountDto GetStudentAlertsCount(int studentId);
    }
}