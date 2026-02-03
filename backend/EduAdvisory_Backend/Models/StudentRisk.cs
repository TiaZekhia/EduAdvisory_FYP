using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduAdvisory_Backend.Models;

[Table("student_risk")]
public partial class StudentRisk
{
    [Key]
    [Column("risk_id")]
    public int RiskId { get; set; }

    [Column("student_id")]
    public int? StudentId { get; set; }

    [Column("course_code")]
    [StringLength(20)]
    public string? CourseCode { get; set; }

    [Column("risk_level")]
    [StringLength(20)]
    public string? RiskLevel { get; set; }

    [Column("risk_score")]
    [Precision(5, 2)]
    public decimal? RiskScore { get; set; }

    [Column("calculated_at", TypeName = "timestamp without time zone")]
    public DateTime? CalculatedAt { get; set; }

    [ForeignKey("CourseCode")]
    [InverseProperty("StudentRisks")]
    public virtual SisCourse? CourseCodeNavigation { get; set; }

    [ForeignKey("StudentId")]
    [InverseProperty("StudentRisks")]
    public virtual SisStudent? Student { get; set; }
}
