namespace EduAdvisory_Backend.DTOs.Student
{
    public class StudentAlertsResponseDto
    {
        public int Count { get; set; }
        public int High { get; set; }
        public int Medium { get; set; }
        public int Low { get; set; }
        public List<StudentAlertDto> Alerts { get; set; } = new();
    }
}
