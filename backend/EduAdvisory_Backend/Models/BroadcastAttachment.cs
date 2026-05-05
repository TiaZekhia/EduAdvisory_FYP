using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduAdvisory_Backend.Models;

[Table("broadcast_attachment")]
public class BroadcastAttachment
{
    [Key]
    [Column("attachment_id")]
    public int AttachmentId { get; set; }

    [Column("broadcast_message_id")]
    public int BroadcastMessageId { get; set; }

    [Column("file_name")]
    [StringLength(255)]
    public string FileName { get; set; } = null!;

    [Column("stored_file_name")]
    [StringLength(255)]
    public string StoredFileName { get; set; } = null!;

    [Column("file_url")]
    public string FileUrl { get; set; } = null!;

    [Column("content_type")]
    [StringLength(100)]
    public string ContentType { get; set; } = null!;

    [Column("file_size")]
    public long FileSize { get; set; }

    [Column("uploaded_at")]
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(BroadcastMessageId))]
    public BroadcastMessage BroadcastMessage { get; set; } = null!;
}