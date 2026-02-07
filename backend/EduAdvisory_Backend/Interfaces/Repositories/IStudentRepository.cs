using EduAdvisory_Backend.Models;

namespace EduAdvisory_Backend.Interfaces.Repositories
{
    public interface IStudentRepository
    {
        SisStudent GetById(int studentId);
        List<SisStudentCourseHistory> GetCourseHistory(int studentId);
        List<SisCurrentEnrollment> GetCurrentEnrollment(int studentId);
    }
}
