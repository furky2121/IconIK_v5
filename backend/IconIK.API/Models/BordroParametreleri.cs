using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IconIK.API.Models
{
    /// <summary>
    /// Bordro hesaplamalarında kullanılan parametreler (2025 Türkiye mevzuatı)
    /// </summary>
    [Table("bordro_parametreleri")]
    public class BordroParametreleri
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("yil")]
        public int Yil { get; set; }

        [Required]
        [Column("donem")]
        public int Donem { get; set; } // 1-12 arası ay

        // Asgari Ücret ve AGI
        [Required]
        [Column("asgari_ucret_brut", TypeName = "decimal(10,2)")]
        public decimal AsgariUcretBrut { get; set; } = 20002.50m; // 2025

        [Required]
        [Column("asgari_ucret_net", TypeName = "decimal(10,2)")]
        public decimal AsgariUcretNet { get; set; } = 17002.12m; // 2025

        [Required]
        [Column("agi_orani", TypeName = "decimal(5,2)")]
        public decimal AgiOrani { get; set; } = 15.00m; // %15

        [Required]
        [Column("agi_tutari", TypeName = "decimal(10,2)")]
        public decimal AgiTutari { get; set; } = 2140.20m; // 2025

        // SGK Parametreleri
        [Required]
        [Column("sgk_isci_orani", TypeName = "decimal(5,2)")]
        public decimal SgkIsciOrani { get; set; } = 14.00m; // %14

        [Required]
        [Column("sgk_isveren_orani", TypeName = "decimal(5,2)")]
        public decimal SgkIsverenOrani { get; set; } = 20.50m; // %20.5

        [Required]
        [Column("sgk_tavan_brut", TypeName = "decimal(10,2)")]
        public decimal SgkTavanBrut { get; set; } = 147073.50m; // 2025

        [Required]
        [Column("sgk_taban_brut", TypeName = "decimal(10,2)")]
        public decimal SgkTabanBrut { get; set; } = 20002.50m; // 2025

        // İşsizlik Sigortası
        [Required]
        [Column("issizlik_isci_orani", TypeName = "decimal(5,2)")]
        public decimal IssizlikIsciOrani { get; set; } = 1.00m; // %1

        [Required]
        [Column("issizlik_isveren_orani", TypeName = "decimal(5,2)")]
        public decimal IssizlikIsverenOrani { get; set; } = 2.00m; // %2

        // Damga Vergisi
        [Required]
        [Column("damga_vergisi_orani", TypeName = "decimal(5,4)")]
        public decimal DamgaVergisiOrani { get; set; } = 0.759m; // %0.759

        // Gelir Vergisi Dilimleri (2025)
        [Column("vergi_dilim1_ust_sinir", TypeName = "decimal(10,2)")]
        public decimal VergiDilim1UstSinir { get; set; } = 110000m;

        [Column("vergi_dilim1_oran", TypeName = "decimal(5,2)")]
        public decimal VergiDilim1Oran { get; set; } = 15.00m; // %15

        [Column("vergi_dilim2_ust_sinir", TypeName = "decimal(10,2)")]
        public decimal VergiDilim2UstSinir { get; set; } = 230000m;

        [Column("vergi_dilim2_oran", TypeName = "decimal(5,2)")]
        public decimal VergiDilim2Oran { get; set; } = 20.00m; // %20

        [Column("vergi_dilim3_ust_sinir", TypeName = "decimal(10,2)")]
        public decimal VergiDilim3UstSinir { get; set; } = 870000m;

        [Column("vergi_dilim3_oran", TypeName = "decimal(5,2)")]
        public decimal VergiDilim3Oran { get; set; } = 27.00m; // %27

        [Column("vergi_dilim4_ust_sinir", TypeName = "decimal(10,2)")]
        public decimal VergiDilim4UstSinir { get; set; } = 3000000m;

        [Column("vergi_dilim4_oran", TypeName = "decimal(5,2)")]
        public decimal VergiDilim4Oran { get; set; } = 35.00m; // %35

        [Column("vergi_dilim5_oran", TypeName = "decimal(5,2)")]
        public decimal VergiDilim5Oran { get; set; } = 40.00m; // %40 (3M üzeri)

        // Asgari Geçim İndirimi Oranları
        [Column("agi_bekar_oran", TypeName = "decimal(5,2)")]
        public decimal AgiBekarOran { get; set; } = 50.00m; // %50

        [Column("agi_evli_oran", TypeName = "decimal(5,2)")]
        public decimal AgiEvliOran { get; set; } = 60.00m; // %60

        [Column("agi_cocuk1_oran", TypeName = "decimal(5,2)")]
        public decimal AgiCocuk1Oran { get; set; } = 70.00m; // %70 (1 çocuk)

        [Column("agi_cocuk2_oran", TypeName = "decimal(5,2)")]
        public decimal AgiCocuk2Oran { get; set; } = 80.00m; // %80 (2 çocuk)

        [Column("agi_cocuk3_oran", TypeName = "decimal(5,2)")]
        public decimal AgiCocuk3Oran { get; set; } = 90.00m; // %90 (3+ çocuk)

        // Mesai Parametreleri
        [Column("normal_mesai_carpan", TypeName = "decimal(5,2)")]
        public decimal NormalMesaiCarpan { get; set; } = 1.00m;

        [Column("hafta_ici_mesai_carpan", TypeName = "decimal(5,2)")]
        public decimal HaftaIciMesaiCarpan { get; set; } = 1.50m; // %150

        [Column("hafta_sonu_mesai_carpan", TypeName = "decimal(5,2)")]
        public decimal HaftaSonuMesaiCarpan { get; set; } = 2.00m; // %200

        [Column("gece_mesai_carpan", TypeName = "decimal(5,2)")]
        public decimal GeceMesaiCarpan { get; set; } = 1.25m; // %125

        [Column("resmi_tatil_carpan", TypeName = "decimal(5,2)")]
        public decimal ResmiTatilCarpan { get; set; } = 2.00m; // %200

        // Yıllık İzin Parametreleri
        [Column("yillik_izin_1_5_yil", TypeName = "int")]
        public int YillikIzin1_5Yil { get; set; } = 14; // gün

        [Column("yillik_izin_5_15_yil", TypeName = "int")]
        public int YillikIzin5_15Yil { get; set; } = 20; // gün

        [Column("yillik_izin_15_yil_ustu", TypeName = "int")]
        public int YillikIzin15YilUstu { get; set; } = 26; // gün

        // Kıdem Tazminatı
        [Column("kidem_tavan", TypeName = "decimal(10,2)")]
        public decimal KidemTavan { get; set; } = 52734.72m; // 2025 tavan

        // Audit
        [Column("aktif")]
        public bool Aktif { get; set; } = true;

        [Column("aciklama")]
        [MaxLength(500)]
        public string? Aciklama { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("created_by")]
        public int? CreatedBy { get; set; }
    }
}
