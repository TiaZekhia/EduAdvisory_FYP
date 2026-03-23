using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace EduAdvisory_Backend.Models;

[Table("advisor_availability")]
public partial class AdvisorAvailability
{
    [Key]
    [Column("availability_id")]
    public int AvailabilityId { get; set; }

    [Column("advisor_id")]
    public int AdvisorId { get; set; }

    [Column("start_at", TypeName = "timestamp with time zone")]
    public DateTimeOffset StartAt { get; set; }

    [Column("end_at", TypeName = "timestamp with time zone")]
    public DateTimeOffset EndAt { get; set; }

    [Column("is_booked")]
    public bool IsBooked { get; set; } = false;

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at", TypeName = "timestamp with time zone")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [ForeignKey("AdvisorId")]
    public virtual Advisor Advisor { get; set; } = null!;
}