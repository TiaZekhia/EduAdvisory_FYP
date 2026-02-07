namespace EduAdvisory_Backend.DTOs.Student
{
    public class StudentAnalysisDto
    {
        public int StudentId { get; set; }
        public bool IsOnTrack { get; set; }
        public List<string> MissingCurrentSemesterCourses { get; set; }
        public List<string> FailedNotRetakenCourses { get; set; }
        public List<string> BlockingCourses { get; set; }
    }
}
