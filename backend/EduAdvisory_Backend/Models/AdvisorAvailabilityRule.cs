using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduAdvisory_Backend.Models;

[Table("advisor_availability_rule")]
public partial class AdvisorAvailabilityRule
{
    [Key]
    [Column("rule_id")]
    public int RuleId { get; set; }

    [Column("advisor_id")]
    public int AdvisorId { get; set; }

    // 0=Sunday, 1=Monday, ..., 6=Saturday
    [Column("day_of_week")]
    public int DayOfWeek { get; set; }

    [Column("start_time")]
    public TimeSpan StartTime { get; set; }

    [Column("end_time")]
    public TimeSpan EndTime { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at", TypeName = "timestamp with time zone")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [ForeignKey(nameof(AdvisorId))]
    public virtual Advisor Advisor { get; set; } = null!;
}