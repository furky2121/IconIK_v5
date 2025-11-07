using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IconIK.API.Models
{
    [Table("izin_talepleri")]
    public class IzinTalebi
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("personel_id")]
        public int PersonelId { get; set; }

        [Required]
        [Column("izin_baslama_tarihi")]
        public DateTime IzinBaslamaTarihi { get; set; }

        [Required]
        [Column("isbasi_tarihi")]
        public DateTime IsbasiTarihi { get; set; }

        [Required]
        [Column("gun_sayisi")]
        public int GunSayisi { get; set; }

        [MaxLength(50)]
        [Column("izin_tipi")]
        public string IzinTipi { get; set; } = "Yıllık İzin";

        [Column("aciklama")]
        public string? Aciklama { get; set; }

        [MaxLength(20)]
        [Column("durum")]
        public string Durum { get; set; } = "Beklemede";

        [Column("onaylayan_id")]
        public int? OnaylayanId { get; set; }

        [Column("onay_tarihi")]
        public DateTime? OnayTarihi { get; set; }

        [Column("onay_notu")]
        public string? OnayNotu { get; set; }

        [MaxLength(100)]
        [Column("gorev_yeri")]
        public string? GorevYeri { get; set; }

        [MaxLength(500)]
        [Column("rapor_dosya_yolu")]
        public string? RaporDosyaYolu { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("PersonelId")]
        public virtual Personel Personel { get; set; } = null!;

        [ForeignKey("OnaylayanId")]
        public virtual Personel? Onaylayan { get; set; }
    }
}