using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IconIK.API.Data;
using IconIK.API.Models;
using System.Text.Json;

namespace IconIK.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MulakatController : ControllerBase
    {
        private readonly IconIKContext _context;

        public MulakatController(IconIKContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<object>> GetMulakatlar()
        {
            try
            {
                var mulakatlar = await _context.Mulakatlar
                    .Include(m => m.Basvuru)
                        .ThenInclude(b => b.Aday)
                    .Include(m => m.Basvuru)
                        .ThenInclude(b => b.Ilan)
                    .Include(m => m.MulakatYapan)
                    .OrderBy(m => m.Tarih)
                    .Select(m => new
                    {
                        m.Id,
                        AdayAd = m.Basvuru.Aday.Ad + " " + m.Basvuru.Aday.Soyad,
                        AdayEmail = m.Basvuru.Aday.Email,
                        AdayTelefon = m.Basvuru.Aday.Telefon,
                        IlanBaslik = m.Basvuru.Ilan.Baslik,
                        Tur = m.Tur.ToString(),
                        TurText = GetMulakatTurText(m.Tur),
                        m.Tarih,
                        m.Sure,
                        m.Lokasyon,
                        MulakatYapan = m.MulakatYapan.Ad + " " + m.MulakatYapan.Soyad,
                        m.Durum,
                        m.Notlar,
                        m.Puan,
                        m.Sonuc,
                        m.CreatedAt
                    })
                    .ToListAsync();

                return new { success = true, data = mulakatlar, message = "Mülakatlar başarıyla getirildi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpGet("takvim")]
        public async Task<ActionResult<object>> GetMulakatTakvimi([FromQuery] DateTime? baslangic, [FromQuery] DateTime? bitis)
        {
            try
            {
                var query = _context.Mulakatlar
                    .Include(m => m.Basvuru)
                        .ThenInclude(b => b.Aday)
                    .Include(m => m.Basvuru)
                        .ThenInclude(b => b.Ilan)
                    .Include(m => m.MulakatYapan)
                    .AsQueryable();

                if (baslangic.HasValue)
                {
                    query = query.Where(m => m.Tarih >= baslangic.Value);
                }

                if (bitis.HasValue)
                {
                    query = query.Where(m => m.Tarih <= bitis.Value);
                }

                var mulakatlar = await query
                    .OrderBy(m => m.Tarih)
                    .Select(m => new
                    {
                        m.Id,
                        title = m.Basvuru.Aday.Ad + " " + m.Basvuru.Aday.Soyad + " - " + GetMulakatTurText(m.Tur),
                        start = m.Tarih,
                        end = m.Tarih.AddMinutes(m.Sure),
                        backgroundColor = GetMulakatColor(m.Tur),
                        borderColor = GetMulakatColor(m.Tur),
                        extendedProps = new
                        {
                            m.Id,
                            AdayAd = m.Basvuru.Aday.Ad + " " + m.Basvuru.Aday.Soyad,
                            IlanBaslik = m.Basvuru.Ilan.Baslik,
                            Tur = GetMulakatTurText(m.Tur),
                            m.Lokasyon,
                            MulakatYapan = m.MulakatYapan.Ad + " " + m.MulakatYapan.Soyad,
                            m.Durum,
                            m.Notlar
                        }
                    })
                    .ToListAsync();

                return new { success = true, data = mulakatlar, message = "Mülakat takvimi başarıyla getirildi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpGet("personel/{personelId}")]
        public async Task<ActionResult<object>> GetPersonelMulakatlari(int personelId)
        {
            try
            {
                var mulakatlar = await _context.Mulakatlar
                    .Where(m => m.MulakatYapanId == personelId)
                    .Include(m => m.Basvuru)
                        .ThenInclude(b => b.Aday)
                    .Include(m => m.Basvuru)
                        .ThenInclude(b => b.Ilan)
                    .OrderBy(m => m.Tarih)
                    .Select(m => new
                    {
                        m.Id,
                        AdayAd = m.Basvuru.Aday.Ad + " " + m.Basvuru.Aday.Soyad,
                        IlanBaslik = m.Basvuru.Ilan.Baslik,
                        Tur = GetMulakatTurText(m.Tur),
                        m.Tarih,
                        m.Sure,
                        m.Lokasyon,
                        m.Durum,
                        m.Notlar,
                        m.Puan,
                        m.Sonuc
                    })
                    .ToListAsync();

                return new { success = true, data = mulakatlar, message = "Personel mülakatları başarıyla getirildi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetMulakat(int id)
        {
            try
            {
                var mulakat = await _context.Mulakatlar
                    .Include(m => m.Basvuru)
                        .ThenInclude(b => b.Aday)
                    .Include(m => m.Basvuru)
                        .ThenInclude(b => b.Ilan)
                    .Include(m => m.MulakatYapan)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (mulakat == null)
                {
                    return new { success = false, message = "Mülakat bulunamadı." };
                }

                var result = new
                {
                    mulakat.Id,
                    mulakat.BasvuruId,
                    Basvuru = new
                    {
                        mulakat.Basvuru.Id,
                        Aday = new
                        {
                            mulakat.Basvuru.Aday.Id,
                            mulakat.Basvuru.Aday.Ad,
                            mulakat.Basvuru.Aday.Soyad,
                            AdSoyad = mulakat.Basvuru.Aday.Ad + " " + mulakat.Basvuru.Aday.Soyad,
                            mulakat.Basvuru.Aday.Email,
                            mulakat.Basvuru.Aday.Telefon,
                            mulakat.Basvuru.Aday.OzgecmisDosyasi
                        },
                        Ilan = new
                        {
                            mulakat.Basvuru.Ilan.Id,
                            mulakat.Basvuru.Ilan.Baslik
                        }
                    },
                    Tur = mulakat.Tur.ToString(),
                    TurText = GetMulakatTurText(mulakat.Tur),
                    mulakat.Tarih,
                    mulakat.Sure,
                    mulakat.Lokasyon,
                    mulakat.MulakatYapanId,
                    MulakatYapan = new
                    {
                        mulakat.MulakatYapan.Id,
                        mulakat.MulakatYapan.Ad,
                        mulakat.MulakatYapan.Soyad,
                        AdSoyad = mulakat.MulakatYapan.Ad + " " + mulakat.MulakatYapan.Soyad
                    },
                    mulakat.Durum,
                    mulakat.Notlar,
                    mulakat.Puan,
                    mulakat.Sonuc,
                    mulakat.CreatedAt
                };

                return new { success = true, data = result, message = "Mülakat başarıyla getirildi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<object>> CreateMulakat([FromBody] JsonElement mulakatData)
        {
            try
            {
                var mulakat = new Mulakat
                {
                    BasvuruId = mulakatData.GetProperty("basvuruId").GetInt32(),
                    MulakatYapanId = mulakatData.GetProperty("mulakatYapanId").GetInt32(),
                    Tarih = mulakatData.GetProperty("tarih").GetDateTime(),
                    Sure = mulakatData.TryGetProperty("sure", out var sure) ? sure.GetInt32() : 60,
                    Lokasyon = mulakatData.TryGetProperty("lokasyon", out var lokasyon) ? lokasyon.GetString() : null,
                    Notlar = mulakatData.TryGetProperty("notlar", out var notlar) ? notlar.GetString() : null,
                    Durum = "Planlandı"
                };

                if (mulakatData.TryGetProperty("tur", out var tur))
                {
                    if (Enum.TryParse<MulakatTuru>(tur.GetString(), out var mulakatTuru))
                        mulakat.Tur = mulakatTuru;
                }

                _context.Mulakatlar.Add(mulakat);
                await _context.SaveChangesAsync();

                return new { success = true, data = new { id = mulakat.Id }, message = "Mülakat başarıyla oluşturuldu." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<object>> UpdateMulakat(int id, [FromBody] JsonElement mulakatData)
        {
            try
            {
                var mulakat = await _context.Mulakatlar.FindAsync(id);
                if (mulakat == null)
                {
                    return new { success = false, message = "Mülakat bulunamadı." };
                }

                if (mulakatData.TryGetProperty("tur", out var tur))
                {
                    if (Enum.TryParse<MulakatTuru>(tur.GetString(), out var mulakatTuru))
                        mulakat.Tur = mulakatTuru;
                }

                if (mulakatData.TryGetProperty("tarih", out var tarih))
                    mulakat.Tarih = tarih.GetDateTime();

                if (mulakatData.TryGetProperty("sure", out var sure))
                    mulakat.Sure = sure.GetInt32();

                if (mulakatData.TryGetProperty("lokasyon", out var lokasyon))
                    mulakat.Lokasyon = lokasyon.GetString();

                if (mulakatData.TryGetProperty("mulakatYapanId", out var mulakatYapanId))
                    mulakat.MulakatYapanId = mulakatYapanId.GetInt32();

                if (mulakatData.TryGetProperty("durum", out var durum))
                    mulakat.Durum = durum.GetString() ?? "";

                if (mulakatData.TryGetProperty("notlar", out var notlar))
                    mulakat.Notlar = notlar.GetString();

                if (mulakatData.TryGetProperty("puan", out var puan))
                    mulakat.Puan = !puan.ValueKind.Equals(JsonValueKind.Null) ? puan.GetInt32() : null;

                if (mulakatData.TryGetProperty("sonuc", out var sonuc))
                    mulakat.Sonuc = sonuc.GetString();

                await _context.SaveChangesAsync();

                return new { success = true, message = "Mülakat başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpPut("{id}/tamamla")]
        public async Task<ActionResult<object>> MulakatTamamla(int id, [FromBody] JsonElement degerlendirmeData)
        {
            try
            {
                var mulakat = await _context.Mulakatlar
                    .Include(m => m.Basvuru)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (mulakat == null)
                {
                    return new { success = false, message = "Mülakat bulunamadı." };
                }

                mulakat.Durum = "Tamamlandı";

                if (degerlendirmeData.TryGetProperty("puan", out var puan))
                {
                    var puanDegeri = puan.GetInt32();
                    if (puanDegeri < 0 || puanDegeri > 100)
                    {
                        return new { success = false, message = "Puan 0-100 arasında olmalıdır." };
                    }
                    mulakat.Puan = puanDegeri;
                }

                if (degerlendirmeData.TryGetProperty("sonuc", out var sonuc))
                    mulakat.Sonuc = sonuc.GetString();

                if (degerlendirmeData.TryGetProperty("notlar", out var notlar))
                    mulakat.Notlar = notlar.GetString();

                // Başvuru durumunu güncelle
                if (mulakat.Sonuc == "Başarılı")
                {
                    mulakat.Basvuru.Durum = BasvuruDurumu.MulakatTamamlandi;
                }
                else if (mulakat.Sonuc == "Başarısız")
                {
                    mulakat.Basvuru.Durum = BasvuruDurumu.Reddedildi;
                }

                await _context.SaveChangesAsync();

                return new { success = true, message = "Mülakat başarıyla tamamlandı." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpPut("{id}/iptal")]
        public async Task<ActionResult<object>> MulakatIptal(int id, [FromBody] JsonElement iptalData)
        {
            try
            {
                var mulakat = await _context.Mulakatlar.FindAsync(id);
                if (mulakat == null)
                {
                    return new { success = false, message = "Mülakat bulunamadı." };
                }

                mulakat.Durum = "İptal Edildi";

                if (iptalData.TryGetProperty("notlar", out var notlar))
                    mulakat.Notlar = notlar.GetString();

                await _context.SaveChangesAsync();

                return new { success = true, message = "Mülakat iptal edildi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<object>> DeleteMulakat(int id)
        {
            try
            {
                var mulakat = await _context.Mulakatlar.FindAsync(id);
                if (mulakat == null)
                {
                    return new { success = false, message = "Mülakat bulunamadı." };
                }

                if (mulakat.Durum == "Tamamlandı")
                {
                    return new { success = false, message = "Tamamlanmış mülakat silinemez." };
                }

                _context.Mulakatlar.Remove(mulakat);
                await _context.SaveChangesAsync();

                return new { success = true, message = "Mülakat başarıyla silindi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpGet("istatistik")]
        public async Task<ActionResult<object>> MulakatIstatistikleri()
        {
            try
            {
                var toplam = await _context.Mulakatlar.CountAsync();
                var planli = await _context.Mulakatlar.CountAsync(m => m.Durum == "Planlandı");
                var tamamlandi = await _context.Mulakatlar.CountAsync(m => m.Durum == "Tamamlandı");
                var iptal = await _context.Mulakatlar.CountAsync(m => m.Durum == "İptal Edildi");
                var basarili = await _context.Mulakatlar.CountAsync(m => m.Sonuc == "Başarılı");
                var basarisiz = await _context.Mulakatlar.CountAsync(m => m.Sonuc == "Başarısız");

                var istatistikler = new
                {
                    Toplam = toplam,
                    Planli = planli,
                    Tamamlandi = tamamlandi,
                    IptalEdildi = iptal,
                    Basarili = basarili,
                    Basarisiz = basarisiz,
                    BasariOrani = tamamlandi > 0 ? Math.Round((double)basarili / tamamlandi * 100, 2) : 0
                };

                return new { success = true, data = istatistikler, message = "Mülakat istatistikleri başarıyla getirildi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        private static string GetMulakatTurText(MulakatTuru tur)
        {
            return tur switch
            {
                MulakatTuru.HR => "İK Mülakatı",
                MulakatTuru.Teknik => "Teknik Mülakat",
                MulakatTuru.Yonetici => "Yönetici Mülakatı",
                MulakatTuru.GenelMudur => "Genel Müdür Mülakatı",
                MulakatTuru.Video => "Video Mülakat",
                _ => "Bilinmiyor"
            };
        }

        private static string GetMulakatColor(MulakatTuru tur)
        {
            return tur switch
            {
                MulakatTuru.HR => "#3b82f6",        // Mavi
                MulakatTuru.Teknik => "#10b981",    // Yeşil
                MulakatTuru.Yonetici => "#f59e0b",  // Turuncu
                MulakatTuru.GenelMudur => "#ef4444", // Kırmızı
                MulakatTuru.Video => "#8b5cf6",     // Mor
                _ => "#6b7280"                      // Gri
            };
        }
    }
}