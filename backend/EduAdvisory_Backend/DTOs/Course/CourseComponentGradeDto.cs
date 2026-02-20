namespace EduAdvisory_Backend.DTOs.Course
{
    public class CourseComponentGradeDto
    {
        public string ComponentName { get; set; }
        public decimal? Grade { get; set; }
    }

    public class CurrentCoursePerformanceDto
    {
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public int Credits { get; set; }

        public int? AbsencesCount { get; set; }
        public int? MaxAbsences { get; set; }

        public List<CourseComponentGradeDto> Components { get; set; } = new();
    }
}