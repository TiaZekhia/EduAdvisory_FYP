using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduAdvisory_Backend.Models;

[Table("chat_message")]
public class ChatMessage
{
    [Key]
    [Column("message_id")]
    public int MessageId { get; set; }

    [Column("conversation_id")]
    public int ConversationId { get; set; }

    [Column("sender_user_id")]
    public int SenderUserId { get; set; }

    [Column("content")]
    public string Content { get; set; } = null!;

    [Column("sent_at")]
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    [Column("is_read")]
    public bool IsRead { get; set; } = false;

    [ForeignKey(nameof(ConversationId))]
    public Conversation Conversation { get; set; } = null!;

    [ForeignKey(nameof(SenderUserId))]
    public User SenderUser { get; set; } = null!;

    public ICollection<MessageAttachment> Attachments { get; set; } = new List<MessageAttachment>();
}