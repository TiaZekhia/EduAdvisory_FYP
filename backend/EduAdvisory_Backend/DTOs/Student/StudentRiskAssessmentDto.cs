namespace EduAdvisory_Backend.DTOs.Student
{
    public class StudentRiskAssessmentDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = "";
        public string ProgramCode { get; set; } = "";
        public int CurrentSemester { get; set; }

        public string RiskLevel { get; set; } = ""; // HIGH / MEDIUM / LOW
        public int RiskScore { get; set; } // 0-100

        public StudentRiskFactorsDto Factors { get; set; } = new();

        public List<string> MainReasons { get; set; } = new();
        public string RecommendedAction { get; set; } = "";

        // 3c - only global alerts
        public StudentAlertsResponseDto GlobalAlertsSummary { get; set; } = new();

        // 3d - per-course assessment
        public CourseRiskSummaryDto CourseSummary { get; set; } = new();
        public List<CourseRiskAssessmentDto> CourseAssessments { get; set; } = new();
    }

    public class StudentRiskFactorsDto
    {
        public int CoursePortfolioRiskScore { get; set; }
        public int GpaRiskScore { get; set; }
        public int AcademicStatusRiskScore { get; set; }
        public int CreditDelayRiskScore { get; set; }
        public int CurrentSemesterMissingRiskScore { get; set; }

        public decimal? CurrentGpa { get; set; }
        public string AcademicStatus { get; set; } = "";

        public int ExpectedCreditsByNow { get; set; }
        public int EarnedCredits { get; set; }
        public int DelayedCredits { get; set; }

        public int CurrentSemesterMissingCoursesCount { get; set; }
    }

    public class CourseRiskSummaryDto
    {
        public int High { get; set; }
        public int Medium { get; set; }
        public int Low { get; set; }
        public int Count { get; set; }
    }

    public class CourseRiskAssessmentDto
    {
        public string CourseCode { get; set; } = "";
        public string CourseName { get; set; } = "";
        public int Credits { get; set; }

        public string RiskLevel { get; set; } = ""; // HIGH / MEDIUM / LOW
        public int RiskScore { get; set; } // 0-100

        public int? AbsencesCount { get; set; }
        public int? MaxAbsences { get; set; }

        public List<CourseRiskFactorDto> RiskFactors { get; set; } = new();
        public List<CourseComponentRiskDto> Components { get; set; } = new();

        public string Recommendation { get; set; } = "";
    }

    public class CourseRiskFactorDto
    {
        public string Severity { get; set; } = ""; // HIGH / MEDIUM / LOW
        public string Title { get; set; } = "";
        public string Detail { get; set; } = "";
    }

    public class CourseComponentRiskDto
    {
        public string ComponentName { get; set; } = "";
        public decimal? Grade { get; set; }
        public int WeightPercentage { get; set; }
        public int RiskScore { get; set; } // 0-100
    }

    public class CourseGradingSchemaDto
    {
        public string CourseCode { get; set; } = "";
        public string ComponentName { get; set; } = "";
        public int WeightPercentage { get; set; }
    }
}