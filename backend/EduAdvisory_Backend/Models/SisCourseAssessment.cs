using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduAdvisory_Backend.Models;

[Table("sis_course_assessment")]
public partial class SisCourseAssessment
{
    [Key]
    [Column("assessment_id")]
    public int AssessmentId { get; set; }

    [Column("student_id")]
    public int StudentId { get; set; }

    [Column("course_code")]
    [StringLength(20)]
    public string CourseCode { get; set; } = null!;

    [Column("course_credits")]
    public int? CourseCredits { get; set; }

    [Column("absences_count")]
    public int? AbsencesCount { get; set; }

    [Column("max_absences")]
    public int? MaxAbsences { get; set; }

    [Column("semester_start_date")]
    public DateOnly? SemesterStartDate { get; set; }

    [Column("semester_end_date")]
    public DateOnly? SemesterEndDate { get; set; }

    [Column("last_updated", TypeName = "timestamp without time zone")]
    public DateTime? LastUpdated { get; set; }

    [ForeignKey("CourseCode")]
    [InverseProperty("SisCourseAssessments")]
    public virtual SisCourse CourseCodeNavigation { get; set; } = null!;

    [ForeignKey("StudentId")]
    [InverseProperty("SisCourseAssessments")]
    public virtual SisStudent Student { get; set; } = null!;
}
