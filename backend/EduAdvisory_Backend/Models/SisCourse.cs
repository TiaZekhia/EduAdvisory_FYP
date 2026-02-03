using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduAdvisory_Backend.Models;

[Table("sis_course")]
public partial class SisCourse
{
    [Key]
    [Column("course_code")]
    [StringLength(20)]
    public string CourseCode { get; set; } = null!;

    [Column("course_name")]
    [StringLength(150)]
    public string CourseName { get; set; } = null!;

    [Column("credits")]
    public int Credits { get; set; }

    [InverseProperty("CourseCodeNavigation")]
    public virtual ICollection<CourseGradingSchema> CourseGradingSchemas { get; set; } = new List<CourseGradingSchema>();

    [InverseProperty("CourseCodeNavigation")]
    public virtual ICollection<GeneratedStudyPlan> GeneratedStudyPlans { get; set; } = new List<GeneratedStudyPlan>();

    [InverseProperty("CourseCodeNavigation")]
    public virtual ICollection<SisCourseAssessment> SisCourseAssessments { get; set; } = new List<SisCourseAssessment>();

    [InverseProperty("CourseCodeNavigation")]
    public virtual ICollection<SisCurrentEnrollment> SisCurrentEnrollments { get; set; } = new List<SisCurrentEnrollment>();

    [InverseProperty("CourseCodeNavigation")]
    public virtual ICollection<SisStudentCourseHistory> SisStudentCourseHistories { get; set; } = new List<SisStudentCourseHistory>();

    [InverseProperty("CourseCodeNavigation")]
    public virtual ICollection<SisStudentGrade> SisStudentGrades { get; set; } = new List<SisStudentGrade>();

    [InverseProperty("CourseCodeNavigation")]
    public virtual ICollection<StudentRisk> StudentRisks { get; set; } = new List<StudentRisk>();

    [InverseProperty("CourseCodeNavigation")]
    public virtual ICollection<StudyGuide> StudyGuides { get; set; } = new List<StudyGuide>();

    [ForeignKey("PrerequisiteCourseCode")]
    [InverseProperty("PrerequisiteCourseCodes")]
    public virtual ICollection<SisCourse> CourseCodes { get; set; } = new List<SisCourse>();

    [ForeignKey("CourseCode")]
    [InverseProperty("CourseCodes")]
    public virtual ICollection<SisCourse> PrerequisiteCourseCodes { get; set; } = new List<SisCourse>();
}
