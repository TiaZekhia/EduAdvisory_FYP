namespace EduAdvisory_Backend.DTOs.CoursePlan
{
    public class PlannedSemesterDto
    {
        public int PlannedSemester { get; set; }           // e.g. 9, 10, 11...
        public string TermLabel { get; set; } = "";        // "Fall" / "Spring"
        public int TotalCredits { get; set; }
        public int CoursesCount { get; set; }
        public List<PlannedCourseDto> Courses { get; set; } = new();
    }
}
