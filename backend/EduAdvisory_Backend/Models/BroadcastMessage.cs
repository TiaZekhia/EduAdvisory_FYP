using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduAdvisory_Backend.Models;

[Table("broadcast_message")]
public class BroadcastMessage
{
    [Key]
    [Column("broadcast_message_id")]
    public int BroadcastMessageId { get; set; }

    [Column("advisor_id")]
    public int AdvisorId { get; set; }

    [Column("title")]
    [StringLength(150)]
    public string Title { get; set; } = null!;

    [Column("content")]
    public string Content { get; set; } = null!;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(AdvisorId))]
    public Advisor Advisor { get; set; } = null!;

    public ICollection<BroadcastRecipient> Recipients { get; set; } = new List<BroadcastRecipient>();
}