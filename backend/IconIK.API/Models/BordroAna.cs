using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IconIK.API.Models
{
    /// <summary>
    /// Ana bordro kayıtları (aylık bordro özeti)
    /// </summary>
    [Table("bordro_ana")]
    public class BordroAna
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("personel_id")]
        public int PersonelId { get; set; }

        [Column("puantaj_id")]
        public int? PuantajId { get; set; }

        [Required]
        [Column("donem_yil")]
        public int DonemYil { get; set; }

        [Required]
        [Column("donem_ay")]
        public int DonemAy { get; set; } // 1-12

        [Column("bordro_no")]
        [MaxLength(50)]
        public string? BordroNo { get; set; } // Otomatik oluşan bordro numarası

        // Personel Bilgileri (Snapshot - bordro kesildiği andaki bilgiler)
        [Column("pozisyon_adi")]
        [MaxLength(100)]
        public string? PozisyonAdi { get; set; }

        [Column("departman_adi")]
        [MaxLength(100)]
        public string? DepartmanAdi { get; set; }

        [Column("medeni_durum")]
        [MaxLength(20)]
        public string? MedeniDurum { get; set; }

        [Column("cocuk_sayisi")]
        public int CocukSayisi { get; set; } = 0;

        [Column("engelli_durumu")]
        public bool EngelliDurumu { get; set; } = false;

        // Temel Maaş Bilgileri
        [Required]
        [Column("brut_maas", TypeName = "decimal(12,2)")]
        public decimal BrutMaas { get; set; } // Temel brüt maaş

        [Column("toplam_odeme", TypeName = "decimal(12,2)")]
        public decimal ToplamOdeme { get; set; } = 0; // Tüm ödemeler toplamı

        [Column("toplam_kesinti", TypeName = "decimal(12,2)")]
        public decimal ToplamKesinti { get; set; } = 0; // Tüm kesintiler toplamı

        [Required]
        [Column("net_ucret", TypeName = "decimal(12,2)")]
        public decimal NetUcret { get; set; } // Net ödeme tutarı

        // SGK Hesaplamaları
        [Column("sgk_matrahi", TypeName = "decimal(12,2)")]
        public decimal SgkMatrahi { get; set; } = 0;

        [Column("sgk_isci_payi", TypeName = "decimal(12,2)")]
        public decimal SgkIsciPayi { get; set; } = 0;

        [Column("sgk_isveren_payi", TypeName = "decimal(12,2)")]
        public decimal SgkIsverenPayi { get; set; } = 0;

        [Column("issizlik_isci_payi", TypeName = "decimal(12,2)")]
        public decimal IssizlikIsciPayi { get; set; } = 0;

        [Column("issizlik_isveren_payi", TypeName = "decimal(12,2)")]
        public decimal IssizlikIsverenPayi { get; set; } = 0;

        // Vergi Hesaplamaları
        [Column("gelir_vergisi_matrahi", TypeName = "decimal(12,2)")]
        public decimal GelirVergisiMatrahi { get; set; } = 0;

        [Column("gelir_vergisi", TypeName = "decimal(12,2)")]
        public decimal GelirVergisi { get; set; } = 0;

        [Column("damga_vergisi", TypeName = "decimal(12,2)")]
        public decimal DamgaVergisi { get; set; } = 0;

        // AGI (Asgari Geçim İndirimi)
        [Column("agi_tutari", TypeName = "decimal(12,2)")]
        public decimal AgiTutari { get; set; } = 0;

        [Column("agi_orani", TypeName = "decimal(5,2)")]
        public decimal AgiOrani { get; set; } = 0;

        // Kümülatif Vergi (Yıllık Toplam)
        [Column("kumulatif_gelir", TypeName = "decimal(15,2)")]
        public decimal KumulatifGelir { get; set; } = 0; // Yıl başından bu aya kadar toplam gelir

        [Column("kumulatif_vergi", TypeName = "decimal(15,2)")]
        public decimal KumulatifVergi { get; set; } = 0; // Yıl başından bu aya kadar toplam vergi

        // İşveren Maliyeti
        [Column("isveren_maliyeti", TypeName = "decimal(12,2)")]
        public decimal IsverenMaliyeti { get; set; } = 0; // Toplam işveren maliyeti

        // Ödeme Bilgileri
        [Column("odeme_sekli")]
        [MaxLength(50)]
        public string? OdemeSekli { get; set; } = "Banka"; // Banka, Nakit, Çek

        [Column("odeme_tarihi")]
        public DateTime? OdemeTarihi { get; set; }

        [Column("odeme_durumu")]
        [MaxLength(20)]
        public string OdemeDurumu { get; set; } = "Beklemede"; // Beklemede, Odendi, Iptal

        // Bordro Durumu
        [Column("bordro_durumu")]
        [MaxLength(20)]
        public string BordroDurumu { get; set; } = "Taslak"; // Taslak, Onayda, Onaylandi, Odendi, Iptal

        [Column("onay_durumu")]
        [MaxLength(20)]
        public string OnayDurumu { get; set; } = "Beklemede"; // Beklemede, Onaylandi, Reddedildi

        // Notlar
        [Column("aciklama")]
        public string? Aciklama { get; set; }

        [Column("ozel_notlar")]
        public string? OzelNotlar { get; set; }

        // Audit
        [Column("aktif")]
        public bool Aktif { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("created_by")]
        public int? CreatedBy { get; set; }

        [Column("hesaplama_tarihi")]
        public DateTime? HesaplamaTarihi { get; set; }

        // İlişkiler
        [ForeignKey("PersonelId")]
        public virtual Personel Personel { get; set; } = null!;

        [ForeignKey("PuantajId")]
        public virtual Puantaj? Puantaj { get; set; }

        public virtual ICollection<BordroOdeme> BordroOdemeler { get; set; } = new List<BordroOdeme>();
        public virtual ICollection<BordroKesinti> BordroKesintiler { get; set; } = new List<BordroKesinti>();
        public virtual ICollection<BordroOnay> BordroOnaylar { get; set; } = new List<BordroOnay>();
    }
}
