using System.ComponentModel.DataAnnotations;
namespace EduAdvisory_Backend.DTOs.Student
{
    public class CreateMeetingRequestDto
    {
        public int AvailabilityId { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }
    }
}
