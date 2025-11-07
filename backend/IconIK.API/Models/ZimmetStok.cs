using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IconIK.API.Models
{
    [Table("zimmet_stok")]
    public class ZimmetStok
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("malzeme_adi")]
        [StringLength(200)]
        [Required]
        public string MalzemeAdi { get; set; } = string.Empty;

        [Column("kategori")]
        [StringLength(100)]
        public string? Kategori { get; set; }

        [Column("marka")]
        [StringLength(100)]
        public string? Marka { get; set; }

        [Column("model")]
        [StringLength(100)]
        public string? Model { get; set; }

        [Column("seri_no")]
        [StringLength(100)]
        public string? SeriNo { get; set; }

        [Column("miktar")]
        public int Miktar { get; set; } = 1;

        [Column("kalan_miktar")]
        public int KalanMiktar { get; set; } = 1;

        [Column("birim")]
        [StringLength(20)]
        public string? Birim { get; set; } = "Adet";

        [Column("aciklama")]
        [StringLength(500)]
        public string? Aciklama { get; set; }

        [Column("onay_durumu")]
        [StringLength(20)]
        public string OnayDurumu { get; set; } = "Bekliyor"; // Bekliyor, Onaylandi, Reddedildi

        [Column("onaylayan_id")]
        public int? OnaylayanId { get; set; }

        [Column("onay_tarihi")]
        public DateTime? OnayTarihi { get; set; }

        [Column("onay_notu")]
        [StringLength(500)]
        public string? OnayNotu { get; set; }

        [Column("aktif")]
        public bool Aktif { get; set; } = true;

        [Column("olusturan_id")]
        public int? OlusturanId { get; set; }

        [Column("olusturma_tarihi")]
        public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;

        [Column("guncelleme_tarihi")]
        public DateTime? GuncellemeTarihi { get; set; }

        [ForeignKey("OnaylayanId")]
        public virtual Personel? Onaylayan { get; set; }

        [ForeignKey("OlusturanId")]
        public virtual Personel? Olusturan { get; set; }
    }
}