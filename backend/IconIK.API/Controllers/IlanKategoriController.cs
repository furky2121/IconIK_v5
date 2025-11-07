using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IconIK.API.Data;
using IconIK.API.Models;
using System.Text.Json;
using Npgsql;

namespace IconIK.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IlanKategoriController : ControllerBase
    {
        private readonly IconIKContext _context;

        public IlanKategoriController(IconIKContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<object>> GetIlanKategorileri()
        {
            try
            {
                var kategoriler = await _context.IlanKategoriler
                    .OrderBy(k => k.Ad)
                    .Select(k => new
                    {
                        k.Id,
                        k.Ad,
                        k.Aciklama,
                        k.Aktif,
                        k.CreatedAt,
                        IlanSayisi = k.IsIlanlari.Count()
                    })
                    .ToListAsync();

                return new { success = true, data = kategoriler, message = "İlan kategorileri başarıyla getirildi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpGet("Aktif")]
        public async Task<ActionResult<object>> GetAktifIlanKategorileri()
        {
            try
            {
                var aktifKategoriler = await _context.IlanKategoriler
                    .Where(k => k.Aktif)
                    .Select(k => new
                    {
                        k.Id,
                        k.Ad
                    })
                    .OrderBy(k => k.Ad)
                    .ToListAsync();

                return new { success = true, data = aktifKategoriler, message = "Aktif ilan kategorileri başarıyla getirildi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetIlanKategori(int id)
        {
            try
            {
                var kategori = await _context.IlanKategoriler.FindAsync(id);
                if (kategori == null)
                {
                    return new { success = false, message = "İlan kategorisi bulunamadı." };
                }

                return new { success = true, data = kategori, message = "İlan kategorisi başarıyla getirildi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<object>> CreateIlanKategori([FromBody] JsonElement kategoriData)
        {
            try
            {
                var kategori = new IlanKategori
                {
                    Ad = kategoriData.GetProperty("ad").GetString() ?? "",
                    Aciklama = kategoriData.TryGetProperty("aciklama", out var aciklama) ? aciklama.GetString() : null
                };

                _context.IlanKategoriler.Add(kategori);
                await _context.SaveChangesAsync();

                return new { success = true, data = new { id = kategori.Id }, message = "İlan kategorisi başarıyla oluşturuldu." };
            }
            catch (DbUpdateException ex) when (ex.InnerException is NpgsqlException npgsqlEx && npgsqlEx.SqlState == "23505")
            {
                return new { success = false, message = "Bu isimde bir kategori zaten mevcut." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<object>> UpdateIlanKategori(int id, [FromBody] JsonElement kategoriData)
        {
            try
            {
                var kategori = await _context.IlanKategoriler.FindAsync(id);
                if (kategori == null)
                {
                    return new { success = false, message = "İlan kategorisi bulunamadı." };
                }

                if (kategoriData.TryGetProperty("ad", out var ad))
                    kategori.Ad = ad.GetString() ?? "";

                if (kategoriData.TryGetProperty("aciklama", out var aciklama))
                    kategori.Aciklama = aciklama.GetString();

                if (kategoriData.TryGetProperty("aktif", out var aktif))
                    kategori.Aktif = aktif.GetBoolean();

                await _context.SaveChangesAsync();

                return new { success = true, message = "İlan kategorisi başarıyla güncellendi." };
            }
            catch (DbUpdateException ex) when (ex.InnerException is NpgsqlException npgsqlEx && npgsqlEx.SqlState == "23505")
            {
                return new { success = false, message = "Bu isimde bir kategori zaten mevcut." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<object>> DeleteIlanKategori(int id)
        {
            try
            {
                var kategori = await _context.IlanKategoriler
                    .Include(k => k.IsIlanlari)
                    .FirstOrDefaultAsync(k => k.Id == id);

                if (kategori == null)
                {
                    return new { success = false, message = "İlan kategorisi bulunamadı." };
                }

                if (kategori.IsIlanlari.Any())
                {
                    return new { success = false, message = "Bu kategoriye ait iş ilanları bulunduğu için kategori silinemez." };
                }

                _context.IlanKategoriler.Remove(kategori);
                await _context.SaveChangesAsync();

                return new { success = true, message = "İlan kategorisi başarıyla silindi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }
    }
}