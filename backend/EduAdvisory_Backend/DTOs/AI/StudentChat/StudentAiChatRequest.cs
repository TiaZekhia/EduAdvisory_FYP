using System.ComponentModel.DataAnnotations;

namespace EduAdvisory_Backend.DTOs.AI.StudentChat;

public class StudentAiChatRequest
{
    public int? SessionId { get; set; }

    [Required]
    public string Message { get; set; } = string.Empty;

    public string? CourseCode { get; set; }
}