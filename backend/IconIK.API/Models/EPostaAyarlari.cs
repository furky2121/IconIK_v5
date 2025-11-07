using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IconIK.API.Models
{
    [Table("eposta_ayarlari")]
    public class EPostaAyarlari
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("smtp_host")]
        [MaxLength(255)]
        public string SmtpHost { get; set; } = string.Empty;

        [Required]
        [Column("smtp_port")]
        public int SmtpPort { get; set; } = 587;

        [Required]
        [Column("smtp_username")]
        [MaxLength(255)]
        public string SmtpUsername { get; set; } = string.Empty;

        [Required]
        [Column("smtp_password")]
        [MaxLength(500)]
        public string SmtpPassword { get; set; } = string.Empty; // Encrypted

        [Required]
        [Column("enable_ssl")]
        public bool EnableSsl { get; set; } = true;

        [Required]
        [Column("from_email")]
        [MaxLength(255)]
        public string FromEmail { get; set; } = string.Empty;

        [Required]
        [Column("from_name")]
        [MaxLength(255)]
        public string FromName { get; set; } = string.Empty;

        [Column("aktif")]
        public bool Aktif { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
