using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace IconIK.API.Models
{
    [Table("departmanlar")]
    public class Departman
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("ad")]
        public string Ad { get; set; } = string.Empty;

        [MaxLength(20)]
        [Column("kod")]
        public string? Kod { get; set; }

        [Column("aciklama")]
        public string? Aciklama { get; set; }

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