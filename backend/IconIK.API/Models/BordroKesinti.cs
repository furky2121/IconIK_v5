using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IconIK.API.Models
{
    /// <summary>
    /// Bordro kesinti detayları (her bir kesinti kalemi)
    /// </summary>
    [Table("bordro_kesintiler")]
    public class BordroKesinti
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("bordro_id")]
        public int BordroId { get; set; }

        [Required]
        [Column("kesinti_tanimi_id")]
        public int KesintiTanimiId { get; set; }

        // Kesinti Detayları
        [Required]
        [MaxLength(100)]
        [Column("kesinti_kodu")]
        public string KesintiKodu { get; set; } = string.Empty; // SGK, VERGI, AVANS vb.

        [Required]
        [MaxLength(200)]
        [Column("kesinti_adi")]
        public string KesintiAdi { get; set; } = string.Empty; // SGK İşçi Payı, Gelir Vergisi vb.

        [Required]
        [MaxLength(50)]
        [Column("kesinti_turu")]
        public string KesintiTuru { get; set; } = string.Empty; // Yasal, Icra, Avans, Nafaka, Diger

        [Required]
        [Column("tutar", TypeName = "decimal(12,2)")]
        public decimal Tutar { get; set; }

        [Column("matrah", TypeName = "decimal(12,2)")]
        public decimal? Matrah { get; set; } // Kesintinin hesaplandığı matrah

        [Column("oran", TypeName = "decimal(5,2)")]
        public decimal? Oran { get; set; } // Kesinti oranı (%)

        [Column("otomatik_hesaplama")]
        public bool OtomatikHesaplama { get; set; } = false;

        // Taksit Bilgileri (Avans vb. için)
        [Column("taksit_no")]
        public int? TaksitNo { get; set; }

        [Column("toplam_taksit")]
        public int? ToplamTaksit { get; set; }

        [Column("kalan_borc", TypeName = "decimal(12,2)")]
        public decimal? KalanBorc { get; set; }

        // Referans (Avans, İcra dosya no vb.)
        [Column("referans_id")]
        public int? ReferansId { get; set; }

        [Column("referans_tablo")]
        [MaxLength(100)]
        public string? ReferansTablo { get; set; } // AvanseTalepleri, IcraDosyalari vb.

        [Column("referans_no")]
        [MaxLength(100)]
        public string? ReferansNo { get; set; } // Dosya no, belge no vb.

        // Açıklama
        [Column("aciklama")]
        [MaxLength(500)]
        public string? Aciklama { get; set; }

        [Column("hesaplama_detayi")]
        public string? HesaplamaDetayi { get; set; } // JSON formatında hesaplama detayı

        [Column("sira_no")]
        public int SiraNo { get; set; } = 0;

        // Audit
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("created_by")]
        public int? CreatedBy { get; set; }

        // İlişkiler
        [ForeignKey("BordroId")]
        public virtual BordroAna Bordro { get; set; } = null!;

        [ForeignKey("KesintiTanimiId")]
        public virtual KesintiTanimi KesintiTanimi { get; set; } = null!;
    }
}
