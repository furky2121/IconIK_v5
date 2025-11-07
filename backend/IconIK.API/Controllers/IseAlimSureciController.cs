using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IconIK.API.Data;
using IconIK.API.Models;
using System.Text.Json;

namespace IconIK.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IseAlimSureciController : ControllerBase
    {
        private readonly IconIKContext _context;

        public IseAlimSureciController(IconIKContext context)
        {
            _context = context;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var thisMonth = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                var lastMonth = thisMonth.AddMonths(-1);

                // Aktif ilanlar
                var aktifIlanlar = await _context.IsIlanlari
                    .Where(i => i.Aktif && i.Durum == IlanDurumu.Aktif)
                    .CountAsync();

                // Toplam başvuru sayısı
                var toplamBasvuru = await _context.Basvurular
                    .Where(b => b.Aktif)
                    .CountAsync();

                // Bu ay başvuru sayısı
                var buAyBasvuru = await _context.Basvurular
                    .Where(b => b.Aktif && b.BasvuruTarihi >= thisMonth)
                    .CountAsync();

                // Durum bazında başvurular
                var durumBazindaBasvuru = await _context.Basvurular
                    .Where(b => b.Aktif)
                    .GroupBy(b => b.Durum)
                    .Select(g => new
                    {
                        Durum = g.Key.ToString(),
                        Sayi = g.Count()
                    })
                    .ToListAsync();

                // Bekleyen mülakatlar (bu hafta)
                var haftaBaslangici = DateTime.SpecifyKind(today.AddDays(-(int)today.DayOfWeek), DateTimeKind.Utc);
                var haftaSonu = haftaBaslangici.AddDays(7);

                var bekleyenMulakatlar = await _context.Mulakatlar
                    .Where(m => m.Tarih >= haftaBaslangici && m.Tarih < haftaSonu &&
                               m.Durum == "Planlandı")
                    .CountAsync();

                // En çok başvuru alan ilanlar
                var enCokBasvuruAlanIlanlar = await _context.IsIlanlari
                    .Include(i => i.Pozisyon)
                    .Include(i => i.Departman)
                    .Include(i => i.Basvurular)
                    .Where(i => i.Aktif)
                    .Select(i => new
                    {
                        i.Id,
                        i.Baslik,
                        Pozisyon = i.Pozisyon.Ad,
                        Departman = i.Departman.Ad,
                        BasvuruSayisi = i.Basvurular.Count(b => b.Aktif),
                        i.YayinTarihi
                    })
                    .OrderByDescending(i => i.BasvuruSayisi)
                    .Take(5)
                    .ToListAsync();

                // Son başvurular
                var sonBasvurular = await _context.Basvurular
                    .Include(b => b.Aday)
                    .Include(b => b.Ilan)
                    .ThenInclude(i => i.Pozisyon)
                    .Where(b => b.Aktif)
                    .OrderByDescending(b => b.BasvuruTarihi)
                    .Take(10)
                    .Select(b => new
                    {
                        b.Id,
                        AdayAd = b.Aday.Ad + " " + b.Aday.Soyad,
                        IlanBaslik = b.Ilan.Baslik,
                        Pozisyon = b.Ilan.Pozisyon.Ad,
                        b.BasvuruTarihi,
                        Durum = b.Durum.ToString(),
                        b.Puan
                    })
                    .ToListAsync();

                // Bu hafta mülakatlar
                var buHaftaMulakatlar = await _context.Mulakatlar
                    .Include(m => m.Basvuru)
                    .ThenInclude(b => b.Aday)
                    .Include(m => m.Basvuru)
                    .ThenInclude(b => b.Ilan)
                    .Include(m => m.MulakatYapan)
                    .Where(m => m.Tarih >= haftaBaslangici && m.Tarih < haftaSonu)
                    .OrderBy(m => m.Tarih)
                    .Select(m => new
                    {
                        m.Id,
                        AdayAd = m.Basvuru.Aday.Ad + " " + m.Basvuru.Aday.Soyad,
                        IlanBaslik = m.Basvuru.Ilan.Baslik,
                        m.Tarih,
                        m.Sure,
                        Tur = m.Tur.ToString(),
                        MulakatYapan = m.MulakatYapan.Ad + " " + m.MulakatYapan.Soyad,
                        m.Durum,
                        m.Lokasyon
                    })
                    .ToListAsync();

                // Aylık başvuru trendi (son 6 ay)
                var aylikTrend = new List<object>();
                for (int i = 5; i >= 0; i--)
                {
                    var ay = thisMonth.AddMonths(-i);
                    var ayBaslangic = ay;
                    var aySon = ay.AddMonths(1);

                    var aySayisi = await _context.Basvurular
                        .Where(b => b.Aktif && b.BasvuruTarihi >= ayBaslangic && b.BasvuruTarihi < aySon)
                        .CountAsync();

                    aylikTrend.Add(new
                    {
                        Ay = ay.ToString("MMM yyyy"),
                        Sayi = aySayisi
                    });
                }

                // Başarı oranları
                var iseAlinanlar = await _context.Basvurular
                    .Where(b => b.Aktif && b.Durum == BasvuruDurumu.IseAlindi)
                    .CountAsync();

                var teklifVerilenler = await _context.TeklifMektuplari
                    .CountAsync();

                var basariOrani = toplamBasvuru > 0 ? Math.Round((double)iseAlinanlar / toplamBasvuru * 100, 1) : 0;
                var teklifOrani = toplamBasvuru > 0 ? Math.Round((double)teklifVerilenler / toplamBasvuru * 100, 1) : 0;

                var result = new
                {
                    ozet = new
                    {
                        aktifIlanlar,
                        toplamBasvuru,
                        buAyBasvuru,
                        bekleyenMulakatlar,
                        basariOrani,
                        teklifOrani
                    },
                    durumBazindaBasvuru,
                    enCokBasvuruAlanIlanlar,
                    sonBasvurular,
                    buHaftaMulakatlar,
                    aylikTrend
                };

                return Ok(new { success = true, data = result, message = "İşe alım süreç verileri başarıyla getirildi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata: {ex.Message}" });
            }
        }

        [HttpGet("istatistikler")]
        public async Task<IActionResult> GetIstatistikler()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var thisYear = new DateTime(today.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                // Departman bazında başvuru dağılımı
                var departmanBazinda = await _context.IsIlanlari
                    .Include(i => i.Departman)
                    .Include(i => i.Basvurular)
                    .Where(i => i.Aktif)
                    .GroupBy(i => i.Departman.Ad)
                    .Select(g => new
                    {
                        Departman = g.Key,
                        ToplamBasvuru = g.Sum(i => i.Basvurular.Count(b => b.Aktif)),
                        IseAlim = g.Sum(i => i.Basvurular.Count(b => b.Aktif && b.Durum == BasvuruDurumu.IseAlindi))
                    })
                    .OrderByDescending(d => d.ToplamBasvuru)
                    .ToListAsync();

                // Pozisyon bazında başvuru dağılımı
                var pozisyonBazinda = await _context.IsIlanlari
                    .Include(i => i.Pozisyon)
                    .Include(i => i.Basvurular)
                    .Where(i => i.Aktif)
                    .GroupBy(i => i.Pozisyon.Ad)
                    .Select(g => new
                    {
                        Pozisyon = g.Key,
                        ToplamBasvuru = g.Sum(i => i.Basvurular.Count(b => b.Aktif)),
                        IseAlim = g.Sum(i => i.Basvurular.Count(b => b.Aktif && b.Durum == BasvuruDurumu.IseAlindi))
                    })
                    .OrderByDescending(p => p.ToplamBasvuru)
                    .Take(10)
                    .ToListAsync();

                // Mülakat türü başarı oranları
                var mulakatBasari = await _context.Mulakatlar
                    .Include(m => m.Basvuru)
                    .Where(m => m.Puan.HasValue)
                    .GroupBy(m => m.Tur)
                    .Select(g => new
                    {
                        Tur = g.Key.ToString(),
                        ToplamMulakat = g.Count(),
                        OrtalamaPuan = Math.Round(g.Average(m => m.Puan.Value), 1),
                        BasariliMulakat = g.Count(m => m.Sonuc == "Başarılı")
                    })
                    .ToListAsync();

                // Kaynak analizi (nereden geliyorlar)
                var kaynakAnalizi = await _context.Adaylar
                    .Where(a => a.Aktif)
                    .GroupBy(a => a.LinkedinUrl != null ? "LinkedIn" : "Diğer")
                    .Select(g => new
                    {
                        Kaynak = g.Key,
                        AdaySayisi = g.Count()
                    })
                    .ToListAsync();

                var result = new
                {
                    departmanBazinda,
                    pozisyonBazinda,
                    mulakatBasari,
                    kaynakAnalizi
                };

                return Ok(new { success = true, data = result, message = "İstatistikler başarıyla getirildi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata: {ex.Message}" });
            }
        }

        [HttpGet("surecler")]
        public async Task<IActionResult> GetAktifSurecler()
        {
            try
            {
                var aktifSurecler = await _context.Basvurular
                    .Include(b => b.Aday)
                    .Include(b => b.Ilan)
                    .ThenInclude(i => i.Pozisyon)
                    .Include(b => b.Ilan)
                    .ThenInclude(i => i.Departman)
                    .Include(b => b.Mulakatlar)
                    .Where(b => b.Aktif && b.Durum != BasvuruDurumu.Reddedildi &&
                               b.Durum != BasvuruDurumu.AdayVazgecti &&
                               b.Durum != BasvuruDurumu.IseAlindi)
                    .OrderByDescending(b => b.BasvuruTarihi)
                    .Select(b => new
                    {
                        b.Id,
                        AdayBilgi = new
                        {
                            b.Aday.Id,
                            AdSoyad = b.Aday.Ad + " " + b.Aday.Soyad,
                            b.Aday.Email,
                            b.Aday.Telefon,
                            b.Aday.ToplamDeneyim
                        },
                        IlanBilgi = new
                        {
                            b.Ilan.Id,
                            b.Ilan.Baslik,
                            Pozisyon = b.Ilan.Pozisyon.Ad,
                            Departman = b.Ilan.Departman.Ad
                        },
                        b.BasvuruTarihi,
                        Durum = b.Durum.ToString(),
                        b.Puan,
                        b.BeklenenMaas,
                        SonrakiAdim = GetSonrakiAdim(b.Durum),
                        MulakatSayisi = b.Mulakatlar.Count,
                        SonMulakatTarihi = b.Mulakatlar
                            .OrderByDescending(m => m.Tarih)
                            .Select(m => m.Tarih)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = aktifSurecler, message = "Aktif süreçler başarıyla getirildi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata: {ex.Message}" });
            }
        }

        private static string GetSonrakiAdim(BasvuruDurumu durum)
        {
            return durum switch
            {
                BasvuruDurumu.YeniBasvuru => "İlk Değerlendirme",
                BasvuruDurumu.Degerlendiriliyor => "HR Mülakatı Planlama",
                BasvuruDurumu.MulakatBekleniyor => "Mülakat Gerçekleştirme",
                BasvuruDurumu.MulakatTamamlandi => "Sonuç Değerlendirme",
                BasvuruDurumu.TeklifVerildi => "Aday Yanıtı Bekleme",
                _ => "İşlem Bekliyor"
            };
        }
    }
}