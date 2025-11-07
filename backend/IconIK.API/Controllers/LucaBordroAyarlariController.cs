using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using IconIK.API.Services;
using IconIK.API.Models;
using System.Text.Json;

namespace IconIK.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LucaBordroAyarlariController : ControllerBase
    {
        private readonly ILucaBordroAyarlariService _service;
        private readonly ILogger<LucaBordroAyarlariController> _logger;

        public LucaBordroAyarlariController(
            ILucaBordroAyarlariService service,
            ILogger<LucaBordroAyarlariController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var ayarlar = await _service.GetAllAsync();
                return Ok(new { success = true, data = ayarlar });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Luca ayarları getirme hatası");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var ayar = await _service.GetByIdAsync(id);
                if (ayar == null)
                    return NotFound(new { success = false, message = "Ayar bulunamadı" });

                return Ok(new { success = true, data = ayar });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Luca ayar getirme hatası: {id}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("aktif")]
        public async Task<IActionResult> GetAktif()
        {
            try
            {
                var ayar = await _service.GetAktifAyarAsync();
                return Ok(new { success = true, data = ayar });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Aktif Luca ayar getirme hatası");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] JsonElement data)
        {
            try
            {
                var ayarlar = JsonSerializer.Deserialize<LucaBordroAyarlari>(data.GetRawText(), new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (ayarlar == null)
                    return BadRequest(new { success = false, message = "Geçersiz veri" });

                var yeniAyar = await _service.CreateAsync(ayarlar);
                return Ok(new { success = true, data = yeniAyar, message = "Luca ayarları başarıyla kaydedildi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Luca ayar oluşturma hatası");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] JsonElement data)
        {
            try
            {
                var ayarlar = JsonSerializer.Deserialize<LucaBordroAyarlari>(data.GetRawText(), new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (ayarlar == null)
                    return BadRequest(new { success = false, message = "Geçersiz veri" });

                var guncellenenAyar = await _service.UpdateAsync(id, ayarlar);
                if (guncellenenAyar == null)
                    return NotFound(new { success = false, message = "Ayar bulunamadı" });

                return Ok(new { success = true, data = guncellenenAyar, message = "Luca ayarları başarıyla güncellendi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Luca ayar güncelleme hatası: {id}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var silindi = await _service.DeleteAsync(id);
                if (!silindi)
                    return NotFound(new { success = false, message = "Ayar bulunamadı" });

                return Ok(new { success = true, message = "Luca ayarları başarıyla silindi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Luca ayar silme hatası: {id}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("{id}/test-baglanti")]
        public async Task<IActionResult> TestBaglanti(int id)
        {
            try
            {
                var (success, message) = await _service.TestBaglantiAsync(id);
                return Ok(new { success, message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Luca bağlantı testi hatası: {id}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
