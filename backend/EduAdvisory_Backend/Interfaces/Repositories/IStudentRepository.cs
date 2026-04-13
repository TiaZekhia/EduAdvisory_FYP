using EduAdvisory_Backend.DTOs.Course;
using EduAdvisory_Backend.DTOs.Messages;
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
        ProgressSummaryDto GetProgressSummary(int studentId);
        List<DepartmentCreditsDto> GetProgressDepartments(int studentId);
        List<SemesterHistoryDto> GetProgressHistory(int studentId);
        List<StudyGuideComparisonDto> GetStudyGuideComparison(int studentId);

        List<string> GetPassedCourses(int studentId);
        List<string> GetFailedNotRetakenCourses(int studentId);
        Dictionary<string, int> GetStudyGuideRecommendedSemester(string programCode); // courseCode -> recommendedSemester
        Dictionary<string, (string name, int credits)> GetCoursesMeta(List<string> courseCodes);

        Dictionary<string, List<string>> GetPrerequisitesMap(List<string> courseCodes); // course -> prereqs
        StudentMessagesSummaryDto GetStudentMessagesSummary(int studentId);
        List<StudentMessageDto> GetStudentMessages(int studentId, int limit = 20);

        StudentMeetingsSummaryDto GetStudentMeetingsSummary(int studentId);
        List<StudentMeetingDto> GetUpcomingMeetings(int studentId, int limit = 3);
        List<StudentMeetingDto> GetPastMeetings(int studentId, int limit = 10);
        StudentAdvisorDto? GetStudentAdvisor(int studentId);
        Dictionary<string, List<CourseGradingSchemaDto>> GetCourseGradingSchema(List<string> courseCodes);
    }
}
