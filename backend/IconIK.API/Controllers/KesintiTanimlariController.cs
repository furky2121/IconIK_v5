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
    public class KesintiTanimlariController : ControllerBase
    {
        private readonly IconIKContext _context;

        public KesintiTanimlariController(IconIKContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tüm kesinti tanımlarını getir
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var tanimlar = await _context.KesintiTanimlari
                    .OrderBy(t => t.KesintiTuru)
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
        /// Sadece aktif kesinti tanımlarını getir (dropdown için)
        /// </summary>
        [HttpGet("aktif")]
        public async Task<IActionResult> GetAktif()
        {
            try
            {
                var tanimlar = await _context.KesintiTanimlari
                    .Where(t => t.Aktif)
                    .OrderBy(t => t.KesintiTuru)
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
        /// Belirli bir kesinti tanımını getir
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var tanim = await _context.KesintiTanimlari.FindAsync(id);
                if (tanim == null)
                    return NotFound(new { success = false, message = "Kesinti tanımı bulunamadı" });

                return Ok(new { success = true, data = tanim });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Yeni kesinti tanımı oluştur
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] KesintiTanimi tanim)
        {
            try
            {
                // Aynı kod ile tanım var mı kontrol et
                var mevcut = await _context.KesintiTanimlari
                    .FirstOrDefaultAsync(t => t.Kod == tanim.Kod);

                if (mevcut != null)
                    return BadRequest(new { success = false, message = "Bu kesinti kodu zaten kullanılıyor" });

                tanim.Aktif = true;
                tanim.CreatedAt = DateTime.UtcNow;
                tanim.UpdatedAt = DateTime.UtcNow;

                _context.KesintiTanimlari.Add(tanim);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, data = tanim, message = "Kesinti tanımı başarıyla oluşturuldu" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Kesinti tanımını güncelle
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] KesintiTanimi guncellenmis)
        {
            try
            {
                var tanim = await _context.KesintiTanimlari.FindAsync(id);
                if (tanim == null)
                    return NotFound(new { success = false, message = "Kesinti tanımı bulunamadı" });

                // Kod değiştiriliyorsa, başkası kullanıyor mu kontrol et
                if (tanim.Kod != guncellenmis.Kod)
                {
                    var kodKullaniliyor = await _context.KesintiTanimlari
                        .AnyAsync(t => t.Kod == guncellenmis.Kod && t.Id != id);

                    if (kodKullaniliyor)
                        return BadRequest(new { success = false, message = "Bu kesinti kodu başka bir tanımda kullanılıyor" });
                }

                // Güncelle
                tanim.Kod = guncellenmis.Kod;
                tanim.Ad = guncellenmis.Ad;
                tanim.KesintiTuru = guncellenmis.KesintiTuru;
                tanim.Aciklama = guncellenmis.Aciklama;
                tanim.OtomatikHesaplama = guncellenmis.OtomatikHesaplama;
                tanim.Taksitlenebilir = guncellenmis.Taksitlenebilir;
                tanim.Aktif = guncellenmis.Aktif;
                tanim.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, data = tanim, message = "Kesinti tanımı başarıyla güncellendi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Kesinti tanımını pasifleştir veya sil
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var tanim = await _context.KesintiTanimlari.FindAsync(id);
                if (tanim == null)
                    return NotFound(new { success = false, message = "Kesinti tanımı bulunamadı" });

                // Kullanılıyor mu kontrol et
                var kullaniliyor = await _context.BordroKesintiler
                    .AnyAsync(k => k.KesintiKodu == tanim.Kod);

                if (kullaniliyor)
                {
                    // Kullanılıyorsa sadece pasifleştir
                    tanim.Aktif = false;
                    tanim.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return Ok(new { success = true, message = "Kesinti tanımı pasifleştirildi (bordroda kullanıldığı için)" });
                }
                else
                {
                    // Kullanılmıyorsa tamamen sil
                    _context.KesintiTanimlari.Remove(tanim);
                    await _context.SaveChangesAsync();
                    return Ok(new { success = true, message = "Kesinti tanımı başarıyla silindi" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Kesinti türüne göre tanımları getir
        /// </summary>
        [HttpGet("tur/{kesintiTuru}")]
        public async Task<IActionResult> GetByTur(string kesintiTuru)
        {
            try
            {
                var tanimlar = await _context.KesintiTanimlari
                    .Where(t => t.KesintiTuru == kesintiTuru && t.Aktif)
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
        /// Kesinti türlerini grupla ve say
        /// </summary>
        [HttpGet("tur-ozeti")]
        public async Task<IActionResult> GetTurOzeti()
        {
            try
            {
                var ozet = await _context.KesintiTanimlari
                    .Where(t => t.Aktif)
                    .GroupBy(t => t.KesintiTuru)
                    .Select(g => new
                    {
                        KesintiTuru = g.Key,
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

        /// <summary>
        /// Taksitlendirilebilir kesintileri getir
        /// </summary>
        [HttpGet("taksitlenebilir")]
        public async Task<IActionResult> GetTaksitlenebilir()
        {
            try
            {
                var tanimlar = await _context.KesintiTanimlari
                    .Where(t => t.Taksitlenebilir && t.Aktif)
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
        /// Otomatik hesaplanan kesintileri getir
        /// </summary>
        [HttpGet("otomatik")]
        public async Task<IActionResult> GetOtomatik()
        {
            try
            {
                var tanimlar = await _context.KesintiTanimlari
                    .Where(t => t.OtomatikHesaplama && t.Aktif)
                    .OrderBy(t => t.Ad)
                    .ToListAsync();

                return Ok(new { success = true, data = tanimlar });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
