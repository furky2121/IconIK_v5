using System.ComponentModel.DataAnnotations;

namespace IconIK.API.Models
{
    public class KademeEkranYetkisi
    {
        public int Id { get; set; }
        
        [Required]
        public int KademeId { get; set; }
        
        [Required]
        public int EkranYetkisiId { get; set; }
        
        public bool OkumaYetkisi { get; set; } = true;
        public bool YazmaYetkisi { get; set; } = false;
        public bool SilmeYetkisi { get; set; } = false;
        public bool GuncellemeYetkisi { get; set; } = false;
        
        public bool Aktif { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual Kademe Kademe { get; set; } = null!;
        public virtual EkranYetkisi EkranYetkisi { get; set; } = null!;
    }
}