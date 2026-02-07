using EduAdvisory_Backend.Interfaces.Repositories;
using EduAdvisory_Backend.Models;

namespace EduAdvisory_Backend.Repositories
{
    public class CoursePrerequisiteRepository : ICoursePrerequisiteRepository
    {
        private readonly EduAdvisoryDbContext _context;

        public CoursePrerequisiteRepository(EduAdvisoryDbContext context)
        {
            _context = context;
        }

        public List<CoursePrerequisite> GetAll()
        {
            return _context.CoursePrerequisites.ToList();
        }

        public List<CoursePrerequisite> GetByCourseCode(string courseCode)
        {
            return _context.CoursePrerequisites
                .Where(cp => cp.CourseCode == courseCode)
                .ToList();
        }
    }


}
