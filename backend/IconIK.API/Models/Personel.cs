using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IconIK.API.Models
{
    [Table("personeller")]
    public class Personel
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(11)]
        [Column("tc_kimlik")]
        public string TcKimlik { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("ad")]
        public string Ad { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("soyad")]
        public string Soyad { get; set; } = string.Empty;

        [MaxLength(100)]
        [Column("email")]
        public string? Email { get; set; }

        [MaxLength(20)]
        [Column("telefon")]
        public string? Telefon { get; set; }

        [Column("dogum_tarihi")]
        public DateTime? DogumTarihi { get; set; }

        [Required]
        [Column("ise_baslama_tarihi")]
        public DateTime IseBaslamaTarihi { get; set; }

        [Column("cikis_tarihi")]
        public DateTime? CikisTarihi { get; set; }

        [Column("pozisyon_id")]
        public int PozisyonId { get; set; }

        [Column("yonetici_id")]
        public int? YoneticiId { get; set; }

        [Column("maas", TypeName = "decimal(10,2)")]
        public decimal? Maas { get; set; }

        [MaxLength(255)]
        [Column("fotograf_url")]
        public string? FotografUrl { get; set; }

        [Column("adres")]
        public string? Adres { get; set; }

        [MaxLength(20)]
        [Column("medeni_hal")]
        public string? MedeniHal { get; set; }

        [MaxLength(10)]
        [Column("cinsiyet")]
        public string? Cinsiyet { get; set; }

        [MaxLength(30)]
        [Column("askerlik_durumu")]
        public string? AskerlikDurumu { get; set; }

        [MaxLength(50)]
        [Column("egitim_durumu")]
        public string? EgitimDurumu { get; set; }

        [MaxLength(10)]
        [Column("kan_grubu")]
        public string? KanGrubu { get; set; }

        [MaxLength(50)]
        [Column("ehliyet_sinifi")]
        public string? EhliyetSinifi { get; set; }

        [MaxLength(100)]
        [Column("anne_adi")]
        public string? AnneAdi { get; set; }

        [MaxLength(100)]
        [Column("baba_adi")]
        public string? BabaAdi { get; set; }

        [MaxLength(100)]
        [Column("dogum_yeri")]
        public string? DogumYeri { get; set; }

        [MaxLength(10)]
        [Column("nufus_il_kod")]
        public string? NufusIlKod { get; set; }

        [MaxLength(10)]
        [Column("nufus_ilce_kod")]
        public string? NufusIlceKod { get; set; }

        [Column("acil_durum_iletisim")]
        public string? AcilDurumIletisim { get; set; }

        [MaxLength(50)]
        [Column("banka_hesap_no")]
        public string? BankaHesapNo { get; set; }

        [MaxLength(34)]
        [Column("iban_no")]
        public string? IbanNo { get; set; }

        [Column("aktif")]
        public bool Aktif { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("PozisyonId")]
        public virtual Pozisyon Pozisyon { get; set; } = null!;

        [ForeignKey("YoneticiId")]
        public virtual Personel? Yonetici { get; set; }

        public virtual ICollection<Personel> AltCalisanlar { get; set; } = new List<Personel>();
        
        public virtual Kullanici? Kullanici { get; set; }

        public virtual ICollection<IzinTalebi> IzinTalepleri { get; set; } = new List<IzinTalebi>();
        
        public virtual ICollection<IzinTalebi> OnayladigiIzinler { get; set; } = new List<IzinTalebi>();

        public virtual ICollection<PersonelEgitimi> PersonelEgitimleri { get; set; } = new List<PersonelEgitimi>();
    }
}