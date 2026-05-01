using System.ComponentModel.DataAnnotations;

namespace EduAdvisory_Backend.DTOs.Advisor
{
    public class RespondMeetingRequestDto
    {
        [Required]
        public string Decision { get; set; } = "";

        public string? RejectionReason { get; set; }
    }
}
