using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduAdvisory_Backend.Models;

[Table("users")]
[Index("Username", Name = "users_username_key", IsUnique = true)]
public partial class User
{
    [Key]
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("username")]
    [StringLength(100)]
    public string? Username { get; set; }

    [Column("password")]
    public string? Password { get; set; }

    [Column("role")]
    [StringLength(20)]
    public string? Role { get; set; }

    [Column("linked_student_id")]
    public int? LinkedStudentId { get; set; }

    [Column("linked_advisor_id")]
    public int? LinkedAdvisorId { get; set; }

    [ForeignKey("LinkedAdvisorId")]
    [InverseProperty("Users")]
    public virtual Advisor? LinkedAdvisor { get; set; }

    [ForeignKey("LinkedStudentId")]
    [InverseProperty("Users")]
    public virtual SisStudent? LinkedStudent { get; set; }
}
