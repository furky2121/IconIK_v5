using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IconIK.API.Data;
using IconIK.API.Models;
using System.Text.Json;

namespace IconIK.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FirmaAyarlariController : ControllerBase
    {
        private readonly IconIKContext _context;

        public FirmaAyarlariController(IconIKContext context)
        {
            _context = context;
        }

        // GET: api/FirmaAyarlari
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var firmaAyarlari = await _context.FirmaAyarlari.FirstOrDefaultAsync();

                if (firmaAyarlari == null)
                {
                    // İlk kez ayarlar oluşturuluyor
                    firmaAyarlari = new FirmaAyarlari
                    {
                        FirmaAdi = "Bilge Lojistik",
                        LogoUrl = null,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.FirmaAyarlari.Add(firmaAyarlari);
                    await _context.SaveChangesAsync();
                }

                return Ok(new { success = true, data = firmaAyarlari, message = "Firma ayarları getirildi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata: {ex.Message}" });
            }
        }

        // PUT: api/FirmaAyarlari
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] JsonElement jsonElement)
        {
            try
            {
                var firmaAyarlari = await _context.FirmaAyarlari.FirstOrDefaultAsync();

                if (firmaAyarlari == null)
                {
                    firmaAyarlari = new FirmaAyarlari();
                    _context.FirmaAyarlari.Add(firmaAyarlari);
                }

                // Parse JSON
                if (jsonElement.TryGetProperty("firmaAdi", out var firmaAdiProp))
                    firmaAyarlari.FirmaAdi = firmaAdiProp.GetString() ?? string.Empty;

                if (jsonElement.TryGetProperty("logoUrl", out var logoUrlProp))
                    firmaAyarlari.LogoUrl = logoUrlProp.GetString();

                firmaAyarlari.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, data = firmaAyarlari, message = "Firma ayarları güncellendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata: {ex.Message}" });
            }
        }

        // POST: api/FirmaAyarlari/UploadLogo
        [HttpPost("UploadLogo")]
        public async Task<IActionResult> UploadLogo(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { success = false, message = "Dosya seçilmedi." });

                // File validation
                var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".svg" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                    return BadRequest(new { success = false, message = "Geçersiz dosya formatı. Sadece PNG, JPG, JPEG ve SVG dosyaları kabul edilir." });

                if (file.Length > 5 * 1024 * 1024) // 5MB
                    return BadRequest(new { success = false, message = "Dosya boyutu 5MB'dan küçük olmalıdır." });

                // Create upload directory
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "firma-logo");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Get firma ayarlari
                var firmaAyarlari = await _context.FirmaAyarlari.FirstOrDefaultAsync();

                if (firmaAyarlari == null)
                {
                    firmaAyarlari = new FirmaAyarlari
                    {
                        FirmaAdi = "Bilge Lojistik",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.FirmaAyarlari.Add(firmaAyarlari);
                }

                // Delete old logo if exists
                if (!string.IsNullOrEmpty(firmaAyarlari.LogoUrl))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", firmaAyarlari.LogoUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                        System.IO.File.Delete(oldFilePath);
                }

                // Generate unique filename
                var fileName = $"logo{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                    await file.CopyToAsync(stream);

                // Update database
                firmaAyarlari.LogoUrl = $"/uploads/firma-logo/{fileName}";
                firmaAyarlari.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new {
                    success = true,
                    data = new { logoUrl = firmaAyarlari.LogoUrl },
                    message = "Logo başarıyla yüklendi."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata: {ex.Message}" });
            }
        }

        // DELETE: api/FirmaAyarlari/DeleteLogo
        [HttpDelete("DeleteLogo")]
        public async Task<IActionResult> DeleteLogo()
        {
            try
            {
                var firmaAyarlari = await _context.FirmaAyarlari.FirstOrDefaultAsync();

                if (firmaAyarlari == null || string.IsNullOrEmpty(firmaAyarlari.LogoUrl))
                    return NotFound(new { success = false, message = "Logo bulunamadı." });

                // Delete file
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", firmaAyarlari.LogoUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);

                // Update database
                firmaAyarlari.LogoUrl = null;
                firmaAyarlari.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Logo silindi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata: {ex.Message}" });
            }
        }
    }
}
