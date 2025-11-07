using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IconIK.API.Models
{
    [Table("luca_bordro_ayarlari")]
    public class LucaBordroAyarlari
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("baglanti_tipi")]
        [MaxLength(20)]
        public string BaglantiTipi { get; set; } = string.Empty; // API, Dosya, Ikisi

        [Column("api_url")]
        [MaxLength(500)]
        public string? ApiUrl { get; set; }

        [Column("api_key")]
        [MaxLength(500)]
        public string? ApiKey { get; set; } // Encrypted

        [Column("api_username")]
        [MaxLength(255)]
        public string? ApiUsername { get; set; }

        [Column("api_password")]
        [MaxLength(500)]
        public string? ApiPassword { get; set; } // Encrypted

        [Column("dosya_yolu")]
        [MaxLength(500)]
        public string? DosyaYolu { get; set; }

        [Column("otomatik_senkron")]
        public bool OtomatikSenkron { get; set; } = false;

        [Column("senkron_saati")]
        [MaxLength(5)]
        public string? SenkronSaati { get; set; } // Format: "09:00"

        [Column("son_senkron_tarihi")]
        public DateTime? SonSenkronTarihi { get; set; }

        [Column("aktif")]
        public bool Aktif { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
