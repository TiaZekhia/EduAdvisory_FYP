namespace EduAdvisory_Backend.DTOs.Student
{
    public class AdvisorStudentAnalysisDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = "";
        public string ProgramCode { get; set; } = "";
        public int CurrentSemester { get; set; }
        public decimal CurrentGpa { get; set; }
        public string AcademicStatus { get; set; } = "";
        public bool IsOnTrack { get; set; }

        public int CompletedCredits { get; set; }
        public int TotalProgramCredits { get; set; }

        public List<AdvisorCurrentEnrollmentCourseDto> CurrentEnrollment { get; set; } = new();
        public List<AdvisorFailedCourseDto> FailedCourses { get; set; } = new();
        public List<AdvisorMissingCourseDto> MissingCourses { get; set; } = new();
    }

    public class AdvisorCurrentEnrollmentCourseDto
    {
        public string CourseCode { get; set; } = "";
        public string CourseName { get; set; } = "";
        public int Credits { get; set; }
        public string Status { get; set; } = "ENROLLED";
    }

    public class AdvisorFailedCourseDto
    {
        public string CourseCode { get; set; } = "";
        public string CourseName { get; set; } = "";
        public string Semester { get; set; } = "";
        public bool IsRetaken { get; set; }
        public string RetakeStatus { get; set; } = ""; // NOT_RETAKEN / RETAKEN / PASSED_LATER
    }

    public class AdvisorMissingCourseDto
    {
        public string CourseCode { get; set; } = "";
        public string CourseName { get; set; } = "";
        public int RecommendedSemester { get; set; }
        public string Reason { get; set; } = "";
        public string Priority { get; set; } = ""; // HIGH / MEDIUM / LOW
        public List<string> Prerequisites { get; set; } = new();
    }
}