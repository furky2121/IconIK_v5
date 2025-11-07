using IconIK.API.Data;
using IconIK.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace IconIK.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EPostaYonlendirmeController : ControllerBase
    {
        private readonly IconIKContext _context;
        private readonly ILogger<EPostaYonlendirmeController> _logger;

        public EPostaYonlendirmeController(IconIKContext context, ILogger<EPostaYonlendirmeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/EPostaYonlendirme
        [HttpGet]
        public async Task<ActionResult> GetEPostaYonlendirme()
        {
            try
            {
                var yonlendirmeler = await _context.EPostaYonlendirme
                    .OrderByDescending(y => y.CreatedAt)
                    .ToListAsync();

                return Ok(new { success = true, data = yonlendirmeler });
            }
            catch (Exception ex)
            {
                _logger.LogError($"E-posta yönlendirme listesi getirme hatası: {ex.Message}");
                return StatusCode(500, new { success = false, message = "E-posta yönlendirme listesi alınırken hata oluştu" });
            }
        }

        // GET: api/EPostaYonlendirme/5
        [HttpGet("{id}")]
        public async Task<ActionResult> GetEPostaYonlendirme(int id)
        {
            try
            {
                var yonlendirme = await _context.EPostaYonlendirme.FindAsync(id);

                if (yonlendirme == null)
                {
                    return NotFound(new { success = false, message = "E-posta yönlendirme kaydı bulunamadı" });
                }

                return Ok(new { success = true, data = yonlendirme });
            }
            catch (Exception ex)
            {
                _logger.LogError($"E-posta yönlendirme getirme hatası: {ex.Message}");
                return StatusCode(500, new { success = false, message = "E-posta yönlendirme kaydı alınırken hata oluştu" });
            }
        }

        // POST: api/EPostaYonlendirme
        [HttpPost]
        public async Task<ActionResult> PostEPostaYonlendirme([FromBody] JsonElement jsonData)
        {
            try
            {
                var yonlendirme = new EPostaYonlendirme
                {
                    YonlendirmeTuru = jsonData.GetProperty("yonlendirmeTuru").GetString() ?? "MulakatPlanlama",
                    AliciEmail = jsonData.GetProperty("aliciEmail").GetString() ?? "",
                    Aktif = jsonData.TryGetProperty("aktif", out var aktifElement) ? aktifElement.GetBoolean() : true,
                    Aciklama = jsonData.TryGetProperty("aciklama", out var aciklamaElement) ? aciklamaElement.GetString() : null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // GonderimSaati parsing (HH:mm formatında gelecek)
                if (jsonData.TryGetProperty("gonderimSaati", out var saatElement))
                {
                    var saatStr = saatElement.GetString();
                    if (!string.IsNullOrEmpty(saatStr))
                    {
                        // "09:00" formatından TimeSpan'e dönüştür
                        if (TimeSpan.TryParse(saatStr, out var timeSpan))
                        {
                            yonlendirme.GonderimSaati = timeSpan;
                        }
                        else
                        {
                            return BadRequest(new { success = false, message = "Geçersiz saat formatı. HH:mm formatında olmalıdır (örn: 09:00)" });
                        }
                    }
                }

                // Validasyon
                if (string.IsNullOrEmpty(yonlendirme.AliciEmail))
                {
                    return BadRequest(new { success = false, message = "Alıcı e-posta adresi gereklidir" });
                }

                _context.EPostaYonlendirme.Add(yonlendirme);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetEPostaYonlendirme), new { id = yonlendirme.Id }, new
                {
                    success = true,
                    data = yonlendirme,
                    message = "E-posta yönlendirme başarıyla kaydedildi"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"E-posta yönlendirme kaydetme hatası: {ex.Message}");
                return StatusCode(500, new { success = false, message = "E-posta yönlendirme kaydedilirken hata oluştu" });
            }
        }

        // PUT: api/EPostaYonlendirme/5
        [HttpPut("{id}")]
        public async Task<ActionResult> PutEPostaYonlendirme(int id, [FromBody] JsonElement jsonData)
        {
            try
            {
                var yonlendirme = await _context.EPostaYonlendirme.FindAsync(id);
                if (yonlendirme == null)
                {
                    return NotFound(new { success = false, message = "E-posta yönlendirme kaydı bulunamadı" });
                }

                if (jsonData.TryGetProperty("yonlendirmeTuru", out var turElement))
                    yonlendirme.YonlendirmeTuru = turElement.GetString() ?? yonlendirme.YonlendirmeTuru;

                if (jsonData.TryGetProperty("aliciEmail", out var emailElement))
                    yonlendirme.AliciEmail = emailElement.GetString() ?? yonlendirme.AliciEmail;

                if (jsonData.TryGetProperty("aktif", out var aktifElement))
                    yonlendirme.Aktif = aktifElement.GetBoolean();

                if (jsonData.TryGetProperty("aciklama", out var aciklamaElement))
                    yonlendirme.Aciklama = aciklamaElement.GetString();

                // GonderimSaati güncelleme
                if (jsonData.TryGetProperty("gonderimSaati", out var saatElement))
                {
                    var saatStr = saatElement.GetString();
                    if (!string.IsNullOrEmpty(saatStr))
                    {
                        if (TimeSpan.TryParse(saatStr, out var timeSpan))
                        {
                            // Eğer gönderim saati değişiyorsa, son gönderim tarihini sıfırla
                            if (yonlendirme.GonderimSaati != timeSpan)
                            {
                                yonlendirme.SonGonderimTarihi = null;
                                _logger.LogInformation($"Gönderim saati değişti ({yonlendirme.GonderimSaati} -> {timeSpan}), son gönderim tarihi sıfırlandı");
                            }
                            yonlendirme.GonderimSaati = timeSpan;
                        }
                        else
                        {
                            return BadRequest(new { success = false, message = "Geçersiz saat formatı. HH:mm formatında olmalıdır (örn: 09:00)" });
                        }
                    }
                }

                yonlendirme.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, data = yonlendirme, message = "E-posta yönlendirme başarıyla güncellendi" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"E-posta yönlendirme güncelleme hatası: {ex.Message}");
                return StatusCode(500, new { success = false, message = "E-posta yönlendirme güncellenirken hata oluştu" });
            }
        }

        // DELETE: api/EPostaYonlendirme/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteEPostaYonlendirme(int id)
        {
            try
            {
                var yonlendirme = await _context.EPostaYonlendirme.FindAsync(id);
                if (yonlendirme == null)
                {
                    return NotFound(new { success = false, message = "E-posta yönlendirme kaydı bulunamadı" });
                }

                _context.EPostaYonlendirme.Remove(yonlendirme);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "E-posta yönlendirme başarıyla silindi" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"E-posta yönlendirme silme hatası: {ex.Message}");
                return StatusCode(500, new { success = false, message = "E-posta yönlendirme silinirken hata oluştu" });
            }
        }

        // GET: api/EPostaYonlendirme/turler
        [HttpGet("turler")]
        public ActionResult GetYonlendirmeTurleri()
        {
            try
            {
                var turler = new[]
                {
                    new { value = "MulakatPlanlama", label = "Mülakat Planlama" },
                    // Gelecekte eklenebilecek türler:
                    // new { value = "BordroHatirlat", label = "Bordro Hatırlatma" },
                    // new { value = "IzinOnay", label = "İzin Onay Bildirimi" },
                    // new { value = "EgitimHatirlat", label = "Eğitim Hatırlatma" }
                };

                return Ok(new { success = true, data = turler });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Yönlendirme türleri getirme hatası: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Yönlendirme türleri alınırken hata oluştu" });
            }
        }
    }
}
