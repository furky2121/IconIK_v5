using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IconIK.API.Data;
using IconIK.API.Models;
using System.Text.Json;
using System.Text;

namespace IconIK.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IstifaController : ControllerBase
    {
        private readonly IconIKContext _context;

        public IstifaController(IconIKContext context)
        {
            _context = context;
        }

        // GET: api/Istifa
        [HttpGet]
        public async Task<ActionResult<object>> GetIstifaTalepleri(int personelId)
        {
            try
            {
                var query = _context.IstifaTalepleri
                    .Include(i => i.Personel)
                        .ThenInclude(p => p.Pozisyon)
                            .ThenInclude(pos => pos.Departman)
                    .Include(i => i.Onaylayan)
                    .Where(i => i.PersonelId == personelId)
                    .AsQueryable();

                var istifaTalepleri = await query
                    .OrderByDescending(i => i.CreatedAt)
                    .Select(i => new
                    {
                        i.Id,
                        i.PersonelId,
                        PersonelAd = i.Personel.Ad + " " + i.Personel.Soyad,
                        PersonelPozisyon = i.Personel.Pozisyon.Ad,
                        PersonelDepartman = i.Personel.Pozisyon.Departman.Ad,
                        PersonelIseBaslama = i.Personel.IseBaslamaTarihi,
                        i.IstifaTarihi,
                        i.SonCalismaTarihi,
                        i.IstifaNedeni,
                        i.OnayDurumu,
                        i.OnaylayanId,
                        OnaylayanAd = i.Onaylayan != null ? i.Onaylayan.Ad + " " + i.Onaylayan.Soyad : null,
                        i.OnayTarihi,
                        i.OnayNotu,
                        i.CreatedAt,
                        i.UpdatedAt
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = istifaTalepleri });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "İstifa talepleri getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/Istifa/Onay
        [HttpGet("Onay")]
        public async Task<ActionResult<object>> GetOnayBekleyenIstifaTalepleri(int yoneticiId)
        {
            try
            {
                var yoneticiPersonel = await _context.Personeller
                    .Include(p => p.Pozisyon)
                        .ThenInclude(pos => pos.Kademe)
                    .FirstOrDefaultAsync(p => p.Id == yoneticiId);

                if (yoneticiPersonel == null)
                {
                    return NotFound(new { success = false, message = "Yönetici bulunamadı." });
                }

                var query = _context.IstifaTalepleri
                    .Include(i => i.Personel)
                        .ThenInclude(p => p.Pozisyon)
                            .ThenInclude(pos => pos.Departman)
                    .Include(i => i.Onaylayan)
                    .Where(i => i.OnayDurumu == "Beklemede");

                // Kademe seviyesine göre filtreleme
                if (yoneticiPersonel.Pozisyon.Kademe.Seviye > 1)
                {
                    query = query.Where(i => i.Personel.YoneticiId == yoneticiId);
                }

                var istifaTalepleri = await query
                    .OrderByDescending(i => i.CreatedAt)
                    .Select(i => new
                    {
                        i.Id,
                        i.PersonelId,
                        PersonelAd = i.Personel.Ad + " " + i.Personel.Soyad,
                        PersonelPozisyon = i.Personel.Pozisyon.Ad,
                        PersonelDepartman = i.Personel.Pozisyon.Departman.Ad,
                        PersonelIseBaslama = i.Personel.IseBaslamaTarihi,
                        CalismaYili = (DateTime.Now.Year - i.Personel.IseBaslamaTarihi.Year),
                        i.IstifaTarihi,
                        i.SonCalismaTarihi,
                        i.IstifaNedeni,
                        i.OnayDurumu,
                        i.CreatedAt
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = istifaTalepleri });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Onay bekleyen istifa talepleri getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/Istifa/Dilekce/5
        [HttpGet("Dilekce/{id}")]
        public async Task<ActionResult> GetIstifaDilekcesi(int id)
        {
            try
            {
                var istifaTalebi = await _context.IstifaTalepleri
                    .Include(i => i.Personel)
                        .ThenInclude(p => p.Pozisyon)
                            .ThenInclude(pos => pos.Departman)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (istifaTalebi == null)
                {
                    return NotFound(new { success = false, message = "İstifa talebi bulunamadı." });
                }

                var dilekce = GenerateIstifaDilekcesi(istifaTalebi);
                var bytes = Encoding.UTF8.GetBytes(dilekce);
                
                // PDF yerine şimdilik text dosyası olarak döndürüyoruz, ileride PDF kütüphanesi eklenebilir
                return File(bytes, "application/octet-stream", $"istifa_dilekcesi_{istifaTalebi.Personel.Ad}_{istifaTalebi.Personel.Soyad}.txt");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "İstifa dilekçesi oluşturulurken bir hata oluştu.", error = ex.Message });
            }
        }

        private string GenerateIstifaDilekcesi(IstifaTalebi istifaTalebi)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Tarih: {istifaTalebi.IstifaTarihi:dd.MM.yyyy}");
            sb.AppendLine();
            sb.AppendLine("Icon A.Ş.");
            sb.AppendLine("İnsan Kaynakları Müdürlüğüne");
            sb.AppendLine();
            sb.AppendLine($"Sayın Yetkili,");
            sb.AppendLine();
            sb.AppendLine($"{istifaTalebi.Personel.IseBaslamaTarihi:dd.MM.yyyy} tarihinden bu yana {istifaTalebi.Personel.Pozisyon.Departman.Ad} departmanında");
            sb.AppendLine($"{istifaTalebi.Personel.Pozisyon.Ad} pozisyonunda görev yapmaktayım.");
            sb.AppendLine();
            
            if (!string.IsNullOrEmpty(istifaTalebi.IstifaNedeni))
            {
                sb.AppendLine($"{istifaTalebi.IstifaNedeni} nedeniyle");
            }
            else
            {
                sb.AppendLine("Kişisel nedenlerimden dolayı");
            }
            
            sb.AppendLine($"{istifaTalebi.SonCalismaTarihi:dd.MM.yyyy} tarihinde işten ayrılmak istiyorum.");
            sb.AppendLine();
            sb.AppendLine("Bu güne kadar bana verdiğiniz destek ve gösterdiğiniz anlayış için teşekkür ederim.");
            sb.AppendLine();
            sb.AppendLine("Saygılarımla,");
            sb.AppendLine();
            sb.AppendLine($"{istifaTalebi.Personel.Ad} {istifaTalebi.Personel.Soyad}");
            sb.AppendLine($"TC Kimlik No: {istifaTalebi.Personel.TcKimlik}");
            sb.AppendLine($"Telefon: {istifaTalebi.Personel.Telefon}");
            sb.AppendLine($"E-posta: {istifaTalebi.Personel.Email}");
            
            return sb.ToString();
        }

        // POST: api/Istifa
        [HttpPost]
        public async Task<ActionResult<object>> PostIstifaTalebi([FromBody] JsonElement jsonElement)
        {
            try
            {
                var istifaTalebi = new IstifaTalebi();
                
                // JSON parsing
                if (jsonElement.TryGetProperty("personelId", out var personelIdProp))
                    istifaTalebi.PersonelId = personelIdProp.GetInt32();
                    
                if (jsonElement.TryGetProperty("sonCalismaTarihi", out var sonCalismaProp))
                    istifaTalebi.SonCalismaTarihi = sonCalismaProp.GetDateTime();
                    
                if (jsonElement.TryGetProperty("istifaNedeni", out var nedenProp))
                    istifaTalebi.IstifaNedeni = nedenProp.GetString();

                // Aynı personelin beklemede olan başka istifa talebi var mı kontrol et
                var mevcutTalep = await _context.IstifaTalepleri
                    .AnyAsync(i => i.PersonelId == istifaTalebi.PersonelId && i.OnayDurumu == "Beklemede");
                
                if (mevcutTalep)
                {
                    return BadRequest(new { success = false, message = "Beklemede olan bir istifa talebiniz bulunmaktadır." });
                }

                istifaTalebi.IstifaTarihi = DateTime.UtcNow;
                istifaTalebi.OnayDurumu = "Beklemede";
                istifaTalebi.CreatedAt = DateTime.UtcNow;
                istifaTalebi.UpdatedAt = DateTime.UtcNow;

                _context.IstifaTalepleri.Add(istifaTalebi);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, data = istifaTalebi, message = "İstifa talebi başarıyla oluşturuldu." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "İstifa talebi oluşturulurken bir hata oluştu.", error = ex.Message });
            }
        }

        // PUT: api/Istifa/5
        [HttpPut("{id}")]
        public async Task<ActionResult<object>> PutIstifaTalebi(int id, [FromBody] JsonElement jsonElement)
        {
            try
            {
                var istifaTalebi = await _context.IstifaTalepleri.FindAsync(id);
                if (istifaTalebi == null)
                {
                    return NotFound(new { success = false, message = "İstifa talebi bulunamadı." });
                }

                if (istifaTalebi.OnayDurumu != "Beklemede")
                {
                    return BadRequest(new { success = false, message = "Sadece beklemede olan talepler güncellenebilir." });
                }

                // JSON parsing
                if (jsonElement.TryGetProperty("sonCalismaTarihi", out var sonCalismaProp))
                    istifaTalebi.SonCalismaTarihi = sonCalismaProp.GetDateTime();
                    
                if (jsonElement.TryGetProperty("istifaNedeni", out var nedenProp))
                    istifaTalebi.IstifaNedeni = nedenProp.GetString();

                istifaTalebi.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, data = istifaTalebi, message = "İstifa talebi başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "İstifa talebi güncellenirken bir hata oluştu.", error = ex.Message });
            }
        }

        // DELETE: api/Istifa/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<object>> DeleteIstifaTalebi(int id)
        {
            try
            {
                var istifaTalebi = await _context.IstifaTalepleri.FindAsync(id);
                if (istifaTalebi == null)
                {
                    return NotFound(new { success = false, message = "İstifa talebi bulunamadı." });
                }

                if (istifaTalebi.OnayDurumu != "Beklemede")
                {
                    return BadRequest(new { success = false, message = "Sadece beklemede olan talepler silinebilir." });
                }

                _context.IstifaTalepleri.Remove(istifaTalebi);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "İstifa talebi başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "İstifa talebi silinirken bir hata oluştu.", error = ex.Message });
            }
        }

        // POST: api/Istifa/Onayla/5
        [HttpPost("Onayla/{id}")]
        public async Task<ActionResult<object>> OnaylaIstifaTalebi(int id, [FromBody] JsonElement jsonElement)
        {
            try
            {
                var istifaTalebi = await _context.IstifaTalepleri
                    .Include(i => i.Personel)
                    .FirstOrDefaultAsync(i => i.Id == id);
                    
                if (istifaTalebi == null)
                {
                    return NotFound(new { success = false, message = "İstifa talebi bulunamadı." });
                }

                if (istifaTalebi.OnayDurumu != "Beklemede")
                {
                    return BadRequest(new { success = false, message = "Bu talep zaten işleme alınmış." });
                }

                var onaylayanId = jsonElement.GetProperty("onaylayanId").GetInt32();
                var onayNotu = jsonElement.TryGetProperty("onayNotu", out var notProp) ? notProp.GetString() : null;

                istifaTalebi.OnayDurumu = "Onaylandı";
                istifaTalebi.OnaylayanId = onaylayanId;
                istifaTalebi.OnayTarihi = DateTime.UtcNow;
                istifaTalebi.OnayNotu = onayNotu;
                istifaTalebi.UpdatedAt = DateTime.UtcNow;

                // Personeli pasif yap
                istifaTalebi.Personel.Aktif = false;
                istifaTalebi.Personel.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "İstifa talebi onaylandı ve personel pasif duruma geçirildi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "İstifa talebi onaylanırken bir hata oluştu.", error = ex.Message });
            }
        }

        // POST: api/Istifa/Reddet/5
        [HttpPost("Reddet/{id}")]
        public async Task<ActionResult<object>> ReddetIstifaTalebi(int id, [FromBody] JsonElement jsonElement)
        {
            try
            {
                var istifaTalebi = await _context.IstifaTalepleri.FindAsync(id);
                if (istifaTalebi == null)
                {
                    return NotFound(new { success = false, message = "İstifa talebi bulunamadı." });
                }

                if (istifaTalebi.OnayDurumu != "Beklemede")
                {
                    return BadRequest(new { success = false, message = "Bu talep zaten işleme alınmış." });
                }

                var onaylayanId = jsonElement.GetProperty("onaylayanId").GetInt32();
                var onayNotu = jsonElement.TryGetProperty("onayNotu", out var notProp) ? notProp.GetString() : null;

                istifaTalebi.OnayDurumu = "Reddedildi";
                istifaTalebi.OnaylayanId = onaylayanId;
                istifaTalebi.OnayTarihi = DateTime.UtcNow;
                istifaTalebi.OnayNotu = onayNotu;
                istifaTalebi.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "İstifa talebi reddedildi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "İstifa talebi reddedilirken bir hata oluştu.", error = ex.Message });
            }
        }
    }
}