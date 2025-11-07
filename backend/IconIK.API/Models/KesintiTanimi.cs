using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IconIK.API.Models
{
    /// <summary>
    /// Bordro kesinti kalemlerinin tanımları (SGK, Vergi, Avans, İcra vb.)
    /// </summary>
    [Table("kesinti_tanimlari")]
    public class KesintiTanimi
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("kod")]
        public string Kod { get; set; } = string.Empty; // Örn: SGK, VERGI, AVANS, ICRA, NAFAKA

        [Required]
        [MaxLength(200)]
        [Column("ad")]
        public string Ad { get; set; } = string.Empty; // Örn: SGK İşçi Payı, Gelir Vergisi

        [Required]
        [MaxLength(50)]
        [Column("kesinti_turu")]
        public string KesintiTuru { get; set; } = string.Empty; // Yasal, Icra, Avans, Nafaka, Diger

        [Column("aciklama")]
        [MaxLength(500)]
        public string? Aciklama { get; set; }

        // Hesaplama Parametreleri
        [Column("otomatik_hesaplama")]
        public bool OtomatikHesaplama { get; set; } = false; // Otomatik mi hesaplanır?

        [Column("hesaplama_formulu")]
        [MaxLength(500)]
        public string? HesaplamaFormulu { get; set; } // Formül (örn: "SGK_MATRAHI * 0.14")

        [Column("oran", TypeName = "decimal(5,2)")]
        public decimal? Oran { get; set; } // Kesinti oranı (%)

        [Column("sabit_tutar", TypeName = "decimal(10,2)")]
        public decimal? SabitTutar { get; set; } // Sabit kesinti tutarı

        [Column("taksitlenebilir")]
        public bool Taksitlenebilir { get; set; } = false; // Taksitlendirilebilir mi? (Avans için)

        [Column("sira_no")]
        public int SiraNo { get; set; } = 0; // Bordro fişinde gösterim sırası

        // Audit
        [Column("aktif")]
        public bool Aktif { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("created_by")]
        public int? CreatedBy { get; set; }

        // İlişkiler
        public virtual ICollection<BordroKesinti> BordroKesintiler { get; set; } = new List<BordroKesinti>();
    }
}
