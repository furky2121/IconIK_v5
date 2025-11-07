using System.ComponentModel.DataAnnotations;

namespace IconIK.API.Models
{
    public class EkranYetkisi
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string EkranAdi { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string EkranKodu { get; set; } = string.Empty; // personeller, departmanlar, etc.
        
        [StringLength(200)]
        public string? Aciklama { get; set; }
        
        public bool Aktif { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        public virtual ICollection<KademeEkranYetkisi> KademeYetkileri { get; set; } = new List<KademeEkranYetkisi>();
    }
}