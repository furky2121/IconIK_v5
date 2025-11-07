using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IconIK.API.Models
{
    /// <summary>
    /// Bordro onay süreçleri ve geçmişi
    /// </summary>
    [Table("bordro_onaylar")]
    public class BordroOnay
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("bordro_id")]
        public int BordroId { get; set; }

        [Required]
        [Column("onay_seviyesi")]
        public int OnaySeviyesi { get; set; } // 1: Yönetici, 2: İK, 3: Finans, 4: Genel Müdür

        [Required]
        [MaxLength(100)]
        [Column("onay_seviye_adi")]
        public string OnaySeviyeAdi { get; set; } = string.Empty; // Yönetici Onayı, İK Onayı vb.

        [Column("onaylayan_id")]
        public int? OnaylayanId { get; set; }

        [Column("onaylayan_ad_soyad")]
        [MaxLength(200)]
        public string? OnaylayanAdSoyad { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("onay_durumu")]
        public string OnayDurumu { get; set; } = "Beklemede"; // Beklemede, Onaylandi, Reddedildi, Iptal

        [Column("onay_tarihi")]
        public DateTime? OnayTarihi { get; set; }

        [Column("red_nedeni")]
        [MaxLength(1000)]
        public string? RedNedeni { get; set; }

        [Column("onay_notu")]
        [MaxLength(1000)]
        public string? OnayNotu { get; set; }

        // Email bildirimi
        [Column("email_gonderildi")]
        public bool EmailGonderildi { get; set; } = false;

        [Column("email_gonderim_tarihi")]
        public DateTime? EmailGonderimTarihi { get; set; }

        // Audit
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // İlişkiler
        [ForeignKey("BordroId")]
        public virtual BordroAna Bordro { get; set; } = null!;

        [ForeignKey("OnaylayanId")]
        public virtual Personel? Onaylayan { get; set; }
    }
}
