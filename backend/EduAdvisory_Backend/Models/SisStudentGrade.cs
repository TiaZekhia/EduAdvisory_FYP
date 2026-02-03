using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduAdvisory_Backend.Models;

[Table("sis_student_grades")]
public partial class SisStudentGrade
{
    [Key]
    [Column("grade_id")]
    public int GradeId { get; set; }

    [Column("student_id")]
    public int StudentId { get; set; }

    [Column("course_code")]
    [StringLength(20)]
    public string CourseCode { get; set; } = null!;

    [Column("component_name")]
    [StringLength(50)]
    public string? ComponentName { get; set; }

    [Column("grade")]
    [Precision(5, 2)]
    public decimal? Grade { get; set; }

    [ForeignKey("CourseCode")]
    [InverseProperty("SisStudentGrades")]
    public virtual SisCourse CourseCodeNavigation { get; set; } = null!;

    [ForeignKey("StudentId")]
    [InverseProperty("SisStudentGrades")]
    public virtual SisStudent Student { get; set; } = null!;
}
