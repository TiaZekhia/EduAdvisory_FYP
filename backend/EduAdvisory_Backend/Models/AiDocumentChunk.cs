using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pgvector;

namespace EduAdvisory_Backend.Models;

[Table("ai_document_chunks")]
public class AiDocumentChunk
{
    [Key]
    [Column("chunk_id")]
    public int ChunkId { get; set; }

    [Column("document_id")]
    public int DocumentId { get; set; }

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

    [Required]
    [Column("document_type")]
    [MaxLength(50)]
    public string DocumentType { get; set; } = string.Empty;

    [Column("section_title")]
    [MaxLength(255)]
    public string? SectionTitle { get; set; }

    [Required]
    [Column("chunk_text")]
    public string ChunkText { get; set; } = string.Empty;

    [Column("embedding")]
    public Vector? Embedding { get; set; }

    [Column("chunk_index")]
    public int ChunkIndex { get; set; }

    [Column("page_number")]
    public int? PageNumber { get; set; }

    [Column("token_count")]
    public int? TokenCount { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public AiDocument? Document { get; set; }
}