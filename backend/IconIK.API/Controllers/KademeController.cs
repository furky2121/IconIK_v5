using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IconIK.API.Data;
using IconIK.API.Models;

namespace IconIK.API.Controllers
{
    [Route("api/kademe")]
    [ApiController]
    public class KademeController : ControllerBase
    {
        private readonly IconIKContext _context;

        public KademeController(IconIKContext context)
        {
            _context = context;
        }

        // GET: api/Kademe
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Kademe>>> GetKademeler()
        {
            try
            {
                var kademeler = await _context.Kademeler
                    .OrderBy(k => k.Seviye)
                    .ToListAsync();
                
                return Ok(new { success = true, data = kademeler, message = "Kademeler başarıyla listelendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Kademeler listelenirken bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/Kademe/Aktif - Dropdown'lar için sadece aktif kademeler
        [HttpGet("Aktif")]
        public async Task<ActionResult<IEnumerable<Kademe>>> GetAktifKademeler()
        {
            try
            {
                var kademeler = await _context.Kademeler
                    .Where(k => k.Aktif)
                    .OrderBy(k => k.Seviye)
                    .ToListAsync();
                
                return Ok(new { success = true, data = kademeler, message = "Aktif kademeler başarıyla listelendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Aktif kademeler listelenirken bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/Kademe/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Kademe>> GetKademe(int id)
        {
            try
            {
                var kademe = await _context.Kademeler.FindAsync(id);

                if (kademe == null)
                {
                    return NotFound(new { success = false, message = "Kademe bulunamadı." });
                }

                return Ok(new { success = true, data = kademe, message = "Kademe başarıyla getirildi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Kademe getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        // PUT: api/Kademe/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutKademe(int id, [FromBody] Kademe kademe)
        {
            if (id != kademe.Id)
            {
                return BadRequest(new { success = false, message = "Geçersiz kademe ID." });
            }

            try
            {
                // Aynı isimde başka kademe var mı kontrol et
                var existingKademe = await _context.Kademeler
                    .FirstOrDefaultAsync(k => k.Ad.ToLower() == kademe.Ad.ToLower() && k.Id != id);

                if (existingKademe != null)
                {
                    return BadRequest(new { success = false, message = "Bu isimde bir kademe zaten mevcut." });
                }

                // Aynı seviyede başka kademe var mı kontrol et
                var existingSeviye = await _context.Kademeler
                    .FirstOrDefaultAsync(k => k.Seviye == kademe.Seviye && k.Id != id);

                if (existingSeviye != null)
                {
                    return BadRequest(new { success = false, message = "Bu seviyede bir kademe zaten mevcut." });
                }

                _context.Entry(kademe).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Kademe başarıyla güncellendi." });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!KademeExists(id))
                {
                    return NotFound(new { success = false, message = "Kademe bulunamadı." });
                }
                throw;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Kademe güncellenirken bir hata oluştu.", error = ex.Message });
            }
        }

        // POST: api/Kademe
        [HttpPost]
        public async Task<ActionResult<Kademe>> PostKademe([FromBody] Kademe kademe)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage)).ToList();
                return BadRequest(new { success = false, message = "Validation hatası", errors = errors });
            }

            try
            {
                // Aynı isimde kademe var mı kontrol et
                var existingKademe = await _context.Kademeler
                    .FirstOrDefaultAsync(k => k.Ad.ToLower() == kademe.Ad.ToLower());

                if (existingKademe != null)
                {
                    return BadRequest(new { success = false, message = "Bu isimde bir kademe zaten mevcut." });
                }

                // Aynı seviyede kademe var mı kontrol et
                var existingSeviye = await _context.Kademeler
                    .FirstOrDefaultAsync(k => k.Seviye == kademe.Seviye);

                if (existingSeviye != null)
                {
                    return BadRequest(new { success = false, message = "Bu seviyede bir kademe zaten mevcut." });
                }

                kademe.CreatedAt = DateTime.UtcNow;
                kademe.UpdatedAt = DateTime.UtcNow;
                
                _context.Kademeler.Add(kademe);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetKademe", new { id = kademe.Id }, 
                    new { success = true, data = kademe, message = "Kademe başarıyla oluşturuldu." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Kademe oluşturulurken bir hata oluştu.", error = ex.Message });
            }
        }

        // DELETE: api/Kademe/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteKademe(int id)
        {
            try
            {
                var kademe = await _context.Kademeler.FindAsync(id);
                if (kademe == null)
                {
                    return NotFound(new { success = false, message = "Kademe bulunamadı." });
                }

                // Bu kademeye bağlı pozisyon var mı kontrol et (aktif ve pasif tüm pozisyonlar)
                var pozisyonSayisi = await _context.Pozisyonlar.CountAsync(p => p.KademeId == id);
                if (pozisyonSayisi > 0)
                {
                    return BadRequest(new { success = false, message = $"Bu kademeye bağlı {pozisyonSayisi} adet pozisyon bulunmaktadır. Önce pozisyonları siliniz." });
                }

                // Hard delete (çünkü kademe referans tablosu)
                _context.Kademeler.Remove(kademe);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Kademe başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Kademe silinirken bir hata oluştu.", error = ex.Message });
            }
        }

        private bool KademeExists(int id)
        {
            return _context.Kademeler.Any(e => e.Id == id);
        }
    }
}