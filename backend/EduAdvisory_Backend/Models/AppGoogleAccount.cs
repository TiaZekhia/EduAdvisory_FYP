using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduAdvisory_Backend.Models
{
    [Table("app_google_account")]
    public class AppGoogleAccount
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("google_email")]
        [StringLength(150)]
        public string? GoogleEmail { get; set; }

        [Column("access_token")]
        public string? AccessToken { get; set; }

        [Column("refresh_token")]
        public string? RefreshToken { get; set; }

        [Column("token_expiry_utc", TypeName = "timestamp with time zone")]
        public DateTimeOffset? TokenExpiryUtc { get; set; }

        [Column("created_at", TypeName = "timestamp with time zone")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        [Column("updated_at", TypeName = "timestamp with time zone")]
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}