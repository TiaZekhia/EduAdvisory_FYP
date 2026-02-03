using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduAdvisory_Backend.Models;

[Table("generated_study_plan")]
public partial class GeneratedStudyPlan
{
    [Key]
    [Column("plan_id")]
    public int PlanId { get; set; }

    [Column("student_id")]
    public int? StudentId { get; set; }

    [Column("course_code")]
    [StringLength(20)]
    public string? CourseCode { get; set; }

    [Column("planned_semester")]
    public int? PlannedSemester { get; set; }

    [Column("generation_date")]
    public DateOnly? GenerationDate { get; set; }

    [ForeignKey("CourseCode")]
    [InverseProperty("GeneratedStudyPlans")]
    public virtual SisCourse? CourseCodeNavigation { get; set; }

    [ForeignKey("StudentId")]
    [InverseProperty("GeneratedStudyPlans")]
    public virtual SisStudent? Student { get; set; }
}
