namespace EduAdvisory_Backend.DTOs.Student
{
    public class StudentSummaryDto
    {
        public int StudentId { get; set; }
        public string FullName { get; set; }
        public string ProgramCode { get; set; }
        public int CurrentSemester { get; set; }
    }
}
