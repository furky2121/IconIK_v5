using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IconIK.API.Data;
using IconIK.API.Models;
using IconIK.API.Services;
using System.Text.Json;

namespace IconIK.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BasvuruController : ControllerBase
    {
        private readonly IconIKContext _context;
        private readonly IStatusService _statusService;

        public BasvuruController(IconIKContext context, IStatusService statusService)
        {
            _context = context;
            _statusService = statusService;
        }

        [HttpGet]
        public async Task<ActionResult<object>> GetBasvurular()
        {
            try
            {
                var basvurular = await _context.Basvurular
                    .Include(b => b.Ilan)
                        .ThenInclude(i => i.Pozisyon)
                    .Include(b => b.Ilan)
                        .ThenInclude(i => i.Departman)
                    .Include(b => b.Aday)
                    .Include(b => b.Degerlendiren)
                    .OrderByDescending(b => b.BasvuruTarihi)
                    .Select(b => new
                    {
                        b.Id,
                        ilanBaslik = b.Ilan.Baslik,
                        pozisyon = b.Ilan.Pozisyon.Ad,
                        departman = b.Ilan.Departman.Ad,
                        adayAd = b.Aday.Ad + " " + b.Aday.Soyad,
                        adayEmail = b.Aday.Email,
                        adayTelefon = b.Aday.Telefon,
                        basvuruTarihi = b.BasvuruTarihi,
                        durum = b.Durum.ToString(),
                        durumText = GetBasvuruDurumText(b.Durum),
                        beklenenMaas = b.BeklenenMaas,
                        iseBaslamaTarihi = b.IseBaslamaTarihi,
                        degerlendiren = b.Degerlendiren != null ? b.Degerlendiren.Ad + " " + b.Degerlendiren.Soyad : null,
                        puan = b.Puan,
                        puanNotu = b.DegerlendirmeNotu,
                        createdAt = b.CreatedAt,
                        mulakatSayisi = b.Mulakatlar.Count()
                    })
                    .ToListAsync();

                Console.WriteLine($"Başvuru listesi döndürülüyor: {basvurular.Count} kayıt");
                if (basvurular.Any())
                {
                    Console.WriteLine($"İlk başvuru örneği: {System.Text.Json.JsonSerializer.Serialize(basvurular.First())}");
                }
                return new { success = true, data = basvurular, message = "Başvurular başarıyla getirildi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpGet("ilan/{ilanId}")]
        public async Task<ActionResult<object>> GetIlanBasvurulari(int ilanId)
        {
            try
            {
                var basvurular = await _context.Basvurular
                    .Where(b => b.IlanId == ilanId)
                    .Include(b => b.Aday)
                    .Include(b => b.Degerlendiren)
                    .OrderByDescending(b => b.Puan)
                        .ThenByDescending(b => b.BasvuruTarihi)
                    .Select(b => new
                    {
                        b.Id,
                        AdayId = b.Aday.Id,
                        AdayAd = b.Aday.Ad + " " + b.Aday.Soyad,
                        AdayEmail = b.Aday.Email,
                        AdayTelefon = b.Aday.Telefon,
                        AdayDeneyim = b.Aday.ToplamDeneyim,
                        AdayUniversite = b.Aday.Universite,
                        AdayBolum = b.Aday.Bolum,
                        b.BasvuruTarihi,
                        Durum = b.Durum.ToString(),
                        DurumText = GetBasvuruDurumText(b.Durum),
                        b.KapakMektubu,
                        b.BeklenenMaas,
                        b.IseBaslamaTarihi,
                        Degerlendiren = b.Degerlendiren != null ? b.Degerlendiren.Ad + " " + b.Degerlendiren.Soyad : null,
                        b.DegerlendirmeNotu,
                        b.Puan
                    })
                    .ToListAsync();

                return new { success = true, data = basvurular, message = "İlan başvuruları başarıyla getirildi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetBasvuru(int id)
        {
            try
            {
                var basvuru = await _context.Basvurular
                    .Include(b => b.Ilan)
                        .ThenInclude(i => i.Pozisyon)
                    .Include(b => b.Ilan)
                        .ThenInclude(i => i.Departman)
                    .Include(b => b.Aday)
                        .ThenInclude(a => a.Deneyimler)
                    .Include(b => b.Aday)
                        .ThenInclude(a => a.Yetenekler)
                    .Include(b => b.Degerlendiren)
                    .Include(b => b.Mulakatlar)
                        .ThenInclude(m => m.MulakatYapan)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (basvuru == null)
                {
                    return new { success = false, message = "Başvuru bulunamadı." };
                }

                var result = new
                {
                    basvuru.Id,
                    basvuru.IlanId,
                    Ilan = new
                    {
                        basvuru.Ilan.Id,
                        basvuru.Ilan.Baslik,
                        Pozisyon = basvuru.Ilan.Pozisyon.Ad,
                        Departman = basvuru.Ilan.Departman.Ad,
                        basvuru.Ilan.IsTanimi,
                        basvuru.Ilan.Gereksinimler
                    },
                    Aday = new
                    {
                        basvuru.Aday.Id,
                        basvuru.Aday.Ad,
                        basvuru.Aday.Soyad,
                        AdSoyad = basvuru.Aday.Ad + " " + basvuru.Aday.Soyad,
                        basvuru.Aday.Email,
                        basvuru.Aday.Telefon,
                        basvuru.Aday.Universite,
                        basvuru.Aday.Bolum,
                        basvuru.Aday.ToplamDeneyim,
                        basvuru.Aday.OzgecmisDosyasi,
                        basvuru.Aday.LinkedinUrl,
                        Deneyimler = basvuru.Aday.Deneyimler.Select(d => new
                        {
                            d.SirketAd,
                            d.Pozisyon,
                            d.BaslangicTarihi,
                            d.BitisTarihi,
                            d.HalenCalisiyor
                        }),
                        Yetenekler = basvuru.Aday.Yetenekler.Select(y => new
                        {
                            y.Yetenek,
                            y.Seviye
                        })
                    },
                    basvuru.BasvuruTarihi,
                    Durum = basvuru.Durum.ToString(),
                    DurumText = GetBasvuruDurumText(basvuru.Durum),
                    basvuru.KapakMektubu,
                    basvuru.BeklenenMaas,
                    basvuru.IseBaslamaTarihi,
                    basvuru.DegerlendirenId,
                    Degerlendiren = basvuru.Degerlendiren != null ? basvuru.Degerlendiren.Ad + " " + basvuru.Degerlendiren.Soyad : null,
                    basvuru.DegerlendirmeNotu,
                    basvuru.Puan,
                    Mulakatlar = basvuru.Mulakatlar.Select(m => new
                    {
                        m.Id,
                        Tur = m.Tur.ToString(),
                        m.Tarih,
                        m.Sure,
                        m.Lokasyon,
                        MulakatYapan = m.MulakatYapan.Ad + " " + m.MulakatYapan.Soyad,
                        m.Durum,
                        m.Puan,
                        m.Sonuc
                    }).OrderBy(m => m.Tarih)
                };

                return new { success = true, data = result, message = "Başvuru başarıyla getirildi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<object>> CreateBasvuru([FromBody] JsonElement basvuruData)
        {
            try
            {
                var basvuru = new Basvuru
                {
                    IlanId = basvuruData.GetProperty("ilanId").GetInt32(),
                    AdayId = basvuruData.GetProperty("adayId").GetInt32(),
                    KapakMektubu = basvuruData.TryGetProperty("kapakMektubu", out var kapakMektubu) ? kapakMektubu.GetString() : null
                };

                if (basvuruData.TryGetProperty("beklenenMaas", out var beklenenMaas) && !beklenenMaas.ValueKind.Equals(JsonValueKind.Null))
                {
                    basvuru.BeklenenMaas = beklenenMaas.GetDecimal();
                }

                if (basvuruData.TryGetProperty("iseBaslamaTarihi", out var iseBaslamaTarihi) && !iseBaslamaTarihi.ValueKind.Equals(JsonValueKind.Null))
                {
                    basvuru.IseBaslamaTarihi = iseBaslamaTarihi.GetDateTime();
                }

                // Aynı ilan için aynı aday tekrar başvuru yapmış mı kontrol et
                var mevcutBasvuru = await _context.Basvurular
                    .AnyAsync(b => b.IlanId == basvuru.IlanId && b.AdayId == basvuru.AdayId);

                if (mevcutBasvuru)
                {
                    return new { success = false, message = "Bu aday bu ilana zaten başvuru yapmış." };
                }

                _context.Basvurular.Add(basvuru);
                await _context.SaveChangesAsync();

                return new { success = true, data = new { id = basvuru.Id }, message = "Başvuru başarıyla oluşturuldu." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<object>> UpdateBasvuru(int id, [FromBody] JsonElement basvuruData)
        {
            try
            {
                var basvuru = await _context.Basvurular.FindAsync(id);
                if (basvuru == null)
                {
                    return new { success = false, message = "Başvuru bulunamadı." };
                }

                if (basvuruData.TryGetProperty("durum", out var durum))
                {
                    if (Enum.TryParse<BasvuruDurumu>(durum.GetString(), out var basvuruDurumu))
                        basvuru.Durum = basvuruDurumu;
                }

                if (basvuruData.TryGetProperty("degerlendireId", out var degerlendireId))
                    basvuru.DegerlendirenId = !degerlendireId.ValueKind.Equals(JsonValueKind.Null) ? degerlendireId.GetInt32() : null;

                if (basvuruData.TryGetProperty("degerlendirmeNotu", out var degerlendirmeNotu))
                    basvuru.DegerlendirmeNotu = degerlendirmeNotu.GetString();

                if (basvuruData.TryGetProperty("puan", out var puan))
                    basvuru.Puan = puan.GetInt32();

                if (basvuruData.TryGetProperty("beklenenMaas", out var beklenenMaas))
                    basvuru.BeklenenMaas = !beklenenMaas.ValueKind.Equals(JsonValueKind.Null) ? beklenenMaas.GetDecimal() : null;

                if (basvuruData.TryGetProperty("iseBaslamaTarihi", out var iseBaslamaTarihi))
                    basvuru.IseBaslamaTarihi = !iseBaslamaTarihi.ValueKind.Equals(JsonValueKind.Null) ? iseBaslamaTarihi.GetDateTime() : null;

                await _context.SaveChangesAsync();

                return new { success = true, message = "Başvuru başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpPut("{id}/durum")]
        public async Task<ActionResult<object>> DurumGuncelle(int id, [FromBody] JsonElement durumData)
        {
            try
            {
                var basvuru = await _context.Basvurular.FindAsync(id);
                if (basvuru == null)
                {
                    return new { success = false, message = "Başvuru bulunamadı." };
                }

                var yeniDurum = durumData.GetProperty("durum").GetString();
                if (Enum.TryParse<BasvuruDurumu>(yeniDurum, out var basvuruDurumu))
                {
                    basvuru.Durum = basvuruDurumu;
                }

                if (durumData.TryGetProperty("degerlendireId", out var degerlendireId))
                    basvuru.DegerlendirenId = degerlendireId.GetInt32();

                if (durumData.TryGetProperty("durumNotu", out var durumNotu))
                    basvuru.DegerlendirmeNotu = durumNotu.GetString();

                await _context.SaveChangesAsync();

                return new { success = true, message = "Başvuru durumu başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpPut("{id}/puan")]
        public async Task<ActionResult<object>> PuanVer(int id, [FromBody] JsonElement puanData)
        {
            try
            {
                var basvuru = await _context.Basvurular.FindAsync(id);
                if (basvuru == null)
                {
                    return new { success = false, message = "Başvuru bulunamadı." };
                }

                var puan = puanData.GetProperty("puan").GetInt32();
                if (puan < 0 || puan > 100)
                {
                    return new { success = false, message = "Puan 0-100 arasında olmalıdır." };
                }

                basvuru.Puan = puan;

                if (puanData.TryGetProperty("puanNotu", out var puanNotu))
                    basvuru.DegerlendirmeNotu = puanNotu.GetString();

                if (puanData.TryGetProperty("degerlendireId", out var degerlendireId))
                    basvuru.DegerlendirenId = degerlendireId.GetInt32();

                await _context.SaveChangesAsync();

                return new { success = true, message = "Başvuru puanı başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<object>> DeleteBasvuru(int id)
        {
            try
            {
                var basvuru = await _context.Basvurular
                    .Include(b => b.Mulakatlar)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (basvuru == null)
                {
                    return new { success = false, message = "Başvuru bulunamadı." };
                }

                if (basvuru.Mulakatlar.Any())
                {
                    return new { success = false, message = "Bu başvuruya ait mülakatlar bulunduğu için silinemez." };
                }

                _context.Basvurular.Remove(basvuru);
                await _context.SaveChangesAsync();

                return new { success = true, message = "Başvuru başarıyla silindi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpGet("istatistik")]
        public async Task<ActionResult<object>> BasvuruIstatistikleri(
            [FromQuery] DateTime? baslangicTarihi = null,
            [FromQuery] DateTime? bitisTarihi = null,
            [FromQuery] int? departmanId = null,
            [FromQuery] int? pozisyonId = null)
        {
            try
            {
                var query = _context.Basvurular.AsQueryable();

                // Tarih filtresi
                if (baslangicTarihi.HasValue)
                {
                    query = query.Where(b => b.BasvuruTarihi >= baslangicTarihi.Value);
                }
                if (bitisTarihi.HasValue)
                {
                    query = query.Where(b => b.BasvuruTarihi <= bitisTarihi.Value);
                }

                // Departman filtresi
                if (departmanId.HasValue)
                {
                    query = query.Include(b => b.Ilan)
                                .Where(b => b.Ilan.DepartmanId == departmanId.Value);
                }

                // Pozisyon filtresi
                if (pozisyonId.HasValue)
                {
                    query = query.Include(b => b.Ilan)
                                .Where(b => b.Ilan.PozisyonId == pozisyonId.Value);
                }

                var toplamBasvuru = await query.CountAsync();
                var yeniBasvurular = await query.CountAsync(b => b.Durum == BasvuruDurumu.YeniBasvuru);
                var degerlendiriliyor = await query.CountAsync(b => b.Durum == BasvuruDurumu.Degerlendiriliyor);
                var mulakatBekleniyor = await query.CountAsync(b => b.Durum == BasvuruDurumu.MulakatBekleniyor);
                var mulakatTamamlandi = await query.CountAsync(b => b.Durum == BasvuruDurumu.MulakatTamamlandi);
                var teklifVerildi = await query.CountAsync(b => b.Durum == BasvuruDurumu.TeklifVerildi);
                var iseAlindi = await query.CountAsync(b => b.Durum == BasvuruDurumu.IseAlindi);
                var reddedildi = await query.CountAsync(b => b.Durum == BasvuruDurumu.Reddedildi);
                var adayVazgecti = await query.CountAsync(b => b.Durum == BasvuruDurumu.AdayVazgecti);

                // Ek istatistikler
                var ortalamaPuan = await query
                    .Where(b => b.Puan > 0)
                    .AverageAsync(b => (double?)b.Puan) ?? 0;

                // Generate monthly trends for last 6 months
                var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
                var monthlyData = await query
                    .Where(b => b.BasvuruTarihi >= sixMonthsAgo)
                    .GroupBy(b => new {
                        Yil = b.BasvuruTarihi.Year,
                        Ay = b.BasvuruTarihi.Month
                    })
                    .Select(g => new {
                        tarih = $"{g.Key.Yil}-{g.Key.Ay:D2}",
                        toplam = g.Count(),
                        iseAlindi = g.Count(b => b.Durum == BasvuruDurumu.IseAlindi)
                    })
                    .ToListAsync();

                // Create a complete list of last 6 months, filling gaps with zero data
                var aylikTrendler = new List<object>();
                for (int i = 5; i >= 0; i--)
                {
                    var targetDate = DateTime.UtcNow.AddMonths(-i);
                    var tarihStr = $"{targetDate.Year}-{targetDate.Month:D2}";

                    var existingData = monthlyData.FirstOrDefault(m => m.tarih == tarihStr);
                    aylikTrendler.Add(new {
                        tarih = tarihStr,
                        toplam = existingData?.toplam ?? 0,
                        iseAlindi = existingData?.iseAlindi ?? 0
                    });
                }

                // En çok başvuru alan pozisyonlar
                var topPozisyonlar = await _context.Basvurular
                    .Include(b => b.Ilan)
                        .ThenInclude(i => i.Pozisyon)
                    .Where(b => baslangicTarihi == null || b.BasvuruTarihi >= baslangicTarihi)
                    .Where(b => bitisTarihi == null || b.BasvuruTarihi <= bitisTarihi)
                    .GroupBy(b => new {
                        PozisyonId = b.Ilan.PozisyonId,
                        PozisyonAdi = b.Ilan.Pozisyon.Ad
                    })
                    .Select(g => new {
                        pozisyonAdi = g.Key.PozisyonAdi,
                        basvuruSayisi = g.Count(),
                        iseAlimSayisi = g.Count(b => b.Durum == BasvuruDurumu.IseAlindi)
                    })
                    .OrderByDescending(x => x.basvuruSayisi)
                    .Take(5)
                    .ToListAsync();

                var durumIstatistikleri = new
                {
                    toplamBasvuru = toplamBasvuru,
                    yeniBasvuru = yeniBasvurular,
                    degerlendiriliyor = degerlendiriliyor,
                    mulakatBekleniyor = mulakatBekleniyor,
                    mulakatTamamlandi = mulakatTamamlandi,
                    teklifVerildi = teklifVerildi,
                    iseAlindi = iseAlindi,
                    reddedildi = reddedildi,
                    adayVazgecti = adayVazgecti,
                    iseAlimOrani = toplamBasvuru > 0 ? Math.Round((double)iseAlindi / toplamBasvuru * 100, 1) : 0,
                    redOrani = toplamBasvuru > 0 ? Math.Round((double)reddedildi / toplamBasvuru * 100, 1) : 0,
                    ortalamaPuan = Math.Round(ortalamaPuan, 1),
                    aylikTrendler = aylikTrendler,
                    topPozisyonlar = topPozisyonlar,

                    // Ek performans metrikleri
                    aktifSurecler = degerlendiriliyor + mulakatBekleniyor + mulakatTamamlandi + teklifVerildi,
                    tamamlananSurecler = iseAlindi + reddedildi + adayVazgecti,
                    basariOrani = (iseAlindi + teklifVerildi) > 0 && toplamBasvuru > 0 ?
                        Math.Round((double)(iseAlindi + teklifVerildi) / toplamBasvuru * 100, 1) : 0
                };

                return new { success = true, data = durumIstatistikleri, message = "Başvuru istatistikleri başarıyla getirildi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        private static string GetBasvuruDurumText(BasvuruDurumu durum)
        {
            return durum switch
            {
                BasvuruDurumu.YeniBasvuru => "Yeni Başvuru",
                BasvuruDurumu.Degerlendiriliyor => "Değerlendiriliyor",
                BasvuruDurumu.MulakatBekleniyor => "Mülakat Bekliyor",
                BasvuruDurumu.MulakatTamamlandi => "Mülakat Tamamlandı",
                BasvuruDurumu.TeklifVerildi => "Teklif Verildi",
                BasvuruDurumu.IseAlindi => "İşe Alındı",
                BasvuruDurumu.Reddedildi => "Reddedildi",
                BasvuruDurumu.AdayVazgecti => "Aday Vazgeçti",
                _ => "Bilinmiyor"
            };
        }
    }
}