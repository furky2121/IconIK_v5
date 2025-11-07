using Microsoft.EntityFrameworkCore;
using IconIK.API.Models;

namespace IconIK.API.Data
{
    public class IconIKContext : DbContext
    {
        public IconIKContext(DbContextOptions<IconIKContext> options) : base(options)
        {
        }

        public DbSet<Kademe> Kademeler { get; set; }
        public DbSet<Departman> Departmanlar { get; set; }
        public DbSet<Pozisyon> Pozisyonlar { get; set; }
        public DbSet<Personel> Personeller { get; set; }
        public DbSet<Kullanici> Kullanicilar { get; set; }
        public DbSet<IzinTalebi> IzinTalepleri { get; set; }
        public DbSet<IzinTipi> IzinTipleri { get; set; }
        public DbSet<Egitim> Egitimler { get; set; }
        public DbSet<PersonelEgitimi> PersonelEgitimleri { get; set; }
        public DbSet<EkranYetkisi> EkranYetkileri { get; set; }
        public DbSet<KademeEkranYetkisi> KademeEkranYetkileri { get; set; }
        public DbSet<Zimmet> Zimmetler { get; set; }
        public DbSet<ZimmetStok> ZimmetStoklar { get; set; }
        public DbSet<ZimmetMalzeme> ZimmetMalzemeleri { get; set; }
        public DbSet<PersonelZimmet> PersonelZimmetler { get; set; }
        public DbSet<ZimmetStokDosya> ZimmetStokDosyalar { get; set; }
        public DbSet<AvansTalebi> AvansTalepleri { get; set; }
        public DbSet<MasrafTalebi> MasrafTalepleri { get; set; }
        public DbSet<IstifaTalebi> IstifaTalepleri { get; set; }
        
        // Video Eğitim Tabloları
        public DbSet<VideoKategori> VideoKategoriler { get; set; }
        public DbSet<VideoEgitim> VideoEgitimler { get; set; }
        public DbSet<VideoAtama> VideoAtamalar { get; set; }
        public DbSet<VideoIzleme> VideoIzlemeler { get; set; }
        public DbSet<VideoYorum> VideoYorumlar { get; set; }
        public DbSet<VideoSoru> VideoSorular { get; set; }
        public DbSet<VideoSoruCevap> VideoSoruCevaplar { get; set; }
        public DbSet<VideoSertifika> VideoSertifikalar { get; set; }
        public DbSet<PersonelGirisCikis> PersonelGirisCikislar { get; set; }

        // İşe Alım Tabloları
        public DbSet<IlanKategori> IlanKategoriler { get; set; }
        public DbSet<IsIlani> IsIlanlari { get; set; }
        public DbSet<Aday> Adaylar { get; set; }
        public DbSet<AdayDeneyim> AdayDeneyimleri { get; set; }
        public DbSet<AdayYetenek> AdayYetenekleri { get; set; }
        public DbSet<AdayCV> AdayCVleri { get; set; }
        public DbSet<AdayDurumGecmisi> AdayDurumGecmisleri { get; set; }
        public DbSet<AdayEgitim> AdayEgitimleri { get; set; }
        public DbSet<AdaySertifika> AdaySertifikalari { get; set; }
        public DbSet<AdayReferans> AdayReferanslari { get; set; }
        public DbSet<AdayDil> AdayDilleri { get; set; }
        public DbSet<AdayProje> AdayProjeleri { get; set; }
        public DbSet<AdayHobi> AdayHobileri { get; set; }
        public DbSet<Basvuru> Basvurular { get; set; }
        public DbSet<Mulakat> Mulakatlar { get; set; }
        public DbSet<TeklifMektubu> TeklifMektuplari { get; set; }

        // Şehirler tablosu
        public DbSet<Sehir> Sehirler { get; set; }

        // Anket Tabloları
        public DbSet<Anket> Anketler { get; set; }
        public DbSet<AnketSoru> AnketSorular { get; set; }
        public DbSet<AnketSecenek> AnketSecenekler { get; set; }
        public DbSet<AnketAtama> AnketAtamalar { get; set; }
        public DbSet<AnketCevap> AnketCevaplar { get; set; }
        public DbSet<AnketKatilim> AnketKatilimlar { get; set; }

        // Bildirim Tablosu
        public DbSet<Bildirim> Bildirimler { get; set; }

        // KVKK Tablosu
        public DbSet<KVKKIzinMetni> KVKKIzinMetinleri { get; set; }

        // E-Posta Tabloları
        public DbSet<EPostaAyarlari> EPostaAyarlari { get; set; }
        public DbSet<EPostaYonlendirme> EPostaYonlendirme { get; set; }

        // Bordro Tabloları
        public DbSet<BordroParametreleri> BordroParametreleri { get; set; }
        public DbSet<OdemeTanimi> OdemeTanimlari { get; set; }
        public DbSet<KesintiTanimi> KesintiTanimlari { get; set; }
        public DbSet<Puantaj> Puantajlar { get; set; }
        public DbSet<BordroAna> BordroAna { get; set; }
        public DbSet<BordroOdeme> BordroOdemeler { get; set; }
        public DbSet<BordroKesinti> BordroKesintiler { get; set; }
        public DbSet<BordroOnay> BordroOnaylar { get; set; }

        // Luca Bordro Entegrasyonu Tabloları
        public DbSet<LucaBordroAyarlari> LucaBordroAyarlari { get; set; }
        public DbSet<LucaBordro> LucaBordrolar { get; set; }
        public DbSet<OtpKod> OtpKodlar { get; set; }

        // Firma Ayarları
        public DbSet<FirmaAyarlari> FirmaAyarlari { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique constraints
            modelBuilder.Entity<Kademe>()
                .HasIndex(k => k.Ad)
                .IsUnique();

            modelBuilder.Entity<Kademe>()
                .HasIndex(k => k.Seviye)
                .IsUnique();

            modelBuilder.Entity<Departman>()
                .HasIndex(d => d.Ad)
                .IsUnique();

            modelBuilder.Entity<Departman>()
                .HasIndex(d => d.Kod)
                .IsUnique();

            modelBuilder.Entity<Pozisyon>()
                .HasIndex(p => new { p.Ad, p.DepartmanId, p.KademeId })
                .IsUnique();

            modelBuilder.Entity<Personel>()
                .HasIndex(p => p.TcKimlik)
                .IsUnique();

            modelBuilder.Entity<Personel>()
                .HasIndex(p => p.Email)
                .IsUnique();

            modelBuilder.Entity<Kullanici>()
                .HasIndex(k => k.PersonelId)
                .IsUnique();

            modelBuilder.Entity<Kullanici>()
                .HasIndex(k => k.KullaniciAdi)
                .IsUnique();

            modelBuilder.Entity<IzinTipi>()
                .HasIndex(it => it.IzinTipiAdi)
                .IsUnique();

            // Relationships
            modelBuilder.Entity<Pozisyon>()
                .HasOne(p => p.Departman)
                .WithMany(d => d.Pozisyonlar)
                .HasForeignKey(p => p.DepartmanId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Pozisyon>()
                .HasOne(p => p.Kademe)
                .WithMany(k => k.Pozisyonlar)
                .HasForeignKey(p => p.KademeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Personel>()
                .HasOne(p => p.Pozisyon)
                .WithMany(pos => pos.Personeller)
                .HasForeignKey(p => p.PozisyonId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Personel>()
                .HasOne(p => p.Yonetici)
                .WithMany(y => y.AltCalisanlar)
                .HasForeignKey(p => p.YoneticiId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Kullanici>()
                .HasOne(k => k.Personel)
                .WithOne(p => p.Kullanici)
                .HasForeignKey<Kullanici>(k => k.PersonelId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<IzinTalebi>()
                .HasOne(i => i.Personel)
                .WithMany(p => p.IzinTalepleri)
                .HasForeignKey(i => i.PersonelId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<IzinTalebi>()
                .HasOne(i => i.Onaylayan)
                .WithMany(p => p.OnayladigiIzinler)
                .HasForeignKey(i => i.OnaylayanId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<PersonelEgitimi>()
                .HasOne(pe => pe.Personel)
                .WithMany(p => p.PersonelEgitimleri)
                .HasForeignKey(pe => pe.PersonelId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PersonelEgitimi>()
                .HasOne(pe => pe.Egitim)
                .WithMany(e => e.PersonelEgitimleri)
                .HasForeignKey(pe => pe.EgitimId)
                .OnDelete(DeleteBehavior.Cascade);


            // Permission model constraints
            modelBuilder.Entity<EkranYetkisi>()
                .HasIndex(e => e.EkranKodu)
                .IsUnique();

            modelBuilder.Entity<KademeEkranYetkisi>()
                .HasIndex(k => new { k.KademeId, k.EkranYetkisiId })
                .IsUnique();

            // Permission model relationships
            modelBuilder.Entity<KademeEkranYetkisi>()
                .HasOne(k => k.Kademe)
                .WithMany()
                .HasForeignKey(k => k.KademeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<KademeEkranYetkisi>()
                .HasOne(k => k.EkranYetkisi)
                .WithMany(e => e.KademeYetkileri)
                .HasForeignKey(k => k.EkranYetkisiId)
                .OnDelete(DeleteBehavior.Cascade);

            // Zimmet relationships
            modelBuilder.Entity<Zimmet>()
                .HasOne(z => z.Personel)
                .WithMany()
                .HasForeignKey(z => z.PersonelId)
                .OnDelete(DeleteBehavior.Restrict);

            // ZimmetStok relationships
            modelBuilder.Entity<ZimmetStok>()
                .HasOne(zs => zs.Onaylayan)
                .WithMany()
                .HasForeignKey(zs => zs.OnaylayanId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ZimmetStok>()
                .HasOne(zs => zs.Olusturan)
                .WithMany()
                .HasForeignKey(zs => zs.OlusturanId)
                .OnDelete(DeleteBehavior.SetNull);

            // ZimmetMalzeme relationships
            modelBuilder.Entity<ZimmetMalzeme>()
                .HasOne(zm => zm.Zimmet)
                .WithMany()
                .HasForeignKey(zm => zm.ZimmetId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ZimmetMalzeme>()
                .HasOne(zm => zm.ZimmetStok)
                .WithMany()
                .HasForeignKey(zm => zm.ZimmetStokId)
                .OnDelete(DeleteBehavior.Restrict);

            // PersonelZimmet relationships
            modelBuilder.Entity<PersonelZimmet>()
                .HasOne(pz => pz.Personel)
                .WithMany()
                .HasForeignKey(pz => pz.PersonelId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PersonelZimmet>()
                .HasOne(pz => pz.ZimmetStok)
                .WithMany()
                .HasForeignKey(pz => pz.ZimmetStokId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PersonelZimmet>()
                .HasOne(pz => pz.ZimmetVeren)
                .WithMany()
                .HasForeignKey(pz => pz.ZimmetVerenId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<PersonelZimmet>()
                .HasOne(pz => pz.IadeAlan)
                .WithMany()
                .HasForeignKey(pz => pz.IadeAlanId)
                .OnDelete(DeleteBehavior.SetNull);

            // Check constraints (PostgreSQL specific)
            modelBuilder.Entity<IzinTalebi>()
                .ToTable(t => t.HasCheckConstraint("CK_IzinTalebi_Durum", "durum IN ('Beklemede', 'Onaylandı', 'Reddedildi')"));

            modelBuilder.Entity<ZimmetStok>()
                .ToTable(t => t.HasCheckConstraint("CK_ZimmetStok_OnayDurumu", "onay_durumu IN ('Bekliyor', 'Onaylandi', 'Reddedildi')"));

            modelBuilder.Entity<PersonelZimmet>()
                .ToTable(t => t.HasCheckConstraint("CK_PersonelZimmet_Durum", "durum IN ('Zimmetli', 'Iade Edildi')"));

            // PersonelGirisCikis relationships
            modelBuilder.Entity<PersonelGirisCikis>()
                .HasOne(pgc => pgc.Personel)
                .WithMany()
                .HasForeignKey(pgc => pgc.PersonelId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PersonelGirisCikis>()
                .HasIndex(pgc => pgc.PersonelId);

            modelBuilder.Entity<PersonelGirisCikis>()
                .ToTable(t => t.HasCheckConstraint("CK_PersonelGirisCikis_GirisTipi", "giris_tipi IN ('Normal', 'Fazla Mesai', 'Hafta Sonu')"));

            // İşe Alım relationships
            modelBuilder.Entity<IlanKategori>()
                .HasIndex(ik => ik.Ad)
                .IsUnique();

            modelBuilder.Entity<Aday>()
                .HasIndex(a => a.TcKimlik)
                .IsUnique();

            modelBuilder.Entity<Aday>()
                .HasIndex(a => a.Email)
                .IsUnique();

            modelBuilder.Entity<IsIlani>()
                .HasOne(i => i.Kategori)
                .WithMany(k => k.IsIlanlari)
                .HasForeignKey(i => i.KategoriId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<IsIlani>()
                .HasOne(i => i.Pozisyon)
                .WithMany()
                .HasForeignKey(i => i.PozisyonId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<IsIlani>()
                .HasOne(i => i.Departman)
                .WithMany()
                .HasForeignKey(i => i.DepartmanId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<IsIlani>()
                .HasOne(i => i.Olusturan)
                .WithMany()
                .HasForeignKey(i => i.OlusturanId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AdayDeneyim>()
                .HasOne(ad => ad.Aday)
                .WithMany(a => a.Deneyimler)
                .HasForeignKey(ad => ad.AdayId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AdayYetenek>()
                .HasOne(ay => ay.Aday)
                .WithMany(a => a.Yetenekler)
                .HasForeignKey(ay => ay.AdayId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AdayCV>()
                .HasOne(ac => ac.Aday)
                .WithMany(a => a.CVler)
                .HasForeignKey(ac => ac.AdayId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AdayDurumGecmisi>()
                .HasOne(adg => adg.Aday)
                .WithMany(a => a.DurumGecmisi)
                .HasForeignKey(adg => adg.AdayId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AdayDurumGecmisi>()
                .HasOne(adg => adg.DegistirenPersonel)
                .WithMany()
                .HasForeignKey(adg => adg.DegistirenPersonelId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<AdayDurumGecmisi>()
                .HasOne(adg => adg.IlgiliBasvuru)
                .WithMany()
                .HasForeignKey(adg => adg.IlgiliBasvuruId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<AdayDurumGecmisi>()
                .HasOne(adg => adg.IlgiliMulakat)
                .WithMany()
                .HasForeignKey(adg => adg.IlgiliMulakatId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Basvuru>()
                .HasOne(b => b.Ilan)
                .WithMany(i => i.Basvurular)
                .HasForeignKey(b => b.IlanId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Basvuru>()
                .HasOne(b => b.Aday)
                .WithMany(a => a.Basvurular)
                .HasForeignKey(b => b.AdayId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Basvuru>()
                .HasOne(b => b.Degerlendiren)
                .WithMany()
                .HasForeignKey(b => b.DegerlendirenId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Mulakat>()
                .HasOne(m => m.Basvuru)
                .WithMany(b => b.Mulakatlar)
                .HasForeignKey(m => m.BasvuruId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Mulakat>()
                .HasOne(m => m.MulakatYapan)
                .WithMany()
                .HasForeignKey(m => m.MulakatYapanId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TeklifMektubu>()
                .HasOne(t => t.Basvuru)
                .WithMany()
                .HasForeignKey(t => t.BasvuruId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TeklifMektubu>()
                .HasOne(t => t.Hazirlayan)
                .WithMany()
                .HasForeignKey(t => t.HazirlayanId)
                .OnDelete(DeleteBehavior.Restrict);

            // İşe Alım check constraints
            modelBuilder.Entity<IsIlani>()
                .ToTable(t => t.HasCheckConstraint("CK_IsIlani_Durum", "durum IN (1, 2, 3, 4)"));

            modelBuilder.Entity<Aday>()
                .ToTable(t => t.HasCheckConstraint("CK_Aday_Durum", "durum IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13)"));

            modelBuilder.Entity<Basvuru>()
                .ToTable(t => t.HasCheckConstraint("CK_Basvuru_Durum", "durum IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10)"));

            modelBuilder.Entity<Mulakat>()
                .ToTable(t => t.HasCheckConstraint("CK_Mulakat_Tur", "tur IN (1, 2, 3, 4, 5)"));

            modelBuilder.Entity<AdayCV>()
                .ToTable(t => t.HasCheckConstraint("CK_AdayCV_CVTipi", "cv_tipi IN ('Otomatik', 'Yuklenmiş')"));

            // Yeni Aday Entity Relationships
            modelBuilder.Entity<AdayEgitim>()
                .HasOne(ae => ae.Aday)
                .WithMany(a => a.Egitimler)
                .HasForeignKey(ae => ae.AdayId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AdaySertifika>()
                .HasOne(s => s.Aday)
                .WithMany(a => a.Sertifikalar)
                .HasForeignKey(s => s.AdayId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AdayReferans>()
                .HasOne(ar => ar.Aday)
                .WithMany(a => a.Referanslar)
                .HasForeignKey(ar => ar.AdayId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AdayDil>()
                .HasOne(ad => ad.Aday)
                .WithMany(a => a.Diller)
                .HasForeignKey(ad => ad.AdayId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AdayProje>()
                .HasOne(ap => ap.Aday)
                .WithMany(a => a.Projeler)
                .HasForeignKey(ap => ap.AdayId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AdayHobi>()
                .HasOne(ah => ah.Aday)
                .WithMany(a => a.Hobiler)
                .HasForeignKey(ah => ah.AdayId)
                .OnDelete(DeleteBehavior.Cascade);

            // Yeni validation constraints
            modelBuilder.Entity<AdayDil>()
                .ToTable(t => t.HasCheckConstraint("CK_AdayDil_Seviyeler", "okuma_seviyesi BETWEEN 1 AND 5 AND yazma_seviyesi BETWEEN 1 AND 5 AND konusma_seviyesi BETWEEN 1 AND 5"));

            // Şehir constraints
            modelBuilder.Entity<Sehir>()
                .HasIndex(s => s.SehirAd)
                .IsUnique();

            modelBuilder.Entity<Sehir>()
                .HasIndex(s => s.PlakaKodu)
                .IsUnique();

            // Aday DogumTarihi field - explicitly map as date type to prevent timezone issues
            modelBuilder.Entity<Aday>()
                .Property(a => a.DogumTarihi)
                .HasColumnType("date");

            // Anket relationships
            modelBuilder.Entity<Anket>()
                .HasOne(a => a.OlusturanPersonel)
                .WithMany()
                .HasForeignKey(a => a.OlusturanPersonelId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AnketSoru>()
                .HasOne(s => s.Anket)
                .WithMany(a => a.Sorular)
                .HasForeignKey(s => s.AnketId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AnketSecenek>()
                .HasOne(se => se.Soru)
                .WithMany(s => s.Secenekler)
                .HasForeignKey(se => se.SoruId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AnketAtama>()
                .HasOne(aa => aa.Anket)
                .WithMany(a => a.Atamalar)
                .HasForeignKey(aa => aa.AnketId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AnketAtama>()
                .HasOne(aa => aa.Personel)
                .WithMany()
                .HasForeignKey(aa => aa.PersonelId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AnketAtama>()
                .HasOne(aa => aa.Departman)
                .WithMany()
                .HasForeignKey(aa => aa.DepartmanId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AnketAtama>()
                .HasOne(aa => aa.Pozisyon)
                .WithMany()
                .HasForeignKey(aa => aa.PozisyonId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AnketAtama>()
                .HasOne(aa => aa.AtayanPersonel)
                .WithMany()
                .HasForeignKey(aa => aa.AtayanPersonelId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AnketCevap>()
                .HasOne(ac => ac.Anket)
                .WithMany(a => a.Cevaplar)
                .HasForeignKey(ac => ac.AnketId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AnketCevap>()
                .HasOne(ac => ac.Soru)
                .WithMany(s => s.Cevaplar)
                .HasForeignKey(ac => ac.SoruId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AnketCevap>()
                .HasOne(ac => ac.Personel)
                .WithMany()
                .HasForeignKey(ac => ac.PersonelId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AnketCevap>()
                .HasOne(ac => ac.Secenek)
                .WithMany(se => se.Cevaplar)
                .HasForeignKey(ac => ac.SecenekId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<AnketKatilim>()
                .HasOne(ak => ak.Anket)
                .WithMany(a => a.Katilimlar)
                .HasForeignKey(ak => ak.AnketId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AnketKatilim>()
                .HasOne(ak => ak.Personel)
                .WithMany()
                .HasForeignKey(ak => ak.PersonelId)
                .OnDelete(DeleteBehavior.Restrict);

            // Anket check constraints
            modelBuilder.Entity<Anket>()
                .ToTable(t => t.HasCheckConstraint("CK_Anket_Durum", "\"AnketDurumu\" IN ('Taslak', 'Aktif', 'Tamamlandı')"));

            modelBuilder.Entity<AnketSoru>()
                .ToTable(t => t.HasCheckConstraint("CK_AnketSoru_Tipi", "\"SoruTipi\" IN ('TekSecim', 'CokluSecim', 'AcikUclu')"));

            modelBuilder.Entity<AnketAtama>()
                .ToTable(t => t.HasCheckConstraint("CK_AnketAtama_Durum", "\"Durum\" IN ('Atandı', 'Tamamlandı', 'SuresiGecti')"));

            modelBuilder.Entity<AnketKatilim>()
                .ToTable(t => t.HasCheckConstraint("CK_AnketKatilim_Durum", "\"Durum\" IN ('Başlamadı', 'Devam Ediyor', 'Tamamlandı')"));

            // Anket unique constraints
            modelBuilder.Entity<AnketKatilim>()
                .HasIndex(ak => new { ak.AnketId, ak.PersonelId })
                .IsUnique();

            // KVKK relationships
            modelBuilder.Entity<KVKKIzinMetni>()
                .HasOne(k => k.OlusturanPersonel)
                .WithMany()
                .HasForeignKey(k => k.OlusturanPersonelId)
                .OnDelete(DeleteBehavior.SetNull);

            // Bordro relationships
            // BordroParametreleri - Yıl/Dönem unique constraint
            modelBuilder.Entity<BordroParametreleri>()
                .HasIndex(bp => new { bp.Yil, bp.Donem })
                .IsUnique();

            // OdemeTanimi - Kod unique constraint
            modelBuilder.Entity<OdemeTanimi>()
                .HasIndex(ot => ot.Kod)
                .IsUnique();

            // KesintiTanimi - Kod unique constraint
            modelBuilder.Entity<KesintiTanimi>()
                .HasIndex(kt => kt.Kod)
                .IsUnique();

            // Puantaj - Personel/Yıl/Ay unique constraint
            modelBuilder.Entity<Puantaj>()
                .HasIndex(p => new { p.PersonelId, p.DonemYil, p.DonemAy })
                .IsUnique();

            modelBuilder.Entity<Puantaj>()
                .HasOne(p => p.Personel)
                .WithMany()
                .HasForeignKey(p => p.PersonelId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Puantaj>()
                .HasOne(p => p.Onaylayan)
                .WithMany()
                .HasForeignKey(p => p.OnaylayanId)
                .OnDelete(DeleteBehavior.SetNull);

            // BordroAna - Personel/Yıl/Ay unique constraint
            modelBuilder.Entity<BordroAna>()
                .HasIndex(ba => new { ba.PersonelId, ba.DonemYil, ba.DonemAy })
                .IsUnique();

            modelBuilder.Entity<BordroAna>()
                .HasOne(ba => ba.Personel)
                .WithMany()
                .HasForeignKey(ba => ba.PersonelId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BordroAna>()
                .HasOne(ba => ba.Puantaj)
                .WithMany(p => p.Bordrolar)
                .HasForeignKey(ba => ba.PuantajId)
                .OnDelete(DeleteBehavior.SetNull);

            // BordroOdeme relationships
            modelBuilder.Entity<BordroOdeme>()
                .HasOne(bo => bo.Bordro)
                .WithMany(ba => ba.BordroOdemeler)
                .HasForeignKey(bo => bo.BordroId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BordroOdeme>()
                .HasOne(bo => bo.OdemeTanimi)
                .WithMany(ot => ot.BordroOdemeler)
                .HasForeignKey(bo => bo.OdemeTanimiId)
                .OnDelete(DeleteBehavior.Restrict);

            // BordroKesinti relationships
            modelBuilder.Entity<BordroKesinti>()
                .HasOne(bk => bk.Bordro)
                .WithMany(ba => ba.BordroKesintiler)
                .HasForeignKey(bk => bk.BordroId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BordroKesinti>()
                .HasOne(bk => bk.KesintiTanimi)
                .WithMany(kt => kt.BordroKesintiler)
                .HasForeignKey(bk => bk.KesintiTanimiId)
                .OnDelete(DeleteBehavior.Restrict);

            // BordroOnay relationships
            modelBuilder.Entity<BordroOnay>()
                .HasOne(bo => bo.Bordro)
                .WithMany(ba => ba.BordroOnaylar)
                .HasForeignKey(bo => bo.BordroId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BordroOnay>()
                .HasOne(bo => bo.Onaylayan)
                .WithMany()
                .HasForeignKey(bo => bo.OnaylayanId)
                .OnDelete(DeleteBehavior.SetNull);

            // Bordro check constraints
            modelBuilder.Entity<Puantaj>()
                .ToTable(t => t.HasCheckConstraint("CK_Puantaj_OnayDurumu", "onay_durumu IN ('Taslak', 'Onayda', 'Onaylandi', 'Reddedildi')"));

            modelBuilder.Entity<BordroAna>()
                .ToTable(t => t.HasCheckConstraint("CK_BordroAna_BordroDurumu", "bordro_durumu IN ('Taslak', 'Onayda', 'Onaylandi', 'Odendi', 'Iptal')"));

            modelBuilder.Entity<BordroAna>()
                .ToTable(t => t.HasCheckConstraint("CK_BordroAna_OnayDurumu", "onay_durumu IN ('Beklemede', 'Onaylandi', 'Reddedildi')"));

            modelBuilder.Entity<BordroAna>()
                .ToTable(t => t.HasCheckConstraint("CK_BordroAna_OdemeDurumu", "odeme_durumu IN ('Beklemede', 'Odendi', 'Iptal')"));

            modelBuilder.Entity<BordroOnay>()
                .ToTable(t => t.HasCheckConstraint("CK_BordroOnay_OnayDurumu", "onay_durumu IN ('Beklemede', 'Onaylandi', 'Reddedildi', 'Iptal')"));
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Entity.GetType().GetProperty("UpdatedAt") != null)
                {
                    entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
                }

                if (entry.State == EntityState.Added && entry.Entity.GetType().GetProperty("CreatedAt") != null)
                {
                    entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
                }

                // Zimmet-specific timestamp handling
                if (entry.Entity.GetType().GetProperty("GuncellemeTarihi") != null)
                {
                    if (entry.State == EntityState.Modified)
                    {
                        entry.Property("GuncellemeTarihi").CurrentValue = DateTime.UtcNow;
                    }
                }

                if (entry.State == EntityState.Added && entry.Entity.GetType().GetProperty("OlusturmaTarihi") != null)
                {
                    entry.Property("OlusturmaTarihi").CurrentValue = DateTime.UtcNow;
                }

                if (entry.State == EntityState.Added && entry.Entity.GetType().GetProperty("ZimmetTarihi") != null)
                {
                    var currentValue = entry.Property("ZimmetTarihi").CurrentValue;
                    if (currentValue is DateTime dt && dt.Kind != DateTimeKind.Utc)
                    {
                        entry.Property("ZimmetTarihi").CurrentValue = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                    }
                }
            }
        }
    }
}