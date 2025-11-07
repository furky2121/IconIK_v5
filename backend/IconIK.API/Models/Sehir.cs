using System.ComponentModel.DataAnnotations;

namespace IconIK.API.Models
{
    public class Sehir
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string SehirAd { get; set; }

        public int PlakaKodu { get; set; }

        public bool Aktif { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}