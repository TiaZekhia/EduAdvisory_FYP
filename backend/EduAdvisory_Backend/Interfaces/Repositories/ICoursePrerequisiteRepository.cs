using EduAdvisory_Backend.Models;

namespace EduAdvisory_Backend.Interfaces.Repositories
{
    public interface ICoursePrerequisiteRepository
    {
        List<CoursePrerequisite> GetAll();
        List<CoursePrerequisite> GetByCourseCode(string courseCode);
    }
}
