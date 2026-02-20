using EduAdvisory_Backend.DTOs.Course;
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
    }
}
