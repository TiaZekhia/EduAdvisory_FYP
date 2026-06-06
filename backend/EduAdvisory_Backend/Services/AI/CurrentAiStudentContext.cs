using EduAdvisory_Backend.Interfaces.Services.AI;

namespace EduAdvisory_Backend.Services.AI;

public class CurrentAiStudentContext : ICurrentAiStudentContext
{
    public int? StudentId { get; set; }
}