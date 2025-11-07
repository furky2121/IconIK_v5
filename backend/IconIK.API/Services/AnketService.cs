using Microsoft.EntityFrameworkCore;
using IconIK.API.Data;
using IconIK.API.Models;

namespace IconIK.API.Services
{
    public class AnketService : IAnketService
    {
        private readonly IconIKContext _context;
        private readonly IBildirimService _bildirimService;

        public AnketService(IconIKContext context, IBildirimService bildirimService)
        {
            _context = context;
            _bildirimService = bildirimService;
        }

        // ===== ANKET CRUD =====

        public async Task<List<Anket>> GetAllAnketlerAsync()
        {
            return await _context.Anketler
                .Include(a => a.OlusturanPersonel)
                .Include(a => a.Sorular.Where(s => s.Aktif))
                    .ThenInclude(s => s.Secenekler.Where(se => se.Aktif))
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Anket>> GetAktifAnketlerAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.Anketler
                .Include(a => a.OlusturanPersonel)
                .Include(a => a.Sorular.Where(s => s.Aktif))
                    .ThenInclude(s => s.Secenekler.Where(se => se.Aktif))
                .Where(a => a.Aktif && a.AnketDurumu == "Aktif" && a.BaslangicTarihi <= now && a.BitisTarihi >= now)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Anket?> GetAnketByIdAsync(int id)
        {
            return await _context.Anketler
                .Include(a => a.OlusturanPersonel)
                .Include(a => a.Sorular.Where(s => s.Aktif))
                    .ThenInclude(s => s.Secenekler.Where(se => se.Aktif))
                .Include(a => a.Atamalar)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Anket> CreateAnketAsync(Anket anket)
        {
            anket.CreatedAt = DateTime.UtcNow;
            _context.Anketler.Add(anket);
            await _context.SaveChangesAsync();
            return anket;
        }

        public async Task<Anket> UpdateAnketAsync(Anket anket)
        {
            anket.UpdatedAt = DateTime.UtcNow;
            _context.Anketler.Update(anket);
            await _context.SaveChangesAsync();
            return anket;
        }

        public async Task<bool> DeleteAnketAsync(int id)
        {
            var anket = await _context.Anketler.FindAsync(id);
            if (anket == null) return false;

            _context.Anketler.Remove(anket);
            await _context.SaveChangesAsync();
            return true;
        }

        // ===== ANKET ATAMA =====

        public async Task<List<AnketAtama>> GetAnketAtamalariAsync(int anketId)
        {
            return await _context.AnketAtamalar
                .Include(a => a.Personel)
                .Include(a => a.Departman)
                .Include(a => a.Pozisyon)
                .Include(a => a.AtayanPersonel)
                .Where(a => a.AnketId == anketId)
                .OrderByDescending(a => a.AtamaTarihi)
                .ToListAsync();
        }

        public async Task<AnketAtama> CreateAtamaAsync(AnketAtama atama)
        {
            atama.AtamaTarihi = DateTime.UtcNow;
            atama.Durum = "Atandı";
            atama.BildirimGonderildiMi = false;

            _context.AnketAtamalar.Add(atama);
            await _context.SaveChangesAsync();

            // Get survey details for notification
            var anket = await _context.Anketler
                .Include(a => a.OlusturanPersonel)
                .FirstOrDefaultAsync(a => a.Id == atama.AnketId);

            if (anket != null)
            {
                var bildirimBaslik = "Yeni Anket Ataması";
                var bildirimMesaj = $"'{anket.Baslik}' anketi size atandı. Lütfen en kısa sürede tamamlayınız.";
                var actionUrl = "/bana-atanan-anketler";
                var gonderenAd = anket.OlusturanPersonel != null ? $"{anket.OlusturanPersonel.Ad} {anket.OlusturanPersonel.Soyad}" : "Sistem";

                List<int> aliciIdList = new List<int>();

                // Determine recipients based on assignment type
                if (atama.PersonelId.HasValue)
                {
                    // Direct assignment to a person
                    aliciIdList.Add(atama.PersonelId.Value);
                }
                else if (atama.DepartmanId.HasValue)
                {
                    // Assignment to a department - get all personnel in department
                    var departmanPersoneller = await _context.Personeller
                        .Include(p => p.Pozisyon)
                        .Where(p => p.Pozisyon.DepartmanId == atama.DepartmanId && p.Aktif)
                        .Select(p => p.Id)
                        .ToListAsync();
                    aliciIdList.AddRange(departmanPersoneller);
                }
                else if (atama.PozisyonId.HasValue)
                {
                    // Assignment to a position - get all personnel in position
                    var pozisyonPersoneller = await _context.Personeller
                        .Where(p => p.PozisyonId == atama.PozisyonId && p.Aktif)
                        .Select(p => p.Id)
                        .ToListAsync();
                    aliciIdList.AddRange(pozisyonPersoneller);
                }

                // Send notifications to all recipients
                foreach (var aliciId in aliciIdList)
                {
                    var bildirim = new Bildirim
                    {
                        AliciId = aliciId,
                        Baslik = bildirimBaslik,
                        Mesaj = bildirimMesaj,
                        Kategori = "anket",
                        Tip = "info",
                        GonderenAd = gonderenAd,
                        ActionUrl = actionUrl
                    };

                    await _bildirimService.CreateBildirimAsync(bildirim);
                }

                // Update notification status
                if (aliciIdList.Count > 0)
                {
                    atama.BildirimGonderildiMi = true;
                    atama.SonBildirimTarihi = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }

            return atama;
        }

        public async Task<bool> DeleteAtamaAsync(int atamaId)
        {
            var atama = await _context.AnketAtamalar.FindAsync(atamaId);
            if (atama == null) return false;

            _context.AnketAtamalar.Remove(atama);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Anket>> GetBanaAtananAnketlerAsync(int personelId)
        {
            var now = DateTime.UtcNow;

            // Personelin departman ve pozisyonunu al
            var personel = await _context.Personeller
                .Include(p => p.Pozisyon)
                .FirstOrDefaultAsync(p => p.Id == personelId);

            if (personel == null) return new List<Anket>();

            // Personele atanan anketler
            var direktAtananlar = _context.AnketAtamalar
                .Where(a => a.PersonelId == personelId)
                .Select(a => a.AnketId);

            // Departmana atanan anketler
            var departmanAtananlar = _context.AnketAtamalar
                .Where(a => a.DepartmanId == personel.Pozisyon.DepartmanId)
                .Select(a => a.AnketId);

            // Pozisyona atanan anketler
            var pozisyonAtananlar = _context.AnketAtamalar
                .Where(a => a.PozisyonId == personel.PozisyonId)
                .Select(a => a.AnketId);

            // Tüm atanan anket ID'lerini birleştir
            var atananAnketIdleri = direktAtananlar
                .Union(departmanAtananlar)
                .Union(pozisyonAtananlar)
                .Distinct();

            // Anketleri getir
            var anketler = await _context.Anketler
                .Include(a => a.OlusturanPersonel)
                .Include(a => a.Sorular.Where(s => s.Aktif))
                    .ThenInclude(s => s.Secenekler.Where(se => se.Aktif))
                .Where(a => atananAnketIdleri.Contains(a.Id) && a.Aktif && a.AnketDurumu == "Aktif")
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return anketler;
        }

        // ===== ANKET CEVAPLAMA =====

        public async Task<AnketKatilim?> GetKatilimAsync(int anketId, int personelId)
        {
            return await _context.AnketKatilimlar
                .FirstOrDefaultAsync(k => k.AnketId == anketId && k.PersonelId == personelId);
        }

        public async Task<AnketKatilim> BaslatAsync(int anketId, int personelId)
        {
            // Mevcut katılım kontrolü
            var mevcutKatilim = await GetKatilimAsync(anketId, personelId);
            if (mevcutKatilim != null)
            {
                return mevcutKatilim;
            }

            // Yeni katılım oluştur
            var katilim = new AnketKatilim
            {
                AnketId = anketId,
                PersonelId = personelId,
                BaslangicTarihi = DateTime.UtcNow,
                TamamlandiMi = false,
                TamamlananSoruSayisi = 0,
                Durum = "Devam Ediyor"
            };

            _context.AnketKatilimlar.Add(katilim);
            await _context.SaveChangesAsync();

            return katilim;
        }

        public async Task<List<AnketCevap>> CevapKaydetAsync(int anketId, int personelId, List<AnketCevap> cevaplar)
        {
            // Mevcut cevapları sil (güncelleme için)
            var mevcutCevaplar = await _context.AnketCevaplar
                .Where(c => c.AnketId == anketId && c.PersonelId == personelId)
                .ToListAsync();

            _context.AnketCevaplar.RemoveRange(mevcutCevaplar);

            // Yeni cevapları ekle
            foreach (var cevap in cevaplar)
            {
                cevap.AnketId = anketId;
                cevap.PersonelId = personelId;
                cevap.CevapTarihi = DateTime.UtcNow;
            }

            _context.AnketCevaplar.AddRange(cevaplar);
            await _context.SaveChangesAsync();

            // Katılım durumunu güncelle
            var katilim = await GetKatilimAsync(anketId, personelId);
            if (katilim != null)
            {
                katilim.TamamlananSoruSayisi = cevaplar.Select(c => c.SoruId).Distinct().Count();
                await _context.SaveChangesAsync();
            }

            return cevaplar;
        }

        public async Task<bool> TamamlaAsync(int anketId, int personelId)
        {
            var katilim = await GetKatilimAsync(anketId, personelId);
            if (katilim == null) return false;

            katilim.TamamlandiMi = true;
            katilim.TamamlanmaTarihi = DateTime.UtcNow;
            katilim.Durum = "Tamamlandı";

            // İlgili atama durumunu güncelle
            var atama = await _context.AnketAtamalar
                .FirstOrDefaultAsync(a => a.AnketId == anketId && a.PersonelId == personelId);

            if (atama != null)
            {
                atama.Durum = "Tamamlandı";
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // ===== ANKET SONUÇLARI =====

        public async Task<object> GetAnketSonuclariAsync(int anketId)
        {
            // Sonuçlar için tüm seçenekleri yükle (Aktif/Pasif filtresiz)
            var anket = await _context.Anketler
                .Include(a => a.Sorular)
                    .ThenInclude(s => s.Secenekler)
                .FirstOrDefaultAsync(a => a.Id == anketId);

            if (anket == null) return null!;

            var sonuclar = new List<object>();

            foreach (var soru in anket.Sorular.OrderBy(s => s.Sira))
            {
                if (soru.SoruTipi == "AcikUclu")
                {
                    // Açık uçlu sorular için tüm cevapları getir
                    var acikCevaplar = await _context.AnketCevaplar
                        .Include(c => c.Personel)
                        .Where(c => c.SoruId == soru.Id && c.AcikCevap != null)
                        .Select(c => new
                        {
                            PersonelAd = anket.AnonymousMu ? "Anonim" : $"{c.Personel.Ad} {c.Personel.Soyad}",
                            Cevap = c.AcikCevap,
                            Tarih = c.CevapTarihi
                        })
                        .OrderByDescending(c => c.Tarih)
                        .ToListAsync();

                    sonuclar.Add(new
                    {
                        SoruId = soru.Id,
                        SoruMetni = soru.SoruMetni,
                        SoruTipi = soru.SoruTipi,
                        ToplamCevap = acikCevaplar.Count,
                        AcikCevaplar = acikCevaplar
                    });
                }
                else
                {
                    // Çoktan seçmeli sorular için seçenek bazlı istatistikler
                    var secenekSonuclari = new List<object>();

                    // Toplam cevap sayısı
                    var toplamCevap = await _context.AnketCevaplar
                        .Where(c => c.SoruId == soru.Id)
                        .CountAsync();

                    // Bu soruya cevap veren benzersiz katılımcı sayısı
                    var toplamKatilimci = await _context.AnketCevaplar
                        .Where(c => c.SoruId == soru.Id)
                        .Select(c => c.PersonelId)
                        .Distinct()
                        .CountAsync();

                    foreach (var secenek in soru.Secenekler.OrderBy(s => s.Sira))
                    {
                        var secenekCevapSayisi = await _context.AnketCevaplar
                            .Where(c => c.SecenekId == secenek.Id)
                            .CountAsync();

                        // Çoklu seçim soruları için katılımcı sayısına göre yüzde hesapla
                        // Tek seçim soruları için cevap sayısına göre yüzde hesapla
                        var payda = soru.SoruTipi == "CokluSecim" ? toplamKatilimci : toplamCevap;
                        var yuzde = payda > 0 ? (secenekCevapSayisi * 100.0 / payda) : 0;

                        secenekSonuclari.Add(new
                        {
                            SecenekId = secenek.Id,
                            SecenekMetni = secenek.SecenekMetni,
                            CevapSayisi = secenekCevapSayisi,
                            Yuzde = Math.Round(yuzde, 2)
                        });
                    }

                    sonuclar.Add(new
                    {
                        SoruId = soru.Id,
                        SoruMetni = soru.SoruMetni,
                        SoruTipi = soru.SoruTipi,
                        ToplamCevap = toplamCevap,
                        ToplamKatilimci = toplamKatilimci,
                        Secenekler = secenekSonuclari
                    });
                }
            }

            return new
            {
                AnketId = anketId,
                AnketBaslik = anket.Baslik,
                AnonymousMu = anket.AnonymousMu,
                Sorular = sonuclar
            };
        }

        public async Task<object> GetAnketCevaplariAsync(int anketId)
        {
            // Anket bilgisini al
            var anket = await _context.Anketler.FirstOrDefaultAsync(a => a.Id == anketId);
            if (anket == null) return new List<object>();

            // Katılım bilgilerini al
            var katilimlar = await _context.AnketKatilimlar
                .Include(k => k.Personel)
                .Where(k => k.AnketId == anketId)
                .OrderByDescending(k => k.TamamlandiMi)
                .ThenBy(k => k.Personel.Ad)
                .ToListAsync();

            var katilimDetaylari = katilimlar.Select(k => new
            {
                PersonelId = k.PersonelId,
                PersonelAd = anket.AnonymousMu ? "Anonim" : $"{k.Personel.Ad} {k.Personel.Soyad}",
                Durum = k.TamamlandiMi ? "Tamamlandı" : "Devam Ediyor",
                BaslangicTarihi = k.BaslangicTarihi,
                TamamlanmaTarihi = k.TamamlanmaTarihi,
                TamamlananSoruSayisi = k.TamamlananSoruSayisi
            }).ToList();

            return katilimDetaylari;
        }

        public async Task<object> GetKatilimIstatistikleriAsync(int anketId)
        {
            // Toplam atama sayısı
            var toplamAtama = await _context.AnketAtamalar
                .Where(a => a.AnketId == anketId)
                .CountAsync();

            // Personel bazlı atama sayısı (departman ve pozisyon atamaları hariç)
            var personelAtamaSayisi = await _context.AnketAtamalar
                .Where(a => a.AnketId == anketId && a.PersonelId != null)
                .CountAsync();

            // Departman atamaları için personel sayısı
            var departmanAtamalari = await _context.AnketAtamalar
                .Where(a => a.AnketId == anketId && a.DepartmanId != null)
                .Select(a => a.DepartmanId)
                .ToListAsync();

            var departmanPersonelSayisi = 0;
            foreach (var departmanId in departmanAtamalari)
            {
                var count = await _context.Personeller
                    .Include(p => p.Pozisyon)
                    .Where(p => p.Pozisyon.DepartmanId == departmanId && p.Aktif)
                    .CountAsync();
                departmanPersonelSayisi += count;
            }

            // Pozisyon atamaları için personel sayısı
            var pozisyonAtamalari = await _context.AnketAtamalar
                .Where(a => a.AnketId == anketId && a.PozisyonId != null)
                .Select(a => a.PozisyonId)
                .ToListAsync();

            var pozisyonPersonelSayisi = 0;
            foreach (var pozisyonId in pozisyonAtamalari)
            {
                var count = await _context.Personeller
                    .Where(p => p.PozisyonId == pozisyonId && p.Aktif)
                    .CountAsync();
                pozisyonPersonelSayisi += count;
            }

            // Yaklaşık hedef kişi sayısı
            var hedefKisiSayisi = personelAtamaSayisi + departmanPersonelSayisi + pozisyonPersonelSayisi;

            // Katılım sayısı
            var katilimSayisi = await _context.AnketKatilimlar
                .Where(k => k.AnketId == anketId)
                .CountAsync();

            // Tamamlanan katılım sayısı
            var tamamlananSayisi = await _context.AnketKatilimlar
                .Where(k => k.AnketId == anketId && k.TamamlandiMi)
                .CountAsync();

            // Devam eden katılım sayısı
            var devamEdenSayisi = await _context.AnketKatilimlar
                .Where(k => k.AnketId == anketId && !k.TamamlandiMi)
                .CountAsync();

            var katilimOrani = hedefKisiSayisi > 0 ? (tamamlananSayisi * 100.0 / hedefKisiSayisi) : 0;

            return new
            {
                ToplamAtama = toplamAtama,
                HedefKisiSayisi = hedefKisiSayisi,
                KatilimSayisi = katilimSayisi,
                TamamlananSayisi = tamamlananSayisi,
                DevamEdenSayisi = devamEdenSayisi,
                KatilimOrani = Math.Round(katilimOrani, 2)
            };
        }
    }
}
