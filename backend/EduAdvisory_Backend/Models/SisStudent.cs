using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduAdvisory_Backend.Models;

[Table("sis_student")]
public partial class SisStudent
{
    [Key]
    [Column("student_id")]
    public int StudentId { get; set; }

    [Column("first_name")]
    [StringLength(100)]
    public string? FirstName { get; set; }

    [Column("last_name")]
    [StringLength(100)]
    public string? LastName { get; set; }

    [Column("program_code")]
    [StringLength(50)]
    public string? ProgramCode { get; set; }

    [Column("current_semester")]
    public int? CurrentSemester { get; set; }

    [Column("current_gpa")]
    [Precision(5, 2)]
    public decimal? CurrentGpa { get; set; }

    [Column("academic_status")]
    [StringLength(20)]
    public string? AcademicStatus { get; set; }

    [Column("email")]
    [StringLength(150)]
    public string? Email { get; set; }

    [Column("advisor_id")]
    public int? AdvisorId { get; set; }

    [ForeignKey("AdvisorId")]
    [InverseProperty("SisStudents")]
    public virtual Advisor? Advisor { get; set; }

    [InverseProperty("Student")]
    public virtual ICollection<ChatbotHistory> ChatbotHistories { get; set; } = new List<ChatbotHistory>();

    [InverseProperty("Student")]
    public virtual ICollection<GeneratedStudyPlan> GeneratedStudyPlans { get; set; } = new List<GeneratedStudyPlan>();

    [InverseProperty("Student")]
    public virtual ICollection<Meeting> Meetings { get; set; } = new List<Meeting>();

    [InverseProperty("Student")]
    public virtual ICollection<SisCourseAssessment> SisCourseAssessments { get; set; } = new List<SisCourseAssessment>();

    [InverseProperty("Student")]
    public virtual ICollection<SisCurrentEnrollment> SisCurrentEnrollments { get; set; } = new List<SisCurrentEnrollment>();

    [InverseProperty("Student")]
    public virtual ICollection<SisStudentCourseHistory> SisStudentCourseHistories { get; set; } = new List<SisStudentCourseHistory>();

    [InverseProperty("Student")]
    public virtual ICollection<SisStudentGrade> SisStudentGrades { get; set; } = new List<SisStudentGrade>();

    [InverseProperty("Student")]
    public virtual ICollection<StudentRisk> StudentRisks { get; set; } = new List<StudentRisk>();

    [InverseProperty("LinkedStudent")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
