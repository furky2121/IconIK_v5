using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IconIK.API.Data;
using IconIK.API.Models;
using System.Text.Json;

namespace IconIK.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SehirController : ControllerBase
    {
        private readonly IconIKContext _context;

        public SehirController(IconIKContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            try
            {
                var sehirler = await _context.Sehirler
                    .OrderBy(s => s.SehirAd)
                    .ToListAsync();

                return Ok(new { success = true, data = sehirler, message = "Şehirler başarıyla getirildi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Şehirler getirilirken hata oluştu: " + ex.Message });
            }
        }

        [HttpGet("Aktif")]
        public async Task<ActionResult> GetAktif()
        {
            try
            {
                var sehirler = await _context.Sehirler
                    .Where(s => s.Aktif)
                    .OrderBy(s => s.SehirAd)
                    .ToListAsync();

                return Ok(new { success = true, data = sehirler, message = "Aktif şehirler başarıyla getirildi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Aktif şehirler getirilirken hata oluştu: " + ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(int id)
        {
            try
            {
                var sehir = await _context.Sehirler.FindAsync(id);

                if (sehir == null)
                {
                    return NotFound(new { success = false, message = "Şehir bulunamadı" });
                }

                return Ok(new { success = true, data = sehir, message = "Şehir başarıyla getirildi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Şehir getirilirken hata oluştu: " + ex.Message });
            }
        }
    }
}