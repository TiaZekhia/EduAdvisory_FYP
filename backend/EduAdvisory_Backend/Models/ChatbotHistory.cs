using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduAdvisory_Backend.Models;

[Table("chatbot_history")]
public partial class ChatbotHistory
{
    [Key]
    [Column("chatbot_id")]
    public int ChatbotId { get; set; }

    [Column("student_id")]
    public int? StudentId { get; set; }

    [Column("question")]
    public string? Question { get; set; }

    [Column("answer")]
    public string? Answer { get; set; }

    [Column("timestamp", TypeName = "timestamp without time zone")]
    public DateTime? Timestamp { get; set; }

    [ForeignKey("StudentId")]
    [InverseProperty("ChatbotHistories")]
    public virtual SisStudent? Student { get; set; }
}
