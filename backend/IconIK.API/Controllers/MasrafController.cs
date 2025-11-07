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
    public class MasrafController : ControllerBase
    {
        private readonly IconIKContext _context;
        private readonly IMasrafService _masrafService;
        private readonly IBildirimService _bildirimService;

        public MasrafController(IconIKContext context, IMasrafService masrafService, IBildirimService bildirimService)
        {
            _context = context;
            _masrafService = masrafService;
            _bildirimService = bildirimService;
        }

        // GET: api/Masraf
        [HttpGet]
        public async Task<ActionResult<object>> GetMasrafTalepleri(int personelId)
        {
            try
            {
                var query = _context.MasrafTalepleri
                    .Include(m => m.Personel)
                        .ThenInclude(p => p.Pozisyon)
                            .ThenInclude(pos => pos.Departman)
                    .Include(m => m.Onaylayan)
                    .Where(m => m.PersonelId == personelId)
                    .AsQueryable();

                var dbResults = await query
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => new
                    {
                        m.Id,
                        m.PersonelId,
                        PersonelAd = m.Personel.Ad + " " + m.Personel.Soyad,
                        PersonelPozisyon = m.Personel.Pozisyon.Ad,
                        PersonelDepartman = m.Personel.Pozisyon.Departman.Ad,
                        PersonelMaas = m.Personel.Maas,
                        m.MasrafTipi,
                        m.TalepTarihi,
                        m.Tutar,
                        m.Aciklama,
                        m.BelgeUrl,
                        m.OnayDurumu,
                        m.OnaylayanId,
                        OnaylayanAd = m.Onaylayan != null ? m.Onaylayan.Ad + " " + m.Onaylayan.Soyad : null,
                        m.OnayTarihi,
                        m.OnayNotu,
                        m.CreatedAt,
                        m.UpdatedAt
                    })
                    .ToListAsync();

                var masrafTalepleri = dbResults.Select(m => new
                {
                    m.Id,
                    m.PersonelId,
                    m.PersonelAd,
                    m.PersonelPozisyon,
                    m.PersonelDepartman,
                    m.PersonelMaas,
                    m.MasrafTipi,
                    MasrafTipiAd = GetMasrafTipiAd(m.MasrafTipi),
                    m.TalepTarihi,
                    m.Tutar,
                    m.Aciklama,
                    m.BelgeUrl,
                    m.OnayDurumu,
                    m.OnaylayanId,
                    m.OnaylayanAd,
                    m.OnayTarihi,
                    m.OnayNotu,
                    m.CreatedAt,
                    m.UpdatedAt
                }).ToList();

                return Ok(new { success = true, data = masrafTalepleri });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Masraf talepleri getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/Masraf/Onay
        [HttpGet("Onay")]
        public async Task<ActionResult<object>> GetOnayBekleyenMasrafTalepleri(int yoneticiId)
        {
            try
            {
                // Yönetici olduğu personellerin masraf taleplerini getir
                var yoneticiPersonel = await _context.Personeller
                    .Include(p => p.Pozisyon)
                        .ThenInclude(pos => pos.Kademe)
                    .FirstOrDefaultAsync(p => p.Id == yoneticiId);

                if (yoneticiPersonel == null)
                {
                    return NotFound(new { success = false, message = "Yönetici bulunamadı." });
                }

                var query = _context.MasrafTalepleri
                    .Include(m => m.Personel)
                        .ThenInclude(p => p.Pozisyon)
                            .ThenInclude(pos => pos.Departman)
                    .Include(m => m.Onaylayan)
                    .Where(m => m.OnayDurumu == "Beklemede");

                // Kademe seviyesine göre filtreleme
                if (yoneticiPersonel.Pozisyon.Kademe.Seviye > 1)
                {
                    // Sadece kendi altındaki personellerin taleplerini görebilir
                    query = query.Where(m => m.Personel.YoneticiId == yoneticiId);
                }
                // Seviye 1 ise tüm talepleri görebilir

                var dbResults = await query
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => new
                    {
                        m.Id,
                        m.PersonelId,
                        PersonelAd = m.Personel.Ad + " " + m.Personel.Soyad,
                        PersonelPozisyon = m.Personel.Pozisyon.Ad,
                        PersonelDepartman = m.Personel.Pozisyon.Departman.Ad,
                        PersonelMaas = m.Personel.Maas,
                        PersonelFoto = m.Personel.FotografUrl,
                        PersonelEmail = m.Personel.Email,
                        PersonelTelefon = m.Personel.Telefon,
                        m.MasrafTipi,
                        m.TalepTarihi,
                        m.Tutar,
                        m.Aciklama,
                        m.BelgeUrl,
                        m.OnayDurumu,
                        m.CreatedAt
                    })
                    .ToListAsync();

                var masrafTalepleri = dbResults.Select(m => new
                {
                    m.Id,
                    m.PersonelId,
                    m.PersonelAd,
                    m.PersonelPozisyon,
                    m.PersonelDepartman,
                    m.PersonelMaas,
                    m.PersonelFoto,
                    m.PersonelEmail,
                    m.PersonelTelefon,
                    m.MasrafTipi,
                    MasrafTipiAd = GetMasrafTipiAd(m.MasrafTipi),
                    m.TalepTarihi,
                    m.Tutar,
                    m.Aciklama,
                    m.BelgeUrl,
                    m.OnayDurumu,
                    m.CreatedAt
                }).ToList();

                return Ok(new { success = true, data = masrafTalepleri });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Onay bekleyen masraf talepleri getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/Masraf/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetMasrafTalebi(int id)
        {
            try
            {
                var masrafTalebi = await _context.MasrafTalepleri
                    .Include(m => m.Personel)
                        .ThenInclude(p => p.Pozisyon)
                            .ThenInclude(pos => pos.Departman)
                    .Include(m => m.Onaylayan)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (masrafTalebi == null)
                {
                    return NotFound(new { success = false, message = "Masraf talebi bulunamadı." });
                }

                var result = new
                {
                    masrafTalebi.Id,
                    masrafTalebi.PersonelId,
                    PersonelAd = masrafTalebi.Personel.Ad + " " + masrafTalebi.Personel.Soyad,
                    PersonelPozisyon = masrafTalebi.Personel.Pozisyon.Ad,
                    PersonelDepartman = masrafTalebi.Personel.Pozisyon.Departman.Ad,
                    masrafTalebi.MasrafTipi,
                    MasrafTipiAd = GetMasrafTipiAd(masrafTalebi.MasrafTipi),
                    masrafTalebi.TalepTarihi,
                    masrafTalebi.Tutar,
                    masrafTalebi.Aciklama,
                    masrafTalebi.BelgeUrl,
                    masrafTalebi.OnayDurumu,
                    masrafTalebi.OnaylayanId,
                    OnaylayanAd = masrafTalebi.Onaylayan != null ? masrafTalebi.Onaylayan.Ad + " " + masrafTalebi.Onaylayan.Soyad : null,
                    masrafTalebi.OnayTarihi,
                    masrafTalebi.OnayNotu,
                    masrafTalebi.CreatedAt,
                    masrafTalebi.UpdatedAt
                };

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Masraf talebi getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/Masraf/Limit/{personelId}/{masrafTipi}
        [HttpGet("Limit/{personelId}/{masrafTipi}")]
        public async Task<ActionResult<object>> GetMasrafLimit(int personelId, MasrafTipi masrafTipi)
        {
            try
            {
                var limit = await _masrafService.GetMasrafLimit(personelId, masrafTipi);
                var aylikToplam = await _masrafService.GetAylikMasrafToplami(personelId, DateTime.Now.Month, DateTime.Now.Year);
                
                return Ok(new { 
                    success = true, 
                    data = new { 
                        limit = limit,
                        aylikToplam = aylikToplam,
                        kalanLimit = Math.Max(0, limit - aylikToplam),
                        masrafTipi = masrafTipi,
                        masrafTipiAd = GetMasrafTipiAd(masrafTipi)
                    } 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Masraf limiti getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        // POST: api/Masraf
        [HttpPost]
        public async Task<ActionResult<object>> PostMasrafTalebi([FromBody] JsonElement masrafData)
        {
            try
            {
                var personelId = masrafData.GetProperty("personelId").GetInt32();
                var masrafTipi = (MasrafTipi)masrafData.GetProperty("masrafTipi").GetInt32();
                var tutar = masrafData.GetProperty("tutar").GetDecimal();
                var aciklama = masrafData.TryGetProperty("aciklama", out var aciklamaElement) ? aciklamaElement.GetString() : "";

                // Masraf limitini kontrol et
                var limitKontrolu = await _masrafService.CheckMasrafLimit(personelId, masrafTipi, tutar);
                if (!limitKontrolu)
                {
                    var limit = await _masrafService.GetMasrafLimit(personelId, masrafTipi);
                    return BadRequest(new { 
                        success = false, 
                        message = $"Masraf limiti aşıldı. {GetMasrafTipiAd(masrafTipi)} için aylık limitiniz: {limit:C2}" 
                    });
                }

                var masrafTalebi = new MasrafTalebi
                {
                    PersonelId = personelId,
                    MasrafTipi = masrafTipi,
                    TalepTarihi = DateTime.UtcNow,
                    Tutar = tutar,
                    Aciklama = aciklama,
                    OnayDurumu = "Beklemede",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.MasrafTalepleri.Add(masrafTalebi);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetMasrafTalebi), new { id = masrafTalebi.Id }, 
                    new { success = true, data = masrafTalebi, message = "Masraf talebi başarıyla oluşturuldu." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Masraf talebi oluşturulurken bir hata oluştu.", error = ex.Message });
            }
        }

        // PUT: api/Masraf/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<object>> UpdateMasrafTalebi(int id, [FromBody] JsonElement masrafData)
        {
            try
            {
                var existingMasraf = await _context.MasrafTalepleri.FindAsync(id);
                if (existingMasraf == null)
                {
                    return NotFound(new { success = false, message = "Masraf talebi bulunamadı." });
                }

                if (existingMasraf.OnayDurumu != "Beklemede")
                {
                    return BadRequest(new { success = false, message = "Sadece beklemede olan masraf talepleri düzenlenebilir." });
                }

                var personelId = masrafData.GetProperty("personelId").GetInt32();
                var masrafTipi = (MasrafTipi)masrafData.GetProperty("masrafTipi").GetInt32();
                var tutar = masrafData.GetProperty("tutar").GetDecimal();
                var aciklama = masrafData.TryGetProperty("aciklama", out var aciklamaElement) ? aciklamaElement.GetString() : "";
                var belgeUrl = masrafData.TryGetProperty("belgeUrl", out var belgeElement) ? belgeElement.GetString() : null;

                // Eğer tutar veya masraf tipi değişmişse limit kontrolü yap
                if (existingMasraf.Tutar != tutar || existingMasraf.MasrafTipi != masrafTipi)
                {
                    var limitKontrolu = await _masrafService.CheckMasrafLimit(personelId, masrafTipi, tutar);
                    if (!limitKontrolu)
                    {
                        var limit = await _masrafService.GetMasrafLimit(personelId, masrafTipi);
                        return BadRequest(new {
                            success = false,
                            message = $"Masraf limiti aşıldı. {GetMasrafTipiAd(masrafTipi)} için aylık limitiniz: {limit:C2}"
                        });
                    }
                }

                // Güncelleme işlemi
                existingMasraf.MasrafTipi = masrafTipi;
                existingMasraf.Tutar = tutar;
                existingMasraf.Aciklama = aciklama;
                existingMasraf.BelgeUrl = belgeUrl;
                existingMasraf.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, data = existingMasraf, message = "Masraf talebi başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Masraf talebi güncellenirken bir hata oluştu.", error = ex.Message });
            }
        }

        // PUT: api/Masraf/Onayla/{id}
        [HttpPut("Onayla/{id}")]
        public async Task<ActionResult<object>> OnaylaMasrafTalebi(int id, [FromBody] JsonElement onayData)
        {
            try
            {
                var masrafTalebi = await _context.MasrafTalepleri
                    .Include(m => m.Personel)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (masrafTalebi == null)
                {
                    return NotFound(new { success = false, message = "Masraf talebi bulunamadı." });
                }

                if (masrafTalebi.OnayDurumu != "Beklemede")
                {
                    return BadRequest(new { success = false, message = "Bu masraf talebi zaten işlenmiş." });
                }

                var onayDurumu = onayData.GetProperty("onayDurumu").GetString();
                var onaylayanId = onayData.GetProperty("onaylayanId").GetInt32();
                var onayNotu = onayData.TryGetProperty("onayNotu", out var notElement) ? notElement.GetString() : "";

                masrafTalebi.OnayDurumu = onayDurumu ?? "Reddedildi";
                masrafTalebi.OnaylayanId = onaylayanId;
                masrafTalebi.OnayTarihi = DateTime.UtcNow;
                masrafTalebi.OnayNotu = onayNotu;
                masrafTalebi.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Bildirim gönder
                var onaylayan = await _context.Personeller.FindAsync(onaylayanId);
                var masrafTipiAd = GetMasrafTipiAd(masrafTalebi.MasrafTipi);

                if (onayDurumu == "Onaylandı")
                {
                    var bildirim = new Bildirim
                    {
                        AliciId = masrafTalebi.PersonelId,
                        Baslik = "Masraf Talebiniz Onaylandı",
                        Mesaj = $"{masrafTipiAd} kategorisinde {masrafTalebi.Tutar:C2} tutarındaki masraf talebiniz onaylanmıştır.",
                        Kategori = "masraf",
                        Tip = "success",
                        GonderenAd = onaylayan != null ? $"{onaylayan.Ad} {onaylayan.Soyad}" : "Sistem",
                        ActionUrl = "/masraf-talepleri"
                    };
                    await _bildirimService.CreateBildirimAsync(bildirim);
                }
                else
                {
                    var bildirim = new Bildirim
                    {
                        AliciId = masrafTalebi.PersonelId,
                        Baslik = "Masraf Talebiniz Reddedildi",
                        Mesaj = $"{masrafTipiAd} kategorisinde {masrafTalebi.Tutar:C2} tutarındaki masraf talebiniz reddedilmiştir. {(string.IsNullOrEmpty(onayNotu) ? "" : $"Açıklama: {onayNotu}")}",
                        Kategori = "masraf",
                        Tip = "error",
                        GonderenAd = onaylayan != null ? $"{onaylayan.Ad} {onaylayan.Soyad}" : "Sistem",
                        ActionUrl = "/masraf-talepleri"
                    };
                    await _bildirimService.CreateBildirimAsync(bildirim);
                }

                return Ok(new { success = true, data = masrafTalebi, message = "Masraf talebi başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Masraf talebi güncellenirken bir hata oluştu.", error = ex.Message });
            }
        }

        // DELETE: api/Masraf/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<object>> DeleteMasrafTalebi(int id)
        {
            try
            {
                var masrafTalebi = await _context.MasrafTalepleri.FindAsync(id);
                if (masrafTalebi == null)
                {
                    return NotFound(new { success = false, message = "Masraf talebi bulunamadı." });
                }

                if (masrafTalebi.OnayDurumu != "Beklemede")
                {
                    return BadRequest(new { success = false, message = "Onaylanmış veya reddedilmiş masraf talebi silinemez." });
                }

                _context.MasrafTalepleri.Remove(masrafTalebi);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Masraf talebi başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Masraf talebi silinirken bir hata oluştu.", error = ex.Message });
            }
        }

        private static string GetMasrafTipiAd(MasrafTipi masrafTipi)
        {
            return masrafTipi switch
            {
                MasrafTipi.Yemek => "Yemek",
                MasrafTipi.Ulasim => "Ulaşım",
                MasrafTipi.Konaklama => "Konaklama",
                MasrafTipi.Egitim => "Eğitim",
                MasrafTipi.Diger => "Diğer",
                _ => "Bilinmiyor"
            };
        }
    }
}