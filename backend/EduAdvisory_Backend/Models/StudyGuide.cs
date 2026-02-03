using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduAdvisory_Backend.Models;

[Table("study_guide")]
public partial class StudyGuide
{
    [Key]
    [Column("study_guide_id")]
    public int StudyGuideId { get; set; }

    [Column("program_code")]
    [StringLength(50)]
    public string? ProgramCode { get; set; }

    [Column("course_code")]
    [StringLength(20)]
    public string? CourseCode { get; set; }

    [Column("recommended_semester")]
    public int? RecommendedSemester { get; set; }

    [ForeignKey("CourseCode")]
    [InverseProperty("StudyGuides")]
    public virtual SisCourse? CourseCodeNavigation { get; set; }
}
