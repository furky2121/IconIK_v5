using Microsoft.AspNetCore.Mvc;

namespace IconIK.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileUploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };

        public FileUploadController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpPost("avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { success = false, message = "Dosya seçilmedi." });
                }

                // Dosya boyutu kontrolü
                if (file.Length > _maxFileSize)
                {
                    return BadRequest(new { success = false, message = "Dosya boyutu 5MB'dan büyük olamaz." });
                }

                // Dosya uzantısı kontrolü
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!_allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(new { success = false, message = "Sadece resim dosyaları (jpg, jpeg, png, gif, bmp) yüklenebilir." });
                }

                // Upload klasörünü oluştur
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "avatars");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                // Benzersiz dosya adı oluştur
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Dosyayı kaydet
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Dosya URL'sini döndür
                var fileUrl = $"/uploads/avatars/{fileName}";

                return Ok(new 
                { 
                    success = true, 
                    data = new { url = fileUrl, fileName = fileName }, 
                    message = "Fotoğraf başarıyla yüklendi." 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Fotoğraf yüklenirken bir hata oluştu.", error = ex.Message });
            }
        }

        [HttpDelete("avatar/{fileName}")]
        public IActionResult DeleteAvatar(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    return BadRequest(new { success = false, message = "Dosya adı gereklidir." });
                }

                var filePath = Path.Combine(_environment.WebRootPath, "uploads", "avatars", fileName);
                
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    return Ok(new { success = true, message = "Fotoğraf başarıyla silindi." });
                }
                else
                {
                    return NotFound(new { success = false, message = "Dosya bulunamadı." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Fotoğraf silinirken bir hata oluştu.", error = ex.Message });
            }
        }

        [HttpGet("avatar/{fileName}")]
        public IActionResult GetAvatar(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    return BadRequest(new { success = false, message = "Dosya adı gereklidir." });
                }

                var filePath = Path.Combine(_environment.WebRootPath, "uploads", "avatars", fileName);
                
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound(new { success = false, message = "Dosya bulunamadı." });
                }

                var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
                var contentType = fileExtension switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".gif" => "image/gif",
                    ".bmp" => "image/bmp",
                    _ => "application/octet-stream"
                };

                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, contentType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Fotoğraf getirilirken bir hata oluştu.", error = ex.Message });
            }
        }
    }
}