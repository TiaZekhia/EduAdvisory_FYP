using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduAdvisory_Backend.Models;

[Table("broadcast_recipient")]
public class BroadcastRecipient
{
    [Key]
    [Column("broadcast_recipient_id")]
    public int BroadcastRecipientId { get; set; }

    [Column("broadcast_message_id")]
    public int BroadcastMessageId { get; set; }

    [Column("student_id")]
    public int StudentId { get; set; }

    [Column("is_read")]
    public bool IsRead { get; set; } = false;

    [Column("read_at")]
    public DateTime? ReadAt { get; set; }

    [ForeignKey(nameof(BroadcastMessageId))]
    public BroadcastMessage BroadcastMessage { get; set; } = null!;

    [ForeignKey(nameof(StudentId))]
    public SisStudent Student { get; set; } = null!;
}