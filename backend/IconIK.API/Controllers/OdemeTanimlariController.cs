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
    public class OdemeTanimlariController : ControllerBase
    {
        private readonly IconIKContext _context;

        public OdemeTanimlariController(IconIKContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tüm ödeme tanımlarını getir
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var tanimlar = await _context.OdemeTanimlari
                    .OrderBy(t => t.OdemeTuru)
                    .ThenBy(t => t.Ad)
                    .ToListAsync();

                return Ok(new { success = true, data = tanimlar });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Sadece aktif ödeme tanımlarını getir (dropdown için)
        /// </summary>
        [HttpGet("aktif")]
        public async Task<IActionResult> GetAktif()
        {
            try
            {
                var tanimlar = await _context.OdemeTanimlari
                    .Where(t => t.Aktif)
                    .OrderBy(t => t.OdemeTuru)
                    .ThenBy(t => t.Ad)
                    .ToListAsync();

                return Ok(new { success = true, data = tanimlar });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Belirli bir ödeme tanımını getir
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var tanim = await _context.OdemeTanimlari.FindAsync(id);
                if (tanim == null)
                    return NotFound(new { success = false, message = "Ödeme tanımı bulunamadı" });

                return Ok(new { success = true, data = tanim });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Yeni ödeme tanımı oluştur
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OdemeTanimi tanim)
        {
            try
            {
                // Aynı kod ile tanım var mı kontrol et
                var mevcut = await _context.OdemeTanimlari
                    .FirstOrDefaultAsync(t => t.Kod == tanim.Kod);

                if (mevcut != null)
                    return BadRequest(new { success = false, message = "Bu ödeme kodu zaten kullanılıyor" });

                tanim.Aktif = true;
                tanim.CreatedAt = DateTime.UtcNow;
                tanim.UpdatedAt = DateTime.UtcNow;

                _context.OdemeTanimlari.Add(tanim);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, data = tanim, message = "Ödeme tanımı başarıyla oluşturuldu" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Ödeme tanımını güncelle
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] OdemeTanimi guncellenmis)
        {
            try
            {
                var tanim = await _context.OdemeTanimlari.FindAsync(id);
                if (tanim == null)
                    return NotFound(new { success = false, message = "Ödeme tanımı bulunamadı" });

                // Kod değiştiriliyorsa, başkası kullanıyor mu kontrol et
                if (tanim.Kod != guncellenmis.Kod)
                {
                    var kodKullaniliyor = await _context.OdemeTanimlari
                        .AnyAsync(t => t.Kod == guncellenmis.Kod && t.Id != id);

                    if (kodKullaniliyor)
                        return BadRequest(new { success = false, message = "Bu ödeme kodu başka bir tanımda kullanılıyor" });
                }

                // Güncelle
                tanim.Kod = guncellenmis.Kod;
                tanim.Ad = guncellenmis.Ad;
                tanim.OdemeTuru = guncellenmis.OdemeTuru;
                tanim.Aciklama = guncellenmis.Aciklama;
                tanim.SgkMatrahinaDahil = guncellenmis.SgkMatrahinaDahil;
                tanim.VergiMatrahinaDahil = guncellenmis.VergiMatrahinaDahil;
                tanim.DamgaVergisiDahil = guncellenmis.DamgaVergisiDahil;
                tanim.AgiUygulanir = guncellenmis.AgiUygulanir;
                tanim.Aktif = guncellenmis.Aktif;
                tanim.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, data = tanim, message = "Ödeme tanımı başarıyla güncellendi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Ödeme tanımını pasifleştir
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var tanim = await _context.OdemeTanimlari.FindAsync(id);
                if (tanim == null)
                    return NotFound(new { success = false, message = "Ödeme tanımı bulunamadı" });

                // Kullanılıyor mu kontrol et
                var kullaniliyor = await _context.BordroOdemeler
                    .AnyAsync(o => o.OdemeKodu == tanim.Kod);

                if (kullaniliyor)
                {
                    // Kullanılıyorsa sadece pasifleştir
                    tanim.Aktif = false;
                    tanim.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return Ok(new { success = true, message = "Ödeme tanımı pasifleştirildi (bordroda kullanıldığı için)" });
                }
                else
                {
                    // Kullanılmıyorsa tamamen sil
                    _context.OdemeTanimlari.Remove(tanim);
                    await _context.SaveChangesAsync();
                    return Ok(new { success = true, message = "Ödeme tanımı başarıyla silindi" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Ödeme türüne göre tanımları getir
        /// </summary>
        [HttpGet("tur/{odemeTuru}")]
        public async Task<IActionResult> GetByTur(string odemeTuru)
        {
            try
            {
                var tanimlar = await _context.OdemeTanimlari
                    .Where(t => t.OdemeTuru == odemeTuru && t.Aktif)
                    .OrderBy(t => t.Ad)
                    .ToListAsync();

                return Ok(new { success = true, data = tanimlar });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Ödeme türlerini grupla ve say
        /// </summary>
        [HttpGet("tur-ozeti")]
        public async Task<IActionResult> GetTurOzeti()
        {
            try
            {
                var ozet = await _context.OdemeTanimlari
                    .Where(t => t.Aktif)
                    .GroupBy(t => t.OdemeTuru)
                    .Select(g => new
                    {
                        OdemeTuru = g.Key,
                        Adet = g.Count()
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = ozet });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
