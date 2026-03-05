using EduAdvisory_Backend.DTOs.Course;
using EduAdvisory_Backend.DTOs.Meetings;
using EduAdvisory_Backend.DTOs.Student;
using EduAdvisory_Backend.Models;
using EduAdvisory_Backend.DTOs.Meetings;


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

        // meetings page
        StudentMeetingsSummaryDto GetStudentMeetingsSummary(int studentId);
        List<StudentMeetingDto> GetUpcomingMeetings(int studentId, int limit = 3);
        List<StudentMeetingDto> GetPastMeetings(int studentId, int limit = 10);
        StudentAdvisorDto? GetStudentAdvisor(int studentId);
    }
}
