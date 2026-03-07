using EduAdvisory_Backend.DTOs.Course;
using EduAdvisory_Backend.DTOs.Messages;
using EduAdvisory_Backend.DTOs.Student;
using EduAdvisory_Backend.Models;

namespace EduAdvisory_Backend.Interfaces.Repositories
{
    public interface IStudentRepository
    {
        SisStudent GetById(int studentId);
        List<SisStudentCourseHistory> GetCourseHistory(int studentId);
        List<SisCurrentEnrollment> GetCurrentEnrollment(int studentId);
        SisStudent GetByUsername(string username);
        List<CurrentEnrollmentCourseDto> GetCurrentEnrollmentWithCourse(int studentId);
        List<CurrentCoursePerformanceDto> GetCurrentCoursesPerformance(int studentId);

        StudentStatsDto GetStudentStats(int studentId);
        DegreeProgressDto GetDegreeProgress(int studentId);
        int GetUpcomingMeetingsCount(int studentId);
        List<StudentAlertDto> GetStudentAlerts(int studentId, int limit = 5);
        StudentAlertsCountDto GetStudentAlertsCount(int studentId);
        ProgressSummaryDto GetProgressSummary(int studentId);
        List<DepartmentCreditsDto> GetProgressDepartments(int studentId);
        List<SemesterHistoryDto> GetProgressHistory(int studentId);
        List<StudyGuideComparisonDto> GetStudyGuideComparison(int studentId);

        StudentMessagesSummaryDto GetStudentMessagesSummary(int studentId);
        List<StudentMessageDto> GetStudentMessages(int studentId, int limit = 20);
        StudentMessagesAdvisorDto? GetStudentMessagesAdvisor(int studentId);
    }
}
