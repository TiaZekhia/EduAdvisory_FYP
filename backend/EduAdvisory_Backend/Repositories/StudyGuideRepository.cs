using EduAdvisory_Backend.Interfaces.Repositories;
using EduAdvisory_Backend.Models;

namespace EduAdvisory_Backend.Repositories
{
    public class StudyGuideRepository : IStudyGuideRepository
    {
        private readonly EduAdvisoryDbContext _context;

        public StudyGuideRepository(EduAdvisoryDbContext context)
        {
            _context = context;
        }

        public List<StudyGuide> GetByProgram(string programCode)
        {
            return _context.StudyGuides
                .Where(sg => sg.ProgramCode == programCode)
                .ToList();
        }
    }

}
