using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IconIK.API.Models
{
    public enum AdayDurumu
    {
        CVHavuzunda = 1,
        BasvuruYapildi = 2,
        CVInceleniyor = 3,
        MulakatPlanlandi = 4,
        MulakatTamamlandi = 5,
        ReferansKontrolu = 6,
        TeklifHazirlaniyor = 7,
        TeklifGonderildi = 8,
        TeklifOnayiBekleniyor = 9,
        IseBasladi = 10,
        Reddedildi = 11,
        AdayVazgecti = 12,
        KaraListe = 13
    }

    public enum BasvuruDurumu
    {
        YeniBasvuru = 1,
        CVInceleniyor = 2,
        Degerlendiriliyor = 3,
        MulakatBekleniyor = 4,
        MulakatTamamlandi = 5,
        ReferansKontrolu = 6,
        TeklifVerildi = 7,
        IseAlindi = 8,
        Reddedildi = 9,
        AdayVazgecti = 10
    }

    public enum MulakatTuru
    {
        HR = 1,
        Teknik = 2,
        Yonetici = 3,
        GenelMudur = 4,
        Video = 5
    }

    public enum IlanDurumu
    {
        Taslak = 1,
        Aktif = 2,
        Kapali = 3,
        Arsivlendi = 4
    }

    [Table("ilan_kategoriler")]
    public class IlanKategori
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("ad")]
        public string Ad { get; set; } = string.Empty;

        [MaxLength(500)]
        [Column("aciklama")]
        public string? Aciklama { get; set; }

        [Column("aktif")]
        public bool Aktif { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<IsIlani> IsIlanlari { get; set; } = new List<IsIlani>();
    }

    [Table("is_ilanlari")]
    public class IsIlani
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("baslik")]
        public string Baslik { get; set; } = string.Empty;

        [Column("kategori_id")]
        public int KategoriId { get; set; }

        [Column("pozisyon_id")]
        public int PozisyonId { get; set; }

        [Column("departman_id")]
        public int DepartmanId { get; set; }

        [Column("is_tanimi")]
        public string IsTanimi { get; set; } = string.Empty;

        [Column("gereksinimler")]
        public string Gereksinimler { get; set; } = string.Empty;

        [Column("min_maas")]
        public decimal? MinMaas { get; set; }

        [Column("max_maas")]
        public decimal? MaxMaas { get; set; }

        [Column("calisme_sekli")]
        [MaxLength(50)]
        public string? CalismaSekli { get; set; }

        [Column("deneyim_yili")]
        public int DeneyimYili { get; set; } = 0;

        [Column("egitim_seviyesi")]
        [MaxLength(50)]
        public string? EgitimSeviyesi { get; set; }

        [Column("yayin_tarihi")]
        public DateTime? YayinTarihi { get; set; }

        [Column("bitis_tarihi")]
        public DateTime? BitisTarihi { get; set; }

        [Column("durum")]
        public IlanDurumu Durum { get; set; } = IlanDurumu.Taslak;

        [Column("olusturan_id")]
        public int OlusturanId { get; set; }

        [Column("aktif")]
        public bool Aktif { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("KategoriId")]
        public virtual IlanKategori Kategori { get; set; } = null!;

        [ForeignKey("PozisyonId")]
        public virtual Pozisyon Pozisyon { get; set; } = null!;

        [ForeignKey("DepartmanId")]
        public virtual Departman Departman { get; set; } = null!;

        [ForeignKey("OlusturanId")]
        public virtual Personel Olusturan { get; set; } = null!;

        public virtual ICollection<Basvuru> Basvurular { get; set; } = new List<Basvuru>();
    }

    [Table("adaylar")]
    public class Aday
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("ad")]
        public string Ad { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("soyad")]
        public string Soyad { get; set; } = string.Empty;

        [Required]
        [MaxLength(11)]
        [Column("tc_kimlik")]
        public string TcKimlik { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        [Column("telefon")]
        public string? Telefon { get; set; }

        [Column("dogum_tarihi")]
        public DateTime? DogumTarihi { get; set; }

        [MaxLength(10)]
        [Column("cinsiyet")]
        public string? Cinsiyet { get; set; }

        [MaxLength(20)]
        [Column("medeni_durum")]
        public string? MedeniDurum { get; set; }

        [MaxLength(30)]
        [Column("askerlik_durumu")]
        public string? AskerlikDurumu { get; set; }

        [Column("adres")]
        public string? Adres { get; set; }

        [MaxLength(100)]
        [Column("sehir")]
        public string? Sehir { get; set; }

        [MaxLength(100)]
        [Column("universite")]
        public string? Universite { get; set; }

        [MaxLength(100)]
        [Column("bolum")]
        public string? Bolum { get; set; }

        [Column("mezuniyet_yili")]
        public int? MezuniyetYili { get; set; }

        [Column("toplam_deneyim")]
        public int ToplamDeneyim { get; set; } = 0;

        [MaxLength(255)]
        [Column("ozgecmis_dosyasi")]
        public string? OzgecmisDosyasi { get; set; }

        [Column("linkedin_url")]
        public string? LinkedinUrl { get; set; }

        [Column("notlar")]
        public string? Notlar { get; set; }

        [Column("durum")]
        public AdayDurumu Durum { get; set; } = AdayDurumu.CVHavuzunda;

        [Column("durum_guncelleme_tarihi")]
        public DateTime? DurumGuncellenmeTarihi { get; set; }

        [Column("durum_guncelleme_notu")]
        public string? DurumGuncellemeNotu { get; set; }

        [Column("otomatik_cv_olusturuldu")]
        public bool OtomatikCVOlusturuldu { get; set; } = false;

        [Column("cv_dosya_yolu")]
        public string? CVDosyaYolu { get; set; }

        [Column("fotograf_yolu")]
        public string? FotografYolu { get; set; }

        [Column("kara_liste")]
        public bool KaraListe { get; set; } = false;

        [Column("aktif")]
        public bool Aktif { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Basvuru> Basvurular { get; set; } = new List<Basvuru>();
        public virtual ICollection<AdayDeneyim> Deneyimler { get; set; } = new List<AdayDeneyim>();
        public virtual ICollection<AdayYetenek> Yetenekler { get; set; } = new List<AdayYetenek>();
        public virtual ICollection<AdayCV> CVler { get; set; } = new List<AdayCV>();
        public virtual ICollection<AdayDurumGecmisi> DurumGecmisi { get; set; } = new List<AdayDurumGecmisi>();
        public virtual ICollection<AdayEgitim> Egitimler { get; set; } = new List<AdayEgitim>();
        public virtual ICollection<AdaySertifika> Sertifikalar { get; set; } = new List<AdaySertifika>();
        public virtual ICollection<AdayReferans> Referanslar { get; set; } = new List<AdayReferans>();
        public virtual ICollection<AdayDil> Diller { get; set; } = new List<AdayDil>();
        public virtual ICollection<AdayProje> Projeler { get; set; } = new List<AdayProje>();
        public virtual ICollection<AdayHobi> Hobiler { get; set; } = new List<AdayHobi>();
    }

    [Table("aday_cv")]
    public class AdayCV
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("aday_id")]
        public int AdayId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("cv_tipi")]
        public string CVTipi { get; set; } = string.Empty; // "Otomatik", "Yuklenmiş"

        [Column("dosya_adi")]
        public string? DosyaAdi { get; set; }

        [Column("dosya_yolu")]
        public string? DosyaYolu { get; set; }

        [Column("dosya_boyutu")]
        public long? DosyaBoyutu { get; set; }

        [Column("mime_tipi")]
        public string? MimeTipi { get; set; }

        [Column("otomatik_cv_html")]
        public string? OtomatikCVHtml { get; set; }

        [Column("aktif")]
        public bool Aktif { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("AdayId")]
        public virtual Aday Aday { get; set; } = null!;
    }

    [Table("aday_durum_gecmisi")]
    public class AdayDurumGecmisi
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("aday_id")]
        public int AdayId { get; set; }

        [Column("eski_durum")]
        public AdayDurumu? EskiDurum { get; set; }

        [Column("yeni_durum")]
        public AdayDurumu YeniDurum { get; set; }

        [Column("degisiklik_tarihi")]
        public DateTime DegisiklikTarihi { get; set; } = DateTime.UtcNow;

        [Column("degisiklik_notu")]
        public string? DegisiklikNotu { get; set; }

        [Column("degistiren_personel_id")]
        public int? DegistirenPersonelId { get; set; }

        [Column("otomatik_degisiklik")]
        public bool OtomatikDegisiklik { get; set; } = false;

        [Column("ilgili_basvuru_id")]
        public int? IlgiliBasvuruId { get; set; }

        [Column("ilgili_mulakat_id")]
        public int? IlgiliMulakatId { get; set; }

        [ForeignKey("AdayId")]
        public virtual Aday Aday { get; set; } = null!;

        [ForeignKey("DegistirenPersonelId")]
        public virtual Personel? DegistirenPersonel { get; set; }

        [ForeignKey("IlgiliBasvuruId")]
        public virtual Basvuru? IlgiliBasvuru { get; set; }

        [ForeignKey("IlgiliMulakatId")]
        public virtual Mulakat? IlgiliMulakat { get; set; }
    }

    [Table("aday_deneyimler")]
    public class AdayDeneyim
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("aday_id")]
        public int AdayId { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("sirket_ad")]
        public string SirketAd { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("pozisyon")]
        public string Pozisyon { get; set; } = string.Empty;

        [Column("baslangic_tarihi")]
        public DateTime? BaslangicTarihi { get; set; }

        [Column("bitis_tarihi")]
        public DateTime? BitisTarihi { get; set; }

        [Column("halen_calisiyor")]
        public bool HalenCalisiyor { get; set; } = false;

        [Column("aciklama")]
        public string? Aciklama { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("AdayId")]
        public virtual Aday Aday { get; set; } = null!;
    }

    [Table("aday_yetenekler")]
    public class AdayYetenek
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("aday_id")]
        public int AdayId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("yetenek")]
        public string Yetenek { get; set; } = string.Empty;

        [Column("seviye")]
        public int Seviye { get; set; } = 1; // 1-5 arası

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("AdayId")]
        public virtual Aday Aday { get; set; } = null!;
    }

    [Table("basvurular")]
    public class Basvuru
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("ilan_id")]
        public int IlanId { get; set; }

        [Column("aday_id")]
        public int AdayId { get; set; }

        [Column("basvuru_tarihi")]
        public DateTime BasvuruTarihi { get; set; } = DateTime.UtcNow;

        [Column("durum")]
        public BasvuruDurumu Durum { get; set; } = BasvuruDurumu.YeniBasvuru;

        [Column("kapak_mektubu")]
        public string? KapakMektubu { get; set; }

        [Column("beklenen_maas")]
        public decimal? BeklenenMaas { get; set; }

        [Column("ise_baslama_tarihi")]
        public DateTime? IseBaslamaTarihi { get; set; }

        [Column("degerlendiren_id")]
        public int? DegerlendirenId { get; set; }

        [Column("degerlendirme_notu")]
        public string? DegerlendirmeNotu { get; set; }

        [Column("puan")]
        public int Puan { get; set; } = 0; // 0-100 arası

        [Column("aktif")]
        public bool Aktif { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("IlanId")]
        public virtual IsIlani Ilan { get; set; } = null!;

        [ForeignKey("AdayId")]
        public virtual Aday Aday { get; set; } = null!;

        [ForeignKey("DegerlendirenId")]
        public virtual Personel? Degerlendiren { get; set; }

        public virtual ICollection<Mulakat> Mulakatlar { get; set; } = new List<Mulakat>();
    }

    [Table("mulakatlar")]
    public class Mulakat
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("basvuru_id")]
        public int BasvuruId { get; set; }

        [Column("tur")]
        public MulakatTuru Tur { get; set; }

        [Column("tarih")]
        public DateTime Tarih { get; set; }

        [Column("sure")]
        public int Sure { get; set; } = 60; // dakika

        [Column("lokasyon")]
        [MaxLength(200)]
        public string? Lokasyon { get; set; }

        [Column("mulakat_yapan_id")]
        public int MulakatYapanId { get; set; }

        [Column("durum")]
        [MaxLength(20)]
        public string Durum { get; set; } = "Planlandı";

        [Column("notlar")]
        public string? Notlar { get; set; }

        [Column("puan")]
        public int? Puan { get; set; }

        [Column("sonuc")]
        [MaxLength(20)]
        public string? Sonuc { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("BasvuruId")]
        public virtual Basvuru Basvuru { get; set; } = null!;

        [ForeignKey("MulakatYapanId")]
        public virtual Personel MulakatYapan { get; set; } = null!;
    }

    [Table("teklif_mektuplari")]
    public class TeklifMektubu
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("basvuru_id")]
        public int BasvuruId { get; set; }

        [Column("pozisyon")]
        [MaxLength(100)]
        public string Pozisyon { get; set; } = string.Empty;

        [Column("maas")]
        public decimal Maas { get; set; }

        [Column("ek_odemeler")]
        public string? EkOdemeler { get; set; }

        [Column("izin_hakki")]
        public int IzinHakki { get; set; } = 14;

        [Column("ise_baslama_tarihi")]
        public DateTime IseBaslamaTarihi { get; set; }

        [Column("gecerlilik_tarihi")]
        public DateTime GecerlilikTarihi { get; set; }

        [Column("durum")]
        [MaxLength(20)]
        public string Durum { get; set; } = "Beklemede";

        [Column("gonderim_tarihi")]
        public DateTime? GonderimTarihi { get; set; }

        [Column("yanit_tarihi")]
        public DateTime? YanitTarihi { get; set; }

        [Column("red_nedeni")]
        public string? RedNedeni { get; set; }

        [Column("hazirlayan_id")]
        public int HazirlayanId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("BasvuruId")]
        public virtual Basvuru Basvuru { get; set; } = null!;

        [ForeignKey("HazirlayanId")]
        public virtual Personel Hazirlayan { get; set; } = null!;
    }

    [Table("aday_egitimler")]
    public class AdayEgitim
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("aday_id")]
        public int AdayId { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("okul_ad")]
        public string OkulAd { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("bolum")]
        public string Bolum { get; set; } = string.Empty;

        [Column("derece")]
        [MaxLength(50)]
        public string? Derece { get; set; } // Lisans, Yüksek Lisans, Doktora

        [Column("baslangic_yili")]
        public int BaslangicYili { get; set; }

        [Column("mezuniyet_yili")]
        public int? MezuniyetYili { get; set; }

        [Column("devam_ediyor")]
        public bool DevamEdiyor { get; set; } = false;

        [Column("not_ortalamasi")]
        public decimal? NotOrtalamasi { get; set; }

        [Column("aciklama")]
        public string? Aciklama { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("AdayId")]
        public virtual Aday Aday { get; set; } = null!;
    }

    [Table("aday_sertifikalar")]
    public class AdaySertifika
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("aday_id")]
        public int AdayId { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("sertifika_ad")]
        public string SertifikaAd { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        [Column("veren_kurum")]
        public string VerenKurum { get; set; } = string.Empty;

        [Column("tarih")]
        public DateTime Tarih { get; set; }

        [Column("gecerlilik_tarihi")]
        public DateTime? GecerlilikTarihi { get; set; }

        [Column("sertifika_no")]
        [MaxLength(100)]
        public string? SertifikaNo { get; set; }

        [Column("aciklama")]
        public string? Aciklama { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("AdayId")]
        public virtual Aday Aday { get; set; } = null!;
    }

    [Table("aday_referanslar")]
    public class AdayReferans
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("aday_id")]
        public int AdayId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("ad_soyad")]
        public string AdSoyad { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        [Column("sirket")]
        public string Sirket { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("pozisyon")]
        public string Pozisyon { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        [Column("telefon")]
        public string Telefon { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Column("iliski_turu")]
        [MaxLength(50)]
        public string? IliskiTuru { get; set; } // Yönetici, İş Arkadaşı, Müşteri

        [Column("aciklama")]
        public string? Aciklama { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("AdayId")]
        public virtual Aday Aday { get; set; } = null!;
    }

    [Table("aday_diller")]
    public class AdayDil
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("aday_id")]
        public int AdayId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("dil")]
        public string Dil { get; set; } = string.Empty;

        [Column("okuma_seviyesi")]
        public int OkumaSeviyesi { get; set; } = 1; // 1-5 arası

        [Column("yazma_seviyesi")]
        public int YazmaSeviyesi { get; set; } = 1; // 1-5 arası

        [Column("konusma_seviyesi")]
        public int KonusmaSeviyesi { get; set; } = 1; // 1-5 arası

        [Column("sertifika")]
        [MaxLength(100)]
        public string? Sertifika { get; set; } // TOEFL, IELTS, vb.

        [Column("sertifika_puani")]
        [MaxLength(50)]
        public string? SertifikaPuani { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("AdayId")]
        public virtual Aday Aday { get; set; } = null!;
    }

    [Table("aday_projeler")]
    public class AdayProje
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("aday_id")]
        public int AdayId { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("proje_ad")]
        public string ProjeAd { get; set; } = string.Empty;

        [Column("rol")]
        [MaxLength(100)]
        public string? Rol { get; set; }

        [Column("baslangic_tarihi")]
        public DateTime? BaslangicTarihi { get; set; }

        [Column("bitis_tarihi")]
        public DateTime? BitisTarihi { get; set; }

        [Column("devam_ediyor")]
        public bool DevamEdiyor { get; set; } = false;

        [Column("teknolojiler")]
        public string? Teknolojiler { get; set; } // Comma separated

        [Column("aciklama")]
        public string? Aciklama { get; set; }

        [Column("proje_url")]
        [MaxLength(500)]
        public string? ProjeUrl { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("AdayId")]
        public virtual Aday Aday { get; set; } = null!;
    }

    [Table("aday_hobiler")]
    public class AdayHobi
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("aday_id")]
        public int AdayId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("hobi")]
        public string Hobi { get; set; } = string.Empty;

        [Column("kategori")]
        [MaxLength(50)]
        public string? Kategori { get; set; } // Spor, Sanat, Teknoloji, vb.

        [Column("seviye")]
        [MaxLength(50)]
        public string? Seviye { get; set; } // Başlangıç, Orta, İleri

        [Column("aciklama")]
        public string? Aciklama { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("AdayId")]
        public virtual Aday Aday { get; set; } = null!;
    }
}