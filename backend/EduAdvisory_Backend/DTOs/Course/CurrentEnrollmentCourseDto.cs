namespace EduAdvisory_Backend.DTOs.Course
{
    public class CurrentEnrollmentCourseDto
    {
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public int Credits { get; set; }
        public string Semester { get; set; }
    }
}