using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IconIK.API.Data;
using IconIK.API.Models;
using IconIK.API.Services;
using System.Text.Json;

namespace IconIK.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AvansController : ControllerBase
    {
        private readonly IconIKContext _context;
        private readonly IAvansService _avansService;
        private readonly IBildirimService _bildirimService;

        public AvansController(IconIKContext context, IAvansService avansService, IBildirimService bildirimService)
        {
            _context = context;
            _avansService = avansService;
            _bildirimService = bildirimService;
        }

        // GET: api/Avans
        [HttpGet]
        public async Task<ActionResult<object>> GetAvansTalepleri(int personelId)
        {
            try
            {
                var query = _context.AvansTalepleri
                    .Include(a => a.Personel)
                        .ThenInclude(p => p.Pozisyon)
                            .ThenInclude(pos => pos.Departman)
                    .Include(a => a.Onaylayan)
                    .Where(a => a.PersonelId == personelId)
                    .AsQueryable();

                var avansTalepleri = await query
                    .OrderByDescending(a => a.CreatedAt)
                    .Select(a => new
                    {
                        a.Id,
                        a.PersonelId,
                        PersonelAd = a.Personel.Ad + " " + a.Personel.Soyad,
                        PersonelPozisyon = a.Personel.Pozisyon.Ad,
                        PersonelDepartman = a.Personel.Pozisyon.Departman.Ad,
                        PersonelMaas = a.Personel.Maas,
                        a.TalepTarihi,
                        a.TalepTutari,
                        a.Aciklama,
                        a.OnayDurumu,
                        a.OnaylayanId,
                        OnaylayanAd = a.Onaylayan != null ? a.Onaylayan.Ad + " " + a.Onaylayan.Soyad : null,
                        a.OnayTarihi,
                        a.OnayNotu,
                        a.CreatedAt,
                        a.UpdatedAt
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = avansTalepleri });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Avans talepleri getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/Avans/Onay
        [HttpGet("Onay")]
        public async Task<ActionResult<object>> GetOnayBekleyenAvansTalepleri(int yoneticiId)
        {
            try
            {
                // Yönetici olduğu personellerin avans taleplerini getir
                var yoneticiPersonel = await _context.Personeller
                    .Include(p => p.Pozisyon)
                        .ThenInclude(pos => pos.Kademe)
                    .FirstOrDefaultAsync(p => p.Id == yoneticiId);

                if (yoneticiPersonel == null)
                {
                    return NotFound(new { success = false, message = "Yönetici bulunamadı." });
                }

                var query = _context.AvansTalepleri
                    .Include(a => a.Personel)
                        .ThenInclude(p => p.Pozisyon)
                            .ThenInclude(pos => pos.Departman)
                    .Include(a => a.Onaylayan)
                    .Where(a => a.OnayDurumu == "Beklemede");

                // Kademe seviyesine göre filtreleme
                if (yoneticiPersonel.Pozisyon.Kademe.Seviye > 1)
                {
                    // Sadece kendi altındaki personellerin taleplerini görebilir
                    query = query.Where(a => a.Personel.YoneticiId == yoneticiId);
                }
                // Seviye 1 ise tüm talepleri görebilir

                var avansTalepleri = await query
                    .OrderByDescending(a => a.CreatedAt)
                    .Select(a => new
                    {
                        a.Id,
                        a.PersonelId,
                        PersonelAd = a.Personel.Ad + " " + a.Personel.Soyad,
                        PersonelPozisyon = a.Personel.Pozisyon.Ad,
                        PersonelDepartman = a.Personel.Pozisyon.Departman.Ad,
                        PersonelMaas = a.Personel.Maas,
                        MaxAvansLimit = Math.Round((a.Personel.Maas ?? 0) / 3, 2),
                        a.TalepTarihi,
                        a.TalepTutari,
                        a.Aciklama,
                        a.OnayDurumu,
                        a.CreatedAt
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = avansTalepleri });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Onay bekleyen avans talepleri getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/Avans/Limit/5
        [HttpGet("Limit/{personelId}")]
        public async Task<ActionResult<object>> GetAvansLimit(int personelId)
        {
            try
            {
                var maxLimit = await _avansService.GetMaxAvansLimit(personelId);

                // Bu ayki onaylanmış avans tutarı (sadece kullanılan)
                var kullanilanAvans = await _context.AvansTalepleri
                    .Where(a => a.PersonelId == personelId
                        && a.OnayDurumu == "Onaylandı"
                        && a.TalepTarihi.Month == DateTime.Now.Month
                        && a.TalepTarihi.Year == DateTime.Now.Year)
                    .SumAsync(a => a.TalepTutari);

                // Bu ayki beklemede olan avans tutarı
                var onayBekleyen = await _context.AvansTalepleri
                    .Where(a => a.PersonelId == personelId
                        && a.OnayDurumu == "Beklemede"
                        && a.TalepTarihi.Month == DateTime.Now.Month
                        && a.TalepTarihi.Year == DateTime.Now.Year)
                    .SumAsync(a => a.TalepTutari);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        maxLimit = maxLimit,
                        kullanilanAvans = kullanilanAvans,
                        onayBekleyen = onayBekleyen,
                        kalanLimit = maxLimit - kullanilanAvans - onayBekleyen
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Avans limiti hesaplanırken bir hata oluştu.", error = ex.Message });
            }
        }

        // POST: api/Avans
        [HttpPost]
        public async Task<ActionResult<object>> PostAvansTalebi([FromBody] JsonElement jsonElement)
        {
            try
            {
                var avansTalebi = new AvansTalebi();
                
                // JSON parsing
                if (jsonElement.TryGetProperty("personelId", out var personelIdProp))
                    avansTalebi.PersonelId = personelIdProp.GetInt32();
                    
                if (jsonElement.TryGetProperty("talepTutari", out var tutarProp))
                    avansTalebi.TalepTutari = tutarProp.GetDecimal();
                    
                if (jsonElement.TryGetProperty("aciklama", out var aciklamaProp))
                    avansTalebi.Aciklama = aciklamaProp.GetString();

                // Limit kontrolü
                var limitCheck = await _avansService.CheckAvansLimit(avansTalebi.PersonelId, avansTalebi.TalepTutari);
                if (!limitCheck)
                {
                    return BadRequest(new { success = false, message = "Avans tutarı limitinizi aşmaktadır." });
                }

                avansTalebi.TalepTarihi = DateTime.UtcNow;
                avansTalebi.OnayDurumu = "Beklemede";
                avansTalebi.CreatedAt = DateTime.UtcNow;
                avansTalebi.UpdatedAt = DateTime.UtcNow;

                _context.AvansTalepleri.Add(avansTalebi);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, data = avansTalebi, message = "Avans talebi başarıyla oluşturuldu." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Avans talebi oluşturulurken bir hata oluştu.", error = ex.Message });
            }
        }

        // PUT: api/Avans/5
        [HttpPut("{id}")]
        public async Task<ActionResult<object>> PutAvansTalebi(int id, [FromBody] JsonElement jsonElement)
        {
            try
            {
                var avansTalebi = await _context.AvansTalepleri.FindAsync(id);
                if (avansTalebi == null)
                {
                    return NotFound(new { success = false, message = "Avans talebi bulunamadı." });
                }

                // Sadece beklemede olan talepler güncellenebilir
                if (avansTalebi.OnayDurumu != "Beklemede")
                {
                    return BadRequest(new { success = false, message = "Sadece beklemede olan talepler güncellenebilir." });
                }

                // JSON parsing
                if (jsonElement.TryGetProperty("talepTutari", out var tutarProp))
                {
                    var yeniTutar = tutarProp.GetDecimal();
                    
                    // Limit kontrolü
                    var limitCheck = await _avansService.CheckAvansLimit(avansTalebi.PersonelId, yeniTutar - avansTalebi.TalepTutari);
                    if (!limitCheck)
                    {
                        return BadRequest(new { success = false, message = "Avans tutarı limitinizi aşmaktadır." });
                    }
                    
                    avansTalebi.TalepTutari = yeniTutar;
                }
                    
                if (jsonElement.TryGetProperty("aciklama", out var aciklamaProp))
                    avansTalebi.Aciklama = aciklamaProp.GetString();

                avansTalebi.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, data = avansTalebi, message = "Avans talebi başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Avans talebi güncellenirken bir hata oluştu.", error = ex.Message });
            }
        }

        // DELETE: api/Avans/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<object>> DeleteAvansTalebi(int id)
        {
            try
            {
                var avansTalebi = await _context.AvansTalepleri.FindAsync(id);
                if (avansTalebi == null)
                {
                    return NotFound(new { success = false, message = "Avans talebi bulunamadı." });
                }

                // Sadece beklemede olan talepler silinebilir
                if (avansTalebi.OnayDurumu != "Beklemede")
                {
                    return BadRequest(new { success = false, message = "Sadece beklemede olan talepler silinebilir." });
                }

                _context.AvansTalepleri.Remove(avansTalebi);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Avans talebi başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Avans talebi silinirken bir hata oluştu.", error = ex.Message });
            }
        }

        // POST: api/Avans/Onayla/5
        [HttpPost("Onayla/{id}")]
        public async Task<ActionResult<object>> OnaylaAvansTalebi(int id, [FromBody] JsonElement jsonElement)
        {
            try
            {
                var avansTalebi = await _context.AvansTalepleri
                    .Include(a => a.Personel)
                    .Include(a => a.Onaylayan)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (avansTalebi == null)
                {
                    return NotFound(new { success = false, message = "Avans talebi bulunamadı." });
                }

                if (avansTalebi.OnayDurumu != "Beklemede")
                {
                    return BadRequest(new { success = false, message = "Bu talep zaten işleme alınmış." });
                }

                var onaylayanId = jsonElement.GetProperty("onaylayanId").GetInt32();
                var onayNotu = jsonElement.TryGetProperty("onayNotu", out var notProp) ? notProp.GetString() : null;

                avansTalebi.OnayDurumu = "Onaylandı";
                avansTalebi.OnaylayanId = onaylayanId;
                avansTalebi.OnayTarihi = DateTime.UtcNow;
                avansTalebi.OnayNotu = onayNotu;
                avansTalebi.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Bildirim gönder
                var onaylayan = await _context.Personeller.FindAsync(onaylayanId);
                var bildirim = new Bildirim
                {
                    AliciId = avansTalebi.PersonelId,
                    Baslik = "Avans Talebiniz Onaylandı",
                    Mesaj = $"{avansTalebi.TalepTutari:C2} tutarındaki avans talebiniz onaylanmıştır.",
                    Kategori = "avans",
                    Tip = "success",
                    GonderenAd = onaylayan != null ? $"{onaylayan.Ad} {onaylayan.Soyad}" : "Sistem",
                    ActionUrl = "/avans-talepleri"
                };
                await _bildirimService.CreateBildirimAsync(bildirim);

                return Ok(new { success = true, message = "Avans talebi onaylandı." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Avans talebi onaylanırken bir hata oluştu.", error = ex.Message });
            }
        }

        // POST: api/Avans/Reddet/5
        [HttpPost("Reddet/{id}")]
        public async Task<ActionResult<object>> ReddetAvansTalebi(int id, [FromBody] JsonElement jsonElement)
        {
            try
            {
                var avansTalebi = await _context.AvansTalepleri
                    .Include(a => a.Personel)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (avansTalebi == null)
                {
                    return NotFound(new { success = false, message = "Avans talebi bulunamadı." });
                }

                if (avansTalebi.OnayDurumu != "Beklemede")
                {
                    return BadRequest(new { success = false, message = "Bu talep zaten işleme alınmış." });
                }

                var onaylayanId = jsonElement.GetProperty("onaylayanId").GetInt32();
                var onayNotu = jsonElement.TryGetProperty("onayNotu", out var notProp) ? notProp.GetString() : null;

                avansTalebi.OnayDurumu = "Reddedildi";
                avansTalebi.OnaylayanId = onaylayanId;
                avansTalebi.OnayTarihi = DateTime.UtcNow;
                avansTalebi.OnayNotu = onayNotu;
                avansTalebi.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Bildirim gönder
                var onaylayan = await _context.Personeller.FindAsync(onaylayanId);
                var bildirim = new Bildirim
                {
                    AliciId = avansTalebi.PersonelId,
                    Baslik = "Avans Talebiniz Reddedildi",
                    Mesaj = $"{avansTalebi.TalepTutari:C2} tutarındaki avans talebiniz reddedilmiştir. {(string.IsNullOrEmpty(onayNotu) ? "" : $"Açıklama: {onayNotu}")}",
                    Kategori = "avans",
                    Tip = "error",
                    GonderenAd = onaylayan != null ? $"{onaylayan.Ad} {onaylayan.Soyad}" : "Sistem",
                    ActionUrl = "/avans-talepleri"
                };
                await _bildirimService.CreateBildirimAsync(bildirim);

                return Ok(new { success = true, message = "Avans talebi reddedildi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Avans talebi reddedilirken bir hata oluştu.", error = ex.Message });
            }
        }
    }
}