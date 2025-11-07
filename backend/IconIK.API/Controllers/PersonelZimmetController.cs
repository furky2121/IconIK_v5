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
    public class PersonelZimmetController : ControllerBase
    {
        private readonly IconIKContext _context;

        public PersonelZimmetController(IconIKContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var zimmetler = await _context.PersonelZimmetler
                    .Include(pz => pz.Personel)
                        .ThenInclude(p => p.Pozisyon)
                            .ThenInclude(pos => pos.Departman)
                    .Include(pz => pz.ZimmetStok)
                    .Include(pz => pz.ZimmetVeren)
                    .Include(pz => pz.IadeAlan)
                    .OrderByDescending(pz => pz.ZimmetTarihi)
                    .Select(pz => new
                    {
                        pz.Id,
                        pz.PersonelId,
                        PersonelAdSoyad = pz.Personel.Ad + " " + pz.Personel.Soyad,
                        DepartmanAd = pz.Personel.Pozisyon.Departman.Ad,
                        PozisyonAd = pz.Personel.Pozisyon.Ad,
                        pz.ZimmetStokId,
                        MalzemeAdi = pz.ZimmetStok.MalzemeAdi,
                        Marka = pz.ZimmetStok.Marka,
                        Model = pz.ZimmetStok.Model,
                        SeriNo = pz.ZimmetStok.SeriNo,
                        pz.ZimmetMiktar,
                        pz.ZimmetTarihi,
                        pz.IadeTarihi,
                        pz.Durum,
                        pz.ZimmetNotu,
                        pz.IadeNotu,
                        ZimmetVerenAdSoyad = pz.ZimmetVeren != null ? pz.ZimmetVeren.Ad + " " + pz.ZimmetVeren.Soyad : null,
                        IadeAlanAdSoyad = pz.IadeAlan != null ? pz.IadeAlan.Ad + " " + pz.IadeAlan.Soyad : null,
                        pz.Aktif,
                        pz.OlusturmaTarihi,
                        pz.GuncellemeTarihi
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = zimmetler });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Veriler getirilirken hata oluştu: " + ex.Message });
            }
        }

        [HttpGet("GroupedByPersonel")]
        public async Task<IActionResult> GetGroupedByPersonel()
        {
            try
            {
                var grupluZimmetler = await _context.PersonelZimmetler
                    .Include(pz => pz.Personel)
                        .ThenInclude(p => p.Pozisyon)
                            .ThenInclude(pos => pos.Departman)
                    .Include(pz => pz.ZimmetStok)
                    .Where(pz => pz.Aktif)
                    .GroupBy(pz => new
                    {
                        pz.PersonelId,
                        PersonelAdSoyad = pz.Personel.Ad + " " + pz.Personel.Soyad,
                        DepartmanAd = pz.Personel.Pozisyon.Departman.Ad,
                        PozisyonAd = pz.Personel.Pozisyon.Ad,
                        PersonelEmail = pz.Personel.Email,
                        PersonelTelefon = pz.Personel.Telefon
                    })
                    .Select(g => new
                    {
                        PersonelId = g.Key.PersonelId,
                        PersonelAdSoyad = g.Key.PersonelAdSoyad,
                        DepartmanAd = g.Key.DepartmanAd,
                        PozisyonAd = g.Key.PozisyonAd,
                        PersonelEmail = g.Key.PersonelEmail,
                        PersonelTelefon = g.Key.PersonelTelefon,
                        ToplamZimmet = g.Count(),
                        AktifZimmet = g.Count(z => z.Durum == "Zimmetli"),
                        IadeEdilmis = g.Count(z => z.Durum == "Iade Edildi"),
                        SonZimmetTarihi = g.Max(z => z.ZimmetTarihi),
                        ZimmetDetaylar = g.Select(z => new
                        {
                            z.Id,
                            z.ZimmetStokId,
                            MalzemeAdi = z.ZimmetStok.MalzemeAdi,
                            Marka = z.ZimmetStok.Marka,
                            Model = z.ZimmetStok.Model,
                            SeriNo = z.ZimmetStok.SeriNo,
                            z.ZimmetMiktar,
                            z.ZimmetTarihi,
                            z.IadeTarihi,
                            z.Durum,
                            z.ZimmetNotu,
                            z.IadeNotu
                        }).OrderByDescending(z => z.ZimmetTarihi).ToList()
                    })
                    .OrderBy(g => g.PersonelAdSoyad)
                    .ToListAsync();

                return Ok(new { success = true, data = grupluZimmetler });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Gruplu veriler getirilirken hata oluştu: " + ex.Message });
            }
        }

        [HttpGet("Personel/{personelId:int}")]
        public async Task<IActionResult> GetByPersonelId(int personelId)
        {
            try
            {
                var zimmetler = await _context.PersonelZimmetler
                    .Include(pz => pz.ZimmetStok)
                    .Include(pz => pz.ZimmetVeren)
                    .Include(pz => pz.IadeAlan)
                    .Where(pz => pz.PersonelId == personelId && pz.Aktif)
                    .OrderByDescending(pz => pz.ZimmetTarihi)
                    .Select(pz => new
                    {
                        pz.Id,
                        pz.ZimmetStokId,
                        MalzemeAdi = pz.ZimmetStok.MalzemeAdi,
                        Marka = pz.ZimmetStok.Marka,
                        Model = pz.ZimmetStok.Model,
                        SeriNo = pz.ZimmetStok.SeriNo,
                        pz.ZimmetMiktar,
                        pz.ZimmetTarihi,
                        pz.IadeTarihi,
                        pz.Durum,
                        pz.ZimmetNotu,
                        pz.IadeNotu,
                        ZimmetVerenAdSoyad = pz.ZimmetVeren != null ? pz.ZimmetVeren.Ad + " " + pz.ZimmetVeren.Soyad : null,
                        IadeAlanAdSoyad = pz.IadeAlan != null ? pz.IadeAlan.Ad + " " + pz.IadeAlan.Soyad : null
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = zimmetler });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Personel zimmetleri getirilirken hata oluştu: " + ex.Message });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var zimmet = await _context.PersonelZimmetler
                    .Include(pz => pz.Personel)
                        .ThenInclude(p => p.Pozisyon)
                            .ThenInclude(pos => pos.Departman)
                    .Include(pz => pz.ZimmetStok)
                    .Include(pz => pz.ZimmetVeren)
                    .Include(pz => pz.IadeAlan)
                    .FirstOrDefaultAsync(pz => pz.Id == id);

                if (zimmet == null)
                {
                    return NotFound(new { success = false, message = "Personel zimmet kaydı bulunamadı" });
                }

                var result = new
                {
                    zimmet.Id,
                    zimmet.PersonelId,
                    PersonelAdSoyad = zimmet.Personel.Ad + " " + zimmet.Personel.Soyad,
                    DepartmanAd = zimmet.Personel.Pozisyon.Departman.Ad,
                    PozisyonAd = zimmet.Personel.Pozisyon.Ad,
                    zimmet.ZimmetStokId,
                    MalzemeAdi = zimmet.ZimmetStok.MalzemeAdi,
                    Marka = zimmet.ZimmetStok.Marka,
                    Model = zimmet.ZimmetStok.Model,
                    SeriNo = zimmet.ZimmetStok.SeriNo,
                    zimmet.ZimmetMiktar,
                    zimmet.ZimmetTarihi,
                    zimmet.IadeTarihi,
                    zimmet.Durum,
                    zimmet.ZimmetNotu,
                    zimmet.IadeNotu,
                    ZimmetVerenAdSoyad = zimmet.ZimmetVeren != null ? zimmet.ZimmetVeren.Ad + " " + zimmet.ZimmetVeren.Soyad : null,
                    IadeAlanAdSoyad = zimmet.IadeAlan != null ? zimmet.IadeAlan.Ad + " " + zimmet.IadeAlan.Soyad : null,
                    zimmet.Aktif,
                    zimmet.OlusturmaTarihi,
                    zimmet.GuncellemeTarihi
                };

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Personel zimmet getirilirken hata oluştu: " + ex.Message });
            }
        }

        [HttpGet("{id:int}/ZimmetFormu")]
        public async Task<IActionResult> GetZimmetFormu(int id)
        {
            try
            {
                var zimmet = await _context.PersonelZimmetler
                    .Include(pz => pz.Personel)
                        .ThenInclude(p => p.Pozisyon)
                            .ThenInclude(pos => pos.Departman)
                    .Include(pz => pz.ZimmetStok)
                    .Include(pz => pz.ZimmetVeren)
                        .ThenInclude(zv => zv.Pozisyon)
                    .Include(pz => pz.IadeAlan)
                        .ThenInclude(ia => ia.Pozisyon)
                    .FirstOrDefaultAsync(pz => pz.Id == id);

                if (zimmet == null)
                {
                    return NotFound(new { success = false, message = "Personel zimmet kaydı bulunamadı" });
                }

                var result = new
                {
                    zimmet.Id,
                    // Personel bilgileri
                    PersonelId = zimmet.PersonelId,
                    PersonelAd = zimmet.Personel.Ad,
                    PersonelSoyad = zimmet.Personel.Soyad,
                    PersonelAdSoyad = zimmet.Personel.Ad + " " + zimmet.Personel.Soyad,
                    PersonelEmail = zimmet.Personel.Email,
                    PersonelTelefon = zimmet.Personel.Telefon,
                    DepartmanAd = zimmet.Personel.Pozisyon.Departman.Ad,
                    PozisyonAd = zimmet.Personel.Pozisyon.Ad,
                    
                    // Zimmet edilen malzeme bilgileri
                    ZimmetStokId = zimmet.ZimmetStokId,
                    MalzemeAdi = zimmet.ZimmetStok.MalzemeAdi,
                    Kategori = zimmet.ZimmetStok.Kategori,
                    Marka = zimmet.ZimmetStok.Marka,
                    Model = zimmet.ZimmetStok.Model,
                    SeriNo = zimmet.ZimmetStok.SeriNo,
                    Birim = zimmet.ZimmetStok.Birim,
                    Aciklama = zimmet.ZimmetStok.Aciklama,
                    
                    // Zimmet işlem bilgileri
                    ZimmetMiktar = zimmet.ZimmetMiktar,
                    ZimmetTarihi = zimmet.ZimmetTarihi,
                    IadeTarihi = zimmet.IadeTarihi,
                    Durum = zimmet.Durum,
                    ZimmetNotu = zimmet.ZimmetNotu,
                    IadeNotu = zimmet.IadeNotu,
                    
                    // Zimmet veren bilgileri
                    ZimmetVerenId = zimmet.ZimmetVerenId,
                    ZimmetVerenAdSoyad = zimmet.ZimmetVeren != null ? zimmet.ZimmetVeren.Ad + " " + zimmet.ZimmetVeren.Soyad : null,
                    ZimmetVerenPozisyon = zimmet.ZimmetVeren?.Pozisyon?.Ad,
                    
                    // İade alan bilgileri
                    IadeAlanId = zimmet.IadeAlanId,
                    IadeAlanAdSoyad = zimmet.IadeAlan != null ? zimmet.IadeAlan.Ad + " " + zimmet.IadeAlan.Soyad : null,
                    IadeAlanPozisyon = zimmet.IadeAlan?.Pozisyon?.Ad,
                    
                    zimmet.Aktif,
                    zimmet.OlusturmaTarihi,
                    zimmet.GuncellemeTarihi
                };

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Zimmet formu getirilirken hata oluştu: " + ex.Message });
            }
        }

        [HttpPost("Bulk")]
        public async Task<IActionResult> CreateBulk([FromBody] JsonElement jsonElement)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var personelId = jsonElement.GetProperty("personelId").GetInt32();
                var zimmetVerenId = jsonElement.TryGetProperty("zimmetVerenId", out var zimmetVerenIdProp) 
                    ? zimmetVerenIdProp.GetInt32() : (int?)null;
                var zimmetTarihi = jsonElement.TryGetProperty("zimmetTarihi", out var zimmetTarihiProp) 
                    ? zimmetTarihiProp.GetDateTime() : DateTime.UtcNow;
                var zimmetNotu = jsonElement.TryGetProperty("zimmetNotu", out var zimmetNotuProp) 
                    ? zimmetNotuProp.GetString() : null;

                // Get zimmet items array
                var zimmetItemsArray = jsonElement.GetProperty("zimmetItems").EnumerateArray();
                var createdZimmetIds = new List<int>();

                foreach (var zimmetItemElement in zimmetItemsArray)
                {
                    var zimmetStokId = zimmetItemElement.GetProperty("zimmetStokId").GetInt32();
                    var zimmetMiktar = zimmetItemElement.GetProperty("zimmetMiktar").GetInt32();

                    // Check if stock exists and has enough quantity
                    var stok = await _context.ZimmetStoklar.FindAsync(zimmetStokId);
                    if (stok == null)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { success = false, message = $"Zimmet stok bulunamadı (ID: {zimmetStokId})" });
                    }

                    if (stok.OnayDurumu != "Onaylandi")
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { success = false, message = $"Sadece onaylanmış stoklar zimmetlenebilir ({stok.MalzemeAdi})" });
                    }

                    if (stok.KalanMiktar < zimmetMiktar)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { success = false, message = $"Yetersiz stok: {stok.MalzemeAdi}. Kalan stok: {stok.KalanMiktar}, Talep edilen: {zimmetMiktar}. Lütfen mevcut stok miktarından fazla zimmet talebinde bulunmayın." });
                    }
                }

                // If all validations pass, create zimmet records
                foreach (var zimmetItemElement in zimmetItemsArray)
                {
                    var zimmetStokId = zimmetItemElement.GetProperty("zimmetStokId").GetInt32();
                    var zimmetMiktar = zimmetItemElement.GetProperty("zimmetMiktar").GetInt32();

                    var stok = await _context.ZimmetStoklar.FindAsync(zimmetStokId);

                    // Create PersonelZimmet record
                    var personelZimmet = new PersonelZimmet
                    {
                        PersonelId = personelId,
                        ZimmetStokId = zimmetStokId,
                        ZimmetMiktar = zimmetMiktar,
                        ZimmetTarihi = zimmetTarihi,
                        Durum = "Zimmetli",
                        ZimmetNotu = zimmetNotu,
                        ZimmetVerenId = zimmetVerenId,
                        Aktif = true
                    };

                    _context.PersonelZimmetler.Add(personelZimmet);
                    await _context.SaveChangesAsync(); // Save to get the ID

                    createdZimmetIds.Add(personelZimmet.Id);

                    // Update stock quantity
                    stok!.KalanMiktar -= zimmetMiktar;
                    stok.GuncellemeTarihi = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { success = true, data = createdZimmetIds, message = $"{createdZimmetIds.Count} adet zimmet kaydı başarıyla oluşturuldu" });
            }
            catch (DbUpdateException ex) when (ex.InnerException is NpgsqlException npgsqlEx)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { success = false, message = "Veritabanı hatası: " + npgsqlEx.Message });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { success = false, message = "Personel zimmet oluşturulurken hata oluştu: " + ex.Message });
            }
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] JsonElement jsonElement)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var personelId = jsonElement.GetProperty("personelId").GetInt32();
                var zimmetStokId = jsonElement.GetProperty("zimmetStokId").GetInt32();
                var zimmetMiktar = jsonElement.GetProperty("zimmetMiktar").GetInt32();

                // Check if stock exists and has enough quantity
                var stok = await _context.ZimmetStoklar.FindAsync(zimmetStokId);
                if (stok == null)
                {
                    return BadRequest(new { success = false, message = "Zimmet stok bulunamadı" });
                }

                if (stok.OnayDurumu != "Onaylandi")
                {
                    return BadRequest(new { success = false, message = "Sadece onaylanmış stoklar zimmetlenebilir" });
                }

                if (stok.KalanMiktar < zimmetMiktar)
                {
                    return BadRequest(new { success = false, message = $"Yetersiz stok: {stok.MalzemeAdi}. Kalan stok: {stok.KalanMiktar}, Talep edilen: {zimmetMiktar}. Lütfen mevcut stok miktarından fazla zimmet talebinde bulunmayın." });
                }

                // Create PersonelZimmet record
                var personelZimmet = new PersonelZimmet
                {
                    PersonelId = personelId,
                    ZimmetStokId = zimmetStokId,
                    ZimmetMiktar = zimmetMiktar,
                    ZimmetTarihi = jsonElement.TryGetProperty("zimmetTarihi", out var zimmetTarihiProp) 
                        ? zimmetTarihiProp.GetDateTime() : DateTime.UtcNow,
                    Durum = "Zimmetli",
                    ZimmetNotu = jsonElement.TryGetProperty("zimmetNotu", out var zimmetNotuProp) 
                        ? zimmetNotuProp.GetString() : null,
                    ZimmetVerenId = jsonElement.TryGetProperty("zimmetVerenId", out var zimmetVerenIdProp) 
                        ? zimmetVerenIdProp.GetInt32() : null,
                    Aktif = true
                };

                _context.PersonelZimmetler.Add(personelZimmet);

                // Update stock quantity
                stok.KalanMiktar -= zimmetMiktar;
                stok.GuncellemeTarihi = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { success = true, data = personelZimmet, message = "Personel zimmet kaydı başarıyla oluşturuldu" });
            }
            catch (DbUpdateException ex) when (ex.InnerException is NpgsqlException npgsqlEx)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { success = false, message = "Veritabanı hatası: " + npgsqlEx.Message });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { success = false, message = "Personel zimmet oluşturulurken hata oluştu: " + ex.Message });
            }
        }

        [HttpPost("{id:int}/IadeEt")]
        public async Task<IActionResult> IadeEt(int id, [FromBody] JsonElement jsonElement)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var zimmet = await _context.PersonelZimmetler
                    .Include(pz => pz.ZimmetStok)
                    .FirstOrDefaultAsync(pz => pz.Id == id);

                if (zimmet == null)
                {
                    return NotFound(new { success = false, message = "Personel zimmet kaydı bulunamadı" });
                }

                if (zimmet.Durum != "Zimmetli")
                {
                    return BadRequest(new { success = false, message = "Bu zimmet zaten iade edilmiş" });
                }

                // Update PersonelZimmet record
                zimmet.Durum = "Iade Edildi";
                zimmet.IadeTarihi = DateTime.UtcNow;
                zimmet.IadeNotu = jsonElement.TryGetProperty("iadeNotu", out var iadeNotuProp) 
                    ? iadeNotuProp.GetString() : null;
                zimmet.IadeAlanId = jsonElement.TryGetProperty("iadeAlanId", out var iadeAlanIdProp) 
                    ? iadeAlanIdProp.GetInt32() : null;
                zimmet.GuncellemeTarihi = DateTime.UtcNow;

                // Return stock quantity
                zimmet.ZimmetStok.KalanMiktar += zimmet.ZimmetMiktar;
                zimmet.ZimmetStok.GuncellemeTarihi = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { success = true, data = zimmet, message = "Zimmet başarıyla iade edildi" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { success = false, message = "İade işlemi sırasında hata oluştu: " + ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] JsonElement jsonElement)
        {
            try
            {
                var zimmet = await _context.PersonelZimmetler.FindAsync(id);
                if (zimmet == null)
                {
                    return NotFound(new { success = false, message = "Personel zimmet kaydı bulunamadı" });
                }

                if (jsonElement.TryGetProperty("zimmetNotu", out var zimmetNotuProp))
                    zimmet.ZimmetNotu = zimmetNotuProp.GetString();
                if (jsonElement.TryGetProperty("iadeNotu", out var iadeNotuProp))
                    zimmet.IadeNotu = iadeNotuProp.GetString();

                zimmet.GuncellemeTarihi = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, data = zimmet, message = "Personel zimmet kaydı başarıyla güncellendi" });
            }
            catch (DbUpdateException ex) when (ex.InnerException is NpgsqlException npgsqlEx)
            {
                return BadRequest(new { success = false, message = "Veritabanı hatası: " + npgsqlEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Personel zimmet güncellenirken hata oluştu: " + ex.Message });
            }
        }

        [HttpPost("{id:int}/ToggleAktiflik")]
        public async Task<IActionResult> ToggleAktiflik(int id)
        {
            try
            {
                var zimmet = await _context.PersonelZimmetler.FindAsync(id);
                if (zimmet == null)
                {
                    return NotFound(new { success = false, message = "Personel zimmet kaydı bulunamadı" });
                }

                zimmet.Aktif = !zimmet.Aktif;
                zimmet.GuncellemeTarihi = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var durum = zimmet.Aktif ? "aktif" : "pasif";
                return Ok(new { success = true, data = zimmet, message = $"Personel zimmet {durum} yapıldı" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Durum değiştirme işlemi sırasında hata oluştu: " + ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var zimmet = await _context.PersonelZimmetler
                    .Include(pz => pz.ZimmetStok)
                    .FirstOrDefaultAsync(pz => pz.Id == id);

                if (zimmet == null)
                {
                    return NotFound(new { success = false, message = "Personel zimmet kaydı bulunamadı" });
                }

                // If the item is still assigned (not returned), return it to stock
                if (zimmet.Durum == "Zimmetli")
                {
                    zimmet.ZimmetStok.KalanMiktar += zimmet.ZimmetMiktar;
                    zimmet.ZimmetStok.GuncellemeTarihi = DateTime.UtcNow;
                }

                _context.PersonelZimmetler.Remove(zimmet);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { success = true, message = "Personel zimmet kaydı başarıyla silindi" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { success = false, message = "Silme işlemi sırasında hata oluştu: " + ex.Message });
            }
        }
    }
}