using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduAdvisory_Backend.Models;

[Table("advisor")]
[Index("Email", Name = "advisor_email_key", IsUnique = true)]
public partial class Advisor
{
    [Key]
    [Column("advisor_id")]
    public int AdvisorId { get; set; }

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column("email")]
    [StringLength(100)]
    public string Email { get; set; } = null!;

    [Column("office")]
    [StringLength(150)]
    public string Office { get; set; }

    [Column("office_hours")]
    [StringLength(200)]
    public string OfficeHours { get; set; }

    [InverseProperty("Advisor")]
    public virtual ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();

    [InverseProperty("Advisor")]
    public virtual ICollection<Meeting> Meetings { get; set; } = new List<Meeting>();

    [InverseProperty("Advisor")]
    public virtual ICollection<SisStudent> SisStudents { get; set; } = new List<SisStudent>();

    [InverseProperty("LinkedAdvisor")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
