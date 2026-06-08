namespace EduAdvisory_Backend.DTOs.Advisor
{
    public class AdvisorStudentOverviewDto
    {
        public int StudentId { get; set; }
        public string Name { get; set; } = "";
        public int CurrentSemester { get; set; }
        public decimal Gpa { get; set; }
        public string AcademicStatus { get; set; } = "";
        public string ProgramCode { get; set; } = "";
    }
}