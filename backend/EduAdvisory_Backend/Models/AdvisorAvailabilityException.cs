using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduAdvisory_Backend.Models;

[Table("advisor_availability_exception")]
public class AdvisorAvailabilityException
{
    [Key]
    [Column("exception_id")]
    public int ExceptionId { get; set; }

    [Column("advisor_id")]
    public int AdvisorId { get; set; }

    [Column("exception_date")]
    public DateOnly ExceptionDate { get; set; }

    /// <summary>Null = full day blocked. Non-null = only this time range is blocked.</summary>
    [Column("start_time")]
    public TimeSpan? StartTime { get; set; }

    [Column("end_time")]
    public TimeSpan? EndTime { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Advisor Advisor { get; set; } = null!;
}
