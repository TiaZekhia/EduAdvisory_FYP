using EduAdvisory_Backend.DTOs.Student;

namespace EduAdvisory_Backend.Interfaces.Services
{
    public interface IStudentAnalysisService
    {
        StudentAnalysisDto AnalyzeStudent(int studentId);
        AdvisorStudentAnalysisDto AnalyzeStudentForAdvisor(int studentId);
    }
}