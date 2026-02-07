using EduAdvisory_Backend.Interfaces.Repositories;
using EduAdvisory_Backend.Models;

namespace EduAdvisory_Backend.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly EduAdvisoryDbContext _context;

        public StudentRepository(EduAdvisoryDbContext context)
        {
            _context = context;
        }

        public SisStudent GetById(int studentId)
        {
            return _context.SisStudents
                .FirstOrDefault(s => s.StudentId == studentId);
        }

        public List<SisStudentCourseHistory> GetCourseHistory(int studentId)
        {
            return _context.SisStudentCourseHistories
                .Where(h => h.StudentId == studentId)
                .ToList();
        }

        public List<SisCurrentEnrollment> GetCurrentEnrollment(int studentId)
        {
            return _context.SisCurrentEnrollments
                .Where(e => e.StudentId == studentId)
                .ToList();
        }
    }

}
