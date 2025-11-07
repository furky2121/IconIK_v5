using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IconIK.API.Models
{
    [Table("istifa_talepleri")]
    public class IstifaTalebi
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("personel_id")]
        public int PersonelId { get; set; }

        [Required]
        [Column("istifa_tarihi")]
        public DateTime IstifaTarihi { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("son_calisma_tarihi")]
        public DateTime SonCalismaTarihi { get; set; }

        [MaxLength(500)]
        [Column("istifa_nedeni")]
        public string? IstifaNedeni { get; set; }

        [MaxLength(20)]
        [Column("onay_durumu")]
        public string OnayDurumu { get; set; } = "Beklemede";

        [Column("onaylayan_id")]
        public int? OnaylayanId { get; set; }

        [Column("onay_tarihi")]
        public DateTime? OnayTarihi { get; set; }

        [MaxLength(500)]
        [Column("onay_notu")]
        public string? OnayNotu { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("PersonelId")]
        public virtual Personel Personel { get; set; } = null!;

        [ForeignKey("OnaylayanId")]
        public virtual Personel? Onaylayan { get; set; }
    }
}