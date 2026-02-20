namespace EduAdvisory_Backend.DTOs.Student
{
    public class StudentMeSummaryDto
    {
        public int StudentId { get; set; }
        public string FullName { get; set; }
        public string ProgramCode { get; set; }
        public int CurrentSemester { get; set; }
        public decimal? CurrentGpa { get; set; }
        public string AcademicStatus { get; set; }
    }
}
