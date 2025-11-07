using IconIK.API.Data;
using IconIK.API.Models;
using IconIK.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace IconIK.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EPostaAyarlariController : ControllerBase
    {
        private readonly IconIKContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<EPostaAyarlariController> _logger;

        public EPostaAyarlariController(
            IconIKContext context,
            IEmailService emailService,
            ILogger<EPostaAyarlariController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        // GET: api/EPostaAyarlari
        [HttpGet]
        public async Task<ActionResult> GetEPostaAyarlari()
        {
            try
            {
                var ayarlar = await _context.EPostaAyarlari
                    .OrderByDescending(e => e.CreatedAt)
                    .ToListAsync();

                // Şifreleri frontend'e gönderme (güvenlik)
                foreach (var ayar in ayarlar)
                {
                    ayar.SmtpPassword = "********";
                }

                return Ok(new { success = true, data = ayarlar });
            }
            catch (Exception ex)
            {
                _logger.LogError($"E-posta ayarları getirme hatası: {ex.Message}");
                return StatusCode(500, new { success = false, message = "E-posta ayarları alınırken hata oluştu" });
            }
        }

        // GET: api/EPostaAyarlari/5
        [HttpGet("{id}")]
        public async Task<ActionResult> GetEPostaAyarlari(int id)
        {
            try
            {
                var ayarlar = await _context.EPostaAyarlari.FindAsync(id);

                if (ayarlar == null)
                {
                    return NotFound(new { success = false, message = "E-posta ayarları bulunamadı" });
                }

                // Şifreyi frontend'e gönderme
                ayarlar.SmtpPassword = "********";

                return Ok(new { success = true, data = ayarlar });
            }
            catch (Exception ex)
            {
                _logger.LogError($"E-posta ayarı getirme hatası: {ex.Message}");
                return StatusCode(500, new { success = false, message = "E-posta ayarı alınırken hata oluştu" });
            }
        }

        // POST: api/EPostaAyarlari
        [HttpPost]
        public async Task<ActionResult> PostEPostaAyarlari([FromBody] JsonElement jsonData)
        {
            try
            {
                var ayarlar = new EPostaAyarlari
                {
                    SmtpHost = jsonData.GetProperty("smtpHost").GetString() ?? "",
                    SmtpPort = jsonData.GetProperty("smtpPort").GetInt32(),
                    SmtpUsername = jsonData.GetProperty("smtpUsername").GetString() ?? "",
                    EnableSsl = jsonData.GetProperty("enableSsl").GetBoolean(),
                    FromEmail = jsonData.GetProperty("fromEmail").GetString() ?? "",
                    FromName = jsonData.GetProperty("fromName").GetString() ?? "",
                    Aktif = jsonData.TryGetProperty("aktif", out var aktifElement) ? aktifElement.GetBoolean() : true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Şifreyi şifrele
                var plainPassword = jsonData.GetProperty("smtpPassword").GetString() ?? "";
                ayarlar.SmtpPassword = EmailService.EncryptString(plainPassword);

                // Eğer bu aktif olarak işaretlendiyse, diğerlerini pasif yap
                if (ayarlar.Aktif)
                {
                    var mevcutAyarlar = await _context.EPostaAyarlari.Where(e => e.Aktif).ToListAsync();
                    foreach (var mevcut in mevcutAyarlar)
                    {
                        mevcut.Aktif = false;
                        mevcut.UpdatedAt = DateTime.UtcNow;
                    }
                }

                _context.EPostaAyarlari.Add(ayarlar);
                await _context.SaveChangesAsync();

                // Şifreyi frontend'e gönderme
                ayarlar.SmtpPassword = "********";

                return CreatedAtAction(nameof(GetEPostaAyarlari), new { id = ayarlar.Id }, new
                {
                    success = true,
                    data = ayarlar,
                    message = "E-posta ayarları başarıyla kaydedildi"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"E-posta ayarları kaydetme hatası: {ex.Message}");
                _logger.LogError($"Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner Exception: {ex.InnerException.Message}");
                }
                return StatusCode(500, new { success = false, message = $"E-posta ayarları kaydedilirken hata oluştu: {ex.Message}" });
            }
        }

        // PUT: api/EPostaAyarlari/5
        [HttpPut("{id}")]
        public async Task<ActionResult> PutEPostaAyarlari(int id, [FromBody] JsonElement jsonData)
        {
            try
            {
                var ayarlar = await _context.EPostaAyarlari.FindAsync(id);
                if (ayarlar == null)
                {
                    return NotFound(new { success = false, message = "E-posta ayarları bulunamadı" });
                }

                ayarlar.SmtpHost = jsonData.GetProperty("smtpHost").GetString() ?? ayarlar.SmtpHost;
                ayarlar.SmtpPort = jsonData.GetProperty("smtpPort").GetInt32();
                ayarlar.SmtpUsername = jsonData.GetProperty("smtpUsername").GetString() ?? ayarlar.SmtpUsername;
                ayarlar.EnableSsl = jsonData.GetProperty("enableSsl").GetBoolean();
                ayarlar.FromEmail = jsonData.GetProperty("fromEmail").GetString() ?? ayarlar.FromEmail;
                ayarlar.FromName = jsonData.GetProperty("fromName").GetString() ?? ayarlar.FromName;
                ayarlar.Aktif = jsonData.TryGetProperty("aktif", out var aktifElement) ? aktifElement.GetBoolean() : ayarlar.Aktif;
                ayarlar.UpdatedAt = DateTime.UtcNow;

                // Şifre değiştiyse şifrele
                if (jsonData.TryGetProperty("smtpPassword", out var passwordElement))
                {
                    var plainPassword = passwordElement.GetString();
                    if (!string.IsNullOrEmpty(plainPassword) && plainPassword != "********")
                    {
                        ayarlar.SmtpPassword = EmailService.EncryptString(plainPassword);
                    }
                }

                // Eğer bu aktif olarak işaretlendiyse, diğerlerini pasif yap
                if (ayarlar.Aktif)
                {
                    var mevcutAyarlar = await _context.EPostaAyarlari.Where(e => e.Aktif && e.Id != id).ToListAsync();
                    foreach (var mevcut in mevcutAyarlar)
                    {
                        mevcut.Aktif = false;
                        mevcut.UpdatedAt = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync();

                // Şifreyi frontend'e gönderme
                ayarlar.SmtpPassword = "********";

                return Ok(new { success = true, data = ayarlar, message = "E-posta ayarları başarıyla güncellendi" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"E-posta ayarları güncelleme hatası: {ex.Message}");
                return StatusCode(500, new { success = false, message = "E-posta ayarları güncellenirken hata oluştu" });
            }
        }

        // DELETE: api/EPostaAyarlari/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteEPostaAyarlari(int id)
        {
            try
            {
                var ayarlar = await _context.EPostaAyarlari.FindAsync(id);
                if (ayarlar == null)
                {
                    return NotFound(new { success = false, message = "E-posta ayarları bulunamadı" });
                }

                _context.EPostaAyarlari.Remove(ayarlar);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "E-posta ayarları başarıyla silindi" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"E-posta ayarları silme hatası: {ex.Message}");
                return StatusCode(500, new { success = false, message = "E-posta ayarları silinirken hata oluştu" });
            }
        }

        // POST: api/EPostaAyarlari/test
        [HttpPost("test")]
        public async Task<ActionResult> TestEmailConnection()
        {
            try
            {
                var (success, message, details) = await _emailService.TestEmailConnectionAsync();

                if (success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = message
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = message,
                        details = details
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMTP test endpoint hatası");
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Bağlantı testi sırasında beklenmeyen bir hata oluştu: {ex.Message}",
                    details = ex.StackTrace
                });
            }
        }

        // POST: api/EPostaAyarlari/test-mulakat-email
        [HttpPost("test-mulakat-email")]
        public async Task<ActionResult> TestMulakatEmail([FromBody] JsonElement jsonData)
        {
            try
            {
                var recipientEmail = jsonData.GetProperty("recipientEmail").GetString();
                var tarih = jsonData.GetProperty("tarih").GetDateTime();

                if (string.IsNullOrEmpty(recipientEmail))
                {
                    return BadRequest(new { success = false, message = "Alıcı e-posta adresi gerekli" });
                }

                var result = await _emailService.SendMulakatBildirimAsync(tarih, recipientEmail);

                if (result)
                {
                    return Ok(new { success = true, message = "Test e-postası başarıyla gönderildi!" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "E-posta gönderilemedi! Lütfen ayarlarınızı kontrol edin." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Test e-posta gönderme hatası: {ex.Message}");
                return StatusCode(500, new { success = false, message = $"E-posta gönderilirken hata oluştu: {ex.Message}" });
            }
        }
    }
}
