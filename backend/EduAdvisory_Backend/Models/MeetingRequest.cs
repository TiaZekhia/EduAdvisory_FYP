using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EduAdvisory_Backend.Models;

[Table("meeting_request")]
public partial class MeetingRequest
{
    [Key]
    [Column("request_id")]
    public int RequestId { get; set; }

    [Column("student_id")]
    public int StudentId { get; set; }

    [Column("advisor_id")]
    public int AdvisorId { get; set; }

    [Column("start_at", TypeName = "timestamp with time zone")]
    public DateTimeOffset StartAt { get; set; }

    [Column("end_at", TypeName = "timestamp with time zone")]
    public DateTimeOffset EndAt { get; set; }

    [Column("reason")]
    [StringLength(500)]
    public string? Reason { get; set; }

    [Column("status")]
    [StringLength(20)]
    public string Status { get; set; } = "PENDING"; // PENDING, ACCEPTED, REJECTED, CANCELLED

    [Column("rejection_reason")]
    [StringLength(500)]
    public string? RejectionReason { get; set; }

    [Column("requested_at", TypeName = "timestamp with time zone")]
    public DateTimeOffset RequestedAt { get; set; } = DateTimeOffset.UtcNow;

    [Column("responded_at", TypeName = "timestamp with time zone")]
    public DateTimeOffset? RespondedAt { get; set; }

    [ForeignKey("StudentId")]
    public virtual SisStudent Student { get; set; } = null!;

    [ForeignKey("AdvisorId")]
    public virtual Advisor Advisor { get; set; } = null!;
}