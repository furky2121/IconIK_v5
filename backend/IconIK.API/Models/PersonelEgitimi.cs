using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IconIK.API.Models
{
    [Table("personel_egitimleri")]
    public class PersonelEgitimi
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("personel_id")]
        public int PersonelId { get; set; }

        [Column("egitim_id")]
        public int EgitimId { get; set; }

        [MaxLength(20)]
        [Column("katilim_durumu")]
        public string KatilimDurumu { get; set; } = "Atandı";

        [Column("puan")]
        public int? Puan { get; set; }

        [MaxLength(255)]
        [Column("sertifika_url")]
        public string? SertifikaUrl { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("PersonelId")]
        public virtual Personel Personel { get; set; } = null!;

        [ForeignKey("EgitimId")]
        public virtual Egitim Egitim { get; set; } = null!;
    }
}