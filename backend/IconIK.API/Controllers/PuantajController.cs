using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using IconIK.API.Data;
using IconIK.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace IconIK.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PuantajController : ControllerBase
    {
        private readonly IconIKContext _context;

        public PuantajController(IconIKContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tüm puantajları getir
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? yil, [FromQuery] int? ay, [FromQuery] int? personelId)
        {
            try
            {
                var query = _context.Puantajlar
                    .Include(p => p.Personel)
                        .ThenInclude(per => per.Pozisyon)
                    .Include(p => p.Onaylayan)
                    .AsQueryable();

                if (yil.HasValue)
                    query = query.Where(p => p.DonemYil == yil.Value);

                if (ay.HasValue)
                    query = query.Where(p => p.DonemAy == ay.Value);

                if (personelId.HasValue)
                    query = query.Where(p => p.PersonelId == personelId.Value);

                var puantajlar = await query
                    .OrderByDescending(p => p.DonemYil)
                    .ThenByDescending(p => p.DonemAy)
                    .ThenBy(p => p.Personel.Ad)
                    .ToListAsync();

                return Ok(new { success = true, data = puantajlar });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Puantaj detayını getir
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var puantaj = await _context.Puantajlar
                    .Include(p => p.Personel)
                    .Include(p => p.Onaylayan)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (puantaj == null)
                    return NotFound(new { success = false, message = "Puantaj bulunamadı" });

                return Ok(new { success = true, data = puantaj });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Yeni puantaj oluştur
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] JsonElement body)
        {
            try
            {
                var personelId = body.GetProperty("personelId").GetInt32();
                var donemYil = body.GetProperty("donemYil").GetInt32();
                var donemAy = body.GetProperty("donemAy").GetInt32();

                // Aynı dönem için puantaj var mı kontrol et
                var mevcutPuantaj = await _context.Puantajlar
                    .FirstOrDefaultAsync(p => p.PersonelId == personelId &&
                                             p.DonemYil == donemYil &&
                                             p.DonemAy == donemAy);

                if (mevcutPuantaj != null)
                    return BadRequest(new { success = false, message = "Bu personel için bu dönemde zaten puantaj mevcut" });

                var puantaj = new Puantaj
                {
                    PersonelId = personelId,
                    DonemYil = donemYil,
                    DonemAy = donemAy,
                    CalisanGunSayisi = body.TryGetProperty("calisanGunSayisi", out var cgsElement) ? cgsElement.GetInt32() : 0,
                    HaftaSonuCalisma = body.TryGetProperty("haftaSonuCalisma", out var hscElement) ? hscElement.GetInt32() : 0,
                    ResmiTatilCalisma = body.TryGetProperty("resmiTatilCalisma", out var rtcElement) ? rtcElement.GetInt32() : 0,
                    YillikIzin = body.TryGetProperty("yillikIzin", out var yiElement) ? yiElement.GetInt32() : 0,
                    UcretsizIzin = body.TryGetProperty("ucretsizIzin", out var uiElement) ? uiElement.GetInt32() : 0,
                    HastalikIzni = body.TryGetProperty("hastalikIzni", out var hiElement) ? hiElement.GetInt32() : 0,
                    MazeretIzni = body.TryGetProperty("mazeretIzni", out var miElement) ? miElement.GetInt32() : 0,
                    HaftaIciMesaiSaat = body.TryGetProperty("haftaIciMesaiSaat", out var himsElement) ? himsElement.GetDecimal() : 0,
                    HaftaSonuMesaiSaat = body.TryGetProperty("haftaSonuMesaiSaat", out var hsmsElement) ? hsmsElement.GetDecimal() : 0,
                    GeceMesaiSaat = body.TryGetProperty("geceMesaiSaat", out var gmsElement) ? gmsElement.GetDecimal() : 0,
                    ResmiTatilMesaiSaat = body.TryGetProperty("resmiTatilMesaiSaat", out var rtmsElement) ? rtmsElement.GetDecimal() : 0,
                    GecGelmeDakika = body.TryGetProperty("gecGelmeDakika", out var ggdElement) ? ggdElement.GetInt32() : 0,
                    ErkenCikmaDakika = body.TryGetProperty("erkenCikmaDakika", out var ecdElement) ? ecdElement.GetInt32() : 0,
                    DevamsizlikGun = body.TryGetProperty("devamsizlikGun", out var dgElement) ? dgElement.GetInt32() : 0,
                    Notlar = body.TryGetProperty("notlar", out var notElement) ? notElement.GetString() : null,
                    OnayDurumu = "Taslak",
                    Aktif = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Toplamları hesapla
                puantaj.ToplamCalisilanGun = puantaj.CalisanGunSayisi + puantaj.HaftaSonuCalisma + puantaj.ResmiTatilCalisma;
                puantaj.ToplamMesaiSaat = puantaj.HaftaIciMesaiSaat + puantaj.HaftaSonuMesaiSaat + puantaj.GeceMesaiSaat + puantaj.ResmiTatilMesaiSaat;

                _context.Puantajlar.Add(puantaj);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, data = puantaj, message = "Puantaj başarıyla oluşturuldu" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Puantaj güncelle
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] JsonElement body)
        {
            try
            {
                var puantaj = await _context.Puantajlar.FindAsync(id);
                if (puantaj == null)
                    return NotFound(new { success = false, message = "Puantaj bulunamadı" });

                // Onaylandıysa güncellenemez
                if (puantaj.OnayDurumu == "Onaylandi")
                    return BadRequest(new { success = false, message = "Onaylanmış puantaj güncellenemez" });

                // Güncellemeleri uygula
                if (body.TryGetProperty("calisanGunSayisi", out var cgsElement))
                    puantaj.CalisanGunSayisi = cgsElement.GetInt32();
                if (body.TryGetProperty("haftaSonuCalisma", out var hscElement))
                    puantaj.HaftaSonuCalisma = hscElement.GetInt32();
                if (body.TryGetProperty("resmiTatilCalisma", out var rtcElement))
                    puantaj.ResmiTatilCalisma = rtcElement.GetInt32();
                if (body.TryGetProperty("yillikIzin", out var yiElement))
                    puantaj.YillikIzin = yiElement.GetInt32();
                if (body.TryGetProperty("ucretsizIzin", out var uiElement))
                    puantaj.UcretsizIzin = uiElement.GetInt32();
                if (body.TryGetProperty("hastalikIzni", out var hiElement))
                    puantaj.HastalikIzni = hiElement.GetInt32();
                if (body.TryGetProperty("mazeretIzni", out var miElement))
                    puantaj.MazeretIzni = miElement.GetInt32();
                if (body.TryGetProperty("haftaIciMesaiSaat", out var himsElement))
                    puantaj.HaftaIciMesaiSaat = himsElement.GetDecimal();
                if (body.TryGetProperty("haftaSonuMesaiSaat", out var hsmsElement))
                    puantaj.HaftaSonuMesaiSaat = hsmsElement.GetDecimal();
                if (body.TryGetProperty("geceMesaiSaat", out var gmsElement))
                    puantaj.GeceMesaiSaat = gmsElement.GetDecimal();
                if (body.TryGetProperty("resmiTatilMesaiSaat", out var rtmsElement))
                    puantaj.ResmiTatilMesaiSaat = rtmsElement.GetDecimal();
                if (body.TryGetProperty("gecGelmeDakika", out var ggdElement))
                    puantaj.GecGelmeDakika = ggdElement.GetInt32();
                if (body.TryGetProperty("erkenCikmaDakika", out var ecdElement))
                    puantaj.ErkenCikmaDakika = ecdElement.GetInt32();
                if (body.TryGetProperty("devamsizlikGun", out var dgElement))
                    puantaj.DevamsizlikGun = dgElement.GetInt32();
                if (body.TryGetProperty("notlar", out var notElement))
                    puantaj.Notlar = notElement.GetString();

                // Toplamları yeniden hesapla
                puantaj.ToplamCalisilanGun = puantaj.CalisanGunSayisi + puantaj.HaftaSonuCalisma + puantaj.ResmiTatilCalisma;
                puantaj.ToplamMesaiSaat = puantaj.HaftaIciMesaiSaat + puantaj.HaftaSonuMesaiSaat + puantaj.GeceMesaiSaat + puantaj.ResmiTatilMesaiSaat;

                puantaj.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new { success = true, data = puantaj, message = "Puantaj başarıyla güncellendi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Puantaj onayla
        /// </summary>
        [HttpPost("{id}/onayla")]
        public async Task<IActionResult> Onayla(int id, [FromBody] JsonElement body)
        {
            try
            {
                var puantaj = await _context.Puantajlar.FindAsync(id);
                if (puantaj == null)
                    return NotFound(new { success = false, message = "Puantaj bulunamadı" });

                var onaylayanId = body.GetProperty("onaylayanId").GetInt32();

                puantaj.OnayDurumu = "Onaylandi";
                puantaj.OnaylayanId = onaylayanId;
                puantaj.OnayTarihi = DateTime.UtcNow;
                puantaj.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Puantaj başarıyla onaylandı" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Puantaj reddet
        /// </summary>
        [HttpPost("{id}/reddet")]
        public async Task<IActionResult> Reddet(int id, [FromBody] JsonElement body)
        {
            try
            {
                var puantaj = await _context.Puantajlar.FindAsync(id);
                if (puantaj == null)
                    return NotFound(new { success = false, message = "Puantaj bulunamadı" });

                var onaylayanId = body.GetProperty("onaylayanId").GetInt32();
                var redNedeni = body.GetProperty("redNedeni").GetString() ?? "";

                puantaj.OnayDurumu = "Reddedildi";
                puantaj.OnaylayanId = onaylayanId;
                puantaj.OnayTarihi = DateTime.UtcNow;
                puantaj.RedNedeni = redNedeni;
                puantaj.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Puantaj reddedildi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Puantaj sil
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var puantaj = await _context.Puantajlar.FindAsync(id);
                if (puantaj == null)
                    return NotFound(new { success = false, message = "Puantaj bulunamadı" });

                // Onaylandıysa veya bordroda kullanılıyorsa silinemez
                if (puantaj.OnayDurumu == "Onaylandi")
                    return BadRequest(new { success = false, message = "Onaylanmış puantaj silinemez" });

                var bordroKullaniyor = await _context.BordroAna.AnyAsync(b => b.PuantajId == id);
                if (bordroKullaniyor)
                    return BadRequest(new { success = false, message = "Bordroda kullanılan puantaj silinemez" });

                _context.Puantajlar.Remove(puantaj);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Puantaj başarıyla silindi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
