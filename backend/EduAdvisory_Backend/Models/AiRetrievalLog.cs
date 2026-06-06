using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduAdvisory_Backend.Models;

[Table("ai_retrieval_logs")]
public class AiRetrievalLog
{
    [Key]
    [Column("retrieval_log_id")]
    public int RetrievalLogId { get; set; }

    [Column("student_id")]
    public int StudentId { get; set; }

    [Column("session_id")]
    public int? SessionId { get; set; }

    [Required]
    [Column("question")]
    public string Question { get; set; } = string.Empty;

    [Column("retrieved_chunk_ids")]
    public string? RetrievedChunkIds { get; set; }

    [Column("top_similarity_score")]
    public double? TopSimilarityScore { get; set; }

    [Column("plugin_used")]
    [MaxLength(100)]
    public string? PluginUsed { get; set; }

    [Column("response_source")]
    [MaxLength(50)]
    public string ResponseSource { get; set; } = "rag";
    // rag, plugin, hybrid, fallback

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}