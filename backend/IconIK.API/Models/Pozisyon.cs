using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace IconIK.API.Models
{
    [Table("pozisyonlar")]
    public class Pozisyon
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("ad")]
        public string Ad { get; set; } = string.Empty;

        [Column("departman_id")]
        public int DepartmanId { get; set; }

        [Column("kademe_id")]
        public int KademeId { get; set; }

        [Column("min_maas", TypeName = "decimal(10,2)")]
        public decimal? MinMaas { get; set; }

        [Column("max_maas", TypeName = "decimal(10,2)")]
        public decimal? MaxMaas { get; set; }

        [Column("aciklama")]
        public string? Aciklama { get; set; }

        [Column("aktif")]
        public bool Aktif { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("DepartmanId")]
        [JsonIgnore]
        public virtual Departman Departman { get; set; } = null!;

        [ForeignKey("KademeId")]
        [JsonIgnore]
        public virtual Kademe Kademe { get; set; } = null!;

        [JsonIgnore]
        public virtual ICollection<Personel> Personeller { get; set; } = new List<Personel>();
    }
}