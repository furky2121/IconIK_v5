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
    public class BordroParametreController : ControllerBase
    {
        private readonly IconIKContext _context;

        public BordroParametreController(IconIKContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tüm bordro parametrelerini getir
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var parametreler = await _context.BordroParametreleri
                    .Where(p => p.Aktif)
                    .OrderByDescending(p => p.Yil)
                    .ThenByDescending(p => p.Donem)
                    .ToListAsync();

                return Ok(new { success = true, data = parametreler });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Belirli yıl/dönem için parametre getir
        /// </summary>
        [HttpGet("{yil}/{donem}")]
        public async Task<IActionResult> GetByYilDonem(int yil, int donem)
        {
            try
            {
                var parametre = await _context.BordroParametreleri
                    .FirstOrDefaultAsync(p => p.Yil == yil && p.Donem == donem && p.Aktif);

                if (parametre == null)
                {
                    // En güncel parametreyi getir
                    parametre = await _context.BordroParametreleri
                        .Where(p => p.Aktif)
                        .OrderByDescending(p => p.Yil)
                        .ThenByDescending(p => p.Donem)
                        .FirstOrDefaultAsync();
                }

                if (parametre == null)
                    return NotFound(new { success = false, message = "Bordro parametresi bulunamadı" });

                return Ok(new { success = true, data = parametre });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Yeni parametre oluştur
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BordroParametreleri parametre)
        {
            try
            {
                // Aynı yıl/dönem için parametre var mı kontrol et
                var mevcut = await _context.BordroParametreleri
                    .FirstOrDefaultAsync(p => p.Yil == parametre.Yil && p.Donem == parametre.Donem);

                if (mevcut != null)
                    return BadRequest(new { success = false, message = "Bu yıl/dönem için parametre zaten mevcut" });

                parametre.Aktif = true;
                parametre.CreatedAt = DateTime.UtcNow;
                parametre.UpdatedAt = DateTime.UtcNow;

                _context.BordroParametreleri.Add(parametre);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, data = parametre, message = "Parametre başarıyla oluşturuldu" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Parametre güncelle
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] BordroParametreleri guncellenmisParametre)
        {
            try
            {
                var parametre = await _context.BordroParametreleri.FindAsync(id);
                if (parametre == null)
                    return NotFound(new { success = false, message = "Parametre bulunamadı" });

                // Güncelle
                parametre.AsgariUcretBrut = guncellenmisParametre.AsgariUcretBrut;
                parametre.AsgariUcretNet = guncellenmisParametre.AsgariUcretNet;
                parametre.AgiOrani = guncellenmisParametre.AgiOrani;
                parametre.AgiTutari = guncellenmisParametre.AgiTutari;
                parametre.SgkIsciOrani = guncellenmisParametre.SgkIsciOrani;
                parametre.SgkIsverenOrani = guncellenmisParametre.SgkIsverenOrani;
                parametre.SgkTavanBrut = guncellenmisParametre.SgkTavanBrut;
                parametre.SgkTabanBrut = guncellenmisParametre.SgkTabanBrut;
                parametre.IssizlikIsciOrani = guncellenmisParametre.IssizlikIsciOrani;
                parametre.IssizlikIsverenOrani = guncellenmisParametre.IssizlikIsverenOrani;
                parametre.DamgaVergisiOrani = guncellenmisParametre.DamgaVergisiOrani;
                parametre.VergiDilim1UstSinir = guncellenmisParametre.VergiDilim1UstSinir;
                parametre.VergiDilim1Oran = guncellenmisParametre.VergiDilim1Oran;
                parametre.VergiDilim2UstSinir = guncellenmisParametre.VergiDilim2UstSinir;
                parametre.VergiDilim2Oran = guncellenmisParametre.VergiDilim2Oran;
                parametre.VergiDilim3UstSinir = guncellenmisParametre.VergiDilim3UstSinir;
                parametre.VergiDilim3Oran = guncellenmisParametre.VergiDilim3Oran;
                parametre.VergiDilim4UstSinir = guncellenmisParametre.VergiDilim4UstSinir;
                parametre.VergiDilim4Oran = guncellenmisParametre.VergiDilim4Oran;
                parametre.VergiDilim5Oran = guncellenmisParametre.VergiDilim5Oran;
                parametre.AgiBekarOran = guncellenmisParametre.AgiBekarOran;
                parametre.AgiEvliOran = guncellenmisParametre.AgiEvliOran;
                parametre.AgiCocuk1Oran = guncellenmisParametre.AgiCocuk1Oran;
                parametre.AgiCocuk2Oran = guncellenmisParametre.AgiCocuk2Oran;
                parametre.AgiCocuk3Oran = guncellenmisParametre.AgiCocuk3Oran;
                parametre.KidemTavan = guncellenmisParametre.KidemTavan;
                parametre.Aciklama = guncellenmisParametre.Aciklama;
                parametre.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, data = parametre, message = "Parametre başarıyla güncellendi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Parametreyi pasifleştir
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var parametre = await _context.BordroParametreleri.FindAsync(id);
                if (parametre == null)
                    return NotFound(new { success = false, message = "Parametre bulunamadı" });

                parametre.Aktif = false;
                parametre.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Parametre pasifleştirildi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// En güncel parametreyi getir
        /// </summary>
        [HttpGet("guncel")]
        public async Task<IActionResult> GetGuncel()
        {
            try
            {
                var parametre = await _context.BordroParametreleri
                    .Where(p => p.Aktif)
                    .OrderByDescending(p => p.Yil)
                    .ThenByDescending(p => p.Donem)
                    .FirstOrDefaultAsync();

                if (parametre == null)
                    return NotFound(new { success = false, message = "Aktif parametre bulunamadı" });

                return Ok(new { success = true, data = parametre });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
