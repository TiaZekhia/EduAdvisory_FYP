using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduAdvisory_Backend.Models;

[Table("course_grading_schema")]
public partial class CourseGradingSchema
{
    [Key]
    [Column("grading_id")]
    public int GradingId { get; set; }

    [Column("course_code")]
    [StringLength(20)]
    public string CourseCode { get; set; } = null!;

    [Column("component_name")]
    [StringLength(50)]
    public string? ComponentName { get; set; }

    [Column("weight_percentage")]
    public int? WeightPercentage { get; set; }

    [ForeignKey("CourseCode")]
    [InverseProperty("CourseGradingSchemas")]
    public virtual SisCourse CourseCodeNavigation { get; set; } = null!;
}
