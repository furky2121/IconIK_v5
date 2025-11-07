using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IconIK.API.Models
{
    [Table("Bildirimler")]
    public class Bildirim
    {
        [Key]
        public int Id { get; set; }

        public int AliciId { get; set; } // Bildirimi alan personel

        [Required]
        [StringLength(200)]
        public string Baslik { get; set; }

        [Required]
        [StringLength(1000)]
        public string Mesaj { get; set; }

        [StringLength(50)]
        public string Kategori { get; set; } = "sistem"; // izin, egitim, dogum_gunu, sistem, avans, istifa, masraf, duyuru, anket

        [StringLength(20)]
        public string Tip { get; set; } = "info"; // info, success, warning, error

        public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;

        public bool Okundu { get; set; } = false;

        public DateTime? OkunmaTarihi { get; set; }

        [StringLength(100)]
        public string? GonderenAd { get; set; }

        [StringLength(200)]
        public string? ActionUrl { get; set; }

        // Navigation properties
        [ForeignKey("AliciId")]
        public virtual Personel Alici { get; set; }
    }
}
