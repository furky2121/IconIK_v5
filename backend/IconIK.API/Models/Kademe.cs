using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace IconIK.API.Models
{
    [Table("kademeler")]
    public class Kademe
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("ad")]
        public string Ad { get; set; } = string.Empty;

        [Required]
        [Column("seviye")]
        public int Seviye { get; set; }

        [Column("aktif")]
        public bool Aktif { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public virtual ICollection<Pozisyon> Pozisyonlar { get; set; } = new List<Pozisyon>();
    }
}