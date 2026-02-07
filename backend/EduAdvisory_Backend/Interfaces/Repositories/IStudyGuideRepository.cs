using EduAdvisory_Backend.Models;

namespace EduAdvisory_Backend.Interfaces.Repositories
{
    public interface IStudyGuideRepository
    {
        List<StudyGuide> GetByProgram(string programCode);
    }
}
