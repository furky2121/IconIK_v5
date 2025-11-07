using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IconIK.API.Data;
using IconIK.API.Models;
using IconIK.API.Services;
using System.Security.Cryptography;
using System.Text;

namespace IconIK.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfilController : ControllerBase
    {
        private readonly IconIKContext _context;
        private readonly IUserService _userService;

        public ProfilController(IconIKContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        // GET: api/Profil/5
        [HttpGet("{kullaniciId}")]
        public async Task<ActionResult<object>> GetKullaniciProfil(int kullaniciId)
        {
            try
            {
                var kullanici = await _context.Kullanicilar
                    .Include(k => k.Personel)
                        .ThenInclude(p => p.Pozisyon)
                            .ThenInclude(pos => pos.Departman)
                    .Include(k => k.Personel)
                        .ThenInclude(p => p.Pozisyon)
                            .ThenInclude(pos => pos.Kademe)
                    .Include(k => k.Personel)
                        .ThenInclude(p => p.Yonetici)
                    .Where(k => k.Id == kullaniciId)
                    .Select(k => new
                    {
                        KullaniciId = k.Id,
                        k.KullaniciAdi,
                        k.Aktif,
                        SonGiris = k.SonGirisTarihi,
                        PersonelBilgileri = new
                        {
                            k.Personel.Id,
                            k.Personel.Ad,
                            k.Personel.Soyad,
                            k.Personel.Email,
                            k.Personel.Telefon,
                            k.Personel.DogumTarihi,
                            k.Personel.IseBaslamaTarihi,
                            k.Personel.Maas,
                            k.Personel.FotografUrl,
                            k.Personel.Adres,
                            DepartmanAd = k.Personel.Pozisyon.Departman.Ad,
                            KademeAd = k.Personel.Pozisyon.Kademe.Ad,
                            PozisyonAd = k.Personel.Pozisyon.Ad,
                            YoneticiAd = k.Personel.Yonetici != null ? k.Personel.Yonetici.Ad + " " + k.Personel.Yonetici.Soyad : null
                        }
                    })
                    .FirstOrDefaultAsync();

                if (kullanici == null)
                {
                    return NotFound(new { success = false, message = "Kullanıcı bulunamadı." });
                }

                return Ok(new { success = true, data = kullanici, message = "Kullanıcı profili başarıyla getirildi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Kullanıcı profili getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/Profil/PersonelOzet/5
        [HttpGet("PersonelOzet/{kullaniciId}")]
        public async Task<ActionResult<object>> GetPersonelOzet(int kullaniciId)
        {
            try
            {
                var kullanici = await _context.Kullanicilar
                    .Include(k => k.Personel)
                    .FirstOrDefaultAsync(k => k.Id == kullaniciId);

                if (kullanici == null)
                {
                    return NotFound(new { success = false, message = "Kullanıcı bulunamadı." });
                }

                var personelId = kullanici.PersonelId;

                // İzin özeti
                var izinOzeti = await _context.IzinTalepleri
                    .Where(i => i.PersonelId == personelId)
                    .GroupBy(i => 1)
                    .Select(g => new
                    {
                        ToplamIzinTalebi = g.Count(),
                        OnaylananIzin = g.Where(i => i.Durum == "Onaylandı").Count(),
                        BekleyenIzin = g.Where(i => i.Durum == "Beklemede").Count(),
                        KullanilanIzinGunu = g.Where(i => i.Durum == "Onaylandı").Sum(i => i.GunSayisi)
                    })
                    .FirstOrDefaultAsync();

                // Eğitim özeti
                var egitimOzeti = await _context.PersonelEgitimleri
                    .Include(pe => pe.Egitim)
                    .Where(pe => pe.PersonelId == personelId)
                    .GroupBy(pe => 1)
                    .Select(g => new
                    {
                        ToplamEgitim = g.Count(),
                        TamamlananEgitim = g.Where(pe => pe.KatilimDurumu == "Tamamladı").Count(),
                        DevamEdenEgitim = g.Where(pe => pe.KatilimDurumu == "Katılıyor").Count(),
                        OrtalamaPuan = g.Where(pe => pe.Puan.HasValue).Average(pe => (double?)pe.Puan) ?? 0,
                        SertifikaliEgitim = g.Where(pe => !string.IsNullOrEmpty(pe.SertifikaUrl)).Count(),
                        ToplamEgitimSaati = g.Where(pe => pe.Egitim.SureSaat.HasValue).Sum(pe => pe.Egitim.SureSaat!.Value)
                    })
                    .FirstOrDefaultAsync();

                // Son bordro bilgisi - Bordro modülü kaldırıldı
                var sonBordro = (object?)null;

                // Yaklaşan eğitimler
                var bugun = DateTime.UtcNow.Date;
                var yaklaşanEgitimler = await _context.PersonelEgitimleri
                    .Include(pe => pe.Egitim)
                    .Where(pe => pe.PersonelId == personelId && 
                                pe.Egitim.BaslangicTarihi >= bugun &&
                                pe.Egitim.Durum == "Planlandı")
                    .OrderBy(pe => pe.Egitim.BaslangicTarihi)
                    .Take(5)
                    .Select(pe => new
                    {
                        pe.Egitim.Id,
                        pe.Egitim.Baslik,
                        pe.Egitim.BaslangicTarihi,
                        pe.Egitim.SureSaat,
                        pe.Egitim.Konum
                    })
                    .ToListAsync();

                // Alt çalışanlar (eğer yönetici ise)
                var altCalisanSayisi = await _context.Personeller
                    .Where(p => p.YoneticiId == personelId && p.Aktif)
                    .CountAsync();

                var personelOzet = new
                {
                    IzinOzeti = izinOzeti ?? new
                    {
                        ToplamIzinTalebi = 0,
                        OnaylananIzin = 0,
                        BekleyenIzin = 0,
                        KullanilanIzinGunu = 0
                    },
                    EgitimOzeti = egitimOzeti ?? new
                    {
                        ToplamEgitim = 0,
                        TamamlananEgitim = 0,
                        DevamEdenEgitim = 0,
                        OrtalamaPuan = 0.0,
                        SertifikaliEgitim = 0,
                        ToplamEgitimSaati = 0
                    },
                    SonBordro = sonBordro,
                    YaklaşanEgitimler = yaklaşanEgitimler,
                    AltCalisanSayisi = altCalisanSayisi
                };

                return Ok(new { success = true, data = personelOzet, message = "Personel özeti başarıyla getirildi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Personel özeti getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        // PUT: api/Profil/GuncelleProfil/5
        [HttpPut("GuncelleProfil/{kullaniciId}")]
        public async Task<IActionResult> GuncelleProfil(int kullaniciId, [FromBody] ProfilGuncelleRequest request)
        {
            try
            {
                var kullanici = await _context.Kullanicilar
                    .Include(k => k.Personel)
                    .FirstOrDefaultAsync(k => k.Id == kullaniciId);

                if (kullanici == null)
                {
                    return NotFound(new { success = false, message = "Kullanıcı bulunamadı." });
                }

                var personel = kullanici.Personel;

                // Email değişikliği kontrolü (başka personelde aynı email var mı)
                if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != personel.Email)
                {
                    var mevcutEmail = await _context.Personeller
                        .AnyAsync(p => p.Email == request.Email && p.Id != personel.Id);
                    
                    if (mevcutEmail)
                    {
                        return BadRequest(new { success = false, message = "Bu email adresi başka bir personel tarafından kullanılmaktadır." });
                    }
                }

                // Personel bilgilerini güncelle
                if (!string.IsNullOrWhiteSpace(request.Email))
                    personel.Email = request.Email;
                
                if (!string.IsNullOrWhiteSpace(request.Telefon))
                    personel.Telefon = request.Telefon;
                
                if (!string.IsNullOrWhiteSpace(request.Adres))
                    personel.Adres = request.Adres;
                
                if (!string.IsNullOrWhiteSpace(request.FotografUrl))
                    personel.FotografUrl = request.FotografUrl;

                personel.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Profil başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Profil güncellenirken bir hata oluştu.", error = ex.Message });
            }
        }

        // PUT: api/Profil/SifreDegistir/5
        [HttpPut("SifreDegistir/{kullaniciId}")]
        public async Task<IActionResult> SifreDegistir(int kullaniciId, [FromBody] SifreDegistirRequest request)
        {
            try
            {
                var kullanici = await _context.Kullanicilar.FindAsync(kullaniciId);
                if (kullanici == null)
                {
                    return NotFound(new { success = false, message = "Kullanıcı bulunamadı." });
                }

                // Mevcut şifre kontrolü
                if (!_userService.VerifyPassword(request.MevcutSifre, kullanici.SifreHash))
                {
                    return BadRequest(new { success = false, message = "Mevcut şifre hatalı." });
                }

                // Yeni şifre validasyonu
                if (request.YeniSifre.Length < 6)
                {
                    return BadRequest(new { success = false, message = "Yeni şifre en az 6 karakter olmalıdır." });
                }

                if (request.YeniSifre != request.YeniSifreTekrar)
                {
                    return BadRequest(new { success = false, message = "Yeni şifre ve tekrar şifresi uyuşmuyor." });
                }

                // Yeni şifreyi hash'le ve güncelle
                kullanici.SifreHash = _userService.HashPassword(request.YeniSifre);
                kullanici.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Şifre başarıyla değiştirildi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Şifre değiştirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        // POST: api/Profil/FotografYukle/5
        [HttpPost("FotografYukle/{kullaniciId}")]
        public async Task<ActionResult<object>> FotografYukle(int kullaniciId, IFormFile fotograf)
        {
            try
            {
                if (fotograf == null || fotograf.Length == 0)
                {
                    return BadRequest(new { success = false, message = "Fotoğraf dosyası seçilmedi." });
                }

                // Dosya uzantısı kontrolü
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(fotograf.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(new { success = false, message = "Sadece JPG, JPEG, PNG, GIF dosyaları kabul edilir." });
                }

                // Dosya boyutu kontrolü (5MB)
                if (fotograf.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new { success = false, message = "Dosya boyutu 5MB'dan büyük olamaz." });
                }

                var kullanici = await _context.Kullanicilar
                    .Include(k => k.Personel)
                    .FirstOrDefaultAsync(k => k.Id == kullaniciId);

                if (kullanici == null)
                {
                    return NotFound(new { success = false, message = "Kullanıcı bulunamadı." });
                }

                // Fotoğraf kaydetme işlemi
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = $"{kullanici.PersonelId}_{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await fotograf.CopyToAsync(fileStream);
                }

                // Eski fotoğrafı sil (varsa)
                if (!string.IsNullOrEmpty(kullanici.Personel.FotografUrl))
                {
                    var eskiFotografPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", kullanici.Personel.FotografUrl.TrimStart('/'));
                    if (System.IO.File.Exists(eskiFotografPath))
                    {
                        System.IO.File.Delete(eskiFotografPath);
                    }
                }

                // Veritabanını güncelle
                var fotografUrl = $"/uploads/avatars/{uniqueFileName}";
                kullanici.Personel.FotografUrl = fotografUrl;
                kullanici.Personel.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, data = new { FotografUrl = fotografUrl }, message = "Fotoğraf başarıyla yüklendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Fotoğraf yüklenirken bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/Profil/IzinDetay/5
        [HttpGet("IzinDetay/{kullaniciId}")]
        public async Task<ActionResult<object>> GetKullaniciIzinDetay(int kullaniciId)
        {
            try
            {
                var kullanici = await _context.Kullanicilar
                    .Include(k => k.Personel)
                    .FirstOrDefaultAsync(k => k.Id == kullaniciId);

                if (kullanici == null)
                {
                    return NotFound(new { success = false, message = "Kullanıcı bulunamadı." });
                }

                var personelId = kullanici.PersonelId;
                var personel = kullanici.Personel;
                
                if (personel == null)
                {
                    return BadRequest(new { success = false, message = "Kullanıcının personel kaydı bulunamadı." });
                }

                // İzin hakları hesaplama
                var iseBaslamaTarihi = personel.IseBaslamaTarihi;
                var bugun = DateTime.UtcNow.Date;
                var yillikIzinHakki = CalculateYillikIzinHakki(iseBaslamaTarihi, bugun);

                // Bu yıl kullanılan izin günleri
                var kullanilanIzinGunleri = await _context.IzinTalepleri
                    .Where(i => i.PersonelId == personelId && 
                               i.Durum == "Onaylandı" &&
                               i.IzinBaslamaTarihi.Year == bugun.Year)
                    .SumAsync(i => i.GunSayisi);

                var kalanIzinGunleri = yillikIzinHakki - kullanilanIzinGunleri;

                // Bekleyen izin talepleri
                var bekleyenIzinGunleri = await _context.IzinTalepleri
                    .Where(i => i.PersonelId == personelId && i.Durum == "Beklemede")
                    .SumAsync(i => i.GunSayisi);

                var izinDetay = new
                {
                    YillikIzinHakki = yillikIzinHakki,
                    KullanilanIzin = kullanilanIzinGunleri,
                    KalanIzin = kalanIzinGunleri,
                    BekleyenIzin = bekleyenIzinGunleri,
                    ToplamKullanılabilirIzin = kalanIzinGunleri - bekleyenIzinGunleri,
                    IseBaslamaTarihi = iseBaslamaTarihi,
                    CalismaYili = CalculateCalismaYili(iseBaslamaTarihi, bugun)
                };

                return Ok(new { success = true, data = izinDetay, message = "İzin detayları başarıyla getirildi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "İzin detayları getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private static string GetDonemAdi(int ay, int yil)
        {
            var ayAdlari = new string[] { "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran",
                                         "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık" };
            return $"{ayAdlari[ay - 1]} {yil}";
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

        private static int CalculateCalismaYili(DateTime iseBaslamaTarihi, DateTime bugun)
        {
            var calismaYili = bugun.Year - iseBaslamaTarihi.Year;
            if (bugun.Month < iseBaslamaTarihi.Month || 
                (bugun.Month == iseBaslamaTarihi.Month && bugun.Day < iseBaslamaTarihi.Day))
            {
                calismaYili--;
            }
            
            return Math.Max(0, calismaYili);
        }
    }

    public class ProfilGuncelleRequest
    {
        public string? Email { get; set; }
        public string? Telefon { get; set; }
        public string? Adres { get; set; }
        public string? FotografUrl { get; set; }
    }

    public class SifreDegistirRequest
    {
        public string MevcutSifre { get; set; } = string.Empty;
        public string YeniSifre { get; set; } = string.Empty;
        public string YeniSifreTekrar { get; set; } = string.Empty;
    }
}