using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IconIK.API.Data;
using IconIK.API.Models;
using IconIK.API.Services;
using System.Text.Json;

namespace IconIK.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonelController : ControllerBase
    {
        private readonly IconIKContext _context;
        private readonly IUserService _userService;

        public PersonelController(IconIKContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        // GET: api/Personel
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetPersoneller()
        {
            try
            {
                var personellerRaw = await _context.Personeller
                    .Include(p => p.Pozisyon)
                        .ThenInclude(pos => pos.Departman)
                    .Include(p => p.Pozisyon)
                        .ThenInclude(pos => pos.Kademe)
                    .Include(p => p.Yonetici)
                    .Include(p => p.Kullanici)
                    .Where(p => p.Aktif)
                    .OrderBy(p => p.Ad)
                    .ThenBy(p => p.Soyad)
                    .ToListAsync();

                // DEBUG: İlk personelin kullanıcı adını kontrol et
                if (personellerRaw.Count > 0)
                {
                    var firstPersonel = personellerRaw.First();
                    Console.WriteLine($"DEBUG Personel GET - First personel ID: {firstPersonel.Id}");
                    Console.WriteLine($"DEBUG Personel GET - Kullanici: {(firstPersonel.Kullanici != null ? "Not null" : "NULL")}");
                    Console.WriteLine($"DEBUG Personel GET - KullaniciAdi: {(firstPersonel.Kullanici?.KullaniciAdi ?? "NULL")}");
                }
                
                // Her personel için kullanılan izinleri hesapla
                var personelIds = personellerRaw.Select(p => p.Id).ToList();
                var buYil = DateTime.UtcNow.Year;
                
                Console.WriteLine($"DEBUG: Personel sayısı: {personelIds.Count}, Bu yıl: {buYil}");
                
                var kullanilanIzinler = await _context.IzinTalepleri
                    .Where(i => personelIds.Contains(i.PersonelId) && 
                               i.Durum == "Onaylandı" &&
                               i.IzinBaslamaTarihi.Year == buYil)
                    .GroupBy(i => i.PersonelId)
                    .Select(g => new { 
                        PersonelId = g.Key, 
                        KullanilanGun = g.Sum(i => i.GunSayisi) 
                    })
                    .ToDictionaryAsync(x => x.PersonelId, x => x.KullanilanGun);
                    
                Console.WriteLine($"DEBUG: Kullanılan izin sayısı: {kullanilanIzinler.Count}");

                var personeller = personellerRaw.Select(p => {
                    var toplamHak = CalculateYillikIzinHakki(p.IseBaslamaTarihi, DateTime.UtcNow.Date);
                    var kullanilanIzin = kullanilanIzinler.ContainsKey(p.Id) ? kullanilanIzinler[p.Id] : 0;
                    var kalanIzin = toplamHak - kullanilanIzin;
                    
                    return new
                    {
                        p.Id,
                        p.TcKimlik,
                        KullaniciAdi = p.Kullanici != null ? p.Kullanici.KullaniciAdi : null,
                        p.Ad,
                        p.Soyad,
                        AdSoyad = p.Ad + " " + p.Soyad,
                        p.Email,
                        p.Telefon,
                        p.DogumTarihi,
                        p.IseBaslamaTarihi,
                        p.CikisTarihi,
                        p.PozisyonId,
                        PozisyonAd = p.Pozisyon.Ad,
                        DepartmanAd = p.Pozisyon.Departman.Ad,
                        KademeAd = p.Pozisyon.Kademe.Ad,
                        KademeSeviye = p.Pozisyon.Kademe.Seviye,
                        p.YoneticiId,
                        YoneticiAd = p.Yonetici != null ? p.Yonetici.Ad + " " + p.Yonetici.Soyad : null,
                        p.Maas,
                        p.FotografUrl,
                        p.Adres,
                        p.MedeniHal,
                        p.Cinsiyet,
                        p.AskerlikDurumu,
                        p.EgitimDurumu,
                        p.KanGrubu,
                        p.EhliyetSinifi,
                        p.AnneAdi,
                        p.BabaAdi,
                        p.DogumYeri,
                        p.NufusIlKod,
                        p.NufusIlceKod,
                        p.AcilDurumIletisim,
                        p.BankaHesapNo,
                        p.IbanNo,
                        p.Aktif,
                        p.CreatedAt,
                        p.UpdatedAt,
                        // Kalan izin hesaplaması
                        ToplamIzinHakki = toplamHak,
                        KullanilanIzin = kullanilanIzin,
                        KalanIzinHakki = kalanIzin
                    };
                }).ToList();

                return Ok(new { success = true, data = personeller, message = "Personeller başarıyla listelendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Personeller listelenirken bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/Personel/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<object>> GetPersonel(int id)
        {
            try
            {
                var personelRaw = await _context.Personeller
                    .Include(p => p.Pozisyon)
                        .ThenInclude(pos => pos.Departman)
                    .Include(p => p.Pozisyon)
                        .ThenInclude(pos => pos.Kademe)
                    .Include(p => p.Yonetici)
                    .Include(p => p.Kullanici)
                    .Where(p => p.Id == id && p.Aktif)
                    .FirstOrDefaultAsync();

                if (personelRaw == null)
                {
                    return NotFound(new { success = false, message = "Personel bulunamadı." });
                }

                // İzin hesaplamaları
                var buYil = DateTime.UtcNow.Year;
                var toplamHak = CalculateYillikIzinHakki(personelRaw.IseBaslamaTarihi, DateTime.UtcNow.Date);
                var kullanilanIzin = await _context.IzinTalepleri
                    .Where(i => i.PersonelId == id && 
                               i.Durum == "Onaylandı" &&
                               i.IzinBaslamaTarihi.Year == buYil)
                    .SumAsync(i => i.GunSayisi);
                var kalanIzin = toplamHak - kullanilanIzin;

                var personel = new
                {
                    personelRaw.Id,
                    personelRaw.TcKimlik,
                    personelRaw.Ad,
                    personelRaw.Soyad,
                    personelRaw.Email,
                    personelRaw.Telefon,
                    personelRaw.DogumTarihi,
                    personelRaw.IseBaslamaTarihi,
                    personelRaw.CikisTarihi,
                    personelRaw.PozisyonId,
                    PozisyonAd = personelRaw.Pozisyon.Ad,
                    DepartmanAd = personelRaw.Pozisyon.Departman.Ad,
                    KademeAd = personelRaw.Pozisyon.Kademe.Ad,
                    MinMaas = personelRaw.Pozisyon.MinMaas,
                    MaxMaas = personelRaw.Pozisyon.MaxMaas,
                    personelRaw.YoneticiId,
                    YoneticiAd = personelRaw.Yonetici != null ? personelRaw.Yonetici.Ad + " " + personelRaw.Yonetici.Soyad : null,
                    personelRaw.Maas,
                    personelRaw.FotografUrl,
                    personelRaw.Adres,
                    personelRaw.MedeniHal,
                    personelRaw.Cinsiyet,
                    personelRaw.AskerlikDurumu,
                    personelRaw.EgitimDurumu,
                    personelRaw.KanGrubu,
                    personelRaw.EhliyetSinifi,
                    personelRaw.AnneAdi,
                    personelRaw.BabaAdi,
                    personelRaw.DogumYeri,
                    personelRaw.NufusIlKod,
                    personelRaw.NufusIlceKod,
                    personelRaw.AcilDurumIletisim,
                    personelRaw.BankaHesapNo,
                    personelRaw.IbanNo,
                    personelRaw.Aktif,
                    personelRaw.CreatedAt,
                    personelRaw.UpdatedAt,
                    KullaniciAdi = personelRaw.Kullanici != null ? personelRaw.Kullanici.KullaniciAdi : null,
                    // Kalan izin hesaplaması
                    ToplamIzinHakki = toplamHak,
                    KullanilanIzin = kullanilanIzin,
                    KalanIzinHakki = kalanIzin,
                    // Eski field uyumluluk için
                    IzinHakki = kalanIzin
                };

                return Ok(new { success = true, data = personel, message = "Personel başarıyla getirildi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Personel getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/Personel/Yoneticiler
        [HttpGet("Yoneticiler")]
        public async Task<ActionResult<IEnumerable<object>>> GetYoneticiler()
        {
            try
            {
                var yoneticiler = await _context.Personeller
                    .Include(p => p.Pozisyon)
                        .ThenInclude(pos => pos.Kademe)
                    .Include(p => p.Pozisyon)
                        .ThenInclude(pos => pos.Departman)
                    .Where(p => p.Aktif && p.Pozisyon.Kademe.Seviye <= 6) // Yönetici seviyesi ve üstü
                    .OrderBy(p => p.Pozisyon.Kademe.Seviye)
                    .ThenBy(p => p.Ad)
                    .Select(p => new
                    {
                        p.Id,
                        AdSoyad = p.Ad + " " + p.Soyad,
                        PozisyonAd = p.Pozisyon.Ad,
                        KademeAd = p.Pozisyon.Kademe.Ad,
                        KademeSeviye = p.Pozisyon.Kademe.Seviye,
                        DepartmanId = p.Pozisyon.DepartmanId,
                        DepartmanAd = p.Pozisyon.Departman.Ad
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = yoneticiler, message = "Yöneticiler başarıyla listelendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Yöneticiler listelenirken bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/Personel/AktifListesi
        [HttpGet("AktifListesi")]
        public async Task<ActionResult<IEnumerable<object>>> GetAktifPersoneller()
        {
            try
            {
                var personeller = await _context.Personeller
                    .Include(p => p.Pozisyon)
                        .ThenInclude(pos => pos.Departman)
                    .Include(p => p.Pozisyon)
                        .ThenInclude(pos => pos.Kademe)
                    .Where(p => p.Aktif)
                    .OrderBy(p => p.Ad)
                    .ThenBy(p => p.Soyad)
                    .Select(p => new
                    {
                        p.Id,
                        p.Ad,
                        p.Soyad,
                        AdSoyad = p.Ad + " " + p.Soyad,
                        p.Email,
                        p.Telefon,
                        PozisyonAd = p.Pozisyon.Ad,
                        DepartmanAd = p.Pozisyon.Departman.Ad,
                        DepartmanId = p.Pozisyon.DepartmanId,
                        KademeAd = p.Pozisyon.Kademe.Ad,
                        KademeSeviye = p.Pozisyon.Kademe.Seviye
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = personeller });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Aktif personeller getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/Personel/Aktif (For VideoEgitim module)
        [HttpGet("Aktif")]
        public async Task<ActionResult<IEnumerable<object>>> GetAktifPersonellerForVideo()
        {
            try
            {
                var personeller = await _context.Personeller
                    .Include(p => p.Pozisyon)
                        .ThenInclude(pos => pos.Departman)
                    .Where(p => p.Aktif)
                    .OrderBy(p => p.Ad)
                    .ThenBy(p => p.Soyad)
                    .Select(p => new
                    {
                        p.Id,
                        p.Ad,
                        p.Soyad,
                        AdSoyad = p.Ad + " " + p.Soyad,
                        p.Email,
                        PozisyonAd = p.Pozisyon.Ad,
                        DepartmanAd = p.Pozisyon.Departman.Ad
                    })
                    .ToListAsync();
                
                return Ok(new { success = true, data = personeller });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Aktif personeller getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        // POST: api/Personel
        [HttpPost]
        public async Task<ActionResult<object>> PostPersonel([FromBody] System.Text.Json.JsonElement rawData)
        {
            Console.WriteLine("=== POST /api/Personel - METHOD CALLED ===");
            Console.WriteLine($"POST /api/Personel - Received raw data: {rawData}");
            
            // Manual parsing with camelCase options
            Personel personel;
            try
            {
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                };
                
                // Handle null id by replacing it with 0 in the raw JSON string
                var jsonString = rawData.GetRawText();
                jsonString = jsonString.Replace("\"id\":null", "\"id\":0");
                
                personel = System.Text.Json.JsonSerializer.Deserialize<Personel>(jsonString, options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JSON Parse Error: {ex.Message}");
                return BadRequest(new { success = false, message = "Invalid JSON format", error = ex.Message });
            }
            
            Console.WriteLine($"POST /api/Personel - Parsed personel: {System.Text.Json.JsonSerializer.Serialize(personel)}");

            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // TC Kimlik kontrolü
                if (await _context.Personeller.AnyAsync(p => p.TcKimlik == personel.TcKimlik && p.Aktif))
                {
                    return BadRequest(new { success = false, message = "Bu TC Kimlik numarasına sahip bir personel zaten mevcut." });
                }

                // Email kontrolü - boş string'leri null'a çevir
                if (string.IsNullOrWhiteSpace(personel.Email))
                {
                    personel.Email = null;
                }
                else
                {
                    // Email varsa ve kullanılıyorsa hata ver
                    if (await _context.Personeller.AnyAsync(p => p.Email == personel.Email && p.Aktif))
                    {
                        return BadRequest(new { success = false, message = "Bu email adresine sahip bir personel zaten mevcut." });
                    }
                }

                // Pozisyon var mı kontrol
                var pozisyon = await _context.Pozisyonlar
                    .Include(p => p.Departman)
                    .Include(p => p.Kademe)
                    .FirstOrDefaultAsync(p => p.Id == personel.PozisyonId && p.Aktif);

                if (pozisyon == null)
                {
                    return BadRequest(new { success = false, message = "Geçersiz pozisyon seçimi." });
                }

                // Yönetici kontrolü
                if (personel.YoneticiId.HasValue)
                {
                    var yonetici = await _context.Personeller
                        .Include(p => p.Pozisyon)
                            .ThenInclude(pos => pos.Kademe)
                        .FirstOrDefaultAsync(p => p.Id == personel.YoneticiId && p.Aktif);

                    if (yonetici == null)
                    {
                        return BadRequest(new { success = false, message = "Geçersiz yönetici seçimi." });
                    }

                    // Yönetici kademe kontrolü - Yönetici, personelden üst kademede olmalı
                    if (yonetici.Pozisyon.Kademe.Seviye >= pozisyon.Kademe.Seviye)
                    {
                        return BadRequest(new { success = false, message = "Yönetici, personelden üst kademede olmalıdır." });
                    }
                }

                personel.CreatedAt = DateTime.UtcNow;
                personel.UpdatedAt = DateTime.UtcNow;
                
                _context.Personeller.Add(personel);
                await _context.SaveChangesAsync();

                // Otomatik kullanıcı oluştur
                var kullanici = await _userService.CreateUserForPersonelAsync(personel);

                await transaction.CommitAsync();

                // Maaş uyarısı kontrolü
                var maasUyarisi = CheckMaasUyarisi(personel.Maas, pozisyon.MinMaas, pozisyon.MaxMaas);

                var result = new
                {
                    success = true,
                    data = new
                    {
                        personel.Id,
                        personel.Ad,
                        personel.Soyad,
                        KullaniciAdi = kullanici.KullaniciAdi,
                        DefaultSifre = personel.TcKimlik.Substring(personel.TcKimlik.Length - 4)
                    },
                    message = "Personel ve kullanıcı hesabı başarıyla oluşturuldu.",
                    maasUyarisi = maasUyarisi
                };

                return CreatedAtAction("GetPersonel", new { id = personel.Id }, result);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { success = false, message = "Personel oluşturulurken bir hata oluştu.", error = ex.Message });
            }
        }

        // PUT: api/Personel/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPersonel(int id, [FromBody] System.Text.Json.JsonElement rawData)
        {
            Console.WriteLine($"=== PUT /api/Personel/{id} - METHOD CALLED ===");
            Console.WriteLine($"PUT /api/Personel/{id} - Received raw data: {rawData}");
            
            // Manual parsing with camelCase options
            Personel personel;
            string kullaniciAdi = null;
            try
            {
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                };
                personel = System.Text.Json.JsonSerializer.Deserialize<Personel>(rawData, options);
                
                // Kullanıcı adını ayrıca çıkar
                if (rawData.TryGetProperty("kullaniciAdi", out var kullaniciAdiElement))
                {
                    kullaniciAdi = kullaniciAdiElement.GetString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JSON Parse Error: {ex.Message}");
                return BadRequest(new { success = false, message = "Invalid JSON format", error = ex.Message });
            }
            
            Console.WriteLine($"PUT /api/Personel/{id} - Parsed personel: {System.Text.Json.JsonSerializer.Serialize(personel)}");
            
            if (id != personel.Id)
            {
                return BadRequest(new { success = false, message = "Geçersiz personel ID." });
            }

            try
            {
                // Mevcut personeli getir
                var mevcutPersonel = await _context.Personeller.FindAsync(id);
                if (mevcutPersonel == null || !mevcutPersonel.Aktif)
                {
                    return NotFound(new { success = false, message = "Personel bulunamadı." });
                }

                // TC Kimlik kontrolü (kendisi hariç)
                if (await _context.Personeller.AnyAsync(p => p.TcKimlik == personel.TcKimlik && p.Id != id && p.Aktif))
                {
                    return BadRequest(new { success = false, message = "Bu TC Kimlik numarasına sahip bir personel zaten mevcut." });
                }

                // Email kontrolü - boş string'leri null'a çevir (kendisi hariç)
                if (string.IsNullOrWhiteSpace(personel.Email))
                {
                    personel.Email = null;
                }
                else
                {
                    // Email varsa ve başkası kullanıyorsa hata ver
                    if (await _context.Personeller.AnyAsync(p => p.Email == personel.Email && p.Id != id && p.Aktif))
                    {
                        return BadRequest(new { success = false, message = "Bu email adresine sahip bir personel zaten mevcut." });
                    }
                }

                // Pozisyon kontrolü
                var pozisyon = await _context.Pozisyonlar.FirstOrDefaultAsync(p => p.Id == personel.PozisyonId && p.Aktif);
                if (pozisyon == null)
                {
                    return BadRequest(new { success = false, message = "Geçersiz pozisyon seçimi." });
                }

                // Yönetici kontrolü
                if (personel.YoneticiId.HasValue)
                {
                    var yonetici = await _context.Personeller
                        .Include(p => p.Pozisyon)
                            .ThenInclude(pos => pos.Kademe)
                        .FirstOrDefaultAsync(p => p.Id == personel.YoneticiId && p.Aktif);

                    if (yonetici == null)
                    {
                        return BadRequest(new { success = false, message = "Geçersiz yönetici seçimi." });
                    }
                }

                // Mevcut personeli güncelle
                mevcutPersonel.TcKimlik = personel.TcKimlik;
                mevcutPersonel.Ad = personel.Ad;
                mevcutPersonel.Soyad = personel.Soyad;
                mevcutPersonel.Email = personel.Email;
                mevcutPersonel.Telefon = personel.Telefon;
                mevcutPersonel.DogumTarihi = personel.DogumTarihi;
                mevcutPersonel.IseBaslamaTarihi = personel.IseBaslamaTarihi;
                mevcutPersonel.CikisTarihi = personel.CikisTarihi;
                mevcutPersonel.PozisyonId = personel.PozisyonId;
                mevcutPersonel.YoneticiId = personel.YoneticiId;
                mevcutPersonel.Maas = personel.Maas;
                mevcutPersonel.FotografUrl = personel.FotografUrl;
                mevcutPersonel.Adres = personel.Adres;
                mevcutPersonel.Aktif = personel.Aktif;
                mevcutPersonel.UpdatedAt = DateTime.UtcNow;

                // Kullanıcı adı güncelleme
                if (!string.IsNullOrWhiteSpace(kullaniciAdi))
                {
                    // Kullanıcı adı validasyonu
                    if (!System.Text.RegularExpressions.Regex.IsMatch(kullaniciAdi, @"^[a-zA-Z0-9._]+$"))
                    {
                        return BadRequest(new { success = false, message = "Kullanıcı adı sadece İngilizce harfler, sayılar, nokta (.) ve alt çizgi (_) içerebilir." });
                    }

                    // Kullanıcı adı benzersizlik kontrolü
                    var existingUser = await _context.Kullanicilar.FirstOrDefaultAsync(k => k.KullaniciAdi.ToLower() == kullaniciAdi.ToLower() && k.PersonelId != id);
                    if (existingUser != null)
                    {
                        return BadRequest(new { success = false, message = "Bu kullanıcı adı zaten kullanılıyor." });
                    }

                    // Mevcut kullanıcı kaydını güncelle
                    var kullanici = await _context.Kullanicilar.FirstOrDefaultAsync(k => k.PersonelId == id);
                    if (kullanici != null)
                    {
                        kullanici.KullaniciAdi = kullaniciAdi;
                        kullanici.UpdatedAt = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync();

                // Maaş uyarısı kontrolü
                var maasUyarisi = CheckMaasUyarisi(personel.Maas, pozisyon.MinMaas, pozisyon.MaxMaas);

                var result = new
                {
                    success = true,
                    message = "Personel başarıyla güncellendi.",
                    maasUyarisi = maasUyarisi
                };

                return Ok(result);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PersonelExists(id))
                {
                    return NotFound(new { success = false, message = "Personel bulunamadı." });
                }
                throw;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Personel güncellenirken bir hata oluştu.", error = ex.Message });
            }
        }

        // DELETE: api/Personel/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePersonel(int id)
        {
            try
            {
                var personel = await _context.Personeller
                    .Include(p => p.AltCalisanlar)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (personel == null)
                {
                    return NotFound(new { success = false, message = "Personel bulunamadı." });
                }

                // Alt çalışanları var mı kontrol et
                var altCalisanSayisi = personel.AltCalisanlar.Count(ac => ac.Aktif);
                if (altCalisanSayisi > 0)
                {
                    return BadRequest(new { success = false, message = $"Bu personelin {altCalisanSayisi} adet alt çalışanı bulunmaktadır. Önce alt çalışanların yöneticisini değiştiriniz." });
                }

                // Soft delete
                personel.Aktif = false;
                personel.UpdatedAt = DateTime.UtcNow;

                // Kullanıcı hesabını da pasif yap
                var kullanici = await _context.Kullanicilar.FirstOrDefaultAsync(k => k.PersonelId == id);
                if (kullanici != null)
                {
                    kullanici.Aktif = false;
                    kullanici.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Personel başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Personel silinirken bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/Personel/MaasKontrolu/{pozisyonId}
        [HttpGet("MaasKontrolu/{pozisyonId:int}")]
        public async Task<ActionResult<object>> GetMaasKontroluBilgi(int pozisyonId)
        {
            try
            {
                var pozisyon = await _context.Pozisyonlar
                    .Where(p => p.Id == pozisyonId && p.Aktif)
                    .Select(p => new
                    {
                        p.Id,
                        p.Ad,
                        p.MinMaas,
                        p.MaxMaas
                    })
                    .FirstOrDefaultAsync();

                if (pozisyon == null)
                {
                    return NotFound(new { success = false, message = "Pozisyon bulunamadı." });
                }

                return Ok(new { success = true, data = pozisyon, message = "Pozisyon maaş bilgisi başarıyla getirildi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Maaş kontrolü getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        private bool PersonelExists(int id)
        {
            return _context.Personeller.Any(e => e.Id == id && e.Aktif);
        }

        private string? CheckMaasUyarisi(decimal? maas, decimal? minMaas, decimal? maxMaas)
        {
            if (!maas.HasValue) return null;

            if (minMaas.HasValue && maas < minMaas)
            {
                return $"Uyarı: Girilen maaş (₺{maas:N2}), pozisyonun minimum maaşından (₺{minMaas:N2}) düşüktür.";
            }

            if (maxMaas.HasValue && maas > maxMaas)
            {
                return $"Uyarı: Girilen maaş (₺{maas:N2}), pozisyonun maksimum maaşından (₺{maxMaas:N2}) yüksektir.";
            }

            return null;
        }

        private static int CalculateYillikIzinHakki(DateTime iseBaslamaTarihi, DateTime bugun)
        {
            // İşe başlama tarihinden itibaren geçen tam yıl sayısı
            var calismaYili = bugun.Year - iseBaslamaTarihi.Year;
            
            // Eğer bu yıl henüz işe başlama ayı geçmediyse bir yıl eksilir
            if (bugun.Month < iseBaslamaTarihi.Month || 
                (bugun.Month == iseBaslamaTarihi.Month && bugun.Day < iseBaslamaTarihi.Day))
            {
                calismaYili--;
            }

            // Her yıl 14 gün izin hakkı (minimum 0)
            return Math.Max(0, calismaYili * 14);
        }
    }
}