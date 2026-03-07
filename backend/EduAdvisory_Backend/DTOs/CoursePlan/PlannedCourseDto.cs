namespace EduAdvisory_Backend.DTOs.CoursePlan
{
    public class PlannedCourseDto
    {
        public string CourseCode { get; set; } = "";
        public string CourseName { get; set; } = "";
        public int Credits { get; set; }

        public bool IsRetake { get; set; }                 // was FAILED and must retake
        public int RecommendedSemester { get; set; }        // from study_guide
        public List<string> Prerequisites { get; set; } = new();
        public bool PrereqsSatisfiedBeforeThisSemester { get; set; }
    }
}
