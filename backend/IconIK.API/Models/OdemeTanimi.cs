using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IconIK.API.Models
{
    /// <summary>
    /// Bordro ödeme kalemlerinin tanımları (Sabit/Değişken ödemeler)
    /// </summary>
    [Table("odeme_tanimlari")]
    public class OdemeTanimi
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("kod")]
        public string Kod { get; set; } = string.Empty; // Örn: MAAS, YEMEK, YOL, PRIM, MESAI

        [Required]
        [MaxLength(200)]
        [Column("ad")]
        public string Ad { get; set; } = string.Empty; // Örn: Aylık Maaş, Yemek Yardımı

        [Required]
        [MaxLength(50)]
        [Column("odeme_turu")]
        public string OdemeTuru { get; set; } = string.Empty; // Sabit, Degisken, Mesai, Prim, SosyalHak, Ikramiye

        [Column("aciklama")]
        [MaxLength(500)]
        public string? Aciklama { get; set; }

        // Hesaplama Parametreleri
        [Column("sgk_matrahina_dahil")]
        public bool SgkMatrahinaDahil { get; set; } = true; // SGK matrahına dahil mi?

        [Column("vergi_matrahina_dahil")]
        public bool VergiMatrahinaDahil { get; set; } = true; // Vergi matrahına dahil mi?

        [Column("agi_uygulanir")]
        public bool AgiUygulanir { get; set; } = true; // AGI uygulanır mı?

        [Column("damga_vergisi_dahil")]
        public bool DamgaVergisiDahil { get; set; } = true; // Damga vergisi hesabına dahil mi?

        [Column("varsayilan_tutar", TypeName = "decimal(10,2)")]
        public decimal? VarsayilanTutar { get; set; } // Sabit ödeme tutarı (varsa)

        [Column("hesaplama_formulu")]
        [MaxLength(500)]
        public string? HesaplamaFormulu { get; set; } // Formül (örn: "BRUT_MAAS * 0.15")

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
        public virtual ICollection<BordroOdeme> BordroOdemeler { get; set; } = new List<BordroOdeme>();
    }
}
