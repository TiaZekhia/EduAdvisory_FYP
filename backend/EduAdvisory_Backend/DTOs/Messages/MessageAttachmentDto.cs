namespace EduAdvisory_Backend.DTOs.Messages;

public class MessageAttachmentDto
{
    public int AttachmentId { get; set; }

    public string FileName { get; set; } = null!;

    public string FileUrl { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public long FileSize { get; set; }
}