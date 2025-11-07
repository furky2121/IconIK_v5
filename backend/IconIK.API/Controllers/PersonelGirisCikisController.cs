using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IconIK.API.Data;
using IconIK.API.Models;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace IconIK.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize] // Temporarily disabled for testing
    public class PersonelGirisCikisController : ControllerBase
    {
        private readonly IconIKContext _context;

        public PersonelGirisCikisController(IconIKContext context)
        {
            _context = context;
        }

        // GET: api/PersonelGirisCikis
        [HttpGet]
        public async Task<IActionResult> GetPersonelGirisCikis()
        {
            try
            {
                var girisCikislar = await _context.PersonelGirisCikislar
                    .Include(pgc => pgc.Personel)
                        .ThenInclude(p => p.Pozisyon)
                            .ThenInclude(pos => pos.Departman)
                    .OrderByDescending(pgc => pgc.Id)
                    .ToListAsync();

                var result = girisCikislar.Select(pgc => new
                {
                    pgc.Id,
                    pgc.PersonelId,
                    PersonelAdi = pgc.Personel?.Ad + " " + pgc.Personel?.Soyad,
                    Departman = pgc.Personel?.Pozisyon?.Departman?.Ad ?? "",
                    Pozisyon = pgc.Personel?.Pozisyon?.Ad ?? "",
                    GirisTarihi = pgc.GirisTarihi,
                    CikisTarihi = pgc.CikisTarihi,
                    pgc.GirisTipi,
                    pgc.Aciklama,
                    pgc.CalismaSuresiDakika,
                    pgc.GecKalmaDakika,
                    pgc.ErkenCikmaDakika,
                    pgc.Aktif,
                    OlusturmaTarihi = pgc.OlusturmaTarihi
                }).ToList();

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // GET: api/PersonelGirisCikis/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPersonelGirisCikis(int id)
        {
            try
            {
                var girisCikis = await _context.PersonelGirisCikislar
                    .Include(pgc => pgc.Personel)
                        .ThenInclude(p => p.Pozisyon)
                            .ThenInclude(pos => pos.Departman)
                    .Select(pgc => new
                    {
                        pgc.Id,
                        pgc.PersonelId,
                        PersonelAdi = pgc.Personel.Ad + " " + pgc.Personel.Soyad,
                        Departman = pgc.Personel.Pozisyon.Departman.Ad,
                        Pozisyon = pgc.Personel.Pozisyon.Ad,
                        pgc.GirisTarihi,
                        pgc.CikisTarihi,
                        pgc.GirisTipi,
                        pgc.Aciklama,
                        pgc.CalismaSuresiDakika,
                        pgc.GecKalmaDakika,
                        pgc.ErkenCikmaDakika,
                        pgc.Aktif,
                        pgc.OlusturmaTarihi
                    })
                    .FirstOrDefaultAsync(pgc => pgc.Id == id);

                if (girisCikis == null)
                {
                    return NotFound(new { success = false, message = "Giriş-çıkış kaydı bulunamadı." });
                }

                return Ok(new { success = true, data = girisCikis });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // GET: api/PersonelGirisCikis/Dashboard
        [HttpGet("Dashboard")]
        public async Task<IActionResult> GetDashboardData()
        {
            try
            {
                var today = DateTime.Today;
                var thisWeekStart = today.AddDays(-(int)today.DayOfWeek);
                var thisMonthStart = new DateTime(today.Year, today.Month, 1);

                // Get all records first
                var allRecords = await _context.PersonelGirisCikislar
                    .Include(pgc => pgc.Personel)
                        .ThenInclude(p => p.Pozisyon)
                            .ThenInclude(pos => pos.Departman)
                    .Where(pgc => pgc.Aktif)
                    .ToListAsync();

                // Günlük istatistikler - En son tarihteki kayıtları al
                var sonTarih = allRecords.Any() ? allRecords.Max(pgc => pgc.GirisTarihi.ToLocalTime().Date) : today;
                var bugunKayitlar = allRecords.Where(pgc => pgc.GirisTarihi.ToLocalTime().Date == sonTarih).ToList();
                var bugunToplamPersonel = bugunKayitlar.Count;
                var bugunGecKalanlar = bugunKayitlar.Count(g => g.GecKalmaDakika > 0);
                var bugunErkenCikanlar = bugunKayitlar.Count(g => g.ErkenCikmaDakika > 0);
                var bugunFazlaMesai = bugunKayitlar.Count(g => g.GirisTipi == "Fazla Mesai");

                // Haftalık istatistikler - Son 7 gün
                var yediGunOnce = sonTarih.AddDays(-6);
                var haftalikKayitlar = allRecords.Where(pgc => pgc.GirisTarihi.ToLocalTime().Date >= yediGunOnce && pgc.GirisTarihi.ToLocalTime().Date <= sonTarih).ToList();
                var haftalikOrtalamaCalisma = haftalikKayitlar.Any() 
                    ? haftalikKayitlar.Where(g => g.CalismaSuresiDakika.HasValue).Average(g => g.CalismaSuresiDakika ?? 0) 
                    : 0;

                // Aylık istatistikler - Son 30 gün
                var otuzGunOnce = sonTarih.AddDays(-29);
                var aylikKayitlar = allRecords.Where(pgc => pgc.GirisTarihi.ToLocalTime().Date >= otuzGunOnce && pgc.GirisTarihi.ToLocalTime().Date <= sonTarih).ToList();
                var aylikToplamCalisma = aylikKayitlar.Where(g => g.CalismaSuresiDakika.HasValue).Sum(g => g.CalismaSuresiDakika ?? 0);

                // Son 7 günün günlük dağılımı
                var son7Gun = new List<object>();
                for (int i = 6; i >= 0; i--)
                {
                    var tarih = sonTarih.AddDays(-i);
                    var gunKayitlar = allRecords.Where(pgc => pgc.GirisTarihi.ToLocalTime().Date == tarih).ToList();
                    var ortalamaCalisma = gunKayitlar.Any() && gunKayitlar.Any(g => g.CalismaSuresiDakika.HasValue)
                        ? gunKayitlar.Where(g => g.CalismaSuresiDakika.HasValue).Average(g => g.CalismaSuresiDakika ?? 0)
                        : 0;

                    son7Gun.Add(new
                    {
                        Tarih = tarih.ToString("dd/MM"),
                        Giris = gunKayitlar.Count,
                        OrtalamaCalisma = ortalamaCalisma
                    });
                }

                var dashboardData = new
                {
                    BugunIstatistikleri = new
                    {
                        ToplamPersonel = bugunToplamPersonel,
                        GecKalanlar = bugunGecKalanlar,
                        ErkenCikanlar = bugunErkenCikanlar,
                        FazlaMesai = bugunFazlaMesai
                    },
                    HaftalikIstatistikler = new
                    {
                        OrtalamaCalisma = Math.Round(haftalikOrtalamaCalisma / 60, 1),
                        ToplamKayit = haftalikKayitlar.Count
                    },
                    AylikIstatistikler = new
                    {
                        ToplamCalisma = Math.Round(aylikToplamCalisma / 60.0, 1),
                        ToplamKayit = aylikKayitlar.Count
                    },
                    DepartmanIstatistikleri = new object[0], // Simplified for now
                    GunlukDagilim = son7Gun
                };

                return Ok(new { success = true, data = dashboardData });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // POST: api/PersonelGirisCikis
        [HttpPost]
        public async Task<IActionResult> PostPersonelGirisCikis([FromBody] JsonElement jsonElement)
        {
            try
            {
                var personelId = jsonElement.GetProperty("PersonelId").GetInt32();
                var girisTarihi = jsonElement.GetProperty("GirisTarihi").GetDateTime();
                var cikisTarihi = jsonElement.TryGetProperty("CikisTarihi", out var cikisValue) && cikisValue.ValueKind != JsonValueKind.Null 
                    ? cikisValue.GetDateTime() 
                    : (DateTime?)null;
                var girisTipi = jsonElement.TryGetProperty("GirisTipi", out var tipValue) ? tipValue.GetString() : "Normal";
                var aciklama = jsonElement.TryGetProperty("Aciklama", out var aciklamaValue) ? aciklamaValue.GetString() : null;

                // Çalışma süresini hesapla
                int? calismaSuresiDakika = null;
                if (cikisTarihi.HasValue)
                {
                    calismaSuresiDakika = (int)(cikisTarihi.Value - girisTarihi).TotalMinutes;
                }

                // Geç kalma ve erken çıkış hesaplaması (basit örnek: 08:00 giriş, 17:30 çıkış)
                var standartGiris = girisTarihi.Date.AddHours(8);
                var standartCikis = girisTarihi.Date.AddHours(17).AddMinutes(30);
                
                var gecKalmaDakika = girisTarihi > standartGiris ? (int)(girisTarihi - standartGiris).TotalMinutes : 0;
                var erkenCikmaDakika = cikisTarihi.HasValue && cikisTarihi.Value < standartCikis 
                    ? (int)(standartCikis - cikisTarihi.Value).TotalMinutes : 0;

                var girisCikis = new PersonelGirisCikis
                {
                    PersonelId = personelId,
                    GirisTarihi = girisTarihi,
                    CikisTarihi = cikisTarihi,
                    GirisTipi = girisTipi ?? "Normal",
                    Aciklama = aciklama,
                    CalismaSuresiDakika = calismaSuresiDakika,
                    GecKalmaDakika = gecKalmaDakika,
                    ErkenCikmaDakika = erkenCikmaDakika,
                    Aktif = true,
                    OlusturmaTarihi = DateTime.UtcNow
                };

                _context.PersonelGirisCikislar.Add(girisCikis);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetPersonelGirisCikis", new { id = girisCikis.Id }, new { success = true, data = girisCikis });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // PUT: api/PersonelGirisCikis/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPersonelGirisCikis(int id, [FromBody] JsonElement jsonElement)
        {
            try
            {
                var girisCikis = await _context.PersonelGirisCikislar.FindAsync(id);
                if (girisCikis == null)
                {
                    return NotFound(new { success = false, message = "Giriş-çıkış kaydı bulunamadı." });
                }

                // Update fields
                if (jsonElement.TryGetProperty("PersonelId", out var personelValue))
                    girisCikis.PersonelId = personelValue.GetInt32();
                
                if (jsonElement.TryGetProperty("GirisTarihi", out var girisValue))
                    girisCikis.GirisTarihi = girisValue.GetDateTime();
                
                if (jsonElement.TryGetProperty("CikisTarihi", out var cikisValue))
                {
                    girisCikis.CikisTarihi = cikisValue.ValueKind != JsonValueKind.Null 
                        ? cikisValue.GetDateTime() 
                        : null;
                }
                
                if (jsonElement.TryGetProperty("GirisTipi", out var tipValue))
                    girisCikis.GirisTipi = tipValue.GetString() ?? "Normal";
                
                if (jsonElement.TryGetProperty("Aciklama", out var aciklamaValue))
                    girisCikis.Aciklama = aciklamaValue.GetString();

                // Recalculate working hours
                if (girisCikis.CikisTarihi.HasValue)
                {
                    girisCikis.CalismaSuresiDakika = (int)(girisCikis.CikisTarihi.Value - girisCikis.GirisTarihi).TotalMinutes;
                }

                // Recalculate late arrival and early departure
                var standartGiris = girisCikis.GirisTarihi.Date.AddHours(8);
                var standartCikis = girisCikis.GirisTarihi.Date.AddHours(17).AddMinutes(30);
                
                girisCikis.GecKalmaDakika = girisCikis.GirisTarihi > standartGiris ? (int)(girisCikis.GirisTarihi - standartGiris).TotalMinutes : 0;
                girisCikis.ErkenCikmaDakika = girisCikis.CikisTarihi.HasValue && girisCikis.CikisTarihi.Value < standartCikis 
                    ? (int)(standartCikis - girisCikis.CikisTarihi.Value).TotalMinutes : 0;

                girisCikis.GuncellemeTarihi = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, data = girisCikis });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // DELETE: api/PersonelGirisCikis/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePersonelGirisCikis(int id)
        {
            try
            {
                var girisCikis = await _context.PersonelGirisCikislar.FindAsync(id);
                if (girisCikis == null)
                {
                    return NotFound(new { success = false, message = "Giriş-çıkış kaydı bulunamadı." });
                }

                _context.PersonelGirisCikislar.Remove(girisCikis);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Giriş-çıkış kaydı silindi." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // GET: api/PersonelGirisCikis/ByPersonel/5
        [HttpGet("ByPersonel/{personelId}")]
        public async Task<IActionResult> GetByPersonel(int personelId, [FromQuery] DateTime? baslangicTarihi = null, [FromQuery] DateTime? bitisTarihi = null)
        {
            try
            {
                var query = _context.PersonelGirisCikislar
                    .Include(pgc => pgc.Personel)
                    .Where(pgc => pgc.PersonelId == personelId && pgc.Aktif);

                if (baslangicTarihi.HasValue)
                    query = query.Where(pgc => pgc.GirisTarihi >= baslangicTarihi.Value);

                if (bitisTarihi.HasValue)
                    query = query.Where(pgc => pgc.GirisTarihi <= bitisTarihi.Value);

                var girisCikislar = await query
                    .Select(pgc => new
                    {
                        pgc.Id,
                        pgc.PersonelId,
                        PersonelAdi = pgc.Personel.Ad + " " + pgc.Personel.Soyad,
                        pgc.GirisTarihi,
                        pgc.CikisTarihi,
                        pgc.GirisTipi,
                        pgc.Aciklama,
                        pgc.CalismaSuresiDakika,
                        pgc.GecKalmaDakika,
                        pgc.ErkenCikmaDakika,
                        pgc.OlusturmaTarihi
                    })
                    .OrderByDescending(pgc => pgc.GirisTarihi)
                    .ToListAsync();

                return Ok(new { success = true, data = girisCikislar });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}