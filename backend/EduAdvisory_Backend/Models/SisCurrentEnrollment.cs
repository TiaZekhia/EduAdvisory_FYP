using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduAdvisory_Backend.Models;

[Table("sis_current_enrollment")]
public partial class SisCurrentEnrollment
{
    [Key]
    [Column("enrollment_id")]
    public int EnrollmentId { get; set; }

    [Column("student_id")]
    public int StudentId { get; set; }

    [Column("course_code")]
    [StringLength(20)]
    public string CourseCode { get; set; } = null!;

    [Column("semester")]
    [StringLength(20)]
    public string? Semester { get; set; }

    [ForeignKey("CourseCode")]
    [InverseProperty("SisCurrentEnrollments")]
    public virtual SisCourse CourseCodeNavigation { get; set; } = null!;

    [ForeignKey("StudentId")]
    [InverseProperty("SisCurrentEnrollments")]
    public virtual SisStudent Student { get; set; } = null!;
}
