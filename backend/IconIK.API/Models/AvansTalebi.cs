using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IconIK.API.Models
{
    [Table("avans_talepleri")]
    public class AvansTalebi
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("personel_id")]
        public int PersonelId { get; set; }

        [Required]
        [Column("talep_tarihi")]
        public DateTime TalepTarihi { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("talep_tutari")]
        public decimal TalepTutari { get; set; }

        [MaxLength(500)]
        [Column("aciklama")]
        public string? Aciklama { get; set; }

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