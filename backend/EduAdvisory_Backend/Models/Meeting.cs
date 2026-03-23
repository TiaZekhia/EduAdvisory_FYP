using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduAdvisory_Backend.Models;

[Table("meeting")]
public partial class Meeting
{
    [Key]
    [Column("meeting_id")]
    public int MeetingId { get; set; }

    [Column("advisor_id")]
    public int? AdvisorId { get; set; }

    [Column("student_id")]
    public int? StudentId { get; set; }

    [Column("meeting_date", TypeName = "timestamp with time zone")]
    public DateTimeOffset? MeetingDate { get; set; }

    [Column("start_at", TypeName = "timestamp with time zone")]
    public DateTimeOffset StartAt { get; set; }

    [Column("end_at", TypeName = "timestamp with time zone")]
    public DateTimeOffset EndAt { get; set; }

    [Column("meeting_type")]
    [StringLength(20)]
    public string? MeetingType { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("request_id")]
    public int? RequestId { get; set; }

    [Column("title")]
    [StringLength(150)]
    public string? Title { get; set; }

    [Column("status")]
    [StringLength(20)]
    public string? Status { get; set; } // UPCOMING, COMPLETED, CANCELLED

    [Column("meeting_link")]
    [StringLength(500)]
    public string? MeetingLink { get; set; }

    [Column("duration_minutes")]
    public int? DurationMinutes { get; set; }

    [Column("created_at", TypeName = "timestamp with time zone")]
    public DateTimeOffset? CreatedAt { get; set; }

    [Column("updated_at", TypeName = "timestamp with time zone")]
    public DateTimeOffset? UpdatedAt { get; set; }

    [Column("google_space_name")]
    [StringLength(255)]
    public string? GoogleSpaceName { get; set; }

    [ForeignKey("AdvisorId")]
    [InverseProperty("Meetings")]
    public virtual Advisor? Advisor { get; set; }

    [ForeignKey("StudentId")]
    [InverseProperty("Meetings")]
    public virtual SisStudent? Student { get; set; }
}
