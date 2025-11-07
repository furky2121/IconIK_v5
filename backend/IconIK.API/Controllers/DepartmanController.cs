using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IconIK.API.Data;
using IconIK.API.Models;

namespace IconIK.API.Controllers
{
    [Route("api/departman")]
    [ApiController]
    public class DepartmanController : ControllerBase
    {
        private readonly IconIKContext _context;

        public DepartmanController(IconIKContext context)
        {
            _context = context;
        }

        // GET: api/Departman
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Departman>>> GetDepartmanlar()
        {
            try
            {
                // Silinmiş kayıtlar gösterilmez, aktif ve pasif kayıtlar gösterilir
                var departmanlar = await _context.Departmanlar
                    .OrderBy(d => d.Ad)
                    .ToListAsync();
                
                return Ok(new { success = true, data = departmanlar, message = "Departmanlar başarıyla listelendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Departmanlar listelenirken bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/Departman/Aktif - Dropdown'lar için sadece aktif departmanlar
        [HttpGet("Aktif")]
        public async Task<ActionResult<IEnumerable<Departman>>> GetAktifDepartmanlar()
        {
            try
            {
                var departmanlar = await _context.Departmanlar
                    .Where(d => d.Aktif)
                    .OrderBy(d => d.Ad)
                    .ToListAsync();
                
                return Ok(new { success = true, data = departmanlar, message = "Aktif departmanlar başarıyla listelendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Aktif departmanlar listelenirken bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/Departman/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Departman>> GetDepartman(int id)
        {
            try
            {
                var departman = await _context.Departmanlar
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (departman == null)
                {
                    return NotFound(new { success = false, message = "Departman bulunamadı." });
                }

                return Ok(new { success = true, data = departman, message = "Departman başarıyla getirildi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Departman getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        // PUT: api/Departman/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDepartman(int id, [FromBody] Departman departman)
        {
            if (id != departman.Id)
            {
                return BadRequest(new { success = false, message = "Geçersiz departman ID." });
            }

            try
            {
                // Aynı isimde başka departman var mı kontrol et
                var existingDepartman = await _context.Departmanlar
                    .FirstOrDefaultAsync(d => d.Ad.ToLower() == departman.Ad.ToLower() && d.Id != id);

                if (existingDepartman != null)
                {
                    return BadRequest(new { success = false, message = "Bu isimde bir departman zaten mevcut." });
                }

                // Kod kontrolü 
                if (!string.IsNullOrEmpty(departman.Kod))
                {
                    var existingKod = await _context.Departmanlar
                        .FirstOrDefaultAsync(d => d.Kod!.ToLower() == departman.Kod.ToLower() && d.Id != id);

                    if (existingKod != null)
                    {
                        return BadRequest(new { success = false, message = "Bu kodda bir departman zaten mevcut." });
                    }
                }

                _context.Entry(departman).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Departman başarıyla güncellendi." });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DepartmanExists(id))
                {
                    return NotFound(new { success = false, message = "Departman bulunamadı." });
                }
                throw;
            }
            catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException pgEx)
            {
                if (pgEx.SqlState == "23505") // Unique constraint violation
                {
                    if (pgEx.ConstraintName == "IX_departmanlar_kod")
                    {
                        return BadRequest(new { success = false, message = "Bu kodda bir departman zaten mevcut." });
                    }
                    else if (pgEx.ConstraintName?.Contains("ad") == true)
                    {
                        return BadRequest(new { success = false, message = "Bu isimde bir departman zaten mevcut." });
                    }
                    return BadRequest(new { success = false, message = "Departman bilgileri başka bir kayıtla çakışıyor." });
                }
                return StatusCode(500, new { success = false, message = "Departman güncellenirken bir veritabanı hatası oluştu.", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Departman güncellenirken bir hata oluştu.", error = ex.Message });
            }
        }

        // POST: api/Departman
        [HttpPost]
        public async Task<ActionResult<Departman>> PostDepartman([FromBody] Departman request)
        {
            Console.WriteLine($"POST /api/Departman - Received data: {System.Text.Json.JsonSerializer.Serialize(request)}");
            
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage)).ToList();
                Console.WriteLine($"ModelState Invalid: {string.Join(", ", errors)}");
                return BadRequest(new { success = false, message = "Validation hatası", errors = errors });
            }

            try
            {
                // Aynı isimde departman var mı kontrol et
                var existingDepartman = await _context.Departmanlar
                    .FirstOrDefaultAsync(d => d.Ad.ToLower() == request.Ad.ToLower());

                if (existingDepartman != null)
                {
                    return BadRequest(new { success = false, message = "Bu isimde bir departman zaten mevcut." });
                }

                // Kod kontrolü  
                if (!string.IsNullOrEmpty(request.Kod))
                {
                    var existingKod = await _context.Departmanlar
                        .FirstOrDefaultAsync(d => d.Kod!.ToLower() == request.Kod.ToLower());

                    if (existingKod != null)
                    {
                        return BadRequest(new { success = false, message = "Bu kodda bir departman zaten mevcut." });
                    }
                }

                request.CreatedAt = DateTime.UtcNow;
                request.UpdatedAt = DateTime.UtcNow;
                
                _context.Departmanlar.Add(request);
                await _context.SaveChangesAsync();

                Console.WriteLine($"POST /api/Departman - Successfully created departman with ID: {request.Id}");
                
                return Ok(new { success = true, data = request, message = "Departman başarıyla oluşturuldu." });
            }
            catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException pgEx)
            {
                if (pgEx.SqlState == "23505") // Unique constraint violation
                {
                    if (pgEx.ConstraintName == "IX_departmanlar_kod")
                    {
                        return BadRequest(new { success = false, message = "Bu kodda bir departman zaten mevcut." });
                    }
                    else if (pgEx.ConstraintName?.Contains("ad") == true)
                    {
                        return BadRequest(new { success = false, message = "Bu isimde bir departman zaten mevcut." });
                    }
                    return BadRequest(new { success = false, message = "Departman bilgileri başka bir kayıtla çakışıyor." });
                }
                return StatusCode(500, new { success = false, message = "Departman oluşturulurken bir veritabanı hatası oluştu.", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Departman oluşturulurken bir hata oluştu.", error = ex.Message });
            }
        }

        // DELETE: api/Departman/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartman(int id)
        {
            Console.WriteLine($"DELETE /api/Departman/{id} - Delete request received");
            try
            {
                var departman = await _context.Departmanlar.FindAsync(id);
                if (departman == null)
                {
                    return NotFound(new { success = false, message = "Departman bulunamadı." });
                }

                // Bu departmana bağlı pozisyon var mı kontrol et (aktif ve pasif tüm pozisyonlar)
                var pozisyonSayisi = await _context.Pozisyonlar.CountAsync(p => p.DepartmanId == id);
                if (pozisyonSayisi > 0)
                {
                    return BadRequest(new { success = false, message = $"Bu departmana bağlı {pozisyonSayisi} adet pozisyon bulunmaktadır. Önce pozisyonları siliniz." });
                }

                // Bu departmana bağlı personel var mı kontrol et
                var personelSayisi = await _context.Personeller
                    .Join(_context.Pozisyonlar, p => p.PozisyonId, pos => pos.Id, (p, pos) => new { p, pos })
                    .CountAsync(x => x.pos.DepartmanId == id);
                if (personelSayisi > 0)
                {
                    return BadRequest(new { success = false, message = $"Bu departmana bağlı {personelSayisi} adet personel bulunmaktadır. Önce personelleri siliniz." });
                }

                // Hard delete
                _context.Departmanlar.Remove(departman);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Departman başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Departman silinirken bir hata oluştu.", error = ex.Message });
            }
        }

        private bool DepartmanExists(int id)
        {
            return _context.Departmanlar.Any(e => e.Id == id);
        }
    }
}