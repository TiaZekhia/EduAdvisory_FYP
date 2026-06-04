using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduAdvisory_Backend.Models;

[Table("ai_documents")]
public class AiDocument
{
    [Key]
    [Column("document_id")]
    public int DocumentId { get; set; }

    [Required]
    [Column("title")]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Column("document_type")]
    [MaxLength(50)]
    public string DocumentType { get; set; } = string.Empty;
    // study_guide or course_syllabus

    [Required]
    [Column("scope")]
    [MaxLength(50)]
    public string Scope { get; set; } = "course";
    // course, program, general

    [Column("course_code")]
    [MaxLength(50)]
    public string? CourseCode { get; set; }

    [Column("program_code")]
    [MaxLength(50)]
    public string? ProgramCode { get; set; }

    [Column("academic_year")]
    [MaxLength(50)]
    public string? AcademicYear { get; set; }

    [Column("semester")]
    [MaxLength(50)]
    public string? Semester { get; set; }

    [Required]
    [Column("file_name")]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [Column("file_path")]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [Required]
    [Column("status")]
    [MaxLength(50)]
    public string Status { get; set; } = "Uploaded";
    // Uploaded, Processing, Processed, Failed, Deleted

    [Column("uploaded_by_user_id")]
    public int? UploadedByUserId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("processed_at")]
    public DateTime? ProcessedAt { get; set; }

    [Column("error_message")]
    public string? ErrorMessage { get; set; }

    public ICollection<AiDocumentChunk> Chunks { get; set; } = new List<AiDocumentChunk>();
}