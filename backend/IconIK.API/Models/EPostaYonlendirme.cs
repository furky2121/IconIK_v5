using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IconIK.API.Models
{
    [Table("eposta_yonlendirme")]
    public class EPostaYonlendirme
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("yonlendirme_turu")]
        [MaxLength(100)]
        public string YonlendirmeTuru { get; set; } = string.Empty; // "MulakatPlanlama", "BordroHatirlat", vb.

        [Required]
        [Column("alici_email")]
        [MaxLength(255)]
        public string AliciEmail { get; set; } = string.Empty;

        [Required]
        [Column("gonderim_saati")]
        public TimeSpan GonderimSaati { get; set; } // Saat:Dakika formatında (örn: 09:00)

        [Column("aktif")]
        public bool Aktif { get; set; } = true;

        [Column("son_gonderim_tarihi")]
        public DateTime? SonGonderimTarihi { get; set; }

        [Column("aciklama")]
        [MaxLength(500)]
        public string? Aciklama { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
