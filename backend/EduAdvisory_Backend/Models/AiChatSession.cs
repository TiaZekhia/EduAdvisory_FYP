using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduAdvisory_Backend.Models;

[Table("ai_chat_sessions")]
public class AiChatSession
{
    [Key]
    [Column("session_id")]
    public int SessionId { get; set; }

    [Column("student_id")]
    public int StudentId { get; set; }

    [Column("started_at")]
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    [Column("last_activity_at")]
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;

    public ICollection<AiChatMessage> Messages { get; set; } = new List<AiChatMessage>();
}