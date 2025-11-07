using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IconIK.API.Models
{
    [Table("egitimler")]
    public class Egitim
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("baslik")]
        public string Baslik { get; set; } = string.Empty;

        [Column("aciklama")]
        public string? Aciklama { get; set; }

        [Required]
        [Column("baslangic_tarihi")]
        public DateTime BaslangicTarihi { get; set; }

        [Required]
        [Column("bitis_tarihi")]
        public DateTime BitisTarihi { get; set; }

        [Column("sure_saat")]
        public int? SureSaat { get; set; }

        [MaxLength(100)]
        [Column("egitmen")]
        public string? Egitmen { get; set; }

        [MaxLength(200)]
        [Column("konum")]
        public string? Konum { get; set; }

        [Column("kapasite")]
        public int? Kapasite { get; set; }

        [MaxLength(20)]
        [Column("durum")]
        public string Durum { get; set; } = "Planlandı";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<PersonelEgitimi> PersonelEgitimleri { get; set; } = new List<PersonelEgitimi>();
    }
}