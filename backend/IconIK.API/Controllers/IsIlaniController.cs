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
    public class IsIlaniController : ControllerBase
    {
        private readonly IconIKContext _context;

        public IsIlaniController(IconIKContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<object>> GetIsIlanlari()
        {
            try
            {
                var isIlanlari = await _context.IsIlanlari
                    .Include(i => i.Kategori)
                    .Include(i => i.Pozisyon)
                    .Include(i => i.Departman)
                    .Include(i => i.Olusturan)
                    .OrderByDescending(i => i.CreatedAt)
                    .Select(i => new
                    {
                        i.Id,
                        i.Baslik,
                        Kategori = i.Kategori.Ad,
                        Pozisyon = i.Pozisyon.Ad,
                        Departman = i.Departman.Ad,
                        i.IsTanimi,
                        i.Gereksinimler,
                        i.MinMaas,
                        i.MaxMaas,
                        i.CalismaSekli,
                        i.DeneyimYili,
                        i.EgitimSeviyesi,
                        i.YayinTarihi,
                        i.BitisTarihi,
                        Durum = i.Durum.ToString(),
                        Olusturan = i.Olusturan.Ad + " " + i.Olusturan.Soyad,
                        i.Aktif,
                        i.CreatedAt,
                        BasvuruSayisi = i.Basvurular.Count()
                    })
                    .ToListAsync();

                return new { success = true, data = isIlanlari, message = "İş ilanları başarıyla getirildi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpGet("Aktif")]
        public async Task<ActionResult<object>> GetAktifIsIlanlari()
        {
            try
            {
                var aktifIlanlar = await _context.IsIlanlari
                    .Where(i => i.Aktif && i.Durum == IlanDurumu.Aktif)
                    .Include(i => i.Pozisyon)
                    .Include(i => i.Departman)
                    .Select(i => new
                    {
                        i.Id,
                        i.Baslik,
                        Pozisyon = i.Pozisyon.Ad,
                        Departman = i.Departman.Ad
                    })
                    .OrderBy(i => i.Baslik)
                    .ToListAsync();

                return new { success = true, data = aktifIlanlar, message = "Aktif iş ilanları başarıyla getirildi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetIsIlani(int id)
        {
            try
            {
                var isIlani = await _context.IsIlanlari
                    .Include(i => i.Kategori)
                    .Include(i => i.Pozisyon)
                    .Include(i => i.Departman)
                    .Include(i => i.Olusturan)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (isIlani == null)
                {
                    return new { success = false, message = "İş ilanı bulunamadı." };
                }

                var result = new
                {
                    isIlani.Id,
                    isIlani.Baslik,
                    isIlani.KategoriId,
                    Kategori = isIlani.Kategori.Ad,
                    isIlani.PozisyonId,
                    Pozisyon = isIlani.Pozisyon.Ad,
                    isIlani.DepartmanId,
                    Departman = isIlani.Departman.Ad,
                    isIlani.IsTanimi,
                    isIlani.Gereksinimler,
                    isIlani.MinMaas,
                    isIlani.MaxMaas,
                    isIlani.CalismaSekli,
                    isIlani.DeneyimYili,
                    isIlani.EgitimSeviyesi,
                    isIlani.YayinTarihi,
                    isIlani.BitisTarihi,
                    Durum = isIlani.Durum.ToString(),
                    isIlani.OlusturanId,
                    Olusturan = isIlani.Olusturan.Ad + " " + isIlani.Olusturan.Soyad,
                    isIlani.Aktif,
                    isIlani.CreatedAt
                };

                return new { success = true, data = result, message = "İş ilanı başarıyla getirildi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<object>> CreateIsIlani([FromBody] JsonElement ilanData)
        {
            try
            {
                var isIlani = new IsIlani
                {
                    Baslik = ilanData.GetProperty("baslik").GetString() ?? "",
                    KategoriId = ilanData.GetProperty("kategoriId").GetInt32(),
                    PozisyonId = ilanData.GetProperty("pozisyonId").GetInt32(),
                    DepartmanId = ilanData.GetProperty("departmanId").GetInt32(),
                    IsTanimi = ilanData.GetProperty("isTanimi").GetString() ?? "",
                    Gereksinimler = ilanData.GetProperty("gereksinimler").GetString() ?? "",
                    CalismaSekli = ilanData.TryGetProperty("calismaSekli", out var calismaSekli) ? calismaSekli.GetString() : null,
                    DeneyimYili = ilanData.TryGetProperty("deneyimYili", out var deneyimYili) ? deneyimYili.GetInt32() : 0,
                    EgitimSeviyesi = ilanData.TryGetProperty("egitimSeviyesi", out var egitimSeviyesi) ? egitimSeviyesi.GetString() : null,
                    OlusturanId = ilanData.GetProperty("olusturanId").GetInt32(),
                    Durum = IlanDurumu.Taslak
                };

                if (ilanData.TryGetProperty("minMaas", out var minMaas) && !minMaas.ValueKind.Equals(JsonValueKind.Null))
                {
                    isIlani.MinMaas = minMaas.GetDecimal();
                }

                if (ilanData.TryGetProperty("maxMaas", out var maxMaas) && !maxMaas.ValueKind.Equals(JsonValueKind.Null))
                {
                    isIlani.MaxMaas = maxMaas.GetDecimal();
                }

                if (ilanData.TryGetProperty("yayinTarihi", out var yayinTarihi) && !yayinTarihi.ValueKind.Equals(JsonValueKind.Null))
                {
                    isIlani.YayinTarihi = yayinTarihi.GetDateTime();
                }

                if (ilanData.TryGetProperty("bitisTarihi", out var bitisTarihi) && !bitisTarihi.ValueKind.Equals(JsonValueKind.Null))
                {
                    isIlani.BitisTarihi = bitisTarihi.GetDateTime();
                }

                _context.IsIlanlari.Add(isIlani);
                await _context.SaveChangesAsync();

                return new { success = true, data = new { id = isIlani.Id }, message = "İş ilanı başarıyla oluşturuldu." };
            }
            catch (DbUpdateException ex) when (ex.InnerException is NpgsqlException npgsqlEx && npgsqlEx.SqlState == "23505")
            {
                return new { success = false, message = "Bu kriterlerde bir ilan zaten mevcut." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<object>> UpdateIsIlani(int id, [FromBody] JsonElement ilanData)
        {
            try
            {
                var isIlani = await _context.IsIlanlari.FindAsync(id);
                if (isIlani == null)
                {
                    return new { success = false, message = "İş ilanı bulunamadı." };
                }

                if (ilanData.TryGetProperty("baslik", out var baslik))
                    isIlani.Baslik = baslik.GetString() ?? "";

                if (ilanData.TryGetProperty("kategoriId", out var kategoriId))
                    isIlani.KategoriId = kategoriId.GetInt32();

                if (ilanData.TryGetProperty("pozisyonId", out var pozisyonId))
                    isIlani.PozisyonId = pozisyonId.GetInt32();

                if (ilanData.TryGetProperty("departmanId", out var departmanId))
                    isIlani.DepartmanId = departmanId.GetInt32();

                if (ilanData.TryGetProperty("isTanimi", out var isTanimi))
                    isIlani.IsTanimi = isTanimi.GetString() ?? "";

                if (ilanData.TryGetProperty("gereksinimler", out var gereksinimler))
                    isIlani.Gereksinimler = gereksinimler.GetString() ?? "";

                if (ilanData.TryGetProperty("minMaas", out var minMaas))
                    isIlani.MinMaas = !minMaas.ValueKind.Equals(JsonValueKind.Null) ? minMaas.GetDecimal() : null;

                if (ilanData.TryGetProperty("maxMaas", out var maxMaas))
                    isIlani.MaxMaas = !maxMaas.ValueKind.Equals(JsonValueKind.Null) ? maxMaas.GetDecimal() : null;

                if (ilanData.TryGetProperty("calismaSekli", out var calismaSekli))
                    isIlani.CalismaSekli = calismaSekli.GetString();

                if (ilanData.TryGetProperty("deneyimYili", out var deneyimYili))
                    isIlani.DeneyimYili = deneyimYili.GetInt32();

                if (ilanData.TryGetProperty("egitimSeviyesi", out var egitimSeviyesi))
                    isIlani.EgitimSeviyesi = egitimSeviyesi.GetString();

                if (ilanData.TryGetProperty("yayinTarihi", out var yayinTarihi))
                    isIlani.YayinTarihi = !yayinTarihi.ValueKind.Equals(JsonValueKind.Null) ? yayinTarihi.GetDateTime() : null;

                if (ilanData.TryGetProperty("bitisTarihi", out var bitisTarihi))
                    isIlani.BitisTarihi = !bitisTarihi.ValueKind.Equals(JsonValueKind.Null) ? bitisTarihi.GetDateTime() : null;

                if (ilanData.TryGetProperty("durum", out var durum))
                {
                    if (Enum.TryParse<IlanDurumu>(durum.GetString(), out var ilanDurumu))
                        isIlani.Durum = ilanDurumu;
                }

                if (ilanData.TryGetProperty("aktif", out var aktif))
                    isIlani.Aktif = aktif.GetBoolean();

                await _context.SaveChangesAsync();

                return new { success = true, message = "İş ilanı başarıyla güncellendi." };
            }
            catch (DbUpdateException ex) when (ex.InnerException is NpgsqlException npgsqlEx && npgsqlEx.SqlState == "23505")
            {
                return new { success = false, message = "Bu kriterlerde bir ilan zaten mevcut." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<object>> DeleteIsIlani(int id)
        {
            try
            {
                var isIlani = await _context.IsIlanlari.FindAsync(id);
                if (isIlani == null)
                {
                    return new { success = false, message = "İş ilanı bulunamadı." };
                }

                _context.IsIlanlari.Remove(isIlani);
                await _context.SaveChangesAsync();

                return new { success = true, message = "İş ilanı başarıyla silindi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpPut("{id}/yayin")]
        public async Task<ActionResult<object>> YayinlaIsIlani(int id)
        {
            try
            {
                var isIlani = await _context.IsIlanlari.FindAsync(id);
                if (isIlani == null)
                {
                    return new { success = false, message = "İş ilanı bulunamadı." };
                }

                isIlani.Durum = IlanDurumu.Aktif;
                isIlani.YayinTarihi = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new { success = true, message = "İş ilanı başarıyla yayınlandı." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpPut("{id}/kapat")]
        public async Task<ActionResult<object>> KapatIsIlani(int id)
        {
            try
            {
                var isIlani = await _context.IsIlanlari.FindAsync(id);
                if (isIlani == null)
                {
                    return new { success = false, message = "İş ilanı bulunamadı." };
                }

                isIlani.Durum = IlanDurumu.Kapali;
                await _context.SaveChangesAsync();

                return new { success = true, message = "İş ilanı başarıyla kapatıldı." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }
    }
}