using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IconIK.API.Data;
using IconIK.API.Models;
using IconIK.API.Services;

namespace IconIK.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IzinTalebiController : ControllerBase
    {
        private readonly IconIKContext _context;
        private readonly IIzinService _izinService;
        private readonly IBildirimService _bildirimService;

        public IzinTalebiController(IconIKContext context, IIzinService izinService, IBildirimService bildirimService)
        {
            _context = context;
            _izinService = izinService;
            _bildirimService = bildirimService;
        }

        // GET: api/IzinTalebi - Sadece kişinin kendi talepleri
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetIzinTalepleri(int personelId)
        {
            try
            {
                // Artık sadece belirli bir personelin kendi taleplerini getir
                var query = _context.IzinTalepleri
                    .Include(i => i.Personel)
                        .ThenInclude(p => p.Pozisyon)
                            .ThenInclude(pos => pos.Departman)
                    .Include(i => i.Personel)
                        .ThenInclude(p => p.Pozisyon)
                            .ThenInclude(pos => pos.Kademe)
                    .Include(i => i.Onaylayan)
                    .Where(i => i.PersonelId == personelId) // Sadece bu personelin talepleri
                    .AsQueryable();

                var izinTalepleri = await query
                    .OrderByDescending(i => i.CreatedAt)
                    .Select(i => new
                    {
                        i.Id,
                        i.PersonelId,
                        PersonelAd = i.Personel.Ad + " " + i.Personel.Soyad,
                        PersonelPozisyon = i.Personel.Pozisyon.Ad,
                        PersonelDepartman = i.Personel.Pozisyon.Departman.Ad,
                        PersonelKademe = i.Personel.Pozisyon.Kademe.Ad,
                        PersonelFotograf = i.Personel.FotografUrl,
                        IzinBaslamaTarihi = i.IzinBaslamaTarihi,
                        IsbasiTarihi = i.IsbasiTarihi,
                        i.GunSayisi,
                        i.IzinTipi,
                        i.Aciklama,
                        i.GorevYeri,
                        i.RaporDosyaYolu,
                        i.Durum,
                        i.OnaylayanId,
                        OnaylayanAd = i.Onaylayan != null && i.Durum == "Onaylandı" ? i.Onaylayan.Ad + " " + i.Onaylayan.Soyad : null,
                        ReddedenAd = i.Onaylayan != null && i.Durum == "Reddedildi" ? i.Onaylayan.Ad + " " + i.Onaylayan.Soyad : null,
                        i.OnayTarihi,
                        i.OnayNotu,
                        i.CreatedAt,
                        i.UpdatedAt
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = izinTalepleri, message = "İzin talepleri başarıyla listelendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "İzin talepleri listelenirken bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/IzinTalebi/Admin - Get all leave requests (for administrators)
        [HttpGet("Admin")]
        public async Task<ActionResult<object>> GetAllIzinTalepleri()
        {
            try
            {
                var query = _context.IzinTalepleri
                    .Include(i => i.Personel)
                        .ThenInclude(p => p.Pozisyon)
                            .ThenInclude(pos => pos.Departman)
                    .Include(i => i.Personel)
                        .ThenInclude(p => p.Pozisyon)
                            .ThenInclude(pos => pos.Kademe)
                    .Include(i => i.Onaylayan)
                    .AsQueryable();

                var izinTalepleri = await query
                    .OrderByDescending(i => i.CreatedAt)
                    .Select(i => new
                    {
                        i.Id,
                        i.PersonelId,
                        PersonelAd = i.Personel.Ad + " " + i.Personel.Soyad,
                        PersonelPozisyon = i.Personel.Pozisyon.Ad,
                        PersonelDepartman = i.Personel.Pozisyon.Departman.Ad,
                        PersonelKademe = i.Personel.Pozisyon.Kademe.Ad,
                        PersonelFotograf = i.Personel.FotografUrl,
                        IzinBaslamaTarihi = i.IzinBaslamaTarihi,
                        IsbasiTarihi = i.IsbasiTarihi,
                        i.GunSayisi,
                        i.IzinTipi,
                        i.Aciklama,
                        i.GorevYeri,
                        i.RaporDosyaYolu,
                        i.Durum,
                        i.OnaylayanId,
                        OnaylayanAd = i.Onaylayan != null && i.Durum == "Onaylandı" ? i.Onaylayan.Ad + " " + i.Onaylayan.Soyad : null,
                        ReddedenAd = i.Onaylayan != null && i.Durum == "Reddedildi" ? i.Onaylayan.Ad + " " + i.Onaylayan.Soyad : null,
                        i.OnayTarihi,
                        i.OnayNotu,
                        i.CreatedAt,
                        i.UpdatedAt
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = izinTalepleri, message = "Tüm izin talepleri başarıyla listelendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "İzin talepleri listelenirken bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/IzinTalebi/FixDays
        [HttpGet("FixDays")]
        public async Task<ActionResult<object>> AdminFixDayCalculations()
        {
            try
            {
                var allRecords = await _context.IzinTalepleri.ToListAsync();
                var updatedRecords = new List<object>();
                var updatedCount = 0;
                
                Console.WriteLine($"DEBUG FIX: Found {allRecords.Count} total records");
                
                foreach (var record in allRecords)
                {
                    var correctGunSayisi = _izinService.CalculateGunSayisi(record.IzinBaslamaTarihi, record.IsbasiTarihi);
                    
                    if (record.GunSayisi != correctGunSayisi)
                    {
                        Console.WriteLine($"DEBUG FIX: Record ID {record.Id}: {record.IzinBaslamaTarihi:yyyy-MM-dd} to {record.IsbasiTarihi:yyyy-MM-dd}");
                        Console.WriteLine($"DEBUG FIX: Old: {record.GunSayisi} days, Correct: {correctGunSayisi} days");
                        
                        updatedRecords.Add(new {
                            id = record.Id,
                            dates = $"{record.IzinBaslamaTarihi:yyyy-MM-dd} to {record.IsbasiTarihi:yyyy-MM-dd}",
                            oldDays = record.GunSayisi,
                            newDays = correctGunSayisi
                        });
                        
                        record.GunSayisi = correctGunSayisi;
                        record.UpdatedAt = DateTime.UtcNow;
                        updatedCount++;
                    }
                }
                
                if (updatedCount > 0)
                {
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"DEBUG FIX: Successfully updated {updatedCount} records");
                }
                
                return Ok(new { 
                    success = true, 
                    data = new {
                        totalRecords = allRecords.Count,
                        updatedCount = updatedCount,
                        updatedRecords = updatedRecords
                    },
                    message = $"Day calculation fix completed. {updatedCount} out of {allRecords.Count} records were corrected." 
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG FIX: Error - {ex.Message}");
                return StatusCode(500, new { success = false, message = "Fix failed.", error = ex.Message });
            }
        }

        // GET: api/IzinTalebi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetIzinTalebi(int id)
        {
            try
            {
                var izinTalebi = await _context.IzinTalepleri
                    .Include(i => i.Personel)
                        .ThenInclude(p => p.Pozisyon)
                            .ThenInclude(pos => pos.Departman)
                    .Include(i => i.Personel)
                        .ThenInclude(p => p.Pozisyon)
                            .ThenInclude(pos => pos.Kademe)
                    .Include(i => i.Onaylayan)
                    .Where(i => i.Id == id)
                    .Select(i => new
                    {
                        i.Id,
                        i.PersonelId,
                        PersonelAd = i.Personel.Ad + " " + i.Personel.Soyad,
                        PersonelPozisyon = i.Personel.Pozisyon.Ad,
                        PersonelDepartman = i.Personel.Pozisyon.Departman.Ad,
                        PersonelKademe = i.Personel.Pozisyon.Kademe.Ad,
                        PersonelEmail = i.Personel.Email,
                        PersonelFotograf = i.Personel.FotografUrl,
                        IzinBaslamaTarihi = i.IzinBaslamaTarihi,
                        IsbasiTarihi = i.IsbasiTarihi,
                        i.GunSayisi,
                        i.IzinTipi,
                        i.Aciklama,
                        i.GorevYeri,
                        i.RaporDosyaYolu,
                        i.Durum,
                        i.OnaylayanId,
                        OnaylayanAd = i.Onaylayan != null && i.Durum == "Onaylandı" ? i.Onaylayan.Ad + " " + i.Onaylayan.Soyad : null,
                        ReddedenAd = i.Onaylayan != null && i.Durum == "Reddedildi" ? i.Onaylayan.Ad + " " + i.Onaylayan.Soyad : null,
                        i.OnayTarihi,
                        i.OnayNotu,
                        i.CreatedAt,
                        i.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                if (izinTalebi == null)
                {
                    return NotFound(new { success = false, message = "İzin talebi bulunamadı." });
                }

                return Ok(new { success = true, data = izinTalebi, message = "İzin talebi başarıyla getirildi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "İzin talebi getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/IzinTalebi/PersonelOzet/5
        [HttpGet("PersonelOzet/{personelId}")]
        public async Task<ActionResult<object>> GetPersonelIzinOzeti(int personelId)
        {
            try
            {
                var personel = await _context.Personeller.FindAsync(personelId);
                if (personel == null)
                {
                    return NotFound(new { success = false, message = "Personel bulunamadı." });
                }

                var izinOzeti = await _izinService.GetPersonelIzinOzeti(personelId);

                return Ok(new { success = true, data = izinOzeti, message = "İzin özeti başarıyla getirildi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "İzin özeti getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        // POST: api/IzinTalebi
        [HttpPost]
        public async Task<ActionResult<object>> PostIzinTalebi([FromBody] System.Text.Json.JsonElement jsonElement)
        {
            try
            {
                var izinTalebi = new IzinTalebi();
                
                // JSON parsing
                if (jsonElement.TryGetProperty("personelId", out var personelIdProp))
                    izinTalebi.PersonelId = personelIdProp.GetInt32();
                    
                // Yeni alan adları ile çalış
                if (jsonElement.TryGetProperty("izinBaslamaTarihi", out var izinBaslamaProp))
                {
                    var izinTarih = izinBaslamaProp.GetDateTime();
                    if (jsonElement.TryGetProperty("izinBaslamaSaati", out var izinSaatProp))
                    {
                        var saat = izinSaatProp.GetString() ?? "08:00";
                        var saatParts = saat.Split(':');
                        izinTarih = izinTarih.AddHours(int.Parse(saatParts[0])).AddMinutes(int.Parse(saatParts[1]));
                    }
                    izinTalebi.IzinBaslamaTarihi = DateTime.SpecifyKind(izinTarih, DateTimeKind.Utc);
                }
                    
                if (jsonElement.TryGetProperty("isbasiTarihi", out var isbasiProp))
                {
                    var isbasiTarih = isbasiProp.GetDateTime();
                    if (jsonElement.TryGetProperty("isbasiSaati", out var isbasiSaatProp))
                    {
                        var saat = isbasiSaatProp.GetString() ?? "08:00";
                        var saatParts = saat.Split(':');
                        isbasiTarih = isbasiTarih.AddHours(int.Parse(saatParts[0])).AddMinutes(int.Parse(saatParts[1]));
                    }
                    izinTalebi.IsbasiTarihi = DateTime.SpecifyKind(isbasiTarih, DateTimeKind.Utc);
                }
                    
                if (jsonElement.TryGetProperty("izinTipi", out var tipProp))
                    izinTalebi.IzinTipi = tipProp.GetString() ?? "Yıllık İzin";
                    
                if (jsonElement.TryGetProperty("gorevYeri", out var gorevYeriProp))
                    izinTalebi.GorevYeri = gorevYeriProp.GetString();

                if (jsonElement.TryGetProperty("aciklama", out var aciklamaProp))
                    izinTalebi.Aciklama = aciklamaProp.GetString();

                if (jsonElement.TryGetProperty("raporDosyaYolu", out var raporProp))
                    izinTalebi.RaporDosyaYolu = raporProp.GetString();

                // Frontend'den rapor yüklenecek mi bilgisini al
                bool raporYuklenecek = false;
                if (jsonElement.TryGetProperty("raporYuklenecek", out var raporYuklenecekProp))
                {
                    raporYuklenecek = raporYuklenecekProp.GetBoolean();
                    Console.WriteLine($"DEBUG BACKEND: raporYuklenecek field bulundu, değer: {raporYuklenecek}");
                }
                else
                {
                    Console.WriteLine("DEBUG BACKEND: raporYuklenecek field bulunamadı!");
                }

                // Gün sayısını hesapla (saat bilgisi ile)
                if (jsonElement.TryGetProperty("gunSayisi", out var gunSayisiProp))
                    izinTalebi.GunSayisi = (int)Math.Ceiling(gunSayisiProp.GetDouble());

                // Personel var mı kontrol et
                var personel = await _context.Personeller.FindAsync(izinTalebi.PersonelId);
                if (personel == null || !personel.Aktif)
                {
                    return BadRequest(new { success = false, message = "Geçersiz personel seçimi." });
                }

                // Tarih kontrolü - aynı gün içinde izin alabilmeli (yarım gün için)
                Console.WriteLine($"DEBUG: İzin Başlama: {izinTalebi.IzinBaslamaTarihi}, İşbaşı: {izinTalebi.IsbasiTarihi}");
                if (izinTalebi.IzinBaslamaTarihi > izinTalebi.IsbasiTarihi)
                {
                    return BadRequest(new { success = false, message = "İşbaşı tarihi, izin başlama tarihinden sonra olmalıdır." });
                }

                // Geçmiş tarih kontrolü
                if (izinTalebi.IzinBaslamaTarihi.Date < DateTime.Today)
                {
                    return BadRequest(new { success = false, message = "Geçmiş tarihlerde izin talebi oluşturamazsınız." });
                }

                // Gün sayısını hesapla
                var hesaplananGunSayisi = _izinService.CalculateGunSayisi(izinTalebi.IzinBaslamaTarihi, izinTalebi.IsbasiTarihi);
                Console.WriteLine($"DEBUG POST: Frontend'den gelen gunSayisi: {izinTalebi.GunSayisi}");
                Console.WriteLine($"DEBUG POST: Backend'de hesaplanan gunSayisi: {hesaplananGunSayisi}");
                izinTalebi.GunSayisi = hesaplananGunSayisi;

                if (izinTalebi.GunSayisi <= 0)
                {
                    return BadRequest(new { success = false, message = "İzin talebi en az 1 iş günü olmalıdır." });
                }

                // İzin tipi validation kontrolü (min/max gün, cinsiyet, rapor gereklilik)
                // Eğer frontend'den raporYuklenecek=true geliyorsa, rapor yüklenecek demektir
                bool hasRapor = !string.IsNullOrEmpty(izinTalebi.RaporDosyaYolu) || raporYuklenecek;

                var validationResult = await _izinService.ValidateIzinTalebi(
                    izinTalebi.IzinTipi,
                    izinTalebi.GunSayisi,
                    izinTalebi.PersonelId,
                    hasRapor);

                if (!validationResult.IsValid)
                {
                    return BadRequest(new { success = false, message = validationResult.ErrorMessage });
                }

                // İzin çakışması kontrolü
                var cakismaVar = await _izinService.CheckIzinCakismasi(izinTalebi.PersonelId,
                    izinTalebi.IzinBaslamaTarihi, izinTalebi.IsbasiTarihi);

                if (cakismaVar)
                {
                    return BadRequest(new { success = false, message = "Bu tarihler arasında zaten bir izin talebiniz bulunmaktadır." });
                }

                // İzin hakkı kontrolü (sadece Yıllık İzin için)
                if (izinTalebi.IzinTipi == "Yıllık İzin")
                {
                    var kalanIzin = await _izinService.CalculateKalanIzin(izinTalebi.PersonelId);
                    if (izinTalebi.GunSayisi > kalanIzin)
                    {
                        return BadRequest(new { success = false, message = $"Yeterli izin hakkınız bulunmamaktadır. Kalan izin: {kalanIzin} gün" });
                    }
                }

                izinTalebi.Durum = "Beklemede";
                izinTalebi.CreatedAt = DateTime.UtcNow;
                izinTalebi.UpdatedAt = DateTime.UtcNow;

                _context.IzinTalepleri.Add(izinTalebi);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetIzinTalebi", new { id = izinTalebi.Id },
                    new { success = true, data = new { izinTalebi.Id }, message = "İzin talebi başarıyla oluşturuldu." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "İzin talebi oluşturulurken bir hata oluştu.", error = ex.Message });
            }
        }

        // PUT: api/IzinTalebi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutIzinTalebi(int id, [FromBody] System.Text.Json.JsonElement jsonElement)
        {
            try
            {
                var izinTalebi = new IzinTalebi();
                
                // JSON parsing
                if (jsonElement.TryGetProperty("id", out var idProp))
                    izinTalebi.Id = idProp.GetInt32();
                    
                if (jsonElement.TryGetProperty("personelId", out var personelIdProp))
                    izinTalebi.PersonelId = personelIdProp.GetInt32();
                    
                if (jsonElement.TryGetProperty("izinBaslamaTarihi", out var izinBaslamaProp))
                {
                    var izinTarih = izinBaslamaProp.GetDateTime();
                    if (jsonElement.TryGetProperty("izinBaslamaSaati", out var izinSaatProp))
                    {
                        var saat = izinSaatProp.GetString() ?? "08:00";
                        var saatParts = saat.Split(':');
                        izinTarih = izinTarih.AddHours(int.Parse(saatParts[0])).AddMinutes(int.Parse(saatParts[1]));
                    }
                    izinTalebi.IzinBaslamaTarihi = DateTime.SpecifyKind(izinTarih, DateTimeKind.Utc);
                }
                    
                if (jsonElement.TryGetProperty("isbasiTarihi", out var isbasiProp))
                {
                    var isbasiTarih = isbasiProp.GetDateTime();
                    if (jsonElement.TryGetProperty("isbasiSaati", out var isbasiSaatProp))
                    {
                        var saat = isbasiSaatProp.GetString() ?? "08:00";
                        var saatParts = saat.Split(':');
                        isbasiTarih = isbasiTarih.AddHours(int.Parse(saatParts[0])).AddMinutes(int.Parse(saatParts[1]));
                    }
                    izinTalebi.IsbasiTarihi = DateTime.SpecifyKind(isbasiTarih, DateTimeKind.Utc);
                }
                    
                if (jsonElement.TryGetProperty("izinTipi", out var tipProp))
                    izinTalebi.IzinTipi = tipProp.GetString() ?? "Yıllık İzin";
                    
                if (jsonElement.TryGetProperty("gorevYeri", out var gorevYeriProp))
                    izinTalebi.GorevYeri = gorevYeriProp.GetString();
                    
                if (jsonElement.TryGetProperty("aciklama", out var aciklamaProp))
                    izinTalebi.Aciklama = aciklamaProp.GetString();

                // Gün sayısını hesapla (saat bilgisi ile)
                if (jsonElement.TryGetProperty("gunSayisi", out var gunSayisiProp))
                    izinTalebi.GunSayisi = (int)Math.Ceiling(gunSayisiProp.GetDouble());

                if (id != izinTalebi.Id)
                {
                    return BadRequest(new { success = false, message = "Geçersiz izin talebi ID." });
                }
                var mevcutIzin = await _context.IzinTalepleri.FindAsync(id);
                if (mevcutIzin == null)
                {
                    return NotFound(new { success = false, message = "İzin talebi bulunamadı." });
                }

                // Sadece beklemedeki izinler güncellenebilir
                if (mevcutIzin.Durum != "Beklemede")
                {
                    return BadRequest(new { success = false, message = "Sadece beklemedeki izin talepleri güncellenebilir." });
                }

                // Tarih kontrolü - aynı gün içinde izin alabilmeli (yarım gün için)
                Console.WriteLine($"DEBUG: İzin Başlama: {izinTalebi.IzinBaslamaTarihi}, İşbaşı: {izinTalebi.IsbasiTarihi}");
                if (izinTalebi.IzinBaslamaTarihi > izinTalebi.IsbasiTarihi)
                {
                    return BadRequest(new { success = false, message = "İşbaşı tarihi, izin başlama tarihinden sonra olmalıdır." });
                }

                // Geçmiş tarih kontrolü
                if (izinTalebi.IzinBaslamaTarihi.Date < DateTime.Today)
                {
                    return BadRequest(new { success = false, message = "Geçmiş tarihlerde izin talebi güncelleyemezsiniz." });
                }

                // Gün sayısını hesapla
                var hesaplananGunSayisiPut = _izinService.CalculateGunSayisi(izinTalebi.IzinBaslamaTarihi, izinTalebi.IsbasiTarihi);
                Console.WriteLine($"DEBUG PUT: Frontend'den gelen gunSayisi: {izinTalebi.GunSayisi}");
                Console.WriteLine($"DEBUG PUT: Backend'de hesaplanan gunSayisi: {hesaplananGunSayisiPut}");
                izinTalebi.GunSayisi = hesaplananGunSayisiPut;

                // İzin çakışması kontrolü (kendisi hariç)
                var cakismaVar = await _izinService.CheckIzinCakismasi(izinTalebi.PersonelId, 
                    izinTalebi.IzinBaslamaTarihi, izinTalebi.IsbasiTarihi, id);

                if (cakismaVar)
                {
                    return BadRequest(new { success = false, message = "Bu tarihler arasında zaten bir izin talebiniz bulunmaktadır." });
                }

                // İzin hakkı kontrolü
                var kalanIzin = await _izinService.CalculateKalanIzin(izinTalebi.PersonelId);
                // Mevcut talebin gün sayısını geri ekle
                kalanIzin += mevcutIzin.GunSayisi;

                if (izinTalebi.GunSayisi > kalanIzin)
                {
                    return BadRequest(new { success = false, message = $"Yeterli izin hakkınız bulunmamaktadır. Kalan izin: {kalanIzin} gün" });
                }

                // Güncelleme
                mevcutIzin.IzinBaslamaTarihi = izinTalebi.IzinBaslamaTarihi;
                mevcutIzin.IsbasiTarihi = izinTalebi.IsbasiTarihi;
                mevcutIzin.GunSayisi = izinTalebi.GunSayisi;
                mevcutIzin.IzinTipi = izinTalebi.IzinTipi;
                mevcutIzin.Aciklama = izinTalebi.Aciklama;
                mevcutIzin.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "İzin talebi başarıyla güncellendi." });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!IzinTalebiExists(id))
                {
                    return NotFound(new { success = false, message = "İzin talebi bulunamadı." });
                }
                throw;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "İzin talebi güncellenirken bir hata oluştu.", error = ex.Message });
            }
        }

        // POST: api/IzinTalebi/Onayla/5
        [HttpPost("Onayla/{id}")]
        public async Task<IActionResult> OnaylaIzinTalebi(int id, [FromBody] OnayRequest request)
        {
            try
            {
                var izinTalebi = await _context.IzinTalepleri.FindAsync(id);
                if (izinTalebi == null)
                {
                    return NotFound(new { success = false, message = "İzin talebi bulunamadı." });
                }

                if (izinTalebi.Durum != "Beklemede")
                {
                    return BadRequest(new { success = false, message = "Sadece beklemedeki izin talepleri onaylanabilir." });
                }

                // Onaylama yetkisi kontrolü
                var canApprove = await _izinService.CanPersonelApproveIzin(request.OnaylayanId, izinTalebi.PersonelId);
                if (!canApprove)
                {
                    return BadRequest(new { success = false, message = "Bu izin talebini onaylama yetkiniz bulunmamaktadır." });
                }

                izinTalebi.Durum = "Onaylandı";
                izinTalebi.OnaylayanId = request.OnaylayanId;
                izinTalebi.OnayTarihi = DateTime.UtcNow;
                izinTalebi.OnayNotu = request.OnayNotu;
                izinTalebi.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Bildirim gönder
                var onaylayan = await _context.Personeller.FindAsync(request.OnaylayanId);
                var bildirim = new Bildirim
                {
                    AliciId = izinTalebi.PersonelId,
                    Baslik = "İzin Talebiniz Onaylandı",
                    Mesaj = $"{izinTalebi.IzinBaslamaTarihi:dd.MM.yyyy} - {izinTalebi.IsbasiTarihi:dd.MM.yyyy} tarihleri arası {izinTalebi.GunSayisi} gün süresince {izinTalebi.IzinTipi} talebiniz onaylanmıştır.",
                    Kategori = "izin",
                    Tip = "success",
                    GonderenAd = onaylayan != null ? $"{onaylayan.Ad} {onaylayan.Soyad}" : "Sistem",
                    ActionUrl = "/izin-talepleri"
                };
                await _bildirimService.CreateBildirimAsync(bildirim);

                return Ok(new { success = true, message = "İzin talebi başarıyla onaylandı." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "İzin talebi onaylanırken bir hata oluştu.", error = ex.Message });
            }
        }

        // POST: api/IzinTalebi/Reddet/5
        [HttpPost("Reddet/{id}")]
        public async Task<IActionResult> ReddetIzinTalebi(int id, [FromBody] OnayRequest request)
        {
            try
            {
                var izinTalebi = await _context.IzinTalepleri.FindAsync(id);
                if (izinTalebi == null)
                {
                    return NotFound(new { success = false, message = "İzin talebi bulunamadı." });
                }

                if (izinTalebi.Durum != "Beklemede")
                {
                    return BadRequest(new { success = false, message = "Sadece beklemedeki izin talepleri reddedilebilir." });
                }

                // Reddetme yetkisi kontrolü
                var canApprove = await _izinService.CanPersonelApproveIzin(request.OnaylayanId, izinTalebi.PersonelId);
                if (!canApprove)
                {
                    return BadRequest(new { success = false, message = "Bu izin talebini reddetme yetkiniz bulunmamaktadır." });
                }

                izinTalebi.Durum = "Reddedildi";
                izinTalebi.OnaylayanId = request.OnaylayanId;
                izinTalebi.OnayTarihi = DateTime.UtcNow;
                izinTalebi.OnayNotu = request.OnayNotu;
                izinTalebi.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Bildirim gönder
                var reddeden = await _context.Personeller.FindAsync(request.OnaylayanId);
                var bildirim = new Bildirim
                {
                    AliciId = izinTalebi.PersonelId,
                    Baslik = "İzin Talebiniz Reddedildi",
                    Mesaj = $"{izinTalebi.IzinBaslamaTarihi:dd.MM.yyyy} - {izinTalebi.IsbasiTarihi:dd.MM.yyyy} tarihleri arası {izinTalebi.GunSayisi} gün süresince {izinTalebi.IzinTipi} talebiniz reddedilmiştir. {(string.IsNullOrEmpty(request.OnayNotu) ? "" : $"Açıklama: {request.OnayNotu}")}",
                    Kategori = "izin",
                    Tip = "error",
                    GonderenAd = reddeden != null ? $"{reddeden.Ad} {reddeden.Soyad}" : "Sistem",
                    ActionUrl = "/izin-talepleri"
                };
                await _bildirimService.CreateBildirimAsync(bildirim);

                return Ok(new { success = true, message = "İzin talebi reddedildi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "İzin talebi reddedilirken bir hata oluştu.", error = ex.Message });
            }
        }

        // DELETE: api/IzinTalebi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIzinTalebi(int id)
        {
            try
            {
                var izinTalebi = await _context.IzinTalepleri.FindAsync(id);
                if (izinTalebi == null)
                {
                    return NotFound(new { success = false, message = "İzin talebi bulunamadı." });
                }

                // Sadece beklemedeki izinler silinebilir
                if (izinTalebi.Durum != "Beklemede")
                {
                    return BadRequest(new { success = false, message = "Sadece beklemedeki izin talepleri silinebilir." });
                }

                _context.IzinTalepleri.Remove(izinTalebi);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "İzin talebi başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "İzin talebi silinirken bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/IzinTalebi/Takvim
        [HttpGet("Takvim")]
        public async Task<ActionResult<IEnumerable<object>>> GetIzinTakvimi(int? departmanId = null, int? kullaniciId = null)
        {
            try
            {
                var query = _context.IzinTalepleri
                    .Include(i => i.Personel)
                        .ThenInclude(p => p.Pozisyon)
                            .ThenInclude(pos => pos.Departman)
                    .Include(i => i.Personel)
                        .ThenInclude(p => p.Pozisyon)
                            .ThenInclude(pos => pos.Kademe)
                    .Where(i => i.Durum == "Onaylandı")
                    .AsQueryable();

                // Departman filtreleme
                if (departmanId.HasValue)
                {
                    query = query.Where(i => i.Personel.Pozisyon.DepartmanId == departmanId.Value);
                }

                // Kullanıcı tabanlı hiyerarşik yetkilendirme
                if (kullaniciId.HasValue)
                {
                    // Kullanıcının personel bilgilerini ve kademe seviyesini al
                    var kullanici = await _context.Kullanicilar
                        .Include(k => k.Personel)
                            .ThenInclude(p => p.Pozisyon)
                                .ThenInclude(pos => pos.Kademe)
                        .FirstOrDefaultAsync(k => k.Id == kullaniciId.Value);

                    if (kullanici == null || kullanici.Personel == null)
                    {
                        return BadRequest(new { success = false, message = "Kullanıcı bilgileri bulunamadı." });
                    }

                    var kademeSeviye = kullanici.Personel.Pozisyon?.Kademe?.Seviye ?? 999;
                    var personelId = kullanici.Personel.Id;

                    Console.WriteLine($"İzin Takvimi Yetki Kontrolü - Kullanıcı ID: {kullaniciId}, Personel ID: {personelId}, Kademe Seviye: {kademeSeviye}");

                    // Hiyerarşik yetkilendirme mantığı
                    if (kademeSeviye == 1 || kademeSeviye == 2) 
                    {
                        // Genel Müdür (seviye 1) ve Direktör (seviye 2): Herkesi görebilir
                        Console.WriteLine("İzin Takvimi: Üst düzey yönetici - Tüm izinler görülebilir");
                        // Query zaten tüm kayıtları getiriyor, ek filtre gerekmez
                    }
                    else if (kademeSeviye >= 3 && kademeSeviye <= 6) 
                    {
                        // Grup Müdürü (3), Müdür (4), Yönetici (5), Sorumlu (6): Kendisi + direkt/endirekt bağlı personeller
                        Console.WriteLine("İzin Takvimi: Orta düzey yönetici - Hiyerarşik yetki kontrolü başlıyor");
                        
                        var gorebilecegiPersoneller = new List<int> { personelId }; // Kendi izinlerini de görebilir

                        // Tüm aktif personelleri getir
                        var tumPersoneller = await _context.Personeller
                            .Include(p => p.Pozisyon)
                                .ThenInclude(pos => pos.Kademe)
                            .Where(p => p.Aktif)
                            .ToListAsync();

                        foreach (var personel in tumPersoneller)
                        {
                            if (personel.Id == personelId) continue; // Kendisini zaten ekledi

                            // Bu personelin izin taleplerini onaylayabilir mi kontrol et
                            var canApprove = await _izinService.CanPersonelApproveIzin(personelId, personel.Id);
                            if (canApprove)
                            {
                                gorebilecegiPersoneller.Add(personel.Id);
                                Console.WriteLine($"İzin Takvimi: Yetkili personel eklendi - {personel.Ad} {personel.Soyad} (ID: {personel.Id})");
                            }
                        }

                        Console.WriteLine($"İzin Takvimi: Toplam görülebilecek personel sayısı: {gorebilecegiPersoneller.Count}");
                        query = query.Where(i => gorebilecegiPersoneller.Contains(i.PersonelId));
                    }
                    else 
                    {
                        // Diğer kademeler (7+): Sadece kendi izinlerini görebilir
                        Console.WriteLine($"İzin Takvimi: Alt kademe (seviye {kademeSeviye}) - Sadece kendi izinler");
                        query = query.Where(i => i.PersonelId == personelId);
                    }
                }

                var rawData = await query
                    .Include(i => i.Onaylayan)
                    .Select(i => new
                    {
                        id = i.Id,
                        title = i.Personel.Ad + " " + i.Personel.Soyad + " - " + i.IzinTipi,
                        izinBaslamaTarihi = i.IzinBaslamaTarihi,
                        isbasiTarihi = i.IsbasiTarihi,
                        color = i.IzinTipi == "Yıllık İzin" ? "#4CAF50" : 
                               i.IzinTipi == "Mazeret İzni" ? "#FF9800" :
                               i.IzinTipi == "Hastalık İzni" ? "#F44336" :
                               i.IzinTipi == "Doğum İzni" ? "#E91E63" : "#9E9E9E",
                        personelAd = i.Personel.Ad + " " + i.Personel.Soyad,
                        personelPozisyon = i.Personel.Pozisyon != null ? i.Personel.Pozisyon.Ad : "Pozisyon Tanımsız",
                        personelDepartman = i.Personel.Pozisyon != null && i.Personel.Pozisyon.Departman != null ? i.Personel.Pozisyon.Departman.Ad : "Departman Tanımsız",
                        personelKademe = i.Personel.Pozisyon != null && i.Personel.Pozisyon.Kademe != null ? i.Personel.Pozisyon.Kademe.Ad : "Kademe Tanımsız",
                        kademeSeviye = i.Personel.Pozisyon != null && i.Personel.Pozisyon.Kademe != null ? i.Personel.Pozisyon.Kademe.Seviye : 999,
                        izinTipi = i.IzinTipi,
                        gunSayisi = i.GunSayisi,
                        aciklama = i.Aciklama,
                        onaylayanAd = i.Onaylayan != null ? i.Onaylayan.Ad + " " + i.Onaylayan.Soyad : "-",
                        durum = i.Durum
                    })
                    .OrderBy(i => i.izinBaslamaTarihi)
                    .ThenBy(i => i.personelAd)
                    .ToListAsync();

                // Client-side formatting (after database query) - Send as UTC ISO string
                var izinTakvimi = rawData.Select(i => new
                {
                    id = i.id,
                    title = i.title,
                    start = DateTime.SpecifyKind(i.izinBaslamaTarihi, DateTimeKind.Utc).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    end = DateTime.SpecifyKind(i.isbasiTarihi, DateTimeKind.Utc).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    color = i.color,
                    personelAd = i.personelAd,
                    personelPozisyon = i.personelPozisyon,
                    personelDepartman = i.personelDepartman,
                    personelKademe = i.personelKademe,
                    kademeSeviye = i.kademeSeviye,
                    izinTipi = i.izinTipi,
                    gunSayisi = i.gunSayisi,
                    aciklama = i.aciklama,
                    onaylayanAd = i.onaylayanAd,
                    durum = i.durum
                }).ToList();

                Console.WriteLine($"İzin Takvimi: Toplam {izinTakvimi.Count} izin döndürülüyor");
                
                // DEBUG: İlk kaydın tarih bilgilerini logla
                if (izinTakvimi.Count > 0)
                {
                    var firstRecord = izinTakvimi.First();
                    Console.WriteLine($"DEBUG - İlk kayıt ID: {firstRecord.id}");
                    Console.WriteLine($"DEBUG - DB'den gelen raw başlangıç: {rawData.First().izinBaslamaTarihi}");
                    Console.WriteLine($"DEBUG - DB'den gelen raw işbaşı: {rawData.First().isbasiTarihi}");
                    Console.WriteLine($"DEBUG - API'nin döndürdüğü başlangıç: {firstRecord.start}");
                    Console.WriteLine($"DEBUG - API'nin döndürdüğü işbaşı: {firstRecord.end}");
                }
                
                return Ok(new { success = true, data = izinTakvimi, message = "İzin takvimi başarıyla getirildi." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"İzin Takvimi Hata: {ex.Message}");
                return StatusCode(500, new { success = false, message = "İzin takvimi getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/IzinTalebi/IstatistikDashboard
        [HttpGet("IstatistikDashboard")]
        public async Task<ActionResult<object>> GetIzinIstatistikleri(int? personelId = null)
        {
            try
            {
                var currentYear = DateTime.Now.Year;

                var query = _context.IzinTalepleri
                    .Include(i => i.Personel)
                    .AsQueryable();

                if (personelId.HasValue)
                {
                    query = query.Where(i => i.PersonelId == personelId.Value);
                }

                var istatistikler = new
                {
                    ToplamTalep = await query.CountAsync(),
                    BekleyenTalep = await query.CountAsync(i => i.Durum == "Beklemede"),
                    OnaylananTalep = await query.CountAsync(i => i.Durum == "Onaylandı"),
                    ReddedilenTalep = await query.CountAsync(i => i.Durum == "Reddedildi"),
                    
                    BuYilToplam = await query.CountAsync(i => i.IzinBaslamaTarihi.Year == currentYear),
                    BuYilOnaylanan = await query.CountAsync(i => i.IzinBaslamaTarihi.Year == currentYear && i.Durum == "Onaylandı"),
                    
                    BuAyToplam = await query.CountAsync(i => i.IzinBaslamaTarihi.Year == currentYear && i.IzinBaslamaTarihi.Month == DateTime.Now.Month),
                    BuAyOnaylanan = await query.CountAsync(i => i.IzinBaslamaTarihi.Year == currentYear && i.IzinBaslamaTarihi.Month == DateTime.Now.Month && i.Durum == "Onaylandı"),
                    
                    ToplamIzinGunu = await query.Where(i => i.Durum == "Onaylandı").SumAsync(i => i.GunSayisi)
                };

                return Ok(new { success = true, data = istatistikler, message = "İzin istatistikleri başarıyla getirildi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "İzin istatistikleri getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/IzinTalebi/BekleyenTalepler
        [HttpGet("BekleyenTalepler")]
        public async Task<ActionResult<IEnumerable<object>>> GetBekleyenIzinTalepleri(int onaylayanId)
        {
            try
            {
                // Onaylayan kişinin bilgilerini al
                var onaylayan = await _context.Personeller
                    .Include(p => p.Pozisyon)
                        .ThenInclude(pos => pos.Kademe)
                    .Include(p => p.Pozisyon)
                        .ThenInclude(pos => pos.Departman)
                    .FirstOrDefaultAsync(p => p.Id == onaylayanId);

                if (onaylayan == null)
                {
                    return BadRequest(new { success = false, message = "Geçersiz onaylayan ID." });
                }

                var query = _context.IzinTalepleri
                    .Include(i => i.Personel)
                        .ThenInclude(p => p.Pozisyon)
                            .ThenInclude(pos => pos.Departman)
                    .Include(i => i.Personel)
                        .ThenInclude(p => p.Pozisyon)
                            .ThenInclude(pos => pos.Kademe)
                    .Include(i => i.Onaylayan)
                    .Where(i => i.Durum == "Beklemede") // Sadece bekleyen talepler
                    .AsQueryable();

                // Yetki kontrolü - Kademe seviyesine göre filtreleme
                var kademeSeviye = onaylayan.Pozisyon.Kademe.Seviye;
                var departmanId = onaylayan.Pozisyon.DepartmanId;

                if (kademeSeviye == 1) // Genel Müdür - Tüm talepleri görür
                {
                    // Filtreleme yok - tüm bekleyen talepleri getir
                }
                else if (kademeSeviye == 2) // Genel Müdür Yardımcısı - Kendi departmanındaki tüm talepleri görür
                {
                    query = query.Where(i => i.Personel.Pozisyon.DepartmanId == departmanId);
                }
                else if (kademeSeviye >= 3 && kademeSeviye <= 6) // Diğer kademeler - Sadece alt personellerin talepleri
                {
                    // Bu personelin altındaki personelleri bul (yonetici_id ile)
                    var altPersonelIds = await _context.Personeller
                        .Where(p => p.YoneticiId == onaylayanId && p.Aktif)
                        .Select(p => p.Id)
                        .ToListAsync();

                    query = query.Where(i => altPersonelIds.Contains(i.PersonelId));
                }
                else
                {
                    // 7 ve üzeri seviye (Uzman, vb.) - İzin onaylama yetkisi yok
                    return Ok(new { success = true, data = new List<object>(), message = "Bu kademe seviyesinde izin onaylama yetkiniz bulunmamaktadır." });
                }

                var bekleyenTalepler = await query
                    .OrderByDescending(i => i.CreatedAt)
                    .Select(i => new
                    {
                        i.Id,
                        i.PersonelId,
                        PersonelAd = i.Personel.Ad + " " + i.Personel.Soyad,
                        PersonelPozisyon = i.Personel.Pozisyon.Ad,
                        PersonelDepartman = i.Personel.Pozisyon.Departman.Ad,
                        PersonelKademe = i.Personel.Pozisyon.Kademe.Ad,
                        PersonelFotograf = i.Personel.FotografUrl,
                        IzinBaslamaTarihi = i.IzinBaslamaTarihi,
                        IsbasiTarihi = i.IsbasiTarihi,
                        i.GunSayisi,
                        i.IzinTipi,
                        i.Aciklama,
                        i.GorevYeri,
                        i.RaporDosyaYolu,
                        i.Durum,
                        i.CreatedAt,
                        i.UpdatedAt
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = bekleyenTalepler, message = $"{bekleyenTalepler.Count} bekleyen izin talebi listelendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Bekleyen izin talepleri listelenirken bir hata oluştu.", error = ex.Message });
            }
        }

        // POST: api/IzinTalebi/{id}/UploadRapor
        [HttpPost("{id}/UploadRapor")]
        public async Task<IActionResult> UploadRapor(int id, IFormFile file)
        {
            try
            {
                var izinTalebi = await _context.IzinTalepleri.FindAsync(id);
                if (izinTalebi == null)
                {
                    return NotFound(new { success = false, message = "İzin talebi bulunamadı." });
                }

                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { success = false, message = "Dosya seçilmedi." });
                }

                // Dosya uzantısı kontrolü
                var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(new { success = false, message = "Geçersiz dosya formatı. İzin verilen: PDF, JPG, PNG, DOC, DOCX" });
                }

                // Dosya boyutu kontrolü (10MB)
                if (file.Length > 10 * 1024 * 1024)
                {
                    return BadRequest(new { success = false, message = "Dosya boyutu 10MB'dan küçük olmalıdır." });
                }

                // Klasör oluştur
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "izin-raporlari");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Benzersiz dosya adı oluştur
                var fileName = $"{id}_{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Eski dosyayı sil (varsa)
                if (!string.IsNullOrEmpty(izinTalebi.RaporDosyaYolu))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", izinTalebi.RaporDosyaYolu.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Dosyayı kaydet
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Veritabanını güncelle
                izinTalebi.RaporDosyaYolu = $"/uploads/izin-raporlari/{fileName}";
                izinTalebi.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new { success = true, data = new { raporDosyaYolu = izinTalebi.RaporDosyaYolu }, message = "Rapor başarıyla yüklendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Rapor yüklenirken bir hata oluştu.", error = ex.Message });
            }
        }

        private bool IzinTalebiExists(int id)
        {
            return _context.IzinTalepleri.Any(e => e.Id == id);
        }
    }

    public class OnayRequest
    {
        public int OnaylayanId { get; set; }
        public string? OnayNotu { get; set; }
    }

    public class IzinTakvimiDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Start { get; set; } = string.Empty;
        public string End { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string PersonelAd { get; set; } = string.Empty;
        public string PersonelPozisyon { get; set; } = string.Empty;
        public string PersonelDepartman { get; set; } = string.Empty;
        public string IzinTipi { get; set; } = string.Empty;
        public int GunSayisi { get; set; }
        public string? Aciklama { get; set; }
    }
}