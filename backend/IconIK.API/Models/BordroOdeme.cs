using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IconIK.API.Models
{
    /// <summary>
    /// Bordro ödeme detayları (her bir ödeme kalemi)
    /// </summary>
    [Table("bordro_odemeler")]
    public class BordroOdeme
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("bordro_id")]
        public int BordroId { get; set; }

        [Required]
        [Column("odeme_tanimi_id")]
        public int OdemeTanimiId { get; set; }

        // Ödeme Detayları
        [Required]
        [MaxLength(100)]
        [Column("odeme_kodu")]
        public string OdemeKodu { get; set; } = string.Empty; // MAAS, YEMEK, YOL vb.

        [Required]
        [MaxLength(200)]
        [Column("odeme_adi")]
        public string OdemeAdi { get; set; } = string.Empty; // Aylık Maaş, Yemek Yardımı vb.

        [Required]
        [MaxLength(50)]
        [Column("odeme_turu")]
        public string OdemeTuru { get; set; } = string.Empty; // Sabit, Degisken, Mesai, Prim, SosyalHak

        [Required]
        [Column("tutar", TypeName = "decimal(12,2)")]
        public decimal Tutar { get; set; }

        [Column("miktar", TypeName = "decimal(10,2)")]
        public decimal? Miktar { get; set; } // Mesai saati, gün sayısı vb.

        [Column("birim")]
        [MaxLength(20)]
        public string? Birim { get; set; } // Saat, Gün, Adet vb.

        [Column("birim_fiyat", TypeName = "decimal(10,2)")]
        public decimal? BirimFiyat { get; set; }

        // Vergi & SGK Durumu
        [Column("sgk_matrahina_dahil")]
        public bool SgkMatrahinaDahil { get; set; } = true;

        [Column("vergi_matrahina_dahil")]
        public bool VergiMatrahinaDahil { get; set; } = true;

        [Column("agi_uygulanir")]
        public bool AgiUygulanir { get; set; } = true;

        [Column("damga_vergisi_dahil")]
        public bool DamgaVergisiDahil { get; set; } = true;

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

        [ForeignKey("OdemeTanimiId")]
        public virtual OdemeTanimi OdemeTanimi { get; set; } = null!;
    }
}
