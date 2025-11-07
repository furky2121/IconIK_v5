using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IconIK.API.Models
{
    // Ana Anket Tablosu
    [Table("Anketler")]
    public class Anket
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Baslik { get; set; }

        [StringLength(1000)]
        public string? Aciklama { get; set; }

        [Required]
        public DateTime BaslangicTarihi { get; set; }

        [Required]
        public DateTime BitisTarihi { get; set; }

        [Required]
        [StringLength(50)]
        public string AnketDurumu { get; set; } = "Taslak"; // Taslak, Aktif, Tamamlandı

        public bool AnonymousMu { get; set; } = false; // Anonim anket mi?

        public bool Aktif { get; set; } = true;

        public int OlusturanPersonelId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("OlusturanPersonelId")]
        public virtual Personel OlusturanPersonel { get; set; }

        public virtual ICollection<AnketSoru> Sorular { get; set; }
        public virtual ICollection<AnketAtama> Atamalar { get; set; }
        public virtual ICollection<AnketCevap> Cevaplar { get; set; }
        public virtual ICollection<AnketKatilim> Katilimlar { get; set; }
    }

    // Anket Soruları
    [Table("AnketSorular")]
    public class AnketSoru
    {
        [Key]
        public int Id { get; set; }

        public int AnketId { get; set; }

        [Required]
        [StringLength(1000)]
        public string SoruMetni { get; set; }

        [Required]
        [StringLength(50)]
        public string SoruTipi { get; set; } = "TekSecim"; // TekSecim, CokluSecim, AcikUclu

        public int Sira { get; set; } = 0; // Soru sırası

        public bool ZorunluMu { get; set; } = false;

        public bool Aktif { get; set; } = true;

        // Navigation properties
        [ForeignKey("AnketId")]
        public virtual Anket Anket { get; set; }

        public virtual ICollection<AnketSecenek> Secenekler { get; set; }
        public virtual ICollection<AnketCevap> Cevaplar { get; set; }
    }

    // Anket Seçenekleri (Çoktan seçmeli sorular için)
    [Table("AnketSecenekler")]
    public class AnketSecenek
    {
        [Key]
        public int Id { get; set; }

        public int SoruId { get; set; }

        [Required]
        [StringLength(500)]
        public string SecenekMetni { get; set; }

        public int Sira { get; set; } = 0; // Seçenek sırası

        public bool Aktif { get; set; } = true;

        // Navigation properties
        [ForeignKey("SoruId")]
        public virtual AnketSoru Soru { get; set; }

        public virtual ICollection<AnketCevap> Cevaplar { get; set; }
    }

    // Anket Atamaları (Personel/Departman/Pozisyon bazlı)
    [Table("AnketAtamalar")]
    public class AnketAtama
    {
        [Key]
        public int Id { get; set; }

        public int AnketId { get; set; }

        public int? PersonelId { get; set; } // Kişiye özel atama
        public int? DepartmanId { get; set; } // Departmana toplu atama
        public int? PozisyonId { get; set; } // Pozisyona toplu atama

        public int AtayanPersonelId { get; set; }

        public DateTime AtamaTarihi { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string Durum { get; set; } = "Atandı"; // Atandı, Tamamlandı, SuresiGecti

        [StringLength(500)]
        public string? Not { get; set; }

        public bool BildirimGonderildiMi { get; set; } = false;
        public DateTime? SonBildirimTarihi { get; set; }

        // Navigation properties
        [ForeignKey("AnketId")]
        public virtual Anket Anket { get; set; }

        [ForeignKey("PersonelId")]
        public virtual Personel? Personel { get; set; }

        [ForeignKey("DepartmanId")]
        public virtual Departman? Departman { get; set; }

        [ForeignKey("PozisyonId")]
        public virtual Pozisyon? Pozisyon { get; set; }

        [ForeignKey("AtayanPersonelId")]
        public virtual Personel AtayanPersonel { get; set; }
    }

    // Anket Cevapları
    [Table("AnketCevaplar")]
    public class AnketCevap
    {
        [Key]
        public int Id { get; set; }

        public int AnketId { get; set; }

        public int SoruId { get; set; }

        public int PersonelId { get; set; }

        public int? SecenekId { get; set; } // Çoktan seçmeli için

        [StringLength(2000)]
        public string? AcikCevap { get; set; } // Açık uçlu için

        public DateTime CevapTarihi { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("AnketId")]
        public virtual Anket Anket { get; set; }

        [ForeignKey("SoruId")]
        public virtual AnketSoru Soru { get; set; }

        [ForeignKey("PersonelId")]
        public virtual Personel Personel { get; set; }

        [ForeignKey("SecenekId")]
        public virtual AnketSecenek? Secenek { get; set; }
    }

    // Anket Katılım Durumu
    [Table("AnketKatilimlar")]
    public class AnketKatilim
    {
        [Key]
        public int Id { get; set; }

        public int AnketId { get; set; }

        public int PersonelId { get; set; }

        public DateTime BaslangicTarihi { get; set; } = DateTime.UtcNow;

        public DateTime? TamamlanmaTarihi { get; set; }

        public bool TamamlandiMi { get; set; } = false;

        public int TamamlananSoruSayisi { get; set; } = 0;

        [StringLength(50)]
        public string Durum { get; set; } = "Başlamadı"; // Başlamadı, Devam Ediyor, Tamamlandı

        // Navigation properties
        [ForeignKey("AnketId")]
        public virtual Anket Anket { get; set; }

        [ForeignKey("PersonelId")]
        public virtual Personel Personel { get; set; }
    }
}
