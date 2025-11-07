using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IconIK.API.Models
{
    [Table("izin_tipleri")]
    public class IzinTipi
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("izin_tipi_adi")]
        public string IzinTipiAdi { get; set; } = string.Empty;

        [Column("standart_gun_sayisi")]
        public int? StandartGunSayisi { get; set; }

        [Column("minimum_gun_sayisi")]
        public int? MinimumGunSayisi { get; set; }

        [Column("maksimum_gun_sayisi")]
        public int? MaksimumGunSayisi { get; set; }

        [MaxLength(20)]
        [Column("cinsiyet_kisiti")]
        public string? CinsiyetKisiti { get; set; } // null, "Erkek", "Kadın"

        [Column("rapor_gerekli")]
        public bool RaporGerekli { get; set; } = false;

        [Column("ucretli_mi")]
        public bool UcretliMi { get; set; } = true;

        [MaxLength(20)]
        [Column("renk")]
        public string? Renk { get; set; } // Takvim görünümü için renk kodu

        [Column("aciklama")]
        public string? Aciklama { get; set; }

        [Column("sira")]
        public int Sira { get; set; } = 0;

        [Column("aktif")]
        public bool Aktif { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation property - Bu izin tipini kullanan talepler
        public virtual ICollection<IzinTalebi>? IzinTalepleri { get; set; }
    }
}
