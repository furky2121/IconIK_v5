using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IconIK.API.Data;
using IconIK.API.Models;
using System.Text.Json;

namespace IconIK.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KVKKIzinMetniController : ControllerBase
    {
        private readonly IconIKContext _context;

        public KVKKIzinMetniController(IconIKContext context)
        {
            _context = context;
        }

        // GET: api/KVKKIzinMetni
        [HttpGet]
        public async Task<ActionResult> GetKVKKIzinMetinleri()
        {
            try
            {
                var metinler = await _context.KVKKIzinMetinleri
                    .Include(k => k.OlusturanPersonel)
                    .OrderByDescending(k => k.Versiyon)
                    .ToListAsync();

                var result = metinler.Select(k => new
                {
                    k.Id,
                    k.Baslik,
                    k.Metin,
                    k.Aktif,
                    k.Versiyon,
                    k.YayinlanmaTarihi,
                    k.OlusturanPersonelId,
                    OlusturanPersonel = k.OlusturanPersonel != null ? new
                    {
                        k.OlusturanPersonel.Id,
                        k.OlusturanPersonel.Ad,
                        k.OlusturanPersonel.Soyad,
                        AdSoyad = k.OlusturanPersonel.Ad + " " + k.OlusturanPersonel.Soyad
                    } : null,
                    k.CreatedAt,
                    k.UpdatedAt
                }).ToList();

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "KVKK izin metinleri getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/KVKKIzinMetni/Aktif
        [HttpGet("Aktif")]
        public async Task<ActionResult> GetAktifKVKKMetni()
        {
            try
            {
                var aktifMetin = await _context.KVKKIzinMetinleri
                    .Include(k => k.OlusturanPersonel)
                    .Where(k => k.Aktif)
                    .OrderByDescending(k => k.Versiyon)
                    .FirstOrDefaultAsync();

                if (aktifMetin == null)
                {
                    return Ok(new { success = true, data = (object?)null });
                }

                var result = new
                {
                    aktifMetin.Id,
                    aktifMetin.Baslik,
                    aktifMetin.Metin,
                    aktifMetin.Aktif,
                    aktifMetin.Versiyon,
                    aktifMetin.YayinlanmaTarihi,
                    aktifMetin.OlusturanPersonelId,
                    OlusturanPersonel = aktifMetin.OlusturanPersonel != null ? new
                    {
                        aktifMetin.OlusturanPersonel.Id,
                        aktifMetin.OlusturanPersonel.Ad,
                        aktifMetin.OlusturanPersonel.Soyad,
                        AdSoyad = aktifMetin.OlusturanPersonel.Ad + " " + aktifMetin.OlusturanPersonel.Soyad
                    } : null,
                    aktifMetin.CreatedAt,
                    aktifMetin.UpdatedAt
                };

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Aktif KVKK metni getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/KVKKIzinMetni/5
        [HttpGet("{id}")]
        public async Task<ActionResult> GetKVKKIzinMetni(int id)
        {
            try
            {
                var metin = await _context.KVKKIzinMetinleri
                    .Include(k => k.OlusturanPersonel)
                    .FirstOrDefaultAsync(k => k.Id == id);

                if (metin == null)
                {
                    return NotFound(new { success = false, message = "KVKK izin metni bulunamadı." });
                }

                var result = new
                {
                    metin.Id,
                    metin.Baslik,
                    metin.Metin,
                    metin.Aktif,
                    metin.Versiyon,
                    metin.YayinlanmaTarihi,
                    metin.OlusturanPersonelId,
                    OlusturanPersonel = metin.OlusturanPersonel != null ? new
                    {
                        metin.OlusturanPersonel.Id,
                        metin.OlusturanPersonel.Ad,
                        metin.OlusturanPersonel.Soyad,
                        AdSoyad = metin.OlusturanPersonel.Ad + " " + metin.OlusturanPersonel.Soyad
                    } : null,
                    metin.CreatedAt,
                    metin.UpdatedAt
                };

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "KVKK izin metni getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        // POST: api/KVKKIzinMetni
        [HttpPost]
        public async Task<ActionResult> PostKVKKIzinMetni([FromBody] JsonElement jsonData)
        {
            try
            {
                var metin = new KVKKIzinMetni
                {
                    Baslik = jsonData.GetProperty("baslik").GetString() ?? "",
                    Metin = jsonData.GetProperty("metin").GetString() ?? "",
                    Aktif = jsonData.TryGetProperty("aktif", out var aktifProp) && aktifProp.GetBoolean(),
                    Versiyon = jsonData.TryGetProperty("versiyon", out var versiyonProp) ? versiyonProp.GetInt32() : 1,
                    YayinlanmaTarihi = jsonData.TryGetProperty("yayinlanmaTarihi", out var yayinProp) && yayinProp.ValueKind != JsonValueKind.Null
                        ? yayinProp.GetDateTime()
                        : null,
                    OlusturanPersonelId = jsonData.TryGetProperty("olusturanPersonelId", out var olusturanProp) && olusturanProp.ValueKind != JsonValueKind.Null
                        ? olusturanProp.GetInt32()
                        : null
                };

                // Eğer yeni metin aktif ise, diğer aktif metinleri pasif yap
                if (metin.Aktif)
                {
                    var aktifMetinler = await _context.KVKKIzinMetinleri
                        .Where(k => k.Aktif)
                        .ToListAsync();

                    foreach (var aktifMetin in aktifMetinler)
                    {
                        aktifMetin.Aktif = false;
                    }
                }

                _context.KVKKIzinMetinleri.Add(metin);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "KVKK izin metni başarıyla oluşturuldu.",
                    data = new { metin.Id }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"KVKK POST Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                return StatusCode(500, new { success = false, message = "KVKK izin metni oluşturulurken bir hata oluştu.", error = ex.Message, innerError = ex.InnerException?.Message });
            }
        }

        // PUT: api/KVKKIzinMetni/5
        [HttpPut("{id}")]
        public async Task<ActionResult> PutKVKKIzinMetni(int id, [FromBody] JsonElement jsonData)
        {
            try
            {
                var metin = await _context.KVKKIzinMetinleri.FindAsync(id);

                if (metin == null)
                {
                    return NotFound(new { success = false, message = "KVKK izin metni bulunamadı." });
                }

                metin.Baslik = jsonData.GetProperty("baslik").GetString() ?? metin.Baslik;
                metin.Metin = jsonData.GetProperty("metin").GetString() ?? metin.Metin;

                var yeniAktifDurum = jsonData.TryGetProperty("aktif", out var aktifProp) && aktifProp.GetBoolean();

                // Eğer yeni metin aktif olacaksa, diğer aktif metinleri pasif yap
                if (yeniAktifDurum && !metin.Aktif)
                {
                    var aktifMetinler = await _context.KVKKIzinMetinleri
                        .Where(k => k.Aktif && k.Id != id)
                        .ToListAsync();

                    foreach (var aktifMetin in aktifMetinler)
                    {
                        aktifMetin.Aktif = false;
                    }
                }

                metin.Aktif = yeniAktifDurum;

                if (jsonData.TryGetProperty("versiyon", out var versiyonProp))
                {
                    metin.Versiyon = versiyonProp.GetInt32();
                }

                if (jsonData.TryGetProperty("yayinlanmaTarihi", out var yayinProp) && yayinProp.ValueKind != JsonValueKind.Null)
                {
                    metin.YayinlanmaTarihi = yayinProp.GetDateTime();
                }

                metin.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "KVKK izin metni başarıyla güncellendi.",
                    data = new { metin.Id }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "KVKK izin metni güncellenirken bir hata oluştu.", error = ex.Message });
            }
        }

        // DELETE: api/KVKKIzinMetni/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteKVKKIzinMetni(int id)
        {
            try
            {
                var metin = await _context.KVKKIzinMetinleri.FindAsync(id);

                if (metin == null)
                {
                    return NotFound(new { success = false, message = "KVKK izin metni bulunamadı." });
                }

                _context.KVKKIzinMetinleri.Remove(metin);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "KVKK izin metni başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "KVKK izin metni silinirken bir hata oluştu.", error = ex.Message });
            }
        }

        // POST: api/KVKKIzinMetni/OnayKaydet
        [HttpPost("OnayKaydet")]
        public async Task<ActionResult> OnayKaydet([FromBody] JsonElement jsonData)
        {
            try
            {
                var kullaniciId = jsonData.GetProperty("kullaniciId").GetInt32();
                var onay = jsonData.GetProperty("onay").GetBoolean();

                var kullanici = await _context.Kullanicilar.FindAsync(kullaniciId);

                if (kullanici == null)
                {
                    return NotFound(new { success = false, message = "Kullanıcı bulunamadı." });
                }

                if (onay)
                {
                    kullanici.KVKKOnaylandi = true;
                    kullanici.KVKKOnayTarihi = DateTime.UtcNow;
                }
                else
                {
                    kullanici.KVKKOnaylandi = false;
                    kullanici.KVKKOnayTarihi = null;
                }

                kullanici.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = onay ? "KVKK onayınız kaydedildi." : "KVKK onayınız reddedildi.",
                    data = new { kullanici.KVKKOnaylandi, kullanici.KVKKOnayTarihi }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "KVKK onayı kaydedilirken bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/KVKKIzinMetni/OnayDurumu/5
        [HttpGet("OnayDurumu/{kullaniciId}")]
        public async Task<ActionResult> GetOnayDurumu(int kullaniciId)
        {
            try
            {
                var kullanici = await _context.Kullanicilar.FindAsync(kullaniciId);

                if (kullanici == null)
                {
                    return NotFound(new { success = false, message = "Kullanıcı bulunamadı." });
                }

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        kullanici.KVKKOnaylandi,
                        kullanici.KVKKOnayTarihi
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "KVKK onay durumu getirilirken bir hata oluştu.", error = ex.Message });
            }
        }
    }
}
