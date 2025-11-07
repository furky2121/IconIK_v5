using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using IconIK.API.Services;
using IconIK.API.Models;
using System.Text.Json;

namespace IconIK.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BildirimController : ControllerBase
    {
        private readonly IBildirimService _bildirimService;

        public BildirimController(IBildirimService bildirimService)
        {
            _bildirimService = bildirimService;
        }

        // GET: api/bildirim/personel/{personelId}
        [HttpGet("personel/{personelId}")]
        public async Task<IActionResult> GetBildirimlerByPersonel(int personelId)
        {
            try
            {
                var bildirimler = await _bildirimService.GetBildirimlerByPersonelAsync(personelId);
                return Ok(new { success = true, data = bildirimler, message = "Bildirimler başarıyla getirildi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Bildirimler getirilirken hata oluştu: {ex.Message}" });
            }
        }

        // GET: api/bildirim/personel/{personelId}/okunmamis
        [HttpGet("personel/{personelId}/okunmamis")]
        public async Task<IActionResult> GetOkunmamisBildirimler(int personelId)
        {
            try
            {
                var bildirimler = await _bildirimService.GetOkunmamisBildirimlerAsync(personelId);
                return Ok(new { success = true, data = bildirimler, message = "Okunmamış bildirimler başarıyla getirildi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Okunmamış bildirimler getirilirken hata oluştu: {ex.Message}" });
            }
        }

        // GET: api/bildirim/personel/{personelId}/okunmamis-sayisi
        [HttpGet("personel/{personelId}/okunmamis-sayisi")]
        public async Task<IActionResult> GetOkunmamisSayisi(int personelId)
        {
            try
            {
                var sayi = await _bildirimService.GetOkunmamisSayisiAsync(personelId);
                return Ok(new { success = true, data = sayi, message = "Okunmamış bildirim sayısı başarıyla getirildi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Okunmamış bildirim sayısı getirilirken hata oluştu: {ex.Message}" });
            }
        }

        // POST: api/bildirim
        [HttpPost]
        public async Task<IActionResult> CreateBildirim([FromBody] JsonElement body)
        {
            try
            {
                var bildirim = new Bildirim
                {
                    AliciId = body.GetProperty("aliciId").GetInt32(),
                    Baslik = body.GetProperty("baslik").GetString(),
                    Mesaj = body.GetProperty("mesaj").GetString(),
                    Kategori = body.TryGetProperty("kategori", out var kategori) ? kategori.GetString() : "sistem",
                    Tip = body.TryGetProperty("tip", out var tip) ? tip.GetString() : "info",
                    GonderenAd = body.TryGetProperty("gonderenAd", out var gonderenAd) ? gonderenAd.GetString() : null,
                    ActionUrl = body.TryGetProperty("actionUrl", out var actionUrl) ? actionUrl.GetString() : null
                };

                var createdBildirim = await _bildirimService.CreateBildirimAsync(bildirim);
                return Ok(new { success = true, data = createdBildirim, message = "Bildirim başarıyla oluşturuldu" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Bildirim oluşturulurken hata oluştu: {ex.Message}" });
            }
        }

        // PUT: api/bildirim/{id}/read
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var result = await _bildirimService.MarkAsReadAsync(id);
                if (!result)
                {
                    return NotFound(new { success = false, message = "Bildirim bulunamadı" });
                }

                return Ok(new { success = true, data = result, message = "Bildirim okundu olarak işaretlendi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Bildirim güncellenirken hata oluştu: {ex.Message}" });
            }
        }

        // POST: api/bildirim/personel/{personelId}/read-all
        [HttpPost("personel/{personelId}/read-all")]
        public async Task<IActionResult> MarkAllAsRead(int personelId)
        {
            try
            {
                var result = await _bildirimService.MarkAllAsReadAsync(personelId);
                return Ok(new { success = true, data = result, message = "Tüm bildirimler okundu olarak işaretlendi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Bildirimler güncellenirken hata oluştu: {ex.Message}" });
            }
        }

        // DELETE: api/bildirim/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBildirim(int id)
        {
            try
            {
                var result = await _bildirimService.DeleteBildirimAsync(id);
                if (!result)
                {
                    return NotFound(new { success = false, message = "Bildirim bulunamadı" });
                }

                return Ok(new { success = true, data = result, message = "Bildirim başarıyla silindi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Bildirim silinirken hata oluştu: {ex.Message}" });
            }
        }
    }
}
