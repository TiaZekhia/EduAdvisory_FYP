using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduAdvisory_Backend.Models;

[Table("sis_student_course_history")]
public partial class SisStudentCourseHistory
{
    [Key]
    [Column("history_id")]
    public int HistoryId { get; set; }

    [Column("student_id")]
    public int StudentId { get; set; }

    [Column("course_code")]
    [StringLength(20)]
    public string CourseCode { get; set; } = null!;

    [Column("semester")]
    [StringLength(20)]
    public string? Semester { get; set; }

    [Column("final_grade")]
    [Precision(5, 2)]
    public decimal? FinalGrade { get; set; }

    [Column("status")]
    [StringLength(20)]
    public string? Status { get; set; }

    [ForeignKey("CourseCode")]
    [InverseProperty("SisStudentCourseHistories")]
    public virtual SisCourse CourseCodeNavigation { get; set; } = null!;

    [ForeignKey("StudentId")]
    [InverseProperty("SisStudentCourseHistories")]
    public virtual SisStudent Student { get; set; } = null!;
}
