using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduAdvisory_Backend.Models;

[Table("conversation")]
public class Conversation
{
    [Key]
    [Column("conversation_id")]
    public int ConversationId { get; set; }

    [Column("advisor_id")]
    public int AdvisorId { get; set; }

    [Column("student_id")]
    public int StudentId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(AdvisorId))]
    public Advisor Advisor { get; set; } = null!;

    [ForeignKey(nameof(StudentId))]
    public SisStudent Student { get; set; } = null!;

    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}