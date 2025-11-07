using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IconIK.API.Models
{
    [Table("kullanicilar")]
    public class Kullanici
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("personel_id")]
        public int PersonelId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("kullanici_adi")]
        public string KullaniciAdi { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column("sifre_hash")]
        public string SifreHash { get; set; } = string.Empty;

        [Column("ilk_giris")]
        public bool IlkGiris { get; set; } = true;

        [Column("son_giris_tarihi")]
        public DateTime? SonGirisTarihi { get; set; }

        [Column("kvkk_onaylandi")]
        public bool KVKKOnaylandi { get; set; } = false;

        [Column("kvkk_onay_tarihi")]
        public DateTime? KVKKOnayTarihi { get; set; }

        [Column("aktif")]
        public bool Aktif { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("PersonelId")]
        public virtual Personel Personel { get; set; } = null!;
    }
}