using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IconIK.API.Models
{
    [Table("zimmet_stok_dosyalar")]
    public class ZimmetStokDosya
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("zimmet_stok_id")]
        [Required]
        public int ZimmetStokId { get; set; }

        [Column("dosya_adi")]
        [Required]
        [StringLength(255)]
        public string DosyaAdi { get; set; } = string.Empty;

        [Column("orijinal_adi")]
        [Required]
        [StringLength(255)]
        public string OrijinalAdi { get; set; } = string.Empty;

        [Column("dosya_yolu")]
        [Required]
        [StringLength(500)]
        public string DosyaYolu { get; set; } = string.Empty;

        [Column("dosya_boyutu")]
        public long DosyaBoyutu { get; set; }

        [Column("mime_tipi")]
        [StringLength(100)]
        public string MimeTipi { get; set; } = string.Empty;

        [Column("olusturma_tarihi")]
        public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;

        [ForeignKey("ZimmetStokId")]
        public virtual ZimmetStok ZimmetStok { get; set; } = null!;
    }
}