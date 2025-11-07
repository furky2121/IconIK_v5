using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using IconIK.API.Data;
using IconIK.API.Models;
using IconIK.API.Services;
using System.Security.Claims;
using System.Text.Json;

namespace IconIK.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class VideoEgitimController : ControllerBase
    {
        private readonly IconIKContext _context;
        private readonly IVideoEgitimService _videoEgitimService;

        public VideoEgitimController(IconIKContext context, IVideoEgitimService videoEgitimService)
        {
            _context = context;
            _videoEgitimService = videoEgitimService;
        }

        // Kategorileri getir
        [HttpGet("kategoriler")]
        public async Task<IActionResult> GetKategoriler()
        {
            try
            {
                var kategoriler = await _videoEgitimService.GetKategorilerAsync();
                return Ok(new { success = true, data = kategoriler });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Kategori ekle/güncelle
        [HttpPost("kategori")]
        public async Task<IActionResult> SaveKategori()
        {
            Console.WriteLine($"=== REQUEST: POST /api/VideoEgitim/kategori ===");
            
            try
            {
                // Raw JSON parsing like other controllers
                using var reader = new StreamReader(Request.Body);
                var json = await reader.ReadToEndAsync();
                Console.WriteLine($"Raw JSON: {json}");
                
                var jsonElement = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(json);
                Console.WriteLine($"Parsed JSON: {jsonElement}");

                var kategori = new VideoKategori();
                
                // Parse fields manually
                if (jsonElement.TryGetProperty("id", out var idProp) && idProp.ValueKind != JsonValueKind.Null)
                    kategori.Id = idProp.GetInt32();
                
                if (jsonElement.TryGetProperty("ad", out var adProp))
                    kategori.Ad = adProp.GetString();
                    
                if (jsonElement.TryGetProperty("aciklama", out var aciklamaProp))
                    kategori.Aciklama = aciklamaProp.GetString();
                    
                if (jsonElement.TryGetProperty("icon", out var iconProp))
                    kategori.Icon = iconProp.GetString();
                    
                if (jsonElement.TryGetProperty("renk", out var renkProp))
                    kategori.Renk = renkProp.GetString();
                    
                if (jsonElement.TryGetProperty("sira", out var siraProp))
                    kategori.Sira = siraProp.GetInt32();
                    
                if (jsonElement.TryGetProperty("aktif", out var aktifProp))
                    kategori.Aktif = aktifProp.GetBoolean();

                Console.WriteLine($"Kategori parsed: Id={kategori.Id}, Ad={kategori.Ad}, Aktif={kategori.Aktif}");

                if (string.IsNullOrEmpty(kategori.Ad))
                {
                    return BadRequest(new { success = false, message = "Kategori adı zorunludur" });
                }

                if (kategori.Id == 0)
                {
                    // Yeni kategori
                    kategori.OlusturmaTarihi = DateTime.UtcNow;
                    _context.VideoKategoriler.Add(kategori);
                }
                else
                {
                    // Mevcut kategori güncelleme
                    var existingKategori = await _context.VideoKategoriler.FindAsync(kategori.Id);
                    if (existingKategori == null)
                    {
                        return NotFound(new { success = false, message = "Kategori bulunamadı" });
                    }

                    // Sadece güncellenebilir alanları kopyala
                    existingKategori.Ad = kategori.Ad;
                    existingKategori.Aciklama = kategori.Aciklama;
                    existingKategori.Icon = kategori.Icon;
                    existingKategori.Renk = kategori.Renk;
                    existingKategori.Sira = kategori.Sira;
                    existingKategori.Aktif = kategori.Aktif;
                    
                    _context.VideoKategoriler.Update(existingKategori);
                }

                await _context.SaveChangesAsync();
                Console.WriteLine($"=== RESPONSE: 200 - Kategori kaydedildi ===");
                return Ok(new { success = true, data = kategori, message = "Kategori kaydedildi" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== ERROR: {ex.Message} ===");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Kategori sil
        [HttpDelete("kategori/{id}")]
        public async Task<IActionResult> DeleteKategori(int id)
        {
            Console.WriteLine($"=== REQUEST: DELETE /api/VideoEgitim/kategori/{id} ===");
            
            try
            {
                var kategori = await _context.VideoKategoriler.FindAsync(id);
                if (kategori == null)
                {
                    return NotFound(new { success = false, message = "Kategori bulunamadı" });
                }

                // Bu kategoriye bağlı video eğitimi var mı kontrol et
                var hasVideos = await _context.VideoEgitimler.AnyAsync(v => v.KategoriId == id);
                if (hasVideos)
                {
                    return BadRequest(new { success = false, message = "Bu kategori silinemez. Önce bu kategoriye ait video eğitimlerini başka bir kategoriye taşıyın." });
                }

                _context.VideoKategoriler.Remove(kategori);
                await _context.SaveChangesAsync();

                Console.WriteLine($"=== RESPONSE: 200 - Kategori silindi ===");
                return Ok(new { success = true, message = "Kategori silindi" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== ERROR: {ex.Message} ===");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Tüm eğitimleri getir (admin için)
        [HttpGet("tumegitimler")]
        public async Task<IActionResult> GetTumEgitimler()
        {
            try
            {
                var egitimler = await _context.VideoEgitimler
                    .Include(e => e.Kategori)
                    .OrderByDescending(e => e.OlusturmaTarihi)
                    .Select(e => new
                    {
                        e.Id,
                        e.Baslik,
                        e.Aciklama,
                        e.VideoUrl,
                        e.ThumbnailUrl,
                        e.Sure,
                        e.Seviye,
                        e.Egitmen,
                        e.ZorunluMu,
                        e.Aktif,
                        e.IzlenmeMinimum,
                        e.SonTamamlanmaTarihi,
                        KategoriAd = e.Kategori.Ad,
                        e.KategoriId,
                        ToplamIzlenme = e.VideoIzlemeler.Count(),
                        TamamlanmaSayisi = e.VideoIzlemeler.Count(i => i.TamamlandiMi),
                        OrtalamaPuan = e.VideoIzlemeler.Where(i => i.Puan.HasValue).Average(i => i.Puan) ?? 0
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = egitimler });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Kategoriye göre eğitimleri getir
        [HttpGet("kategori/{kategoriId}")]
        public async Task<IActionResult> GetEgitimlerByKategori(int kategoriId)
        {
            try
            {
                var egitimler = await _videoEgitimService.GetEgitimlerByKategoriAsync(kategoriId);
                return Ok(new { success = true, data = egitimler });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Personelin eğitimlerini getir
        [HttpGet("benim-egitimlerim")]
        public async Task<IActionResult> GetBenimEgitimlerim()
        {
            try
            {
                var kullaniciIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(kullaniciIdClaim))
                    return Unauthorized(new { success = false, message = "Kullanıcı bilgisi bulunamadı" });

                var kullaniciId = int.Parse(kullaniciIdClaim);
                var kullanici = await _context.Kullanicilar
                    .Include(k => k.Personel)
                    .FirstOrDefaultAsync(k => k.Id == kullaniciId);

                if (kullanici?.Personel == null)
                    return BadRequest(new { success = false, message = "Personel bilgisi bulunamadı" });

                var egitimler = await _videoEgitimService.GetPersonelEgitimleriAsync(kullanici.Personel.Id);
                
                // Atama bilgilerini de al
                var atamalar = await _context.VideoAtamalar
                    .Where(a => a.PersonelId == kullanici.Personel.Id)
                    .ToListAsync();
                
                // İzleme durumlarını ekle ve thumbnail URL'lerini otomatik oluştur
                var egitimlerWithProgress = egitimler.Select(e => {
                    // En güncel izleme kaydını al
                    var izlemeKaydi = e.VideoIzlemeler
                        .OrderByDescending(i => i.IzlemeBaslangic)
                        .FirstOrDefault();
                    var atama = atamalar.FirstOrDefault(a => a.VideoEgitimId == e.Id);
                    var tamamlandiMi = izlemeKaydi?.TamamlandiMi == true || atama?.Durum == "Tamamlandı";
                    
                    return new
                    {
                        e.Id,
                        e.Baslik,
                        e.Aciklama,
                        e.VideoUrl,
                        // Thumbnail URL yoksa video URL'sinden otomatik oluştur
                        ThumbnailUrl = !string.IsNullOrEmpty(e.ThumbnailUrl) ? e.ThumbnailUrl : GetAutoThumbnailUrl(e.VideoUrl),
                        e.Sure,
                        e.Seviye,
                        e.Egitmen,
                        e.ZorunluMu,
                        e.IzlenmeMinimum,
                        e.SonTamamlanmaTarihi,
                        KategoriAd = e.Kategori?.Ad,
                        e.KategoriId,
                        IzlemeKaydi = izlemeKaydi,
                        TamamlandiMi = tamamlandiMi,
                        IzlemeYuzdesi = izlemeKaydi?.IzlemeYuzdesi ?? 0,
                        AtamaDurumu = atama?.Durum ?? "Atandı"
                    };
                }).ToList();

                return Ok(new { success = true, data = egitimlerWithProgress });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Atamalar listesi
        [HttpGet("atamalar")]
        public async Task<IActionResult> GetAtamalar()
        {
            Console.WriteLine($"=== GET /api/VideoEgitim/atamalar ===");
            try
            {
                var atamalar = await _context.VideoAtamalar
                    .Include(a => a.VideoEgitim)
                    .Include(a => a.Personel)
                        .ThenInclude(p => p.Pozisyon)
                        .ThenInclude(poz => poz.Departman)
                    .Include(a => a.Departman)
                    .Include(a => a.Pozisyon)
                        .ThenInclude(poz => poz.Departman)
                    .Include(a => a.AtayanPersonel)
                    .OrderByDescending(a => a.AtamaTarihi)
                    .Select(a => new
                    {
                        a.Id,
                        VideoEgitimAd = a.VideoEgitim.Baslik,
                        PersonelAd = a.Personel != null ? $"{a.Personel.Ad} {a.Personel.Soyad}" : null,
                        // For personnel assignments, get department and position from their assigned position
                        // For department/position assignments, get from the assignment itself
                        DepartmanAd = a.PersonelId != null && a.Personel != null && a.Personel.Pozisyon != null && a.Personel.Pozisyon.Departman != null
                            ? a.Personel.Pozisyon.Departman.Ad
                            : (a.Departman != null ? a.Departman.Ad : (a.Pozisyon != null && a.Pozisyon.Departman != null ? a.Pozisyon.Departman.Ad : null)),
                        PozisyonAd = a.PersonelId != null && a.Personel != null && a.Personel.Pozisyon != null
                            ? a.Personel.Pozisyon.Ad
                            : (a.Pozisyon != null ? a.Pozisyon.Ad : null),
                        AtayanPersonelAd = a.AtayanPersonel != null ? $"{a.AtayanPersonel.Ad} {a.AtayanPersonel.Soyad}" : null,
                        a.AtamaTarihi,
                        a.Durum,
                        a.Not
                    })
                    .ToListAsync();

                Console.WriteLine($"Atamalar count: {atamalar.Count}");
                foreach (var atama in atamalar.Take(3))
                {
                    Console.WriteLine($"ID: {atama.Id}, VideoEgitim: {atama.VideoEgitimAd}, Personel: {atama.PersonelAd}, Departman: {atama.DepartmanAd}, Pozisyon: {atama.PozisyonAd}");
                }

                return Ok(new { success = true, data = atamalar });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Sertifikalar listesi
        [HttpGet("sertifikalar")]
        public async Task<IActionResult> GetSertifikalar()
        {
            try
            {
                var kullaniciIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(kullaniciIdClaim))
                    return Unauthorized(new { success = false, message = "Kullanıcı bilgisi bulunamadı" });

                var kullaniciId = int.Parse(kullaniciIdClaim);
                var kullanici = await _context.Kullanicilar
                    .Include(k => k.Personel)
                    .FirstOrDefaultAsync(k => k.Id == kullaniciId);

                if (kullanici?.Personel == null)
                    return BadRequest(new { success = false, message = "Personel bilgisi bulunamadı" });

                var sertifikalar = await _context.VideoSertifikalar
                    .Include(s => s.VideoEgitim)
                        .ThenInclude(ve => ve.Kategori)
                    .Include(s => s.Personel)
                    .Where(s => s.PersonelId == kullanici.Personel.Id)
                    .OrderByDescending(s => s.VerilisTarihi)
                    .Select(s => new
                    {
                        s.Id,
                        s.SertifikaNo,
                        EgitimAd = s.VideoEgitim.Baslik,
                        PersonelAd = $"{s.Personel.Ad} {s.Personel.Soyad}",
                        s.VerilisTarihi,
                        s.GecerlilikTarihi,
                        Durum = s.GecerlilikTarihi.HasValue && s.GecerlilikTarihi < DateTime.UtcNow ? "Süresi Dolmuş" : s.Durum,
                        s.IzlemeYuzdesi,
                        ToplamSure = s.VideoEgitim.Sure,
                        KategoriAd = s.VideoEgitim.Kategori != null ? s.VideoEgitim.Kategori.Ad : "Bilinmiyor"
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = sertifikalar });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Eğitim detayını getir
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEgitimDetay(int id)
        {
            Console.WriteLine($"=== GET /api/VideoEgitim/{id} ===");
            try
            {
                var kullaniciIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Console.WriteLine($"KullaniciId from claim: {kullaniciIdClaim}");
                int? personelId = null;

                if (!string.IsNullOrEmpty(kullaniciIdClaim))
                {
                    var kullaniciId = int.Parse(kullaniciIdClaim);
                    var kullanici = await _context.Kullanicilar
                        .Include(k => k.Personel)
                        .FirstOrDefaultAsync(k => k.Id == kullaniciId);
                    
                    personelId = kullanici?.Personel?.Id;
                }

                var detay = await _videoEgitimService.GetEgitimDetayAsync(id, personelId);
                Console.WriteLine($"Detay from service: {detay != null}");
                
                // Check if service returned an error response
                if (detay is object errorResponse && 
                    errorResponse.GetType().GetProperty("error")?.GetValue(errorResponse)?.Equals(true) == true)
                {
                    var message = errorResponse.GetType().GetProperty("message")?.GetValue(errorResponse)?.ToString();
                    Console.WriteLine($"Service returned error: {message}");
                    return NotFound(new { success = false, message = message ?? "Eğitim bulunamadı" });
                }

                Console.WriteLine("Returning success response");
                return Ok(new { success = true, data = detay });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Eğitim ekle/güncelle
        [HttpPost("egitim")]
        public async Task<IActionResult> SaveEgitim()
        {
            Console.WriteLine($"=== REQUEST: POST /api/VideoEgitim/egitim ===");
            
            try
            {
                // Raw JSON parsing
                using var reader = new StreamReader(Request.Body);
                var json = await reader.ReadToEndAsync();
                Console.WriteLine($"Raw JSON: {json}");
                
                var jsonElement = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(json);
                Console.WriteLine($"Parsed JSON: {jsonElement}");

                var egitim = new VideoEgitim();
                
                // Parse fields manually
                if (jsonElement.TryGetProperty("id", out var idProp) && idProp.ValueKind != JsonValueKind.Null)
                    egitim.Id = idProp.GetInt32();
                
                if (jsonElement.TryGetProperty("baslik", out var baslikProp))
                    egitim.Baslik = baslikProp.GetString();
                    
                if (jsonElement.TryGetProperty("aciklama", out var aciklamaProp))
                    egitim.Aciklama = aciklamaProp.GetString();
                    
                if (jsonElement.TryGetProperty("videoUrl", out var videoUrlProp))
                    egitim.VideoUrl = videoUrlProp.GetString();
                    
                if (jsonElement.TryGetProperty("thumbnailUrl", out var thumbnailProp))
                    egitim.ThumbnailUrl = thumbnailProp.GetString();
                
                // Eğer thumbnail URL boşsa, video URL'sinden otomatik oluştur
                if (string.IsNullOrEmpty(egitim.ThumbnailUrl))
                {
                    egitim.ThumbnailUrl = GetAutoThumbnailUrl(egitim.VideoUrl);
                }
                
                // Hala boşsa boş string ata (not null constraint için)
                if (string.IsNullOrEmpty(egitim.ThumbnailUrl))
                {
                    egitim.ThumbnailUrl = "";
                }
                    
                // Frontend sends "sureDakika", backend expects "sure"
                if (jsonElement.TryGetProperty("sureDakika", out var sureProp))
                    egitim.Sure = sureProp.GetInt32();
                else if (jsonElement.TryGetProperty("sure", out sureProp))
                    egitim.Sure = sureProp.GetInt32();
                    
                if (jsonElement.TryGetProperty("kategoriId", out var kategoriIdProp))
                    egitim.KategoriId = kategoriIdProp.GetInt32();
                    
                if (jsonElement.TryGetProperty("seviye", out var seviyeProp))
                    egitim.Seviye = seviyeProp.GetString();
                    
                if (jsonElement.TryGetProperty("egitmen", out var egitmenProp))
                    egitim.Egitmen = egitmenProp.GetString();
                    
                if (jsonElement.TryGetProperty("izlenmeMinimum", out var izlenmeProp))
                    egitim.IzlenmeMinimum = izlenmeProp.GetInt32();
                    
                if (jsonElement.TryGetProperty("zorunluMu", out var zorunluProp))
                    egitim.ZorunluMu = zorunluProp.GetBoolean();
                    
                if (jsonElement.TryGetProperty("sonTamamlanmaTarihi", out var sonTarihProp) && sonTarihProp.ValueKind != JsonValueKind.Null)
                {
                    if (DateTime.TryParse(sonTarihProp.GetString(), out var sonTarih))
                        egitim.SonTamamlanmaTarihi = sonTarih;
                }
                    
                if (jsonElement.TryGetProperty("aktif", out var aktifProp))
                    egitim.Aktif = aktifProp.GetBoolean();

                Console.WriteLine($"VideoEgitim parsed: Id={egitim.Id}, Baslik={egitim.Baslik}, KategoriId={egitim.KategoriId}");

                if (string.IsNullOrEmpty(egitim.Baslik))
                {
                    return BadRequest(new { success = false, message = "Eğitim başlığı zorunludur" });
                }

                if (string.IsNullOrEmpty(egitim.VideoUrl))
                {
                    return BadRequest(new { success = false, message = "Video URL'si zorunludur" });
                }

                if (egitim.KategoriId <= 0)
                {
                    return BadRequest(new { success = false, message = "Kategori seçimi zorunludur" });
                }

                if (egitim.Id == 0)
                {
                    // Yeni eğitim
                    egitim.OlusturmaTarihi = DateTime.UtcNow;
                    _context.VideoEgitimler.Add(egitim);
                }
                else
                {
                    // Mevcut eğitim güncelleme
                    var existingEgitim = await _context.VideoEgitimler.FindAsync(egitim.Id);
                    if (existingEgitim == null)
                    {
                        return NotFound(new { success = false, message = "Eğitim bulunamadı" });
                    }

                    // Sadece güncellenebilir alanları kopyala
                    existingEgitim.Baslik = egitim.Baslik;
                    existingEgitim.Aciklama = egitim.Aciklama;
                    existingEgitim.VideoUrl = egitim.VideoUrl;
                    
                    // Thumbnail URL kontrolü
                    if (!string.IsNullOrEmpty(egitim.ThumbnailUrl))
                    {
                        existingEgitim.ThumbnailUrl = egitim.ThumbnailUrl;
                    }
                    else
                    {
                        // Otomatik thumbnail oluştur
                        var autoThumbnail = GetAutoThumbnailUrl(egitim.VideoUrl);
                        existingEgitim.ThumbnailUrl = !string.IsNullOrEmpty(autoThumbnail) ? autoThumbnail : "";
                    }
                    
                    existingEgitim.Sure = egitim.Sure;
                    existingEgitim.KategoriId = egitim.KategoriId;
                    existingEgitim.Seviye = egitim.Seviye;
                    existingEgitim.Egitmen = egitim.Egitmen;
                    existingEgitim.IzlenmeMinimum = egitim.IzlenmeMinimum;
                    existingEgitim.ZorunluMu = egitim.ZorunluMu;
                    existingEgitim.SonTamamlanmaTarihi = egitim.SonTamamlanmaTarihi;
                    existingEgitim.Aktif = egitim.Aktif;
                    existingEgitim.GuncellemeTarihi = DateTime.UtcNow;
                    
                    _context.VideoEgitimler.Update(existingEgitim);
                }

                await _context.SaveChangesAsync();
                Console.WriteLine($"=== RESPONSE: 200 - VideoEgitim kaydedildi ===");
                
                // Return the appropriate entity
                var responseData = egitim.Id == 0 ? egitim : await _context.VideoEgitimler.FindAsync(egitim.Id);
                return Ok(new { success = true, data = responseData, message = "Eğitim kaydedildi" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== ERROR: {ex.Message} ===");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Eğitim ataması yap
        [HttpPost("atama")]
        public async Task<IActionResult> AtamaYap()
        {
            Console.WriteLine($"=== REQUEST: POST /api/VideoEgitim/atama ===");
            
            try
            {
                using var reader = new StreamReader(Request.Body);
                var json = await reader.ReadToEndAsync();
                Console.WriteLine($"Raw JSON: {json}");
                
                var jsonElement = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(json);
                Console.WriteLine($"Parsed JSON: {jsonElement}");
                
                var model = new AtamaModel();
                
                if (jsonElement.TryGetProperty("videoEgitimId", out var videoEgitimIdProp) && videoEgitimIdProp.ValueKind != JsonValueKind.Null)
                    model.VideoEgitimId = videoEgitimIdProp.GetInt32();
                
                // PersonelIds - array parsing
                if (jsonElement.TryGetProperty("personelIds", out var personelIdsProp) && personelIdsProp.ValueKind == JsonValueKind.Array)
                {
                    model.PersonelIds = new List<int>();
                    foreach (var item in personelIdsProp.EnumerateArray())
                    {
                        if (item.ValueKind != JsonValueKind.Null)
                            model.PersonelIds.Add(item.GetInt32());
                    }
                }
                
                // DepartmanIds - array parsing
                if (jsonElement.TryGetProperty("departmanIds", out var departmanIdsProp) && departmanIdsProp.ValueKind == JsonValueKind.Array)
                {
                    model.DepartmanIds = new List<int>();
                    foreach (var item in departmanIdsProp.EnumerateArray())
                    {
                        if (item.ValueKind != JsonValueKind.Null)
                            model.DepartmanIds.Add(item.GetInt32());
                    }
                }
                
                // PozisyonIds - array parsing
                if (jsonElement.TryGetProperty("pozisyonIds", out var pozisyonIdsProp) && pozisyonIdsProp.ValueKind == JsonValueKind.Array)
                {
                    model.PozisyonIds = new List<int>();
                    foreach (var item in pozisyonIdsProp.EnumerateArray())
                    {
                        if (item.ValueKind != JsonValueKind.Null)
                            model.PozisyonIds.Add(item.GetInt32());
                    }
                }
                    
                if (jsonElement.TryGetProperty("not", out var notProp))
                    model.Not = notProp.GetString() ?? "";
                
                var kullaniciIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int? atayanPersonelId = null;
                
                if (!string.IsNullOrEmpty(kullaniciIdClaim))
                {
                    var kullaniciId = int.Parse(kullaniciIdClaim);
                    var kullanici = await _context.Kullanicilar
                        .Include(k => k.Personel)
                        .FirstOrDefaultAsync(k => k.Id == kullaniciId);
                    
                    atayanPersonelId = kullanici?.Personel?.Id;
                }

                Console.WriteLine($"Atama parsed: VideoEgitimId={model.VideoEgitimId}, PersonelIds={model.PersonelIds?.Count ?? 0}, DepartmanIds={model.DepartmanIds?.Count ?? 0}, PozisyonIds={model.PozisyonIds?.Count ?? 0}");

                // Validate required fields
                if (model.VideoEgitimId <= 0)
                {
                    return BadRequest(new { success = false, message = "Video Eğitim ID zorunludur" });
                }

                // At least one target must be specified
                var hasPersonel = model.PersonelIds != null && model.PersonelIds.Any();
                var hasDepartman = model.DepartmanIds != null && model.DepartmanIds.Any();
                var hasPozisyon = model.PozisyonIds != null && model.PozisyonIds.Any();
                
                if (!hasPersonel && !hasDepartman && !hasPozisyon)
                {
                    return BadRequest(new { success = false, message = "En az bir hedef (Personel, Departman veya Pozisyon) seçilmelidir" });
                }

                var atamalar = new List<VideoAtama>();

                // Personellere atama
                if (hasPersonel)
                {
                    foreach (var personelId in model.PersonelIds)
                    {
                        var atama = new VideoAtama
                        {
                            VideoEgitimId = model.VideoEgitimId,
                            PersonelId = personelId,
                            AtamaTarihi = DateTime.UtcNow,
                            Durum = "Atandı",
                            AtayanPersonelId = atayanPersonelId,
                            Not = model.Not
                        };
                        atamalar.Add(atama);
                    }
                }

                // Departmanlara atama
                if (hasDepartman)
                {
                    foreach (var departmanId in model.DepartmanIds)
                    {
                        var atama = new VideoAtama
                        {
                            VideoEgitimId = model.VideoEgitimId,
                            DepartmanId = departmanId,
                            AtamaTarihi = DateTime.UtcNow,
                            Durum = "Atandı",
                            AtayanPersonelId = atayanPersonelId,
                            Not = model.Not
                        };
                        atamalar.Add(atama);
                    }
                }

                // Pozisyonlara atama
                if (hasPozisyon)
                {
                    foreach (var pozisyonId in model.PozisyonIds)
                    {
                        var atama = new VideoAtama
                        {
                            VideoEgitimId = model.VideoEgitimId,
                            PozisyonId = pozisyonId,
                            AtamaTarihi = DateTime.UtcNow,
                            Durum = "Atandı",
                            AtayanPersonelId = atayanPersonelId,
                            Not = model.Not
                        };
                        atamalar.Add(atama);
                    }
                }

                _context.VideoAtamalar.AddRange(atamalar);
                await _context.SaveChangesAsync();

                Console.WriteLine($"=== RESPONSE: 200 - {atamalar.Count} atama yapıldı ===");
                return Ok(new { success = true, data = atamalar.Count, message = $"{atamalar.Count} adet atama yapıldı" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== ERROR: {ex.Message} ===");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Toplu atama yap
        [HttpPost("toplu-atama")]
        public async Task<IActionResult> TopluAtamaYap()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var json = await reader.ReadToEndAsync();
                var jsonElement = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(json);
                
                var model = new TopluAtamaModel();
                
                // VideoEgitimIds - array parsing
                if (jsonElement.TryGetProperty("videoEgitimIds", out var videoEgitimIdsProp) && videoEgitimIdsProp.ValueKind == JsonValueKind.Array)
                {
                    model.VideoEgitimIds = new List<int>();
                    foreach (var item in videoEgitimIdsProp.EnumerateArray())
                    {
                        if (item.ValueKind != JsonValueKind.Null)
                            model.VideoEgitimIds.Add(item.GetInt32());
                    }
                }
                
                // PersonelIds - array parsing
                if (jsonElement.TryGetProperty("personelIds", out var personelIdsProp) && personelIdsProp.ValueKind == JsonValueKind.Array)
                {
                    model.PersonelIds = new List<int>();
                    foreach (var item in personelIdsProp.EnumerateArray())
                    {
                        if (item.ValueKind != JsonValueKind.Null)
                            model.PersonelIds.Add(item.GetInt32());
                    }
                }
                
                // DepartmanIds - array parsing
                if (jsonElement.TryGetProperty("departmanIds", out var departmanIdsProp) && departmanIdsProp.ValueKind == JsonValueKind.Array)
                {
                    model.DepartmanIds = new List<int>();
                    foreach (var item in departmanIdsProp.EnumerateArray())
                    {
                        if (item.ValueKind != JsonValueKind.Null)
                            model.DepartmanIds.Add(item.GetInt32());
                    }
                }
                
                // PozisyonIds - array parsing  
                if (jsonElement.TryGetProperty("pozisyonIds", out var pozisyonIdsProp) && pozisyonIdsProp.ValueKind == JsonValueKind.Array)
                {
                    model.PozisyonIds = new List<int>();
                    foreach (var item in pozisyonIdsProp.EnumerateArray())
                    {
                        if (item.ValueKind != JsonValueKind.Null)
                            model.PozisyonIds.Add(item.GetInt32());
                    }
                }
                    
                if (jsonElement.TryGetProperty("not", out var notProp))
                    model.Not = notProp.GetString() ?? "";
                
                var kullaniciIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int? atayanPersonelId = null;
                
                if (!string.IsNullOrEmpty(kullaniciIdClaim))
                {
                    var kullaniciId = int.Parse(kullaniciIdClaim);
                    var kullanici = await _context.Kullanicilar
                        .Include(k => k.Personel)
                        .FirstOrDefaultAsync(k => k.Id == kullaniciId);
                    
                    atayanPersonelId = kullanici?.Personel?.Id;
                }

                var atamalar = new List<VideoAtama>();

                // Departmanlara atama
                if (model.DepartmanIds != null && model.DepartmanIds.Any())
                {
                    foreach (var departmanId in model.DepartmanIds)
                    {
                        foreach (var egitimId in model.VideoEgitimIds)
                        {
                            var atama = new VideoAtama
                            {
                                VideoEgitimId = egitimId,
                                DepartmanId = departmanId,
                                AtamaTarihi = DateTime.UtcNow,
                                Durum = "Atandı",
                                AtayanPersonelId = atayanPersonelId,
                                Not = model.Not
                            };
                            atamalar.Add(atama);
                        }
                    }
                }

                // Pozisyonlara atama
                if (model.PozisyonIds != null && model.PozisyonIds.Any())
                {
                    foreach (var pozisyonId in model.PozisyonIds)
                    {
                        foreach (var egitimId in model.VideoEgitimIds)
                        {
                            var atama = new VideoAtama
                            {
                                VideoEgitimId = egitimId,
                                PozisyonId = pozisyonId,
                                AtamaTarihi = DateTime.UtcNow,
                                Durum = "Atandı",
                                AtayanPersonelId = atayanPersonelId,
                                Not = model.Not
                            };
                            atamalar.Add(atama);
                        }
                    }
                }

                // Personellere atama
                if (model.PersonelIds != null && model.PersonelIds.Any())
                {
                    foreach (var personelId in model.PersonelIds)
                    {
                        foreach (var egitimId in model.VideoEgitimIds)
                        {
                            var atama = new VideoAtama
                            {
                                VideoEgitimId = egitimId,
                                PersonelId = personelId,
                                AtamaTarihi = DateTime.UtcNow,
                                Durum = "Atandı",
                                AtayanPersonelId = atayanPersonelId,
                                Not = model.Not
                            };
                            atamalar.Add(atama);
                        }
                    }
                }

                _context.VideoAtamalar.AddRange(atamalar);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, data = atamalar.Count, message = $"{atamalar.Count} adet atama yapıldı" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // İzleme kaydı
        [HttpPost("izleme")]
        public async Task<IActionResult> IzlemeKaydet()
        {
            Console.WriteLine($"=== REQUEST: POST /api/VideoEgitim/izleme ===");
            
            try
            {
                // Raw JSON parsing
                using var reader = new StreamReader(Request.Body);
                var json = await reader.ReadToEndAsync();
                Console.WriteLine($"Raw JSON: {json}");
                
                var jsonElement = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(json);
                Console.WriteLine($"Parsed JSON: {jsonElement}");

                var kullaniciIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(kullaniciIdClaim))
                    return Unauthorized(new { success = false, message = "Kullanıcı bilgisi bulunamadı" });

                var kullaniciId = int.Parse(kullaniciIdClaim);
                var kullanici = await _context.Kullanicilar
                    .Include(k => k.Personel)
                    .FirstOrDefaultAsync(k => k.Id == kullaniciId);

                if (kullanici?.Personel == null)
                    return BadRequest(new { success = false, message = "Personel bilgisi bulunamadı" });

                // Parse fields manually from JSON
                var model = new IzlemeKayitModel();
                
                if (jsonElement.TryGetProperty("videoEgitimId", out var videoEgitimIdProp) && videoEgitimIdProp.ValueKind != JsonValueKind.Null)
                    model.VideoEgitimId = videoEgitimIdProp.GetInt32();
                
                if (jsonElement.TryGetProperty("toplamIzlenenSure", out var toplamIzlenenSureProp) && toplamIzlenenSureProp.ValueKind != JsonValueKind.Null)
                    model.ToplamIzlenenSure = toplamIzlenenSureProp.GetInt32();
                
                if (jsonElement.TryGetProperty("izlemeYuzdesi", out var izlemeYuzdesiProp) && izlemeYuzdesiProp.ValueKind != JsonValueKind.Null)
                    model.IzlemeYuzdesi = izlemeYuzdesiProp.GetInt32();
                
                if (jsonElement.TryGetProperty("tamamlandiMi", out var tamamlandiMiProp) && tamamlandiMiProp.ValueKind != JsonValueKind.Null)
                    model.TamamlandiMi = tamamlandiMiProp.GetBoolean();
                
                if (jsonElement.TryGetProperty("cihazTipi", out var cihazTipiProp))
                    model.CihazTipi = cihazTipiProp.GetString() ?? "Web";
                
                if (jsonElement.TryGetProperty("videoPlatform", out var videoPlatformProp))
                    model.VideoPlatform = videoPlatformProp.GetString() ?? "Local";
                
                if (jsonElement.TryGetProperty("izlemeBaslangicSaniye", out var izlemeBaslangicSaniyeProp))
                    model.IzlemeBaslangicSaniye = izlemeBaslangicSaniyeProp.GetInt32();
                
                if (jsonElement.TryGetProperty("izlemeBitisSaniye", out var izlemeBitisSaniyeProp))
                    model.IzlemeBitisSaniye = izlemeBitisSaniyeProp.GetInt32();
                
                if (jsonElement.TryGetProperty("videoToplamSure", out var videoToplamSureProp))
                    model.VideoToplamSure = videoToplamSureProp.GetInt32();

                Console.WriteLine($"Model parsed: VideoEgitimId={model.VideoEgitimId}, ToplamIzlenenSure={model.ToplamIzlenenSure}, IzlemeYuzdesi={model.IzlemeYuzdesi}, TamamlandiMi={model.TamamlandiMi}");

                // Validation
                if (model.VideoEgitimId <= 0)
                {
                    return BadRequest(new { success = false, message = "Video Eğitim ID gerekli" });
                }

                var izleme = new VideoIzleme
                {
                    VideoEgitimId = model.VideoEgitimId,
                    PersonelId = kullanici.Personel.Id,
                    IzlemeBaslangic = DateTime.UtcNow,
                    IzlemeBitis = model.TamamlandiMi ? DateTime.UtcNow : null,
                    ToplamIzlenenSure = model.ToplamIzlenenSure,
                    IzlemeYuzdesi = model.IzlemeYuzdesi,
                    TamamlandiMi = model.TamamlandiMi,
                    TamamlanmaTarihi = model.TamamlandiMi ? DateTime.UtcNow : null,
                    CihazTipi = model.CihazTipi ?? "Web",
                    IpAdresi = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                    
                    // Yeni alanlar
                    VideoPlatform = model.VideoPlatform ?? "Local",
                    IzlemeBaslangicSaniye = model.IzlemeBaslangicSaniye,
                    IzlemeBitisSaniye = model.IzlemeBitisSaniye,
                    VideoToplamSure = model.VideoToplamSure
                };

                var result = await _videoEgitimService.IzlemeKaydetAsync(izleme);
                Console.WriteLine($"Izleme kaydedildi: {result?.Id}");
                
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in IzlemeKaydet: {ex.Message}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // İstatistikler
        [HttpGet("istatistikler")]
        public async Task<IActionResult> GetIstatistikler([FromQuery] int? personelId = null, [FromQuery] int? departmanId = null)
        {
            try
            {
                // Eğer parametreler yoksa, kullanıcının kendi istatistiklerini getir
                if (!personelId.HasValue && !departmanId.HasValue)
                {
                    var kullaniciIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(kullaniciIdClaim))
                    {
                        var kullaniciId = int.Parse(kullaniciIdClaim);
                        var kullanici = await _context.Kullanicilar
                            .Include(k => k.Personel)
                            .FirstOrDefaultAsync(k => k.Id == kullaniciId);
                        
                        personelId = kullanici?.Personel?.Id;
                    }
                }

                var istatistikler = await _videoEgitimService.GetIstatistiklerAsync(personelId, departmanId);
                return Ok(new { success = true, data = istatistikler });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Bekleyen eğitimler
        [HttpGet("bekleyen-egitimler")]
        public async Task<IActionResult> GetBekleyenEgitimler()
        {
            try
            {
                var kullaniciIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(kullaniciIdClaim))
                    return Unauthorized(new { success = false, message = "Kullanıcı bilgisi bulunamadı" });

                var kullaniciId = int.Parse(kullaniciIdClaim);
                var kullanici = await _context.Kullanicilar
                    .Include(k => k.Personel)
                    .FirstOrDefaultAsync(k => k.Id == kullaniciId);

                if (kullanici?.Personel == null)
                    return BadRequest(new { success = false, message = "Personel bilgisi bulunamadı" });

                var bekleyenler = await _videoEgitimService.GetBekleyenEgitimlerAsync(kullanici.Personel.Id);
                return Ok(new { success = true, data = bekleyenler });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Sertifika oluştur
        [HttpPost("sertifika/{videoEgitimId}")]
        public async Task<IActionResult> SertifikaOlustur(int videoEgitimId)
        {
            Console.WriteLine($"=== REQUEST: POST /api/VideoEgitim/sertifika/{videoEgitimId} ===");

            try
            {
                var kullaniciIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Console.WriteLine($"KullaniciId from claim: {kullaniciIdClaim}");
                if (string.IsNullOrEmpty(kullaniciIdClaim))
                    return Unauthorized(new { success = false, message = "Kullanıcı bilgisi bulunamadı" });

                var kullaniciId = int.Parse(kullaniciIdClaim);
                var kullanici = await _context.Kullanicilar
                    .Include(k => k.Personel)
                    .FirstOrDefaultAsync(k => k.Id == kullaniciId);

                if (kullanici?.Personel == null)
                    return BadRequest(new { success = false, message = "Personel bilgisi bulunamadı" });

                var sertifika = await _videoEgitimService.SertifikaOlusturAsync(videoEgitimId, kullanici.Personel.Id);
                
                if (sertifika == null)
                    return BadRequest(new { success = false, message = "Eğitim tamamlanmamış veya sertifika oluşturulamadı" });

                return Ok(new { success = true, data = sertifika, message = "Sertifika oluşturuldu" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Atama güncelle
        [HttpPut("atama/{id}")]
        public async Task<IActionResult> AtamaGuncelle(int id)
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var json = await reader.ReadToEndAsync();
                var jsonElement = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(json);
                
                var atama = await _context.VideoAtamalar.FindAsync(id);
                if (atama == null)
                    return NotFound(new { success = false, message = "Atama bulunamadı" });
                
                if (jsonElement.TryGetProperty("durum", out var durumProp))
                    atama.Durum = durumProp.GetString();
                    
                if (jsonElement.TryGetProperty("not", out var notProp))
                    atama.Not = notProp.GetString() ?? "";
                
                await _context.SaveChangesAsync();
                return Ok(new { success = true, data = atama, message = "Atama güncellendi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Atama sil
        [HttpDelete("atama/{id}")]
        public async Task<IActionResult> AtamaSil(int id)
        {
            try
            {
                var atama = await _context.VideoAtamalar.FindAsync(id);
                if (atama == null)
                    return NotFound(new { success = false, message = "Atama bulunamadı" });

                _context.VideoAtamalar.Remove(atama);
                await _context.SaveChangesAsync();
                
                return Ok(new { success = true, message = "Atama silindi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Otomatik thumbnail URL oluşturma yardımcı metodu
        private string GetAutoThumbnailUrl(string videoUrl)
        {
            if (string.IsNullOrEmpty(videoUrl))
                return null;

            // YouTube thumbnail
            if (videoUrl.Contains("youtube.com/watch?v=") || videoUrl.Contains("youtu.be/"))
            {
                string videoId = null;
                
                if (videoUrl.Contains("youtube.com/watch?v="))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(videoUrl, @"[?&]v=([^&]+)");
                    if (match.Success)
                        videoId = match.Groups[1].Value;
                }
                else if (videoUrl.Contains("youtu.be/"))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(videoUrl, @"youtu\.be/([^?&]+)");
                    if (match.Success)
                        videoId = match.Groups[1].Value;
                }

                if (!string.IsNullOrEmpty(videoId))
                {
                    return $"https://img.youtube.com/vi/{videoId}/maxresdefault.jpg";
                }
            }
            
            // Vimeo thumbnail (basit implementasyon)
            if (videoUrl.Contains("vimeo.com/"))
            {
                var match = System.Text.RegularExpressions.Regex.Match(videoUrl, @"vimeo\.com/(\d+)");
                if (match.Success)
                {
                    // Vimeo thumbnail için daha karmaşık API çağrısı gerekir, şimdilik varsayılan döndür
                    return null; // Vimeo API integration gerekli
                }
            }

            return null; // Desteklenmeyen video türü
        }

        // Arama
        [HttpGet("ara")]
        public async Task<IActionResult> SearchEgitimler([FromQuery] string q)
        {
            try
            {
                var sonuclar = await _videoEgitimService.SearchEgitimlerAsync(q);
                return Ok(new { success = true, data = sonuclar });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // İzleme raporu - Kullanıcı bazlı
        [HttpGet("izleme-raporu")]
        public async Task<IActionResult> GetIzlemeRaporu([FromQuery] int? personelId = null)
        {
            try
            {
                var kullaniciIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(kullaniciIdClaim))
                    return Unauthorized(new { success = false, message = "Kullanıcı bilgisi bulunamadı" });

                var kullaniciId = int.Parse(kullaniciIdClaim);
                var kullanici = await _context.Kullanicilar
                    .Include(k => k.Personel)
                    .FirstOrDefaultAsync(k => k.Id == kullaniciId);

                if (kullanici?.Personel == null)
                    return BadRequest(new { success = false, message = "Personel bilgisi bulunamadı" });

                // Eğer personelId verilmemişse, kendi raporunu getir
                var targetPersonelId = personelId ?? kullanici.Personel.Id;

                var izlemeRaporu = await _context.VideoIzlemeler
                    .Where(i => i.PersonelId == targetPersonelId)
                    .Include(i => i.VideoEgitim)
                    .ThenInclude(v => v.Kategori)
                    .Include(i => i.Personel)
                    .OrderByDescending(i => i.IzlemeBaslangic)
                    .Select(i => new
                    {
                        i.Id,
                        VideoEgitimId = i.VideoEgitimId,
                        VideoBaslik = i.VideoEgitim.Baslik,
                        VideoUrl = i.VideoEgitim.VideoUrl,
                        VideoPlatform = i.VideoPlatform,
                        VideoKategori = i.VideoEgitim.Kategori.Ad,
                        PersonelAd = $"{i.Personel.Ad} {i.Personel.Soyad}",
                        i.IzlemeBaslangic,
                        i.IzlemeBitis,
                        i.ToplamIzlenenSure,
                        i.IzlemeYuzdesi,
                        i.TamamlandiMi,
                        i.TamamlanmaTarihi,
                        i.Puan,
                        i.CihazTipi,
                        i.VideoToplamSure,
                        i.IzlemeBaslangicSaniye,
                        i.IzlemeBitisSaniye,
                        GerekliIzlemeYuzdesi = i.VideoEgitim.IzlenmeMinimum,
                        Durum = i.TamamlandiMi ? "Tamamlandı" : 
                               (i.IzlemeYuzdesi >= i.VideoEgitim.IzlenmeMinimum ? "Gerekli Oran Tamamlandı" : 
                                (i.IzlemeYuzdesi > 0 ? "Devam Ediyor" : "Başlanmadı")),
                        ZorunluMu = i.VideoEgitim.ZorunluMu
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = izlemeRaporu });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Detaylı izleme istatistikleri
        [HttpGet("izleme-istatistikleri")]
        public async Task<IActionResult> GetIzlemeIstatistikleri([FromQuery] int? personelId = null)
        {
            try
            {
                var kullaniciIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(kullaniciIdClaim))
                    return Unauthorized(new { success = false, message = "Kullanıcı bilgisi bulunamadı" });

                var kullaniciId = int.Parse(kullaniciIdClaim);
                var kullanici = await _context.Kullanicilar
                    .Include(k => k.Personel)
                    .FirstOrDefaultAsync(k => k.Id == kullaniciId);

                if (kullanici?.Personel == null)
                    return BadRequest(new { success = false, message = "Personel bilgisi bulunamadı" });

                var targetPersonelId = personelId ?? kullanici.Personel.Id;

                // Platform bazlı istatistikler
                var platformStats = await _context.VideoIzlemeler
                    .Where(i => i.PersonelId == targetPersonelId)
                    .GroupBy(i => i.VideoPlatform)
                    .Select(g => new
                    {
                        Platform = g.Key,
                        ToplamVideo = g.Count(),
                        TamamlananVideo = g.Count(x => x.TamamlandiMi),
                        ToplamIzlemeSuresi = g.Sum(x => x.ToplamIzlenenSure),
                        OrtalamaTamamlanmaOrani = g.Average(x => x.IzlemeYuzdesi)
                    })
                    .ToListAsync();

                // Kategori bazlı istatistikler
                var kategoriStats = await _context.VideoIzlemeler
                    .Where(i => i.PersonelId == targetPersonelId)
                    .Include(i => i.VideoEgitim.Kategori)
                    .GroupBy(i => i.VideoEgitim.Kategori.Ad)
                    .Select(g => new
                    {
                        Kategori = g.Key,
                        ToplamVideo = g.Count(),
                        TamamlananVideo = g.Count(x => x.TamamlandiMi),
                        OrtalamaPuan = g.Where(x => x.Puan.HasValue).Average(x => x.Puan) ?? 0
                    })
                    .ToListAsync();

                // Genel istatistikler
                var genelStats = new
                {
                    ToplamAtananVideo = await _context.VideoAtamalar
                        .CountAsync(a => a.PersonelId == targetPersonelId),
                    
                    ToplamTamamlananVideo = await _context.VideoIzlemeler
                        .CountAsync(i => i.PersonelId == targetPersonelId && i.TamamlandiMi),
                    
                    ToplamIzlemeSuresi = await _context.VideoIzlemeler
                        .Where(i => i.PersonelId == targetPersonelId)
                        .SumAsync(i => i.ToplamIzlenenSure),
                    
                    OrtalamaTamamlanmaOrani = await _context.VideoIzlemeler
                        .Where(i => i.PersonelId == targetPersonelId)
                        .AverageAsync(i => i.IzlemeYuzdesi),
                    
                    ZorunluVideoSayisi = await _context.VideoAtamalar
                        .Include(a => a.VideoEgitim)
                        .CountAsync(a => a.PersonelId == targetPersonelId && a.VideoEgitim.ZorunluMu),
                    
                    ZorunluTamamlananSayisi = await _context.VideoIzlemeler
                        .Include(i => i.VideoEgitim)
                        .CountAsync(i => i.PersonelId == targetPersonelId && 
                                   i.VideoEgitim.ZorunluMu && i.TamamlandiMi)
                };

                var istatistikler = new
                {
                    GenelIstatistikler = genelStats,
                    PlatformIstatistikleri = platformStats,
                    KategoriIstatistikleri = kategoriStats
                };

                return Ok(new { success = true, data = istatistikler });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Video URL'sinden süreyi çek
        [HttpPost("get-video-duration")]
        public async Task<IActionResult> GetVideoDuration()
        {
            Console.WriteLine("=== REQUEST: POST /api/VideoEgitim/get-video-duration ===");
            
            try
            {
                using var reader = new StreamReader(Request.Body);
                var json = await reader.ReadToEndAsync();
                Console.WriteLine($"Raw JSON: {json}");
                
                var jsonDocument = JsonDocument.Parse(json);
                var root = jsonDocument.RootElement;

                var videoUrl = root.GetProperty("videoUrl").GetString();
                
                if (string.IsNullOrEmpty(videoUrl))
                {
                    return BadRequest(new { success = false, message = "Video URL gereklidir" });
                }

                var durationData = await _videoEgitimService.GetVideoDurationAsync(videoUrl);
                
                if (durationData != null)
                {
                    return Ok(new { success = true, data = durationData });
                }
                else
                {
                    return Ok(new { success = false, message = "Video süresi alınamadı" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetVideoDuration: {ex.Message}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Video izleme progress'i güncelle
        [HttpPost("update-progress")]
        public async Task<IActionResult> UpdateVideoProgress()
        {
            Console.WriteLine("=== REQUEST: POST /api/VideoEgitim/update-progress ===");
            
            try
            {
                using var reader = new StreamReader(Request.Body);
                var json = await reader.ReadToEndAsync();
                Console.WriteLine($"Raw JSON: {json}");
                
                var jsonDocument = JsonDocument.Parse(json);
                var root = jsonDocument.RootElement;

                var videoEgitimId = root.GetProperty("videoEgitimId").GetInt32();
                var toplamIzlenenSure = root.GetProperty("toplamIzlenenSure").GetInt32();
                var izlemeYuzdesi = root.GetProperty("izlemeYuzdesi").GetInt32();
                var tamamlandiMi = root.GetProperty("tamamlandiMi").GetBoolean();

                // Optional fields with defaults
                var videoPlatform = root.TryGetProperty("videoPlatform", out var platformElement) 
                    ? platformElement.GetString() : "Local";
                var videoToplamSure = root.TryGetProperty("videoToplamSure", out var toplamSureElement) 
                    ? toplamSureElement.GetInt32() : 0;
                var izlemeBaslangicSaniye = root.TryGetProperty("izlemeBaslangicSaniye", out var baslangicElement) 
                    ? baslangicElement.GetInt32() : 0;
                var izlemeBitisSaniye = root.TryGetProperty("izlemeBitisSaniye", out var bitisElement) 
                    ? bitisElement.GetInt32() : toplamIzlenenSure;
                var cihazTipi = root.TryGetProperty("cihazTipi", out var cihazElement) 
                    ? cihazElement.GetString() : "Desktop";

                // Get user info
                var kullaniciIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(kullaniciIdClaim))
                    return Unauthorized(new { success = false, message = "Kullanıcı bilgisi bulunamadı" });

                var kullaniciId = int.Parse(kullaniciIdClaim);
                var kullanici = await _context.Kullanicilar
                    .Include(k => k.Personel)
                    .FirstOrDefaultAsync(k => k.Id == kullaniciId);

                if (kullanici?.Personel == null)
                    return NotFound(new { success = false, message = "Personel bilgisi bulunamadı" });

                var personel = kullanici.Personel;

                Console.WriteLine($"Updating progress - PersonelId: {personel.Id}, VideoEgitimId: {videoEgitimId}, Progress: {izlemeYuzdesi}%");

                var progressData = new IzlemeKayitModel
                {
                    VideoEgitimId = videoEgitimId,
                    IzlemeBaslangic = DateTime.UtcNow,
                    IzlemeBitis = tamamlandiMi ? DateTime.UtcNow : null,
                    ToplamIzlenenSure = toplamIzlenenSure,
                    IzlemeYuzdesi = izlemeYuzdesi,
                    TamamlandiMi = tamamlandiMi,
                    CihazTipi = cihazTipi,
                    VideoPlatform = videoPlatform,
                    VideoToplamSure = videoToplamSure,
                    IzlemeBaslangicSaniye = izlemeBaslangicSaniye,
                    IzlemeBitisSaniye = izlemeBitisSaniye
                };

                await _videoEgitimService.UpdateVideoProgressAsync(personel.Id, progressData);

                return Ok(new { success = true, message = "Progress başarıyla güncellendi" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateVideoProgress: {ex.Message}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Eğitim raporları için istatistikler
        [AllowAnonymous]
        [HttpGet("rapor-istatistikleri")]
        public async Task<IActionResult> GetRaporIstatistikleri()
        {
            try
            {
                var istatistikler = await _videoEgitimService.GetRaporIstatistikleriAsync();
                return Ok(new { success = true, data = istatistikler });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Personel eğitim özeti
        [AllowAnonymous]
        [HttpGet("personel-egitim-ozeti")]
        public async Task<IActionResult> GetPersonelEgitimOzeti()
        {
            try
            {
                var ozet = await _videoEgitimService.GetPersonelEgitimOzetiAsync();
                return Ok(new { success = true, data = ozet });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Departman raporu
        [AllowAnonymous]
        [HttpGet("departman-raporu")]
        public async Task<IActionResult> GetDepartmanRaporu([FromQuery] int year = 0)
        {
            try
            {
                if (year == 0) year = DateTime.Now.Year;
                var rapor = await _videoEgitimService.GetDepartmanRaporuAsync(year);
                return Ok(new { success = true, data = rapor });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Aylık video eğitim trendi
        [AllowAnonymous]
        [HttpGet("aylik-egitim-trendi")]
        public async Task<IActionResult> GetAylikEgitimTrendi([FromQuery] int year = 0)
        {
            try
            {
                if (year == 0) year = DateTime.Now.Year;
                var trend = await _videoEgitimService.GetAylikEgitimTrendiAsync(year);
                return Ok(new { success = true, data = trend });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }

    // Model sınıfları
    public class AtamaModel
    {
        public int VideoEgitimId { get; set; }
        public List<int> PersonelIds { get; set; }
        public List<int> DepartmanIds { get; set; }
        public List<int> PozisyonIds { get; set; }
        public string Not { get; set; }
    }
    
    public class TopluAtamaModel
    {
        public List<int> VideoEgitimIds { get; set; }
        public List<int> DepartmanIds { get; set; }
        public List<int> PozisyonIds { get; set; }
        public List<int> PersonelIds { get; set; }
        public string Not { get; set; }
    }

    public class IzlemeKayitModel
    {
        public int VideoEgitimId { get; set; }
        public DateTime IzlemeBaslangic { get; set; }
        public DateTime? IzlemeBitis { get; set; }
        public int ToplamIzlenenSure { get; set; }
        public int IzlemeYuzdesi { get; set; }
        public bool TamamlandiMi { get; set; }
        public string CihazTipi { get; set; }
        
        // Yeni alanlar
        public string VideoPlatform { get; set; } = "Local";
        public int IzlemeBaslangicSaniye { get; set; } = 0;
        public int IzlemeBitisSaniye { get; set; } = 0;
        public int VideoToplamSure { get; set; } = 0;
    }
}