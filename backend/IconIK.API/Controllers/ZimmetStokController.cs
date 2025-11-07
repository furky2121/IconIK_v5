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
    public class ZimmetStokController : ControllerBase
    {
        private readonly IconIKContext _context;

        public ZimmetStokController(IconIKContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
        {
                var stoklar = await _context.ZimmetStoklar
                    .Include(zs => zs.Onaylayan)
                    .Include(zs => zs.Olusturan)
                    .OrderByDescending(zs => zs.OlusturmaTarihi)
                    .Select(zs => new
                    {
                        zs.Id,
                        zs.MalzemeAdi,
                        zs.Kategori,
                        zs.Marka,
                        zs.Model,
                        zs.SeriNo,
                        zs.Miktar,
                        zs.KalanMiktar,
                        zs.Birim,
                        zs.Aciklama,
                        zs.OnayDurumu,
                        zs.OnayTarihi,
                        zs.OnayNotu,
                        OnaylayanAdSoyad = zs.Onaylayan != null ? zs.Onaylayan.Ad + " " + zs.Onaylayan.Soyad : null,
                        OlusturanAdSoyad = zs.Olusturan != null ? zs.Olusturan.Ad + " " + zs.Olusturan.Soyad : null,
                        zs.Aktif,
                        zs.OlusturmaTarihi,
                        zs.GuncellemeTarihi
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = stoklar });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Veriler getirilirken hata oluştu: " + ex.Message });
            }
        }

        [HttpGet("OnayBekleyenler")]
        public async Task<IActionResult> GetOnayBekleyenler()
        {
            try
            {
                var bekleyenStoklar = await _context.ZimmetStoklar
                    .Include(zs => zs.Olusturan)
                    .Where(zs => zs.OnayDurumu == "Bekliyor" && zs.Aktif)
                    .OrderBy(zs => zs.OlusturmaTarihi)
                    .Select(zs => new
                    {
                        zs.Id,
                        zs.MalzemeAdi,
                        zs.Kategori,
                        zs.Marka,
                        zs.Model,
                        zs.SeriNo,
                        zs.Miktar,
                        zs.KalanMiktar,
                        zs.Birim,
                        zs.Aciklama,
                        zs.OnayDurumu,
                        OlusturanAdSoyad = zs.Olusturan != null ? zs.Olusturan.Ad + " " + zs.Olusturan.Soyad : null,
                        zs.OlusturmaTarihi
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = bekleyenStoklar });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Onay bekleyen stoklar getirilirken hata oluştu: " + ex.Message });
            }
        }

        [HttpGet("OnaylananStoklar")]
        public async Task<IActionResult> GetOnaylananStoklar()
        {
            try
            {
                var onaylananStoklar = await _context.ZimmetStoklar
                    .Where(zs => zs.OnayDurumu == "Onaylandi" && zs.Aktif && zs.KalanMiktar > 0)
                    .OrderBy(zs => zs.MalzemeAdi)
                    .Select(zs => new
                    {
                        zs.Id,
                        zs.MalzemeAdi,
                        zs.Kategori,
                        zs.Marka,
                        zs.Model,
                        zs.SeriNo,
                        zs.Miktar,
                        zs.KalanMiktar,
                        zs.Birim,
                        zs.Aciklama,
                        FullName = zs.MalzemeAdi + (string.IsNullOrEmpty(zs.Marka) ? "" : " - " + zs.Marka) + 
                                  (string.IsNullOrEmpty(zs.Model) ? "" : " " + zs.Model)
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = onaylananStoklar });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Onaylanan stoklar getirilirken hata oluştu: " + ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var stok = await _context.ZimmetStoklar
                    .Include(zs => zs.Onaylayan)
                    .Include(zs => zs.Olusturan)
                    .FirstOrDefaultAsync(zs => zs.Id == id);

                if (stok == null)
                {
                    return NotFound(new { success = false, message = "Zimmet stok kaydı bulunamadı" });
                }

                var result = new
                {
                    stok.Id,
                    stok.MalzemeAdi,
                    stok.Kategori,
                    stok.Marka,
                    stok.Model,
                    stok.SeriNo,
                    stok.Miktar,
                    stok.Birim,
                    stok.Aciklama,
                    stok.OnayDurumu,
                    stok.OnayTarihi,
                    stok.OnayNotu,
                    OnaylayanAdSoyad = stok.Onaylayan != null ? stok.Onaylayan.Ad + " " + stok.Onaylayan.Soyad : null,
                    OlusturanAdSoyad = stok.Olusturan != null ? stok.Olusturan.Ad + " " + stok.Olusturan.Soyad : null,
                    stok.Aktif,
                    stok.OlusturmaTarihi,
                    stok.GuncellemeTarihi
                };

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Zimmet stok getirilirken hata oluştu: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] JsonElement jsonElement)
        {
            try
            {
                var stok = new ZimmetStok
                {
                    MalzemeAdi = jsonElement.GetProperty("malzemeAdi").GetString() ?? "",
                    Kategori = jsonElement.TryGetProperty("kategori", out var kategoriProp) ? kategoriProp.GetString() : null,
                    Marka = jsonElement.TryGetProperty("marka", out var markaProp) ? markaProp.GetString() : null,
                    Model = jsonElement.TryGetProperty("model", out var modelProp) ? modelProp.GetString() : null,
                    SeriNo = jsonElement.TryGetProperty("seriNo", out var seriNoProp) ? seriNoProp.GetString() : null,
                    Miktar = jsonElement.TryGetProperty("miktar", out var miktarProp) ? miktarProp.GetInt32() : 1,
                    KalanMiktar = jsonElement.TryGetProperty("miktar", out var kalanMiktarProp) ? kalanMiktarProp.GetInt32() : 1,
                    Birim = jsonElement.TryGetProperty("birim", out var birimProp) ? birimProp.GetString() : "Adet",
                    Aciklama = jsonElement.TryGetProperty("aciklama", out var aciklamaProp) ? aciklamaProp.GetString() : null,
                    OnayDurumu = "Bekliyor",
                    OlusturanId = jsonElement.TryGetProperty("olusturanId", out var olusturanIdProp) ? olusturanIdProp.GetInt32() : null,
                    Aktif = true
                };

                _context.ZimmetStoklar.Add(stok);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, data = stok, message = "Zimmet stok kaydı başarıyla oluşturuldu" });
            }
            catch (DbUpdateException ex) when (ex.InnerException is NpgsqlException npgsqlEx)
            {
                return BadRequest(new { success = false, message = "Veritabanı hatası: " + npgsqlEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Zimmet stok oluşturulurken hata oluştu: " + ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] JsonElement jsonElement)
        {
            try
            {
                var stok = await _context.ZimmetStoklar.FindAsync(id);
                if (stok == null)
                {
                    return NotFound(new { success = false, message = "Zimmet stok kaydı bulunamadı" });
                }

                stok.MalzemeAdi = jsonElement.GetProperty("malzemeAdi").GetString() ?? stok.MalzemeAdi;
                if (jsonElement.TryGetProperty("kategori", out var kategoriProp))
                    stok.Kategori = kategoriProp.GetString();
                if (jsonElement.TryGetProperty("marka", out var markaProp))
                    stok.Marka = markaProp.GetString();
                if (jsonElement.TryGetProperty("model", out var modelProp))
                    stok.Model = modelProp.GetString();
                if (jsonElement.TryGetProperty("seriNo", out var seriNoProp))
                    stok.SeriNo = seriNoProp.GetString();
                if (jsonElement.TryGetProperty("miktar", out var miktarProp))
                    stok.Miktar = miktarProp.GetInt32();
                if (jsonElement.TryGetProperty("birim", out var birimProp))
                    stok.Birim = birimProp.GetString();
                if (jsonElement.TryGetProperty("aciklama", out var aciklamaProp))
                    stok.Aciklama = aciklamaProp.GetString();

                stok.GuncellemeTarihi = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, data = stok, message = "Zimmet stok kaydı başarıyla güncellendi" });
            }
            catch (DbUpdateException ex) when (ex.InnerException is NpgsqlException npgsqlEx)
            {
                return BadRequest(new { success = false, message = "Veritabanı hatası: " + npgsqlEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Zimmet stok güncellenirken hata oluştu: " + ex.Message });
            }
        }

        [HttpPost("{id}/Onayla")]
        public async Task<IActionResult> Onayla(int id, [FromBody] JsonElement jsonElement)
        {
            try
            {
                var stok = await _context.ZimmetStoklar.FindAsync(id);
                if (stok == null)
                {
                    return NotFound(new { success = false, message = "Zimmet stok kaydı bulunamadı" });
                }

                if (stok.OnayDurumu != "Bekliyor")
                {
                    return BadRequest(new { success = false, message = "Bu stok zaten onaylanmış veya reddedilmiş" });
                }

                stok.OnayDurumu = "Onaylandi";
                stok.OnaylayanId = jsonElement.TryGetProperty("onaylayanId", out var onaylayanIdProp) ? onaylayanIdProp.GetInt32() : null;
                stok.OnayTarihi = DateTime.UtcNow;
                stok.OnayNotu = jsonElement.TryGetProperty("onayNotu", out var onayNotuProp) ? onayNotuProp.GetString() : null;
                stok.GuncellemeTarihi = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, data = stok, message = "Zimmet stok başarıyla onaylandı" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Onaylama işlemi sırasında hata oluştu: " + ex.Message });
            }
        }

        [HttpPost("{id}/Reddet")]
        public async Task<IActionResult> Reddet(int id, [FromBody] JsonElement jsonElement)
        {
            try
            {
                var stok = await _context.ZimmetStoklar.FindAsync(id);
                if (stok == null)
                {
                    return NotFound(new { success = false, message = "Zimmet stok kaydı bulunamadı" });
                }

                if (stok.OnayDurumu != "Bekliyor")
                {
                    return BadRequest(new { success = false, message = "Bu stok zaten onaylanmış veya reddedilmiş" });
                }

                stok.OnayDurumu = "Reddedildi";
                stok.OnaylayanId = jsonElement.TryGetProperty("onaylayanId", out var onaylayanIdProp) ? onaylayanIdProp.GetInt32() : null;
                stok.OnayTarihi = DateTime.UtcNow;
                stok.OnayNotu = jsonElement.TryGetProperty("onayNotu", out var onayNotuProp) ? onayNotuProp.GetString() : "Stok talebi reddedildi";
                stok.GuncellemeTarihi = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, data = stok, message = "Zimmet stok reddedildi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Red işlemi sırasında hata oluştu: " + ex.Message });
            }
        }

        [HttpPost("{id}/ToggleAktiflik")]
        public async Task<IActionResult> ToggleAktiflik(int id)
        {
            try
            {
                var stok = await _context.ZimmetStoklar.FindAsync(id);
                if (stok == null)
                {
                    return NotFound(new { success = false, message = "Zimmet stok kaydı bulunamadı" });
                }

                stok.Aktif = !stok.Aktif;
                stok.GuncellemeTarihi = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var durum = stok.Aktif ? "aktif" : "pasif";
                return Ok(new { success = true, data = stok, message = $"Zimmet stok {durum} yapıldı" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Durum değiştirme işlemi sırasında hata oluştu: " + ex.Message });
            }
        }

        [HttpPost("ConsumeStock")]
        public async Task<IActionResult> ConsumeStock([FromBody] JsonElement jsonElement)
        {
            try
            {
                var stockConsumptions = jsonElement.GetProperty("stockConsumptions").EnumerateArray();
                
                foreach (var consumption in stockConsumptions)
                {
                    var stokId = consumption.GetProperty("stokId").GetInt32();
                    var consumedQuantity = consumption.GetProperty("quantity").GetInt32();
                    
                    var stok = await _context.ZimmetStoklar.FindAsync(stokId);
                    if (stok == null)
                    {
                        return BadRequest(new { success = false, message = $"Stok bulunamadı: {stokId}" });
                    }
                    
                    if (stok.KalanMiktar < consumedQuantity)
                    {
                        return BadRequest(new { success = false, message = $"Yetersiz stok: {stok.MalzemeAdi}. Mevcut: {stok.KalanMiktar}, Talep edilen: {consumedQuantity}" });
                    }
                    
                    stok.KalanMiktar -= consumedQuantity;
                    stok.GuncellemeTarihi = DateTime.UtcNow;
                }
                
                await _context.SaveChangesAsync();
                
                return Ok(new { success = true, message = "Stok miktarları başarıyla güncellendi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Stok tüketimi sırasında hata oluştu: " + ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var stok = await _context.ZimmetStoklar.FindAsync(id);
                if (stok == null)
                {
                    return NotFound(new { success = false, message = "Zimmet stok kaydı bulunamadı" });
                }

                _context.ZimmetStoklar.Remove(stok);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Zimmet stok kaydı başarıyla silindi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Silme işlemi sırasında hata oluştu: " + ex.Message });
            }
        }

        [HttpPost("{id}/UploadFiles")]
        public async Task<IActionResult> UploadFiles(int id, List<IFormFile> files)
        {
            try
            {
                var stok = await _context.ZimmetStoklar.FindAsync(id);
                if (stok == null)
                {
                    return NotFound(new { success = false, message = "Zimmet stok kaydı bulunamadı" });
                }

                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "zimmet-stok");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                var uploadedFiles = new List<object>();

                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                        var filePath = Path.Combine(uploadsPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        var dosya = new ZimmetStokDosya
                        {
                            ZimmetStokId = id,
                            DosyaAdi = fileName,
                            OrijinalAdi = file.FileName,
                            DosyaYolu = $"/uploads/zimmet-stok/{fileName}",
                            DosyaBoyutu = file.Length,
                            MimeTipi = file.ContentType,
                            OlusturmaTarihi = DateTime.UtcNow
                        };

                        _context.ZimmetStokDosyalar.Add(dosya);
                        
                        uploadedFiles.Add(new
                        {
                            id = dosya.Id,
                            orijinalAdi = dosya.OrijinalAdi,
                            dosyaYolu = dosya.DosyaYolu,
                            dosyaBoyutu = dosya.DosyaBoyutu,
                            mimeTipi = dosya.MimeTipi
                        });
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = $"{uploadedFiles.Count} dosya başarıyla yüklendi", data = uploadedFiles });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Dosya yükleme sırasında hata oluştu: " + ex.Message });
            }
        }

        [HttpGet("{id}/Files")]
        public async Task<IActionResult> GetFiles(int id)
        {
            try
            {
                var dosyalar = await _context.ZimmetStokDosyalar
                    .Where(d => d.ZimmetStokId == id)
                    .OrderBy(d => d.OlusturmaTarihi)
                    .Select(d => new
                    {
                        d.Id,
                        d.OrijinalAdi,
                        d.DosyaYolu,
                        d.DosyaBoyutu,
                        d.MimeTipi,
                        d.OlusturmaTarihi
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = dosyalar });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Dosyalar getirilirken hata oluştu: " + ex.Message });
            }
        }

        [HttpDelete("Files/{dosyaId}")]
        public async Task<IActionResult> DeleteFile(int dosyaId)
        {
            try
            {
                var dosya = await _context.ZimmetStokDosyalar.FindAsync(dosyaId);
                if (dosya == null)
                {
                    return NotFound(new { success = false, message = "Dosya bulunamadı" });
                }

                // Fiziksel dosyayı sil
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", dosya.DosyaYolu.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }

                _context.ZimmetStokDosyalar.Remove(dosya);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Dosya başarıyla silindi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Dosya silme sırasında hata oluştu: " + ex.Message });
            }
        }
    }
}