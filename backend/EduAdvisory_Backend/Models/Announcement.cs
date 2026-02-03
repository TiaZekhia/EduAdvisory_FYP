using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduAdvisory_Backend.Models;

[Table("announcement")]
public partial class Announcement
{
    [Key]
    [Column("announcement_id")]
    public int AnnouncementId { get; set; }

    [Column("advisor_id")]
    public int? AdvisorId { get; set; }

    [Column("title")]
    [StringLength(150)]
    public string? Title { get; set; }

    [Column("content")]
    public string? Content { get; set; }

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("AdvisorId")]
    [InverseProperty("Announcements")]
    public virtual Advisor? Advisor { get; set; }
}
