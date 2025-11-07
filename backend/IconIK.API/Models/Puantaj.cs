using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IconIK.API.Models
{
    /// <summary>
    /// Aylık personel puantaj kayıtları (çalışma günleri, izinler, mesai)
    /// </summary>
    [Table("puantajlar")]
    public class Puantaj
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("personel_id")]
        public int PersonelId { get; set; }

        [Required]
        [Column("donem_yil")]
        public int DonemYil { get; set; }

        [Required]
        [Column("donem_ay")]
        public int DonemAy { get; set; } // 1-12

        // Çalışma Günleri
        [Column("calisan_gun_sayisi")]
        public int CalisanGunSayisi { get; set; } = 0; // Normal çalışılan gün sayısı

        [Column("hafta_sonu_calisma")]
        public int HaftaSonuCalisma { get; set; } = 0; // Hafta sonu çalışma günü

        [Column("resmi_tatil_calisma")]
        public int ResmiTatilCalisma { get; set; } = 0; // Resmi tatil çalışma günü

        // İzin Günleri
        [Column("yillik_izin")]
        public int YillikIzin { get; set; } = 0; // Yıllık izin günü

        [Column("ucretsiz_izin")]
        public int UcretsizIzin { get; set; } = 0; // Ücretsiz izin günü

        [Column("hastalik_izni")]
        public int HastalikIzni { get; set; } = 0; // Hastalık/Rapor izni

        [Column("mazeret_izni")]
        public int MazeretIzni { get; set; } = 0; // Mazeret izni (ölüm, evlenme, doğum vb.)

        // Mesai Saatleri
        [Column("hafta_ici_mesai_saat", TypeName = "decimal(6,2)")]
        public decimal HaftaIciMesaiSaat { get; set; } = 0; // Hafta içi fazla mesai (saat)

        [Column("hafta_sonu_mesai_saat", TypeName = "decimal(6,2)")]
        public decimal HaftaSonuMesaiSaat { get; set; } = 0; // Hafta sonu mesai (saat)

        [Column("gece_mesai_saat", TypeName = "decimal(6,2)")]
        public decimal GeceMesaiSaat { get; set; } = 0; // Gece mesaisi (saat)

        [Column("resmi_tatil_mesai_saat", TypeName = "decimal(6,2)")]
        public decimal ResmiTatilMesaiSaat { get; set; } = 0; // Resmi tatil mesai (saat)

        // Geç Gelme / Erken Çıkma
        [Column("gec_gelme_dakika")]
        public int GecGelmeDakika { get; set; } = 0; // Toplam geç gelme (dakika)

        [Column("erken_cikma_dakika")]
        public int ErkenCikmaDakika { get; set; } = 0; // Toplam erken çıkma (dakika)

        [Column("devamsizlik_gun")]
        public int DevamsizlikGun { get; set; } = 0; // Devamsızlık günü (mazeret yok)

        // Toplam Hesaplamalar
        [Column("toplam_calisilan_gun")]
        public int ToplamCalisilanGun { get; set; } = 0; // Tüm çalışılan günler toplamı

        [Column("toplam_mesai_saat", TypeName = "decimal(6,2)")]
        public decimal ToplamMesaiSaat { get; set; } = 0; // Tüm mesai saatleri toplamı

        // Notlar
        [Column("notlar")]
        public string? Notlar { get; set; }

        // Onay
        [Column("onay_durumu")]
        [MaxLength(20)]
        public string OnayDurumu { get; set; } = "Taslak"; // Taslak, Onayda, Onaylandi, Reddedildi

        [Column("onaylayan_id")]
        public int? OnaylayanId { get; set; }

        [Column("onay_tarihi")]
        public DateTime? OnayTarihi { get; set; }

        [Column("red_nedeni")]
        [MaxLength(500)]
        public string? RedNedeni { get; set; }

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
        [ForeignKey("PersonelId")]
        public virtual Personel Personel { get; set; } = null!;

        [ForeignKey("OnaylayanId")]
        public virtual Personel? Onaylayan { get; set; }

        public virtual ICollection<BordroAna> Bordrolar { get; set; } = new List<BordroAna>();
    }
}
