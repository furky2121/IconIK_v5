using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IconIK.API.Models
{
    [Table("personel_giris_cikis")]
    public class PersonelGirisCikis
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("personel_id")]
        public int PersonelId { get; set; }

        [Column("giris_tarihi")]
        public DateTime GirisTarihi { get; set; }

        [Column("cikis_tarihi")]
        public DateTime? CikisTarihi { get; set; }

        [Column("giris_tipi")]
        [MaxLength(50)]
        public string GirisTipi { get; set; } = "Normal"; // Normal, Fazla Mesai, Hafta Sonu

        [Column("aciklama")]
        [MaxLength(500)]
        public string? Aciklama { get; set; }

        [Column("calisma_suresi_dakika")]
        public int? CalismaSuresiDakika { get; set; }

        [Column("gec_kalma_dakika")]
        public int GecKalmaDakika { get; set; } = 0;

        [Column("erken_cikma_dakika")]
        public int ErkenCikmaDakika { get; set; } = 0;

        [Column("aktif")]
        public bool Aktif { get; set; } = true;

        [Column("olusturma_tarihi")]
        public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;

        [Column("guncelleme_tarihi")]
        public DateTime? GuncellemeTarihi { get; set; }

        // Navigation Properties
        [ForeignKey("PersonelId")]
        public virtual Personel Personel { get; set; }
    }
}