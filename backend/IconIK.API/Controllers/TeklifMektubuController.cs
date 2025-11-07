using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IconIK.API.Data;
using IconIK.API.Models;
using System.Text.Json;

namespace IconIK.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TeklifMektubuController : ControllerBase
    {
        private readonly IconIKContext _context;

        public TeklifMektubuController(IconIKContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<object>> GetTeklifMektuplari()
        {
            try
            {
                var teklifMektuplari = await _context.TeklifMektuplari
                    .Include(tm => tm.Basvuru)
                        .ThenInclude(b => b.Aday)
                    .Include(tm => tm.Basvuru)
                        .ThenInclude(b => b.Ilan)
                    .Include(tm => tm.Hazirlayan)
                    .OrderByDescending(tm => tm.CreatedAt)
                    .Select(tm => new
                    {
                        tm.Id,
                        tm.BasvuruId,
                        tm.Pozisyon,
                        tm.Maas,
                        tm.EkOdemeler,
                        tm.IzinHakki,
                        tm.IseBaslamaTarihi,
                        tm.GecerlilikTarihi,
                        tm.Durum,
                        tm.GonderimTarihi,
                        tm.YanitTarihi,
                        tm.RedNedeni,
                        tm.HazirlayanId,
                        tm.CreatedAt,
                        tm.UpdatedAt,
                        AdayAdi = tm.Basvuru.Aday.Ad + " " + tm.Basvuru.Aday.Soyad,
                        IlanBasligi = tm.Basvuru.Ilan.Baslik,
                        HazirlayanAdi = tm.Hazirlayan.Ad + " " + tm.Hazirlayan.Soyad,
                        DurumAdi = tm.Durum
                    })
                    .ToListAsync();

                return new { success = true, data = teklifMektuplari, message = "Teklif mektupları başarıyla getirildi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Teklif mektupları getirilirken hata oluştu: {ex.Message}" });
            }
        }

        [HttpGet("Aktif")]
        public async Task<ActionResult<object>> GetAktifTeklifMektuplari()
        {
            try
            {
                var teklifMektuplari = await _context.TeklifMektuplari
                    .Include(tm => tm.Basvuru)
                        .ThenInclude(b => b.Aday)
                    .Include(tm => tm.Basvuru)
                        .ThenInclude(b => b.Ilan)
                    .Where(tm => tm.Durum != "İptal Edildi")
                    .OrderByDescending(tm => tm.CreatedAt)
                    .Select(tm => new
                    {
                        tm.Id,
                        tm.BasvuruId,
                        tm.Pozisyon,
                        tm.Maas,
                        tm.GecerlilikTarihi,
                        tm.Durum,
                        AdayAdi = tm.Basvuru.Aday.Ad + " " + tm.Basvuru.Aday.Soyad,
                        IlanBasligi = tm.Basvuru.Ilan.Baslik
                    })
                    .ToListAsync();

                return new { success = true, data = teklifMektuplari, message = "Aktif teklif mektupları başarıyla getirildi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Aktif teklif mektupları getirilirken hata oluştu: {ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetTeklifMektubu(int id)
        {
            try
            {
                var teklifMektubu = await _context.TeklifMektuplari
                    .Include(tm => tm.Basvuru)
                        .ThenInclude(b => b.Aday)
                    .Include(tm => tm.Basvuru)
                        .ThenInclude(b => b.Ilan)
                    .Include(tm => tm.Hazirlayan)
                    .FirstOrDefaultAsync(tm => tm.Id == id);

                if (teklifMektubu == null)
                {
                    return NotFound(new { success = false, message = "Teklif mektubu bulunamadı." });
                }

                var result = new
                {
                    teklifMektubu.Id,
                    teklifMektubu.BasvuruId,
                    teklifMektubu.Pozisyon,
                    teklifMektubu.Maas,
                    teklifMektubu.EkOdemeler,
                    teklifMektubu.IzinHakki,
                    teklifMektubu.IseBaslamaTarihi,
                    teklifMektubu.GecerlilikTarihi,
                    teklifMektubu.Durum,
                    teklifMektubu.GonderimTarihi,
                    teklifMektubu.YanitTarihi,
                    teklifMektubu.RedNedeni,
                    teklifMektubu.HazirlayanId,
                    teklifMektubu.CreatedAt,
                    teklifMektubu.UpdatedAt,
                    AdayAdi = teklifMektubu.Basvuru.Aday.Ad + " " + teklifMektubu.Basvuru.Aday.Soyad,
                    AdayTelefon = teklifMektubu.Basvuru.Aday.Telefon,
                    AdayEmail = teklifMektubu.Basvuru.Aday.Email,
                    IlanBasligi = teklifMektubu.Basvuru.Ilan.Baslik,
                    HazirlayanAdi = teklifMektubu.Hazirlayan.Ad + " " + teklifMektubu.Hazirlayan.Soyad
                };

                return new { success = true, data = result, message = "Teklif mektubu başarıyla getirildi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Teklif mektubu getirilirken hata oluştu: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<object>> CreateTeklifMektubu([FromBody] JsonElement teklifData)
        {
            try
            {
                var basvuruId = teklifData.GetProperty("basvuruId").GetInt32();

                // Başvurunun var olduğunu kontrol et
                var basvuru = await _context.Basvurular
                    .Include(b => b.Aday)
                    .Include(b => b.Ilan)
                    .FirstOrDefaultAsync(b => b.Id == basvuruId);

                if (basvuru == null)
                {
                    return BadRequest(new { success = false, message = "Başvuru bulunamadı." });
                }

                // Bu başvuru için zaten bir teklif mektubu var mı kontrol et
                var existingTeklif = await _context.TeklifMektuplari
                    .FirstOrDefaultAsync(tm => tm.BasvuruId == basvuruId && tm.Durum != "İptal Edildi");

                if (existingTeklif != null)
                {
                    return BadRequest(new { success = false, message = "Bu başvuru için zaten aktif bir teklif mektubu bulunmaktadır." });
                }

                var teklifMektubu = new TeklifMektubu
                {
                    BasvuruId = basvuruId,
                    Pozisyon = teklifData.TryGetProperty("pozisyon", out var pozisyon) ? pozisyon.GetString() ?? basvuru.Ilan.Baslik : basvuru.Ilan.Baslik,
                    Maas = teklifData.TryGetProperty("maas", out var maas) ? maas.GetDecimal() : 0,
                    EkOdemeler = teklifData.TryGetProperty("ekOdemeler", out var ekOdemeler) ? ekOdemeler.GetString() : null,
                    IzinHakki = teklifData.TryGetProperty("izinHakki", out var izin) ? izin.GetInt32() : 14,
                    IseBaslamaTarihi = teklifData.TryGetProperty("iseBaslamaTarihi", out var baslama) && DateTime.TryParse(baslama.GetString(), out var parsedBaslama) ? DateTime.SpecifyKind(parsedBaslama, DateTimeKind.Utc) : DateTime.UtcNow.AddDays(30),
                    GecerlilikTarihi = teklifData.TryGetProperty("gecerlilikTarihi", out var gecerlilik) && DateTime.TryParse(gecerlilik.GetString(), out var parsedGecerlilik) ? DateTime.SpecifyKind(parsedGecerlilik, DateTimeKind.Utc) : DateTime.UtcNow.AddDays(15),
                    Durum = "Beklemede",
                    RedNedeni = null,
                    HazirlayanId = int.Parse(User.FindFirst("PersonelId")?.Value ?? "1"),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.TeklifMektuplari.Add(teklifMektubu);
                await _context.SaveChangesAsync();

                return new { success = true, data = teklifMektubu.Id, message = "Teklif mektubu başarıyla oluşturuldu." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Teklif mektubu oluşturulurken hata oluştu: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<object>> UpdateTeklifMektubu(int id, [FromBody] JsonElement teklifData)
        {
            try
            {
                var teklifMektubu = await _context.TeklifMektuplari
                    .FirstOrDefaultAsync(tm => tm.Id == id);

                if (teklifMektubu == null)
                {
                    return NotFound(new { success = false, message = "Teklif mektubu bulunamadı." });
                }

                // Sadece Beklemede durumundaki teklifler düzenlenebilir
                if (teklifMektubu.Durum != "Beklemede")
                {
                    return BadRequest(new { success = false, message = "Sadece 'Beklemede' durumundaki teklif mektupları düzenlenebilir." });
                }

                // Alanları güncelle
                if (teklifData.TryGetProperty("pozisyon", out var pozisyon))
                    teklifMektubu.Pozisyon = pozisyon.GetString() ?? string.Empty;

                if (teklifData.TryGetProperty("maas", out var maas))
                    teklifMektubu.Maas = maas.GetDecimal();

                if (teklifData.TryGetProperty("ekOdemeler", out var ekOdemeler))
                    teklifMektubu.EkOdemeler = ekOdemeler.GetString();

                if (teklifData.TryGetProperty("izinHakki", out var izin))
                    teklifMektubu.IzinHakki = izin.GetInt32();

                if (teklifData.TryGetProperty("iseBaslamaTarihi", out var baslama) && DateTime.TryParse(baslama.GetString(), out var parsedBaslama))
                    teklifMektubu.IseBaslamaTarihi = DateTime.SpecifyKind(parsedBaslama, DateTimeKind.Utc);

                if (teklifData.TryGetProperty("gecerlilikTarihi", out var gecerlilik) && DateTime.TryParse(gecerlilik.GetString(), out var parsedGecerlilik))
                    teklifMektubu.GecerlilikTarihi = DateTime.SpecifyKind(parsedGecerlilik, DateTimeKind.Utc);

                teklifMektubu.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return new { success = true, message = "Teklif mektubu başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Teklif mektubu güncellenirken hata oluştu: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<object>> DeleteTeklifMektubu(int id)
        {
            try
            {
                var teklifMektubu = await _context.TeklifMektuplari.FindAsync(id);

                if (teklifMektubu == null)
                {
                    return NotFound(new { success = false, message = "Teklif mektubu bulunamadı." });
                }

                // Sadece Beklemede durumundaki teklifler silinebilir
                if (teklifMektubu.Durum != "Beklemede")
                {
                    return BadRequest(new { success = false, message = "Sadece 'Beklemede' durumundaki teklif mektupları silinebilir." });
                }

                _context.TeklifMektuplari.Remove(teklifMektubu);
                await _context.SaveChangesAsync();

                return new { success = true, message = "Teklif mektubu başarıyla silindi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Teklif mektubu silinirken hata oluştu: {ex.Message}" });
            }
        }

        [HttpPut("{id}/gonder")]
        public async Task<ActionResult<object>> TeklifiGonder(int id)
        {
            try
            {
                var teklifMektubu = await _context.TeklifMektuplari
                    .Include(tm => tm.Basvuru)
                        .ThenInclude(b => b.Aday)
                    .FirstOrDefaultAsync(tm => tm.Id == id);

                if (teklifMektubu == null)
                {
                    return NotFound(new { success = false, message = "Teklif mektubu bulunamadı." });
                }

                if (teklifMektubu.Durum != "Beklemede")
                {
                    return BadRequest(new { success = false, message = "Sadece 'Beklemede' durumundaki teklif mektupları gönderilebilir." });
                }

                // Gerekli alanların dolu olduğunu kontrol et
                if (string.IsNullOrEmpty(teklifMektubu.Pozisyon) ||
                    teklifMektubu.Maas <= 0 ||
                    teklifMektubu.GecerlilikTarihi <= DateTime.Now)
                {
                    return BadRequest(new { success = false, message = "Teklif mektubunda eksik veya hatalı bilgiler var. Lütfen kontrol edin." });
                }

                teklifMektubu.Durum = "Gönderildi";
                teklifMektubu.GonderimTarihi = DateTime.UtcNow;
                teklifMektubu.UpdatedAt = DateTime.UtcNow;

                // Başvuru durumunu güncelle
                teklifMektubu.Basvuru.Durum = BasvuruDurumu.TeklifVerildi;

                await _context.SaveChangesAsync();

                return new { success = true, message = "Teklif mektubu başarıyla gönderildi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Teklif mektubu gönderilirken hata oluştu: {ex.Message}" });
            }
        }

        [HttpPut("{id}/aday-yaniti")]
        public async Task<ActionResult<object>> AdayYanitiGuncelle(int id, [FromBody] JsonElement yanitData)
        {
            try
            {
                var teklifMektubu = await _context.TeklifMektuplari
                    .Include(tm => tm.Basvuru)
                    .FirstOrDefaultAsync(tm => tm.Id == id);

                if (teklifMektubu == null)
                {
                    return NotFound(new { success = false, message = "Teklif mektubu bulunamadı." });
                }

                if (teklifMektubu.Durum != "Gönderildi")
                {
                    return BadRequest(new { success = false, message = "Sadece gönderilmiş teklif mektupları için aday yanıtı güncellenebilir." });
                }

                var yanit = yanitData.GetProperty("adayYaniti").GetString();
                var redNedeni = yanitData.TryGetProperty("redNedeni", out var neden) ? neden.GetString() : null;

                switch (yanit)
                {
                    case "KabulEtti":
                        teklifMektubu.Durum = "Kabul Edildi";
                        teklifMektubu.Basvuru.Durum = BasvuruDurumu.IseAlindi;
                        break;
                    case "Reddetti":
                        teklifMektubu.Durum = "Reddedildi";
                        teklifMektubu.RedNedeni = redNedeni;
                        teklifMektubu.Basvuru.Durum = BasvuruDurumu.AdayVazgecti;
                        break;
                    default:
                        return BadRequest(new { success = false, message = "Geçersiz aday yanıtı." });
                }

                teklifMektubu.YanitTarihi = DateTime.UtcNow;
                teklifMektubu.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return new { success = true, message = "Aday yanıtı başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Aday yanıtı güncellenirken hata oluştu: {ex.Message}" });
            }
        }

        [HttpGet("basvuru/{basvuruId}")]
        public async Task<ActionResult<object>> GetBasvuruTeklifi(int basvuruId)
        {
            try
            {
                var teklifMektubu = await _context.TeklifMektuplari
                    .Include(tm => tm.Basvuru)
                        .ThenInclude(b => b.Aday)
                    .Include(tm => tm.Basvuru)
                        .ThenInclude(b => b.Ilan)
                    .Include(tm => tm.Hazirlayan)
                    .FirstOrDefaultAsync(tm => tm.BasvuruId == basvuruId && tm.Durum != "İptal Edildi");

                if (teklifMektubu == null)
                {
                    return NotFound(new { success = false, message = "Bu başvuru için teklif mektubu bulunamadı." });
                }

                var result = new
                {
                    teklifMektubu.Id,
                    teklifMektubu.BasvuruId,
                    teklifMektubu.Pozisyon,
                    teklifMektubu.Maas,
                    teklifMektubu.EkOdemeler,
                    teklifMektubu.IzinHakki,
                    teklifMektubu.IseBaslamaTarihi,
                    teklifMektubu.GecerlilikTarihi,
                    teklifMektubu.Durum,
                    teklifMektubu.GonderimTarihi,
                    teklifMektubu.YanitTarihi,
                    teklifMektubu.RedNedeni,
                    AdayAdi = teklifMektubu.Basvuru.Aday.Ad + " " + teklifMektubu.Basvuru.Aday.Soyad,
                    IlanBasligi = teklifMektubu.Basvuru.Ilan.Baslik,
                    HazirlayanAdi = teklifMektubu.Hazirlayan.Ad + " " + teklifMektubu.Hazirlayan.Soyad
                };

                return new { success = true, data = result, message = "Teklif mektubu başarıyla getirildi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Teklif mektubu getirilirken hata oluştu: {ex.Message}" });
            }
        }

        [HttpGet("istatistik")]
        public async Task<ActionResult<object>> GetTeklifIstatistikleri()
        {
            try
            {
                var toplamTeklif = await _context.TeklifMektuplari.Where(tm => tm.Durum != "İptal Edildi").CountAsync();
                var beklemedeTeklif = await _context.TeklifMektuplari.Where(tm => tm.Durum == "Beklemede").CountAsync();
                var gonderilmisTeklif = await _context.TeklifMektuplari.Where(tm => tm.Durum == "Gönderildi").CountAsync();
                var kabulEdilenTeklif = await _context.TeklifMektuplari.Where(tm => tm.Durum == "Kabul Edildi").CountAsync();
                var reddilenTeklif = await _context.TeklifMektuplari.Where(tm => tm.Durum == "Reddedildi").CountAsync();

                var kabulOrani = toplamTeklif > 0 ? Math.Round((double)kabulEdilenTeklif / toplamTeklif * 100, 2) : 0;
                var redOrani = toplamTeklif > 0 ? Math.Round((double)reddilenTeklif / toplamTeklif * 100, 2) : 0;

                var istatistikler = new
                {
                    toplamTeklif = toplamTeklif,
                    beklemedeTeklif = beklemedeTeklif,
                    gonderilmisTeklif = gonderilmisTeklif,
                    kabulEdilenTeklif = kabulEdilenTeklif,
                    reddilenTeklif = reddilenTeklif,
                    kabulOrani = kabulOrani,
                    redOrani = redOrani
                };

                return new { success = true, data = istatistikler, message = "Teklif istatistikleri başarıyla getirildi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Teklif istatistikleri getirilirken hata oluştu: {ex.Message}" });
            }
        }
    }
}