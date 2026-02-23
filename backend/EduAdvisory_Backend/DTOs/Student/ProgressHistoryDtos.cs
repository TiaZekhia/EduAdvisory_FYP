using System.Collections.Generic;

namespace EduAdvisory_Backend.DTOs.Student
{
    public class CourseHistoryItemDto
    {
        public string CourseCode { get; set; } = "";
        public string CourseName { get; set; } = "";
        public int Credits { get; set; }
        public decimal FinalGrade { get; set; } // computed if possible, else fallback
        public string Status { get; set; } = ""; // PASSED/FAILED
        public List<string> Prerequisites { get; set; } = new();
    }

    public class SemesterHistoryDto
    {
        public string Semester { get; set; } = ""; // e.g. "Semester 2"
        public decimal SemesterGpa { get; set; }    // weighted avg by credits (your rule)
        public int Credits { get; set; }
        public int CoursesCount { get; set; }
        public List<CourseHistoryItemDto> Courses { get; set; } = new();
    }
}