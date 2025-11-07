using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using IconIK.API.Services;
using IconIK.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace IconIK.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BordroController : ControllerBase
    {
        private readonly IBordroService _bordroService;
        private readonly IconIKContext _context;

        public BordroController(IBordroService bordroService, IconIKContext context)
        {
            _bordroService = bordroService;
            _context = context;
        }

        /// <summary>
        /// Tüm bordroları getir (filtreleme ile)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? yil, [FromQuery] int? ay, [FromQuery] int? personelId)
        {
            try
            {
                var query = _context.BordroAna
                    .Include(b => b.Personel)
                        .ThenInclude(p => p.Pozisyon)
                    .AsQueryable();

                if (yil.HasValue)
                    query = query.Where(b => b.DonemYil == yil.Value);

                if (ay.HasValue)
                    query = query.Where(b => b.DonemAy == ay.Value);

                if (personelId.HasValue)
                    query = query.Where(b => b.PersonelId == personelId.Value);

                var bordrolar = await query
                    .OrderByDescending(b => b.DonemYil)
                    .ThenByDescending(b => b.DonemAy)
                    .ThenBy(b => b.Personel.Ad)
                    .ToListAsync();

                return Ok(new { success = true, data = bordrolar });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Bordro detayını getir (ödemeler ve kesintilerle)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var bordro = await _bordroService.GetBordroDetay(id);
                if (bordro == null)
                    return NotFound(new { success = false, message = "Bordro bulunamadı" });

                return Ok(new { success = true, data = bordro });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Tek personel için bordro hesapla
        /// </summary>
        [HttpPost("hesapla")]
        public async Task<IActionResult> HesaplaBordro([FromBody] JsonElement body)
        {
            try
            {
                var personelId = body.GetProperty("personelId").GetInt32();
                var donemYil = body.GetProperty("donemYil").GetInt32();
                var donemAy = body.GetProperty("donemAy").GetInt32();

                int? puantajId = null;
                if (body.TryGetProperty("puantajId", out var puantajElement))
                    puantajId = puantajElement.GetInt32();

                // Aynı dönem için bordro var mı kontrol et
                var mevcutBordro = await _context.BordroAna
                    .FirstOrDefaultAsync(b => b.PersonelId == personelId &&
                                             b.DonemYil == donemYil &&
                                             b.DonemAy == donemAy);

                if (mevcutBordro != null)
                    return BadRequest(new { success = false, message = "Bu personel için bu dönemde zaten bordro mevcut" });

                var bordro = await _bordroService.HesaplaBordro(personelId, donemYil, donemAy, puantajId);

                return Ok(new {
                    success = true,
                    data = bordro,
                    message = "Bordro başarıyla hesaplandı"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Toplu bordro hesapla (tüm personeller veya departman/pozisyon bazlı)
        /// </summary>
        [HttpPost("hesapla-toplu")]
        public async Task<IActionResult> HesaplaTopluBordro([FromBody] JsonElement body)
        {
            try
            {
                var donemYil = body.GetProperty("donemYil").GetInt32();
                var donemAy = body.GetProperty("donemAy").GetInt32();

                int? departmanId = null;
                if (body.TryGetProperty("departmanId", out var depElement))
                    departmanId = depElement.GetInt32();

                int? pozisyonId = null;
                if (body.TryGetProperty("pozisyonId", out var pozElement))
                    pozisyonId = pozElement.GetInt32();

                var bordrolar = await _bordroService.HesaplaTopluBordro(donemYil, donemAy, departmanId, pozisyonId);

                return Ok(new {
                    success = true,
                    data = bordrolar,
                    message = $"{bordrolar.Count} adet bordro başarıyla hesaplandı"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Brüt -> Net hesaplama (önizleme)
        /// </summary>
        [HttpPost("hesapla-brut-net")]
        public async Task<IActionResult> HesaplaBrutNet([FromBody] JsonElement body)
        {
            try
            {
                var brutMaas = body.GetProperty("brutMaas").GetDecimal();
                var medeniDurum = body.TryGetProperty("medeniDurum", out var mdElement)
                    ? mdElement.GetString() ?? "Bekar"
                    : "Bekar";
                var cocukSayisi = body.TryGetProperty("cocukSayisi", out var csElement)
                    ? csElement.GetInt32()
                    : 0;
                var engelliDurumu = body.TryGetProperty("engelliDurumu", out var edElement)
                    ? edElement.GetBoolean()
                    : false;
                var donemYil = body.GetProperty("donemYil").GetInt32();
                var donemAy = body.GetProperty("donemAy").GetInt32();

                var hesapDetay = await _bordroService.HesaplaBrutNet(
                    brutMaas, medeniDurum, cocukSayisi, engelliDurumu, donemYil, donemAy);

                return Ok(new { success = true, data = hesapDetay });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Bordro onayla
        /// </summary>
        [HttpPost("{id}/onayla")]
        public async Task<IActionResult> OnaylaBordro(int id, [FromBody] JsonElement body)
        {
            try
            {
                var onaylayanPersonelId = body.GetProperty("onaylayanPersonelId").GetInt32();
                var onayNotu = body.TryGetProperty("onayNotu", out var notElement)
                    ? notElement.GetString() ?? ""
                    : "";

                var sonuc = await _bordroService.OnayBordro(id, onaylayanPersonelId, onayNotu);

                if (!sonuc)
                    return BadRequest(new { success = false, message = "Bordro onaylanamadı" });

                return Ok(new { success = true, message = "Bordro başarıyla onaylandı" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Bordro reddet
        /// </summary>
        [HttpPost("{id}/reddet")]
        public async Task<IActionResult> ReddetBordro(int id, [FromBody] JsonElement body)
        {
            try
            {
                var onaylayanPersonelId = body.GetProperty("onaylayanPersonelId").GetInt32();
                var redNedeni = body.GetProperty("redNedeni").GetString() ?? "";

                var sonuc = await _bordroService.RedBordro(id, onaylayanPersonelId, redNedeni);

                if (!sonuc)
                    return BadRequest(new { success = false, message = "Bordro reddedilemedi" });

                return Ok(new { success = true, message = "Bordro reddedildi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Bordro sil
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var sonuc = await _bordroService.SilBordro(id);

                if (!sonuc)
                    return BadRequest(new { success = false, message = "Bordro silinemedi. Sadece Taslak veya Reddedildi durumundaki bordrolar silinebilir." });

                return Ok(new { success = true, message = "Bordro başarıyla silindi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Personelin tüm bordrolarını getir
        /// </summary>
        [HttpGet("personel/{personelId}")]
        public async Task<IActionResult> GetPersonelBordrolar(int personelId, [FromQuery] int? yil)
        {
            try
            {
                var bordrolar = await _bordroService.GetPersonelBordrolar(personelId, yil);
                return Ok(new { success = true, data = bordrolar });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Dönem bazlı bordro özeti (dashboard için)
        /// </summary>
        [HttpGet("ozet")]
        public async Task<IActionResult> GetBordroOzet([FromQuery] int yil, [FromQuery] int ay)
        {
            try
            {
                var bordrolar = await _context.BordroAna
                    .Where(b => b.DonemYil == yil && b.DonemAy == ay && b.BordroDurumu != "Iptal")
                    .ToListAsync();

                var ozet = new
                {
                    ToplamPersonel = bordrolar.Count,
                    ToplamBrutMaas = bordrolar.Sum(b => b.BrutMaas),
                    ToplamNetMaas = bordrolar.Sum(b => b.NetUcret),
                    ToplamSgkIsci = bordrolar.Sum(b => b.SgkIsciPayi),
                    ToplamSgkIsveren = bordrolar.Sum(b => b.SgkIsverenPayi),
                    ToplamGelirVergisi = bordrolar.Sum(b => b.GelirVergisi),
                    ToplamKesinti = bordrolar.Sum(b => b.ToplamKesinti),
                    ToplamIsverenMaliyeti = bordrolar.Sum(b => b.IsverenMaliyeti),
                    TaslaklarSayisi = bordrolar.Count(b => b.BordroDurumu == "Taslak"),
                    OnaylanmislarSayisi = bordrolar.Count(b => b.BordroDurumu == "Onaylandi"),
                    OdenenlerSayisi = bordrolar.Count(b => b.OdemeDurumu == "Odendi")
                };

                return Ok(new { success = true, data = ozet });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Departman bazlı bordro dağılımı
        /// </summary>
        [HttpGet("departman-dagilim")]
        public async Task<IActionResult> GetDepartmanDagilim([FromQuery] int yil, [FromQuery] int ay)
        {
            try
            {
                var dagilim = await _context.BordroAna
                    .Where(b => b.DonemYil == yil && b.DonemAy == ay && b.BordroDurumu != "Iptal")
                    .GroupBy(b => b.DepartmanAdi)
                    .Select(g => new
                    {
                        DepartmanAdi = g.Key,
                        PersonelSayisi = g.Count(),
                        ToplamBrutMaas = g.Sum(b => b.BrutMaas),
                        ToplamNetMaas = g.Sum(b => b.NetUcret),
                        ToplamIsverenMaliyeti = g.Sum(b => b.IsverenMaliyeti)
                    })
                    .OrderByDescending(d => d.ToplamBrutMaas)
                    .ToListAsync();

                return Ok(new { success = true, data = dagilim });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
