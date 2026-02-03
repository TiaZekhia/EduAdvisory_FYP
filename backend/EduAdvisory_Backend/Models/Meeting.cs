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

    [Column("meeting_date", TypeName = "timestamp without time zone")]
    public DateTime? MeetingDate { get; set; }

    [Column("meeting_type")]
    [StringLength(20)]
    public string? MeetingType { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [ForeignKey("AdvisorId")]
    [InverseProperty("Meetings")]
    public virtual Advisor? Advisor { get; set; }

    [ForeignKey("StudentId")]
    [InverseProperty("Meetings")]
    public virtual SisStudent? Student { get; set; }
}
