using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduAdvisory_Backend.Models;

[Table("risk_intervention_log")]
public class RiskInterventionLog
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("student_id")]
    public int StudentId { get; set; }

    [Column("advisor_id")]
    public int AdvisorId { get; set; }

    [Column("risk_level")]
    [StringLength(20)]
    public string RiskLevel { get; set; } = string.Empty;

    [Column("risk_score")]
    public int RiskScore { get; set; }

    [Column("action_type")]
    [StringLength(50)]
    public string ActionType { get; set; } = string.Empty;
    // LOW_RISK_MESSAGE, MEDIUM_RISK_ADVISOR_NOTIFICATION, HIGH_RISK_MEETING_RECOMMENDATION

    [Column("status")]
    [StringLength(20)]
    public string Status { get; set; } = "COMPLETED";
    // COMPLETED, PENDING, FAILED, SKIPPED_DUPLICATE

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("created_at", TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("StudentId")]
    public virtual SisStudent? Student { get; set; }

    [ForeignKey("AdvisorId")]
    public virtual Advisor? Advisor { get; set; }
}
