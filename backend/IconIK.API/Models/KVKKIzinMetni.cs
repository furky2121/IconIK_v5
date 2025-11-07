using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IconIK.API.Models
{
    [Table("kvkk_izin_metinleri")]
    public class KVKKIzinMetni
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("baslik")]
        [MaxLength(200)]
        public string Baslik { get; set; } = string.Empty;

        [Required]
        [Column("metin")]
        public string Metin { get; set; } = string.Empty;

        [Column("aktif")]
        public bool Aktif { get; set; } = true;

        [Column("versiyon")]
        public int Versiyon { get; set; } = 1;

        [Column("yayinlanma_tarihi")]
        public DateTime? YayinlanmaTarihi { get; set; }

        [Column("olusturan_personel_id")]
        public int? OlusturanPersonelId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("OlusturanPersonelId")]
        public virtual Personel? OlusturanPersonel { get; set; }
    }
}
