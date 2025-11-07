using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using IconIK.API.Services;
using System.Security.Claims;
using System.Text.Json;

namespace IconIK.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LucaBordroController : ControllerBase
    {
        private readonly ILucaBordroService _lucaBordroService;
        private readonly IOtpService _otpService;
        private readonly ILogger<LucaBordroController> _logger;

        public LucaBordroController(
            ILucaBordroService lucaBordroService,
            IOtpService otpService,
            ILogger<LucaBordroController> logger)
        {
            _lucaBordroService = lucaBordroService;
            _otpService = otpService;
            _logger = logger;
        }

        /// <summary>
        /// Kullanıcının kendi bordrolarını getir (TC kimlik ile filtreleme)
        /// </summary>
        [HttpGet("benim-bordrolarim")]
        public async Task<IActionResult> GetBenimBordrolarim()
        {
            try
            {
                // JWT token'dan TC kimlik bilgisini al
                var tcKimlikClaim = User.Claims.FirstOrDefault(c => c.Type == "TcKimlik");
                if (tcKimlikClaim == null)
                    return BadRequest(new { success = false, message = "TC kimlik bilgisi bulunamadı" });

                var tcKimlik = tcKimlikClaim.Value;
                var bordrolar = await _lucaBordroService.GetBenimBordrolarimAsync(tcKimlik);

                return Ok(new { success = true, data = bordrolar });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bordro listeleme hatası");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Bordro detayını getir
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var bordro = await _lucaBordroService.GetByIdAsync(id);
                if (bordro == null)
                    return NotFound(new { success = false, message = "Bordro bulunamadı" });

                // Güvenlik kontrolü - sadece kendi bordrosunu görebilir
                var tcKimlikClaim = User.Claims.FirstOrDefault(c => c.Type == "TcKimlik");
                if (tcKimlikClaim != null && bordro.TcKimlik != tcKimlikClaim.Value)
                {
                    return Forbid();
                }

                return Ok(new { success = true, data = bordro });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Bordro getirme hatası: {id}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Luca'dan bordro senkronizasyonu (sadece yöneticiler)
        /// </summary>
        [HttpPost("senkronize")]
        public async Task<IActionResult> Senkronize()
        {
            try
            {
                var (success, message, count) = await _lucaBordroService.SenkronizeEtAsync();

                if (success)
                    return Ok(new { success = true, message, count });
                else
                    return BadRequest(new { success = false, message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Luca senkronizasyon hatası");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Excel/CSV dosyasından bordro yükleme (sadece yöneticiler)
        /// </summary>
        [HttpPost("dosya-yukle")]
        public async Task<IActionResult> DosyaYukle([FromForm] IFormFile file)
        {
            try
            {
                if (file == null)
                    return BadRequest(new { success = false, message = "Dosya seçilmedi" });

                var (success, message, count) = await _lucaBordroService.DosyadanYukleAsync(file);

                if (success)
                    return Ok(new { success = true, message, count });
                else
                    return BadRequest(new { success = false, message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dosya yükleme hatası");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Mail'e göndermek için OTP kodu gönder
        /// </summary>
        [HttpPost("{id}/otp-gonder")]
        public async Task<IActionResult> OtpGonder(int id)
        {
            try
            {
                var bordro = await _lucaBordroService.GetByIdAsync(id);
                if (bordro == null)
                    return NotFound(new { success = false, message = "Bordro bulunamadı" });

                // Güvenlik kontrolü
                var tcKimlikClaim = User.Claims.FirstOrDefault(c => c.Type == "TcKimlik");
                if (tcKimlikClaim == null || bordro.TcKimlik != tcKimlikClaim.Value)
                {
                    return Forbid();
                }

                // Kullanıcı bilgilerini al
                var kullaniciIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(kullaniciIdStr, out int kullaniciId))
                    return BadRequest(new { success = false, message = "Kullanıcı bilgisi bulunamadı" });

                var emailClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
                if (emailClaim == null)
                    return BadRequest(new { success = false, message = "E-posta adresi bulunamadı" });

                // OTP oluştur ve gönder
                var (success, otpKodu, message) = await _otpService.OlusturVeGonderAsync(kullaniciId, emailClaim.Value, id);

                return Ok(new { success, message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"OTP gönderme hatası: {id}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// OTP doğrula ve bordroyu mail'e gönder
        /// </summary>
        [HttpPost("{id}/otp-dogrula-ve-gonder")]
        public async Task<IActionResult> OtpDogrulaVeGonder(int id, [FromBody] JsonElement data)
        {
            try
            {
                var otpKodu = data.GetProperty("otpKodu").GetString();
                if (string.IsNullOrEmpty(otpKodu))
                    return BadRequest(new { success = false, message = "OTP kodu girilmedi" });

                var kullaniciIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(kullaniciIdStr, out int kullaniciId))
                    return BadRequest(new { success = false, message = "Kullanıcı bilgisi bulunamadı" });

                // OTP doğrula
                var (otpSuccess, otpMessage) = await _otpService.DogrulaAsync(kullaniciId, otpKodu);
                if (!otpSuccess)
                    return BadRequest(new { success = false, message = otpMessage });

                // Bordroyu mail'e gönder
                var (mailSuccess, mailMessage) = await _lucaBordroService.MaileGonderAsync(id, kullaniciId);

                if (mailSuccess)
                    return Ok(new { success = true, message = "Bordronuz e-posta adresinize gönderildi" });
                else
                    return BadRequest(new { success = false, message = mailMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"OTP doğrulama ve mail gönderme hatası: {id}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
