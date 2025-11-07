using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IconIK.API.Models
{
    // Video Kategorileri
    [Table("VideoKategoriler")]
    public class VideoKategori
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Ad { get; set; }
        
        [StringLength(500)]
        public string Aciklama { get; set; }
        
        [StringLength(50)]
        public string Icon { get; set; } // PrimeReact icon adı
        
        [StringLength(7)]
        public string Renk { get; set; } // Hex renk kodu
        
        public int Sira { get; set; }
        public bool Aktif { get; set; } = true;
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        // Navigation properties
        public virtual ICollection<VideoEgitim> VideoEgitimler { get; set; }
    }
    
    // Video Eğitimleri
    [Table("VideoEgitimler")]
    public class VideoEgitim
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Baslik { get; set; }
        
        [StringLength(1000)]
        public string Aciklama { get; set; }
        
        [Required]
        [StringLength(500)]
        public string VideoUrl { get; set; } // YouTube, Vimeo veya kendi sunucumuz
        
        [StringLength(500)]
        public string ThumbnailUrl { get; set; } // Video önizleme resmi
        
        public int Sure { get; set; } // Dakika cinsinden
        
        public int KategoriId { get; set; }
        
        [StringLength(50)]
        public string Seviye { get; set; } // Başlangıç, Orta, İleri
        
        [StringLength(100)]
        public string Egitmen { get; set; }
        
        public int IzlenmeMinimum { get; set; } = 80; // Tamamlanma için minimum izlenme yüzdesi
        
        public bool ZorunluMu { get; set; } = false;
        
        public DateTime? SonTamamlanmaTarihi { get; set; } // Zorunlu eğitimler için
        
        public bool Aktif { get; set; } = true;
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        public DateTime? GuncellemeTarihi { get; set; }
        
        // Navigation properties
        [ForeignKey("KategoriId")]
        public virtual VideoKategori Kategori { get; set; }
        public virtual ICollection<VideoAtama> VideoAtamalar { get; set; }
        public virtual ICollection<VideoIzleme> VideoIzlemeler { get; set; }
        public virtual ICollection<VideoYorum> VideoYorumlar { get; set; }
        public virtual ICollection<VideoSoru> VideoSorular { get; set; }
    }
    
    // Video Atamaları (Departman veya Kişi bazlı)
    [Table("VideoAtamalar")]
    public class VideoAtama
    {
        [Key]
        public int Id { get; set; }
        
        public int VideoEgitimId { get; set; }
        
        public int? PersonelId { get; set; } // Kişiye özel atama
        public int? DepartmanId { get; set; } // Departmana toplu atama
        public int? PozisyonId { get; set; } // Pozisyona toplu atama
        
        public DateTime AtamaTarihi { get; set; } = DateTime.Now;
        public DateTime? TamamlanmaTarihi { get; set; }
        
        [StringLength(50)]
        public string Durum { get; set; } = "Atandı"; // Atandı, Devam Ediyor, Tamamlandı, Süresi Geçti
        
        public int? AtayanPersonelId { get; set; }
        
        [StringLength(500)]
        public string Not { get; set; }
        
        public bool HatirlatmaGonderildiMi { get; set; } = false;
        public DateTime? SonHatirlatmaTarihi { get; set; }
        
        // Navigation properties
        [ForeignKey("VideoEgitimId")]
        public virtual VideoEgitim VideoEgitim { get; set; }
        
        [ForeignKey("PersonelId")]
        public virtual Personel Personel { get; set; }
        
        [ForeignKey("DepartmanId")]
        public virtual Departman Departman { get; set; }
        
        [ForeignKey("PozisyonId")]
        public virtual Pozisyon Pozisyon { get; set; }
        
        [ForeignKey("AtayanPersonelId")]
        public virtual Personel AtayanPersonel { get; set; }
    }
    
    // Video İzleme Kayıtları
    [Table("VideoIzlemeler")]
    public class VideoIzleme
    {
        [Key]
        public int Id { get; set; }
        
        public int VideoEgitimId { get; set; }
        public int PersonelId { get; set; }
        
        public DateTime IzlemeBaslangic { get; set; }
        public DateTime? IzlemeBitis { get; set; }
        
        public int ToplamIzlenenSure { get; set; } // Saniye cinsinden
        public int IzlemeYuzdesi { get; set; } // 0-100
        
        public bool TamamlandiMi { get; set; } = false;
        public DateTime? TamamlanmaTarihi { get; set; }
        
        public int? Puan { get; set; } // 1-5 yıldız
        
        [StringLength(50)]
        public string CihazTipi { get; set; } // Desktop, Mobile, Tablet
        
        [StringLength(100)]
        public string IpAdresi { get; set; }
        
        // Yeni alanlar
        [StringLength(20)]
        public string VideoPlatform { get; set; } = "Local"; // YouTube, Vimeo, Local
        
        public int IzlemeBaslangicSaniye { get; set; } = 0; // Video'da nereden başladı
        public int IzlemeBitisSaniye { get; set; } = 0; // Video'da nerede bitti
        
        public int VideoToplamSure { get; set; } // Video'nun toplam süresi (saniye)
        
        // Navigation properties
        [ForeignKey("VideoEgitimId")]
        public virtual VideoEgitim VideoEgitim { get; set; }
        
        [ForeignKey("PersonelId")]
        public virtual Personel Personel { get; set; }
    }
    
    // Video Yorumları
    [Table("VideoYorumlar")]
    public class VideoYorum
    {
        [Key]
        public int Id { get; set; }
        
        public int VideoEgitimId { get; set; }
        public int PersonelId { get; set; }
        
        [Required]
        [StringLength(1000)]
        public string Yorum { get; set; }
        
        public int? Puan { get; set; } // 1-5
        
        public DateTime YorumTarihi { get; set; } = DateTime.Now;
        public bool Aktif { get; set; } = true;
        
        // Navigation properties
        [ForeignKey("VideoEgitimId")]
        public virtual VideoEgitim VideoEgitim { get; set; }
        
        [ForeignKey("PersonelId")]
        public virtual Personel Personel { get; set; }
    }
    
    // Eğitim Soruları (Quiz)
    [Table("VideoSorular")]
    public class VideoSoru
    {
        [Key]
        public int Id { get; set; }
        
        public int VideoEgitimId { get; set; }
        
        [Required]
        [StringLength(500)]
        public string Soru { get; set; }
        
        [Required]
        [StringLength(200)]
        public string CevapA { get; set; }
        
        [Required]
        [StringLength(200)]
        public string CevapB { get; set; }
        
        [Required]
        [StringLength(200)]
        public string CevapC { get; set; }
        
        [Required]
        [StringLength(200)]
        public string CevapD { get; set; }
        
        [Required]
        [StringLength(1)]
        public string DogruCevap { get; set; } // A, B, C veya D
        
        public int Puan { get; set; } = 10;
        public int Sira { get; set; }
        public bool Aktif { get; set; } = true;
        
        // Navigation properties
        [ForeignKey("VideoEgitimId")]
        public virtual VideoEgitim VideoEgitim { get; set; }
        public virtual ICollection<VideoSoruCevap> VideoSoruCevaplar { get; set; }
    }
    
    // Personel Quiz Cevapları
    [Table("VideoSoruCevaplar")]
    public class VideoSoruCevap
    {
        [Key]
        public int Id { get; set; }
        
        public int VideoSoruId { get; set; }
        public int PersonelId { get; set; }
        
        [StringLength(1)]
        public string VerilenCevap { get; set; }
        
        public bool DogruMu { get; set; }
        public int AlinanPuan { get; set; }
        public DateTime CevapTarihi { get; set; } = DateTime.Now;
        
        // Navigation properties
        [ForeignKey("VideoSoruId")]
        public virtual VideoSoru VideoSoru { get; set; }
        
        [ForeignKey("PersonelId")]
        public virtual Personel Personel { get; set; }
    }
    
    // Eğitim Sertifikaları
    [Table("VideoSertifikalar")]
    public class VideoSertifika
    {
        [Key]
        public int Id { get; set; }
        
        public int VideoEgitimId { get; set; }
        public int PersonelId { get; set; }
        
        [StringLength(100)]
        public string SertifikaNo { get; set; }
        
        [Column("VerilmeTarihi")]
        public DateTime VerilisTarihi { get; set; } = DateTime.Now;
        public DateTime? GecerlilikTarihi { get; set; }
        
        public int? QuizPuani { get; set; }
        public int IzlemeYuzdesi { get; set; }
        
        [StringLength(500)]
        public string? SertifikaUrl { get; set; } // PDF dosya yolu
        
        [StringLength(50)]
        public string Durum { get; set; } = "Aktif"; // Aktif, İptal, Süresi Geçmiş
        
        // Navigation properties
        [ForeignKey("VideoEgitimId")]
        public virtual VideoEgitim VideoEgitim { get; set; }
        
        [ForeignKey("PersonelId")]
        public virtual Personel Personel { get; set; }
    }
}