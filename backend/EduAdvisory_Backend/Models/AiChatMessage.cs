using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduAdvisory_Backend.Models;

[Table("ai_chat_messages")]
public class AiChatMessage
{
    [Key]
    [Column("message_id")]
    public int MessageId { get; set; }

    [Column("session_id")]
    public int SessionId { get; set; }

    [Required]
    [Column("role")]
    [MaxLength(30)]
    public string Role { get; set; } = string.Empty;
    // user, assistant, system

    [Required]
    [Column("message")]
    public string Message { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public AiChatSession? Session { get; set; }
}