using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IconIK.API.Models
{
    [Table("personel_zimmet")]
    public class PersonelZimmet
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("personel_id")]
        [Required]
        public int PersonelId { get; set; }

        [Column("zimmet_stok_id")]
        [Required]
        public int ZimmetStokId { get; set; }

        [Column("zimmet_miktar")]
        [Required]
        public int ZimmetMiktar { get; set; } = 1;

        [Column("zimmet_tarihi")]
        [Required]
        public DateTime ZimmetTarihi { get; set; } = DateTime.UtcNow;

        [Column("iade_tarihi")]
        public DateTime? IadeTarihi { get; set; }

        [Column("durum")]
        [StringLength(20)]
        public string Durum { get; set; } = "Zimmetli"; // Zimmetli, Iade Edildi

        [Column("zimmet_notu")]
        [StringLength(500)]
        public string? ZimmetNotu { get; set; }

        [Column("iade_notu")]
        [StringLength(500)]
        public string? IadeNotu { get; set; }

        [Column("zimmet_veren_id")]
        public int? ZimmetVerenId { get; set; }

        [Column("iade_alan_id")]
        public int? IadeAlanId { get; set; }

        [Column("aktif")]
        public bool Aktif { get; set; } = true;

        [Column("olusturma_tarihi")]
        public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;

        [Column("guncelleme_tarihi")]
        public DateTime? GuncellemeTarihi { get; set; }

        [ForeignKey("PersonelId")]
        public virtual Personel Personel { get; set; } = null!;

        [ForeignKey("ZimmetStokId")]
        public virtual ZimmetStok ZimmetStok { get; set; } = null!;

        [ForeignKey("ZimmetVerenId")]
        public virtual Personel? ZimmetVeren { get; set; }

        [ForeignKey("IadeAlanId")]
        public virtual Personel? IadeAlan { get; set; }
    }
}