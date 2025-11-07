using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IconIK.API.Data;
using IconIK.API.Models;
using System.Text.Json;

namespace IconIK.API.Controllers
{
    [Route("api/pozisyon")]
    [ApiController]  
    public class PozisyonController : ControllerBase
    {
        private readonly IconIKContext _context;

        public PozisyonController(IconIKContext context)
        {
            _context = context;
        }

        // GET: api/Pozisyon
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetPozisyonlar()
        {
            try
            {
                var pozisyonlar = await _context.Pozisyonlar
                    .Include(p => p.Departman)
                    .Include(p => p.Kademe)
                    .OrderBy(p => p.Departman.Ad)
                    .ThenBy(p => p.Kademe.Seviye)
                    .ThenBy(p => p.Ad)
                    .Select(p => new
                    {
                        p.Id,
                        p.Ad,
                        p.DepartmanId,
                        DepartmanAd = p.Departman.Ad,
                        p.KademeId,
                        KademeAd = p.Kademe.Ad,
                        p.MinMaas,
                        p.MaxMaas,
                        p.Aciklama,
                        p.Aktif,
                        p.CreatedAt,
                        p.UpdatedAt
                    })
                    .ToListAsync();
                
                return Ok(new { success = true, data = pozisyonlar, message = "Pozisyonlar başarıyla listelendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Pozisyonlar listelenirken bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/Pozisyon/Aktif - Dropdown'lar için sadece aktif pozisyonlar
        [HttpGet("Aktif")]
        public async Task<ActionResult<IEnumerable<object>>> GetAktifPozisyonlar()
        {
            try
            {
                var pozisyonlar = await _context.Pozisyonlar
                    .Include(p => p.Departman)
                    .Include(p => p.Kademe)
                    .Where(p => p.Aktif)
                    .OrderBy(p => p.Departman.Ad)
                    .ThenBy(p => p.Kademe.Seviye)
                    .ThenBy(p => p.Ad)
                    .Select(p => new
                    {
                        p.Id,
                        p.Ad,
                        p.DepartmanId,
                        DepartmanAd = p.Departman.Ad,
                        p.KademeId,
                        KademeAd = p.Kademe.Ad,
                        KademeSeviye = p.Kademe.Seviye,
                        p.MinMaas,
                        p.MaxMaas,
                        p.Aciklama,
                        p.Aktif,
                        p.CreatedAt,
                        p.UpdatedAt
                    })
                    .ToListAsync();
                
                return Ok(new { success = true, data = pozisyonlar, message = "Aktif pozisyonlar başarıyla listelendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Aktif pozisyonlar listelenirken bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/Pozisyon/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetPozisyon(int id)
        {
            try
            {
                var pozisyon = await _context.Pozisyonlar
                    .Include(p => p.Departman)
                    .Include(p => p.Kademe)
                    .Where(p => p.Id == id)
                    .Select(p => new
                    {
                        p.Id,
                        p.Ad,
                        p.DepartmanId,
                        DepartmanAd = p.Departman.Ad,
                        p.KademeId,
                        KademeAd = p.Kademe.Ad,
                        p.MinMaas,
                        p.MaxMaas,
                        p.Aciklama,
                        p.Aktif,
                        p.CreatedAt,
                        p.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                if (pozisyon == null)
                {
                    return NotFound(new { success = false, message = "Pozisyon bulunamadı." });
                }

                return Ok(new { success = true, data = pozisyon, message = "Pozisyon başarıyla getirildi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Pozisyon getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/Pozisyon/ByDepartman/5
        [HttpGet("ByDepartman/{departmanId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetPozisyonlarByDepartman(int departmanId)
        {
            try
            {
                // Dropdown için sadece aktif pozisyonlar
                var pozisyonlar = await _context.Pozisyonlar
                    .Include(p => p.Kademe)
                    .Where(p => p.DepartmanId == departmanId && p.Aktif)
                    .OrderBy(p => p.Kademe.Seviye)
                    .ThenBy(p => p.Ad)
                    .Select(p => new
                    {
                        p.Id,
                        p.Ad,
                        p.KademeId,
                        KademeAd = p.Kademe.Ad,
                        p.MinMaas,
                        p.MaxMaas
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = pozisyonlar, message = "Pozisyonlar başarıyla listelendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Pozisyonlar listelenirken bir hata oluştu.", error = ex.Message });
            }
        }

        // PUT: api/Pozisyon/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPozisyon(int id, [FromBody] System.Text.Json.JsonElement rawData)
        {
            Console.WriteLine("=== PUT /api/Pozisyon - METHOD CALLED ===");
            Console.WriteLine($"PUT /api/Pozisyon/{id} - Received raw data: {rawData}");
            
            // Manual parsing with camelCase options
            Pozisyon pozisyon;
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                pozisyon = System.Text.Json.JsonSerializer.Deserialize<Pozisyon>(rawData, options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JSON Parse Error: {ex.Message}");
                return BadRequest(new { success = false, message = "Invalid JSON format", error = ex.Message });
            }
            
            Console.WriteLine($"PUT /api/Pozisyon/{id} - Parsed pozisyon: {System.Text.Json.JsonSerializer.Serialize(pozisyon)}");

            if (id != pozisyon.Id)
            {
                return BadRequest(new { success = false, message = "Geçersiz pozisyon ID." });
            }

            try
            {
                // Aynı isim, departman ve kademede başka pozisyon var mı kontrol et
                var existingPozisyon = await _context.Pozisyonlar
                    .FirstOrDefaultAsync(p => p.Ad.ToLower() == pozisyon.Ad.ToLower() 
                                           && p.DepartmanId == pozisyon.DepartmanId 
                                           && p.KademeId == pozisyon.KademeId 
                                           && p.Id != id 
                                           );

                if (existingPozisyon != null)
                {
                    return BadRequest(new { success = false, message = "Bu isim, departman ve kademede bir pozisyon zaten mevcut." });
                }

                // Maaş aralığı kontrolü
                if (pozisyon.MinMaas.HasValue && pozisyon.MaxMaas.HasValue && pozisyon.MinMaas > pozisyon.MaxMaas)
                {
                    return BadRequest(new { success = false, message = "Minimum maaş, maksimum maaştan büyük olamaz." });
                }

                // Departman ve kademe var mı kontrol et
                var departmanExists = await _context.Departmanlar.AnyAsync(d => d.Id == pozisyon.DepartmanId && d.Aktif);
                var kademeExists = await _context.Kademeler.AnyAsync(k => k.Id == pozisyon.KademeId);

                if (!departmanExists)
                {
                    return BadRequest(new { success = false, message = "Geçersiz departman seçimi." });
                }

                if (!kademeExists)
                {
                    return BadRequest(new { success = false, message = "Geçersiz kademe seçimi." });
                }

                // Existing entity'yi al ve güncelle
                var existingEntity = await _context.Pozisyonlar.FindAsync(id);
                if (existingEntity == null)
                {
                    return NotFound(new { success = false, message = "Pozisyon bulunamadı." });
                }

                // Sadece izin verilen alanları güncelle
                existingEntity.Ad = pozisyon.Ad;
                existingEntity.DepartmanId = pozisyon.DepartmanId;
                existingEntity.KademeId = pozisyon.KademeId;
                existingEntity.MinMaas = pozisyon.MinMaas;
                existingEntity.MaxMaas = pozisyon.MaxMaas;
                existingEntity.Aciklama = pozisyon.Aciklama;
                existingEntity.Aktif = pozisyon.Aktif;
                existingEntity.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Pozisyon başarıyla güncellendi." });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PozisyonExists(id))
                {
                    return NotFound(new { success = false, message = "Pozisyon bulunamadı." });
                }
                throw;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Pozisyon güncellenirken bir hata oluştu.", error = ex.Message });
            }
        }

        // POST: api/pozisyon/test
        [HttpPost("test")]
        public IActionResult TestPost()
        {
            Console.WriteLine("=== TEST POST METHOD CALLED ===");
            return Ok(new { success = true, message = "Test successful" });
        }

        // POST: api/Pozisyon
        [HttpPost]
        public async Task<ActionResult<Pozisyon>> PostPozisyon([FromBody] System.Text.Json.JsonElement rawData)
        {
            Console.WriteLine("=== POST /api/Pozisyon - METHOD CALLED ===");
            Console.WriteLine($"POST /api/Pozisyon - Received raw data: {rawData}");
            
            // Manual parsing with camelCase options
            Pozisyon pozisyon;
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                pozisyon = System.Text.Json.JsonSerializer.Deserialize<Pozisyon>(rawData, options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JSON Parse Error: {ex.Message}");
                return BadRequest(new { success = false, message = "Invalid JSON format", error = ex.Message });
            }
            
            Console.WriteLine($"POST /api/Pozisyon - Parsed pozisyon: {System.Text.Json.JsonSerializer.Serialize(pozisyon)}");
            
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage)).ToList();
                Console.WriteLine($"ModelState Invalid: {string.Join(", ", errors)}");
                foreach (var modelState in ModelState)
                {
                    Console.WriteLine($"Property: {modelState.Key}, Errors: {string.Join(", ", modelState.Value.Errors.Select(e => e.ErrorMessage))}");
                }
                return BadRequest(new { success = false, message = "Validation hatası", errors = errors });
            }

            try
            {
                // Aynı isim, departman ve kademede pozisyon var mı kontrol et
                var existingPozisyon = await _context.Pozisyonlar
                    .FirstOrDefaultAsync(p => p.Ad.ToLower() == pozisyon.Ad.ToLower() 
                                           && p.DepartmanId == pozisyon.DepartmanId 
                                           && p.KademeId == pozisyon.KademeId 
                                           );

                if (existingPozisyon != null)
                {
                    return BadRequest(new { success = false, message = "Bu isim, departman ve kademede bir pozisyon zaten mevcut." });
                }

                // Maaş aralığı kontrolü
                if (pozisyon.MinMaas.HasValue && pozisyon.MaxMaas.HasValue && pozisyon.MinMaas > pozisyon.MaxMaas)
                {
                    return BadRequest(new { success = false, message = "Minimum maaş, maksimum maaştan büyük olamaz." });
                }

                // Departman ve kademe var mı kontrol et
                var departmanExists = await _context.Departmanlar.AnyAsync(d => d.Id == pozisyon.DepartmanId && d.Aktif);
                var kademeExists = await _context.Kademeler.AnyAsync(k => k.Id == pozisyon.KademeId);

                if (!departmanExists)
                {
                    return BadRequest(new { success = false, message = "Geçersiz departman seçimi." });
                }

                if (!kademeExists)
                {
                    return BadRequest(new { success = false, message = "Geçersiz kademe seçimi." });
                }

                pozisyon.CreatedAt = DateTime.UtcNow;
                pozisyon.UpdatedAt = DateTime.UtcNow;
                
                _context.Pozisyonlar.Add(pozisyon);
                await _context.SaveChangesAsync();

                // Pozisyonu detayları ile birlikte döndür
                var createdPozisyon = await _context.Pozisyonlar
                    .Include(p => p.Departman)
                    .Include(p => p.Kademe)
                    .Where(p => p.Id == pozisyon.Id)
                    .Select(p => new
                    {
                        p.Id,
                        p.Ad,
                        p.DepartmanId,
                        DepartmanAd = p.Departman.Ad,
                        p.KademeId,
                        KademeAd = p.Kademe.Ad,
                        p.MinMaas,
                        p.MaxMaas,
                        p.Aciklama,
                        p.Aktif,
                        p.CreatedAt,
                        p.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                return Ok(new { success = true, data = createdPozisyon, message = "Pozisyon başarıyla oluşturuldu." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Pozisyon oluşturulurken bir hata oluştu.", error = ex.Message });
            }
        }

        // DELETE: api/Pozisyon/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePozisyon(int id)
        {
            try
            {
                var pozisyon = await _context.Pozisyonlar.FindAsync(id);
                if (pozisyon == null)
                {
                    return NotFound(new { success = false, message = "Pozisyon bulunamadı." });
                }

                // Bu pozisyona bağlı personel var mı kontrol et (aktif ve pasif tüm personeller)
                var personelSayisi = await _context.Personeller.CountAsync(p => p.PozisyonId == id);
                if (personelSayisi > 0)
                {
                    return BadRequest(new { success = false, message = $"Bu pozisyona bağlı {personelSayisi} adet personel bulunmaktadır. Önce personelleri siliniz." });
                }

                // Hard delete
                _context.Pozisyonlar.Remove(pozisyon);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Pozisyon başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Pozisyon silinirken bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/Pozisyon/MaasBilgi/5
        [HttpGet("MaasBilgi/{pozisyonId}")]
        public async Task<ActionResult<object>> GetPozisyonMaasBilgi(int pozisyonId)
        {
            try
            {
                var pozisyon = await _context.Pozisyonlar
                    .Where(p => p.Id == pozisyonId)
                    .Select(p => new
                    {
                        p.Id,
                        p.Ad,
                        p.MinMaas,
                        p.MaxMaas
                    })
                    .FirstOrDefaultAsync();

                if (pozisyon == null)
                {
                    return NotFound(new { success = false, message = "Pozisyon bulunamadı." });
                }

                return Ok(new { success = true, data = pozisyon, message = "Pozisyon maaş bilgisi başarıyla getirildi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Pozisyon maaş bilgisi getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        private bool PozisyonExists(int id)
        {
            return _context.Pozisyonlar.Any(e => e.Id == id);
        }
    }
}