using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IconIK.API.Models
{
    [Table("otp_kodlar")]
    public class OtpKod
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("kullanici_id")]
        public int KullaniciId { get; set; }

        [Required]
        [Column("email")]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Column("otp_kodu")]
        [MaxLength(6)]
        public string OtpKodu { get; set; } = string.Empty;

        [Column("luca_bordro_id")]
        public int? LucaBordroId { get; set; }

        [Column("olusturma_tarihi")]
        public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;

        [Column("gecerlilik_suresi")]
        public int GecerlilikSuresi { get; set; } = 5; // dakika

        [Column("kullanildi")]
        public bool Kullanildi { get; set; } = false;

        [Column("kullanim_tarihi")]
        public DateTime? KullanimTarihi { get; set; }

        [Column("deneme_sayisi")]
        public int DenemeSayisi { get; set; } = 0;

        [ForeignKey("KullaniciId")]
        public virtual Kullanici Kullanici { get; set; } = null!;

        [ForeignKey("LucaBordroId")]
        public virtual LucaBordro? LucaBordro { get; set; }
    }
}
