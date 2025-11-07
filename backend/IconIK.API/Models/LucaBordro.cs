using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IconIK.API.Models
{
    [Table("luca_bordrolar")]
    public class LucaBordro
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("personel_id")]
        public int? PersonelId { get; set; }

        [Required]
        [Column("tc_kimlik")]
        [MaxLength(11)]
        public string TcKimlik { get; set; } = string.Empty;

        [Column("sicil_no")]
        [MaxLength(50)]
        public string? SicilNo { get; set; }

        [Column("ad_soyad")]
        [MaxLength(200)]
        public string? AdSoyad { get; set; }

        [Required]
        [Column("donem_yil")]
        public int DonemYil { get; set; }

        [Required]
        [Column("donem_ay")]
        public int DonemAy { get; set; }

        [Column("bordro_no")]
        [MaxLength(50)]
        public string? BordroNo { get; set; }

        [Column("brut_maas")]
        public decimal? BrutMaas { get; set; }

        [Column("net_ucret")]
        public decimal? NetUcret { get; set; }

        [Column("toplam_odeme")]
        public decimal? ToplamOdeme { get; set; }

        [Column("toplam_kesinti")]
        public decimal? ToplamKesinti { get; set; }

        [Column("sgk_isci")]
        public decimal? SgkIsci { get; set; }

        [Column("gelir_vergisi")]
        public decimal? GelirVergisi { get; set; }

        [Column("damga_vergisi")]
        public decimal? DamgaVergisi { get; set; }

        [Column("detay_json")]
        [MaxLength]
        public string? DetayJson { get; set; } // Luca'nın orijinal verisi

        [Column("senkron_tarihi")]
        public DateTime SenkronTarihi { get; set; } = DateTime.UtcNow;

        [Column("durum")]
        [MaxLength(20)]
        public string Durum { get; set; } = "Aktif"; // Aktif, Gecmis

        [Column("aktif")]
        public bool Aktif { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("PersonelId")]
        public virtual Personel? Personel { get; set; }
    }
}
