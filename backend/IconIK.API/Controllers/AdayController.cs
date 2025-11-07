using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IconIK.API.Data;
using IconIK.API.Models;
using IconIK.API.Services;
using System.Text.Json;
using Npgsql;

namespace IconIK.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdayController : ControllerBase
    {
        private readonly IconIKContext _context;
        private readonly ICVService _cvService;
        private readonly IStatusService _statusService;

        public AdayController(IconIKContext context, ICVService cvService, IStatusService statusService)
        {
            _context = context;
            _cvService = cvService;
            _statusService = statusService;
        }

        [HttpGet]
        public async Task<ActionResult<object>> GetAdaylar()
        {
            try
            {
                var adaylar = await _context.Adaylar
                    .Include(a => a.Deneyimler)
                    .Include(a => a.Yetenekler)
                    .Include(a => a.Egitimler)
                    .Include(a => a.Basvurular)
                    .OrderByDescending(a => a.CreatedAt)
                    .Select(a => new
                    {
                        a.Id,
                        a.Ad,
                        a.Soyad,
                        AdSoyad = a.Ad + " " + a.Soyad,
                        a.Email,
                        a.Telefon,
                        a.DogumTarihi,
                        a.Cinsiyet,
                        a.MedeniDurum,
                        a.AskerlikDurumu,
                        a.Sehir,
                        Universite = a.Egitimler.Where(e => e.Derece != null)
                            .OrderByDescending(e => e.Derece == "Doktora" ? 4 :
                                                    e.Derece == "Yüksek Lisans" ? 3 :
                                                    e.Derece == "Lisans" ? 2 :
                                                    e.Derece == "Ön Lisans" ? 1 : 0)
                            .ThenByDescending(e => e.MezuniyetYili ?? e.BaslangicYili)
                            .Select(e => e.OkulAd)
                            .FirstOrDefault() ?? a.Universite,
                        a.Bolum,
                        a.MezuniyetYili,
                        ToplamDeneyim = a.Deneyimler
                            .Sum(d => d.BitisTarihi.HasValue ?
                                (d.BitisTarihi.Value.Year - (d.BaslangicTarihi ?? DateTime.UtcNow).Year) * 12 +
                                d.BitisTarihi.Value.Month - (d.BaslangicTarihi ?? DateTime.UtcNow).Month :
                                (DateTime.UtcNow.Year - (d.BaslangicTarihi ?? DateTime.UtcNow).Year) * 12 +
                                DateTime.UtcNow.Month - (d.BaslangicTarihi ?? DateTime.UtcNow).Month),
                        a.OzgecmisDosyasi,
                        a.FotografYolu,
                        a.LinkedinUrl,
                        a.Durum,
                        DurumText = a.Durum.ToString(),
                        a.DurumGuncellenmeTarihi,
                        a.OtomatikCVOlusturuldu,
                        a.KaraListe,
                        a.Aktif,
                        a.CreatedAt,
                        DeneyimSayisi = a.Deneyimler.Count(),
                        YetenekSayisi = a.Yetenekler.Count(),
                        BasvuruSayisi = a.Basvurular.Count()
                    })
                    .ToListAsync();

                // Add formatted experience text after materialization
                var adaylarWithFormatted = adaylar.Select(a => new
                {
                    a.Id,
                    a.Ad,
                    a.Soyad,
                    a.AdSoyad,
                    a.Email,
                    a.Telefon,
                    a.DogumTarihi,
                    a.Cinsiyet,
                    a.MedeniDurum,
                    a.AskerlikDurumu,
                    a.Sehir,
                    a.Universite,
                    a.Bolum,
                    a.MezuniyetYili,
                    a.ToplamDeneyim,
                    DeneyimYil = GetDeneyimFormattedFromMonths(a.ToplamDeneyim),
                    a.OzgecmisDosyasi,
                    a.FotografYolu,
                    a.LinkedinUrl,
                    a.Durum,
                    a.DurumText,
                    a.DurumGuncellenmeTarihi,
                    a.OtomatikCVOlusturuldu,
                    a.KaraListe,
                    a.Aktif,
                    a.CreatedAt,
                    a.DeneyimSayisi,
                    a.YetenekSayisi,
                    a.BasvuruSayisi
                }).ToList();

                return new { success = true, data = adaylarWithFormatted, message = "Adaylar başarıyla getirildi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpGet("Aktif")]
        public async Task<ActionResult<object>> GetAktifAdaylar()
        {
            try
            {
                var aktifAdaylar = await _context.Adaylar
                    .Where(a => a.Aktif && !a.KaraListe)
                    .Select(a => new
                    {
                        a.Id,
                        AdSoyad = a.Ad + " " + a.Soyad,
                        a.Email
                    })
                    .OrderBy(a => a.AdSoyad)
                    .ToListAsync();

                return new { success = true, data = aktifAdaylar, message = "Aktif adaylar başarıyla getirildi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetAday(int id)
        {
            try
            {
                var aday = await _context.Adaylar
                    .Include(a => a.Deneyimler.OrderByDescending(d => d.BaslangicTarihi))
                    .Include(a => a.Yetenekler.OrderBy(y => y.Yetenek))
                    .Include(a => a.Egitimler.OrderByDescending(e => e.BaslangicYili))
                    .Include(a => a.Sertifikalar.OrderByDescending(s => s.Tarih))
                    .Include(a => a.Referanslar)
                    .Include(a => a.Diller)
                    .Include(a => a.Projeler.OrderByDescending(p => p.BaslangicTarihi))
                    .Include(a => a.Hobiler)
                    .Include(a => a.Basvurular)
                        .ThenInclude(b => b.Ilan)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (aday == null)
                {
                    return new { success = false, message = "Aday bulunamadı." };
                }

                var result = new
                {
                    aday.Id,
                    aday.Ad,
                    aday.Soyad,
                    AdSoyad = aday.Ad + " " + aday.Soyad,
                    aday.TcKimlik,
                    aday.Email,
                    aday.Telefon,
                    aday.DogumTarihi,
                    aday.Cinsiyet,
                    aday.MedeniDurum,
                    aday.AskerlikDurumu,
                    aday.Adres,
                    aday.Sehir,
                    aday.Universite,
                    aday.Bolum,
                    aday.MezuniyetYili,
                    aday.ToplamDeneyim,
                    aday.OzgecmisDosyasi,
                    aday.FotografYolu,
                    aday.LinkedinUrl,
                    aday.Notlar,
                    aday.KaraListe,
                    aday.Aktif,
                    aday.CreatedAt,
                    Deneyimler = aday.Deneyimler.Select(d => new
                    {
                        d.Id,
                        d.SirketAd,
                        d.Pozisyon,
                        d.BaslangicTarihi,
                        d.BitisTarihi,
                        d.HalenCalisiyor,
                        d.Aciklama,
                        Sure = d.HalenCalisiyor ?
                            $"{d.BaslangicTarihi?.ToString("MM/yyyy")} - Devam ediyor" :
                            $"{d.BaslangicTarihi?.ToString("MM/yyyy")} - {d.BitisTarihi?.ToString("MM/yyyy")}"
                    }),
                    Yetenekler = aday.Yetenekler.Select(y => new
                    {
                        y.Id,
                        y.Yetenek,
                        y.Seviye,
                        SeviyeText = y.Seviye switch
                        {
                            1 => "Başlangıç",
                            2 => "Temel",
                            3 => "Orta",
                            4 => "İyi",
                            5 => "Uzman",
                            _ => "Bilinmiyor"
                        }
                    }),
                    Basvurular = aday.Basvurular.Select(b => new
                    {
                        b.Id,
                        IlanBaslik = b.Ilan.Baslik,
                        b.BasvuruTarihi,
                        Durum = b.Durum.ToString(),
                        b.Puan
                    }),
                    Egitimler = aday.Egitimler.Select(e => new
                    {
                        e.Id,
                        e.OkulAd,
                        e.Bolum,
                        e.Derece,
                        e.BaslangicYili,
                        e.MezuniyetYili,
                        e.DevamEdiyor,
                        e.NotOrtalamasi,
                        e.Aciklama,
                        Sure = e.DevamEdiyor ?
                            $"{e.BaslangicYili} - Devam ediyor" :
                            $"{e.BaslangicYili} - {e.MezuniyetYili}"
                    }),
                    Sertifikalar = aday.Sertifikalar.Select(s => new
                    {
                        s.Id,
                        s.SertifikaAd,
                        s.VerenKurum,
                        s.Tarih,
                        s.GecerlilikTarihi,
                        s.SertifikaNo,
                        s.Aciklama,
                        GecerlilikDurumu = s.GecerlilikTarihi.HasValue ?
                            (s.GecerlilikTarihi > DateTime.UtcNow ? "Geçerli" : "Süresi Dolmuş") : "Süresiz"
                    }),
                    Referanslar = aday.Referanslar.Select(r => new
                    {
                        r.Id,
                        r.AdSoyad,
                        r.Sirket,
                        r.Pozisyon,
                        r.Telefon,
                        r.Email,
                        r.IliskiTuru,
                        r.Aciklama
                    }),
                    Diller = aday.Diller.Select(d => new
                    {
                        d.Id,
                        d.Dil,
                        d.OkumaSeviyesi,
                        d.YazmaSeviyesi,
                        d.KonusmaSeviyesi,
                        d.Sertifika,
                        d.SertifikaPuani,
                        OkumaSeviyesiText = GetDilSeviyeText(d.OkumaSeviyesi),
                        YazmaSeviyesiText = GetDilSeviyeText(d.YazmaSeviyesi),
                        KonusmaSeviyesiText = GetDilSeviyeText(d.KonusmaSeviyesi)
                    }),
                    Projeler = aday.Projeler.Select(p => new
                    {
                        p.Id,
                        p.ProjeAd,
                        p.Rol,
                        p.BaslangicTarihi,
                        p.BitisTarihi,
                        p.DevamEdiyor,
                        p.Teknolojiler,
                        p.Aciklama,
                        p.ProjeUrl,
                        Sure = p.DevamEdiyor ?
                            $"{p.BaslangicTarihi?.ToString("MM/yyyy")} - Devam ediyor" :
                            $"{p.BaslangicTarihi?.ToString("MM/yyyy")} - {p.BitisTarihi?.ToString("MM/yyyy")}"
                    }),
                    Hobiler = aday.Hobiler.Select(h => new
                    {
                        h.Id,
                        h.Hobi,
                        h.Kategori,
                        h.Seviye,
                        h.Aciklama
                    })
                };

                return new { success = true, data = result, message = "Aday başarıyla getirildi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<object>> CreateAday([FromBody] JsonElement adayData)
        {
            Console.WriteLine("=== ADAY CREATE REQUEST ===");
            Console.WriteLine($"Request JSON: {adayData}");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var aday = new Aday
                {
                    Ad = adayData.GetProperty("ad").GetString() ?? "",
                    Soyad = adayData.GetProperty("soyad").GetString() ?? "",
                    TcKimlik = adayData.GetProperty("tcKimlik").GetString() ?? "",
                    Email = adayData.GetProperty("email").GetString() ?? "",
                    Telefon = adayData.TryGetProperty("telefon", out var telefon) ? telefon.GetString() : null,
                    Cinsiyet = adayData.TryGetProperty("cinsiyet", out var cinsiyet) ? cinsiyet.GetString() : null,
                    MedeniDurum = adayData.TryGetProperty("medeniDurum", out var medeniDurum) ? medeniDurum.GetString() : null,
                    AskerlikDurumu = adayData.TryGetProperty("askerlikDurumu", out var askerlikDurumu) ? askerlikDurumu.GetString() : null,
                    Adres = adayData.TryGetProperty("adres", out var adres) ? adres.GetString() : null,
                    Sehir = adayData.TryGetProperty("sehir", out var sehir) ? sehir.GetString() : null,
                    Universite = adayData.TryGetProperty("universite", out var universite) ? universite.GetString() : null,
                    Bolum = adayData.TryGetProperty("bolum", out var bolum) ? bolum.GetString() : null,
                    LinkedinUrl = adayData.TryGetProperty("linkedinUrl", out var linkedinUrl) ? linkedinUrl.GetString() : null,
                    Notlar = adayData.TryGetProperty("notlar", out var notlar) ? notlar.GetString() : null
                };

                if (adayData.TryGetProperty("dogumTarihi", out var dogumTarihi) && !dogumTarihi.ValueKind.Equals(JsonValueKind.Null))
                {
                    aday.DogumTarihi = dogumTarihi.GetDateTime();
                }

                if (adayData.TryGetProperty("mezuniyetYili", out var mezuniyetYili) && !mezuniyetYili.ValueKind.Equals(JsonValueKind.Null))
                {
                    aday.MezuniyetYili = mezuniyetYili.GetInt32();
                }

                if (adayData.TryGetProperty("toplamDeneyim", out var toplamDeneyim))
                {
                    aday.ToplamDeneyim = toplamDeneyim.GetInt32();
                }

                _context.Adaylar.Add(aday);
                await _context.SaveChangesAsync();

                // Add related entities
                await AddRelatedEntities(aday.Id, adayData);

                await transaction.CommitAsync();

                return new { success = true, data = new { id = aday.Id }, message = "Aday başarıyla oluşturuldu." };
            }
            catch (DbUpdateException ex) when (ex.InnerException is NpgsqlException npgsqlEx && npgsqlEx.SqlState == "23505")
            {
                await transaction.RollbackAsync();
                string message = npgsqlEx.Message?.Contains("tc_kimlik") == true ?
                    "Bu TC Kimlik numarası zaten kayıtlı." :
                    "Bu email adresi zaten kayıtlı.";
                return new { success = false, message };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<object>> UpdateAday(int id, [FromBody] JsonElement adayData)
        {
            Console.WriteLine("=== ADAY UPDATE REQUEST ===");
            Console.WriteLine($"Aday ID: {id}");
            Console.WriteLine("Request received with JSON data");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var aday = await _context.Adaylar.FindAsync(id);
                if (aday == null)
                {
                    return new { success = false, message = "Aday bulunamadı." };
                }

                if (adayData.TryGetProperty("ad", out var ad))
                    aday.Ad = ad.GetString() ?? "";

                if (adayData.TryGetProperty("soyad", out var soyad))
                    aday.Soyad = soyad.GetString() ?? "";

                if (adayData.TryGetProperty("tcKimlik", out var tcKimlik))
                    aday.TcKimlik = tcKimlik.GetString() ?? "";

                if (adayData.TryGetProperty("email", out var email))
                    aday.Email = email.GetString() ?? "";

                if (adayData.TryGetProperty("telefon", out var telefon))
                    aday.Telefon = telefon.ValueKind == JsonValueKind.Null ? null : telefon.GetString();

                if (adayData.TryGetProperty("dogumTarihi", out var dogumTarihi))
                    aday.DogumTarihi = !dogumTarihi.ValueKind.Equals(JsonValueKind.Null) ? dogumTarihi.GetDateTime() : null;

                if (adayData.TryGetProperty("cinsiyet", out var cinsiyet))
                    aday.Cinsiyet = cinsiyet.ValueKind == JsonValueKind.Null ? null : cinsiyet.GetString();

                if (adayData.TryGetProperty("medeniDurum", out var medeniDurum))
                    aday.MedeniDurum = medeniDurum.ValueKind == JsonValueKind.Null ? null : medeniDurum.GetString();

                if (adayData.TryGetProperty("askerlikDurumu", out var askerlikDurumu))
                    aday.AskerlikDurumu = askerlikDurumu.ValueKind == JsonValueKind.Null ? null : askerlikDurumu.GetString();

                if (adayData.TryGetProperty("adres", out var adres))
                    aday.Adres = adres.ValueKind == JsonValueKind.Null ? null : adres.GetString();

                if (adayData.TryGetProperty("sehir", out var sehir))
                    aday.Sehir = sehir.ValueKind == JsonValueKind.Null ? null : sehir.GetString();

                if (adayData.TryGetProperty("universite", out var universite))
                    aday.Universite = universite.ValueKind == JsonValueKind.Null ? null : universite.GetString();

                if (adayData.TryGetProperty("bolum", out var bolum))
                    aday.Bolum = bolum.ValueKind == JsonValueKind.Null ? null : bolum.GetString();

                if (adayData.TryGetProperty("mezuniyetYili", out var mezuniyetYili))
                    aday.MezuniyetYili = !mezuniyetYili.ValueKind.Equals(JsonValueKind.Null) ? mezuniyetYili.GetInt32() : null;

                if (adayData.TryGetProperty("toplamDeneyim", out var toplamDeneyim))
                    aday.ToplamDeneyim = toplamDeneyim.GetInt32();

                if (adayData.TryGetProperty("linkedinUrl", out var linkedinUrl))
                    aday.LinkedinUrl = linkedinUrl.ValueKind == JsonValueKind.Null ? null : linkedinUrl.GetString();

                if (adayData.TryGetProperty("notlar", out var notlar))
                    aday.Notlar = notlar.ValueKind == JsonValueKind.Null ? null : notlar.GetString();

                if (adayData.TryGetProperty("karaListe", out var karaListe))
                    aday.KaraListe = karaListe.GetBoolean();

                if (adayData.TryGetProperty("aktif", out var aktif))
                    aday.Aktif = aktif.GetBoolean();

                await _context.SaveChangesAsync();

                // Update related entities
                await UpdateRelatedEntities(id, adayData);

                await transaction.CommitAsync();

                return new { success = true, message = "Aday başarıyla güncellendi." };
            }
            catch (DbUpdateException ex) when (ex.InnerException is NpgsqlException npgsqlEx && npgsqlEx.SqlState == "23505")
            {
                await transaction.RollbackAsync();
                string message = npgsqlEx.Message?.Contains("tc_kimlik") == true ?
                    "Bu TC Kimlik numarası zaten kayıtlı." :
                    "Bu email adresi zaten kayıtlı.";
                return new { success = false, message };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"=== EXCEPTION IN UPDATE ADAY ===");
                Console.WriteLine($"Exception Type: {ex.GetType().Name}");
                Console.WriteLine($"Exception Message: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<object>> DeleteAday(int id)
        {
            try
            {
                var aday = await _context.Adaylar
                    .Include(a => a.Basvurular)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (aday == null)
                {
                    return new { success = false, message = "Aday bulunamadı." };
                }

                if (aday.Basvurular.Any())
                {
                    return new { success = false, message = "Bu adayın başvuruları bulunduğu için silinemez." };
                }

                _context.Adaylar.Remove(aday);
                await _context.SaveChangesAsync();

                return new { success = true, message = "Aday başarıyla silindi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpPut("{id}/kara-liste")]
        public async Task<ActionResult<object>> KaraListeyeEkle(int id)
        {
            try
            {
                var aday = await _context.Adaylar.FindAsync(id);
                if (aday == null)
                {
                    return new { success = false, message = "Aday bulunamadı." };
                }

                aday.KaraListe = true;
                await _context.SaveChangesAsync();

                return new { success = true, message = "Aday kara listeye eklendi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpDelete("{id}/kara-liste")]
        public async Task<ActionResult<object>> KaraListeydenCikar(int id)
        {
            try
            {
                var aday = await _context.Adaylar.FindAsync(id);
                if (aday == null)
                {
                    return new { success = false, message = "Aday bulunamadı." };
                }

                aday.KaraListe = false;
                await _context.SaveChangesAsync();

                return new { success = true, message = "Aday kara listeden çıkarıldı." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpGet("arama")]
        public async Task<ActionResult<object>> AdayArama([FromQuery] string? ad, [FromQuery] string? sehir, [FromQuery] string? universite, [FromQuery] int? minDeneyim, [FromQuery] int? maxDeneyim)
        {
            try
            {
                var query = _context.Adaylar.AsQueryable();

                if (!string.IsNullOrEmpty(ad))
                {
                    query = query.Where(a => (a.Ad + " " + a.Soyad).ToLower().Contains(ad.ToLower()));
                }

                if (!string.IsNullOrEmpty(sehir))
                {
                    query = query.Where(a => a.Sehir != null && a.Sehir.ToLower().Contains(sehir.ToLower()));
                }

                if (!string.IsNullOrEmpty(universite))
                {
                    query = query.Where(a => a.Universite != null && a.Universite.ToLower().Contains(universite.ToLower()));
                }

                if (minDeneyim.HasValue)
                {
                    query = query.Where(a => a.ToplamDeneyim >= minDeneyim.Value);
                }

                if (maxDeneyim.HasValue)
                {
                    query = query.Where(a => a.ToplamDeneyim <= maxDeneyim.Value);
                }

                var adaylar = await query
                    .Where(a => a.Aktif && !a.KaraListe)
                    .Select(a => new
                    {
                        a.Id,
                        a.Ad,
                        a.Soyad,
                        AdSoyad = a.Ad + " " + a.Soyad,
                        a.Email,
                        a.Sehir,
                        a.Universite,
                        a.ToplamDeneyim,
                        a.CreatedAt
                    })
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();

                return new { success = true, data = adaylar, message = $"{adaylar.Count} aday bulundu." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpPost("{id}/otomatik-cv-olustur")]
        public async Task<ActionResult<object>> OtomatikCVOlustur(int id)
        {
            try
            {
                var htmlContent = await _cvService.OtomatikCVOlustur(id);
                return new { success = true, data = htmlContent, message = "Otomatik CV başarıyla oluşturuldu." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpPost("{id}/cv-yukle")]
        public async Task<ActionResult<object>> CVYukle(int id, IFormFile file)
        {
            try
            {
                var cv = await _cvService.CVYukle(id, file);
                return new { success = true, data = cv, message = "CV başarıyla yüklendi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpGet("{id}/cv-listesi")]
        public async Task<ActionResult<object>> CVListesi(int id)
        {
            try
            {
                var cvler = await _cvService.AdayinCVleriniGetir(id);
                return new { success = true, data = cvler, message = "CV'ler başarıyla getirildi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpGet("{id}/cv-goruntule")]
        public async Task<ActionResult<object>> CVGoruntule(int id, string cvTipi = "Otomatik")
        {
            try
            {
                var htmlContent = await _cvService.CVHtmlGetir(id, cvTipi);
                return new { success = true, data = htmlContent, message = "CV başarıyla getirildi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpDelete("cv/{cvId}")]
        public async Task<ActionResult<object>> CVSil(int cvId)
        {
            try
            {
                var success = await _cvService.CVSil(cvId);
                if (success)
                    return new { success = true, message = "CV başarıyla silindi." };
                else
                    return NotFound(new { success = false, message = "CV bulunamadı." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpPost("{id}/durum-degistir")]
        public async Task<ActionResult<object>> DurumDegistir(int id, [FromBody] JsonElement data)
        {
            try
            {
                var yeniDurum = data.GetProperty("durum").GetInt32();
                var not = data.TryGetProperty("not", out var notElement) ? notElement.GetString() : null;
                var degistirenPersonelId = data.TryGetProperty("degistirenPersonelId", out var personelElement) ? personelElement.GetInt32() : (int?)null;

                await _statusService.AdayDurumDegistir(id, (AdayDurumu)yeniDurum, not, degistirenPersonelId);

                return new { success = true, message = "Aday durumu başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpPost("{id}/fotograf-yukle")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<object>> FotografYukle(int id)
        {
            try
            {
                var file = Request.Form.Files.FirstOrDefault();

                if (file == null || file.Length == 0)
                    return BadRequest(new { success = false, message = "Dosya seçilmedi." });

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                    return BadRequest(new { success = false, message = "Desteklenmeyen dosya formatı. JPG, PNG veya GIF dosyası seçiniz." });

                if (file.Length > 10 * 1024 * 1024) // 10MB limit
                    return BadRequest(new { success = false, message = "Dosya boyutu 10MB'dan büyük olamaz." });

                var aday = await _context.Adaylar.FindAsync(id);
                if (aday == null)
                    return NotFound(new { success = false, message = "Aday bulunamadı." });

                // Eski fotoğrafı sil
                if (!string.IsNullOrEmpty(aday.FotografYolu))
                {
                    var oldPhotoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", aday.FotografYolu);
                    if (System.IO.File.Exists(oldPhotoPath))
                    {
                        System.IO.File.Delete(oldPhotoPath);
                    }
                }

                // Upload klasörünü oluştur
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "aday-fotograflar");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Benzersiz dosya adı oluştur
                var uniqueFileName = $"{id}_{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Dosyayı kaydet
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Database'de yolu güncelle
                var relativePath = $"uploads/aday-fotograflar/{uniqueFileName}";
                aday.FotografYolu = relativePath;
                aday.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return new { success = true, data = new { fotografYolu = relativePath }, message = "Fotoğraf başarıyla yüklendi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpGet("{id}/durum-gecmisi")]
        public async Task<ActionResult<object>> DurumGecmisi(int id)
        {
            try
            {
                var gecmis = await _statusService.AdayDurumGecmisiniGetir(id);
                var gecmisDetay = gecmis.Select(g => new
                {
                    g.Id,
                    EskiDurum = g.EskiDurum?.ToString(),
                    EskiDurumText = g.EskiDurum.HasValue ? StatusService.GetAdayDurumText(g.EskiDurum.Value) : null,
                    YeniDurum = g.YeniDurum.ToString(),
                    YeniDurumText = StatusService.GetAdayDurumText(g.YeniDurum),
                    g.DegisiklikTarihi,
                    g.DegisiklikNotu,
                    DegistirenPersonel = g.DegistirenPersonel != null ? g.DegistirenPersonel.Ad + " " + g.DegistirenPersonel.Soyad : null,
                    g.OtomatikDegisiklik,
                    IlgiliIlan = g.IlgiliBasvuru?.Ilan?.Baslik,
                    IlgiliMulakat = g.IlgiliMulakat != null ? "Mülakat" : null
                });

                return new { success = true, data = gecmisDetay, message = "Durum geçmişi başarıyla getirildi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        [HttpPost("{id}/kara-listeye-ekle")]
        public async Task<ActionResult<object>> KaraListeyeEkle(int id, [FromBody] JsonElement data)
        {
            try
            {
                var not = data.TryGetProperty("not", out var notElement) ? notElement.GetString() : null;
                var degistirenPersonelId = data.TryGetProperty("degistirenPersonelId", out var personelElement) ? personelElement.GetInt32() : (int?)null;

                await _statusService.KaraListeyeEkle(id, degistirenPersonelId, not);

                return new { success = true, message = "Aday kara listeye başarıyla eklendi." };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Hata oluştu: {ex.Message}" });
            }
        }

        private static string GetDilSeviyeText(int seviye)
        {
            return seviye switch
            {
                1 => "Başlangıç",
                2 => "Temel",
                3 => "Orta",
                4 => "İyi",
                5 => "İleri",
                _ => "Bilinmiyor"
            };
        }

        private async Task AddRelatedEntities(int adayId, JsonElement adayData)
        {
            Console.WriteLine($"=== AddRelatedEntities - Aday ID: {adayId} ===");

            // Add Egitimler
            if (adayData.TryGetProperty("egitimler", out var egitimler) && egitimler.ValueKind == JsonValueKind.Array)
            {
                Console.WriteLine($"Egitimler dizisi bulundu, sayı: {egitimler.GetArrayLength()}");
                foreach (var egitim in egitimler.EnumerateArray())
                {
                    var adayEgitim = new AdayEgitim
                    {
                        AdayId = adayId,
                        OkulAd = egitim.TryGetProperty("okulAd", out var okulAd) ? (okulAd.ValueKind == JsonValueKind.Null ? "" : okulAd.GetString()) : "",
                        Bolum = egitim.TryGetProperty("bolum", out var bolum) ? (bolum.ValueKind == JsonValueKind.Null ? "" : bolum.GetString()) : "",
                        Derece = egitim.TryGetProperty("derece", out var derece) ? (derece.ValueKind == JsonValueKind.Null ? null : derece.GetString()) : null,
                        BaslangicYili = egitim.TryGetProperty("baslangicYili", out var baslangicYili) && baslangicYili.ValueKind != JsonValueKind.Null ? baslangicYili.GetInt32() : DateTime.Now.Year,
                        MezuniyetYili = egitim.TryGetProperty("mezuniyetYili", out var mezuniyetYili) && !mezuniyetYili.ValueKind.Equals(JsonValueKind.Null) ? mezuniyetYili.GetInt32() : null,
                        DevamEdiyor = egitim.TryGetProperty("devamEdiyor", out var devamEdiyor) && devamEdiyor.GetBoolean(),
                        NotOrtalamasi = egitim.TryGetProperty("notOrtalamasi", out var notOrtalamasi) && !notOrtalamasi.ValueKind.Equals(JsonValueKind.Null) ? notOrtalamasi.GetDecimal() : null,
                        Aciklama = egitim.TryGetProperty("aciklama", out var aciklama) ? (aciklama.ValueKind == JsonValueKind.Null ? null : aciklama.GetString()) : null
                    };
                    _context.AdayEgitimleri.Add(adayEgitim);
                }
            }

            // Add Deneyimler
            if (adayData.TryGetProperty("deneyimler", out var deneyimler) && deneyimler.ValueKind == JsonValueKind.Array)
            {
                Console.WriteLine($"Deneyimler dizisi bulundu, sayı: {deneyimler.GetArrayLength()}");
                foreach (var deneyim in deneyimler.EnumerateArray())
                {
                    var adayDeneyim = new AdayDeneyim
                    {
                        AdayId = adayId,
                        SirketAd = deneyim.TryGetProperty("sirketAd", out var sirketAd) ? (sirketAd.ValueKind == JsonValueKind.Null ? "" : sirketAd.GetString()) : "",
                        Pozisyon = deneyim.TryGetProperty("pozisyon", out var pozisyon) ? (pozisyon.ValueKind == JsonValueKind.Null ? "" : pozisyon.GetString()) : "",
                        BaslangicTarihi = deneyim.TryGetProperty("baslangicTarihi", out var baslangicTarihi) && baslangicTarihi.ValueKind != JsonValueKind.Null ? DateTime.SpecifyKind(baslangicTarihi.GetDateTime(), DateTimeKind.Utc) : DateTime.UtcNow,
                        BitisTarihi = deneyim.TryGetProperty("bitisTarihi", out var bitisTarihi) && !bitisTarihi.ValueKind.Equals(JsonValueKind.Null) ? bitisTarihi.GetDateTime() : null,
                        HalenCalisiyor = deneyim.TryGetProperty("halenCalisiyor", out var halenCalisiyor) && halenCalisiyor.GetBoolean(),
                        Aciklama = deneyim.TryGetProperty("aciklama", out var aciklama) ? (aciklama.ValueKind == JsonValueKind.Null ? null : aciklama.GetString()) : null
                    };
                    _context.AdayDeneyimleri.Add(adayDeneyim);
                }
            }

            // Add Yetenekler
            if (adayData.TryGetProperty("yetenekler", out var yetenekler) && yetenekler.ValueKind == JsonValueKind.Array)
            {
                Console.WriteLine($"Yetenekler dizisi bulundu, sayı: {yetenekler.GetArrayLength()}");
                foreach (var yetenek in yetenekler.EnumerateArray())
                {
                    var adayYetenek = new AdayYetenek
                    {
                        AdayId = adayId,
                        Yetenek = yetenek.TryGetProperty("yetenek", out var yetenekAd) ? (yetenekAd.ValueKind == JsonValueKind.Null ? "" : yetenekAd.GetString()) : "",
                        Seviye = yetenek.TryGetProperty("seviye", out var seviye) ? seviye.GetInt32() : 1
                    };
                    _context.AdayYetenekleri.Add(adayYetenek);
                }
            }

            // Add Sertifikalar
            if (adayData.TryGetProperty("sertifikalar", out var sertifikalar) && sertifikalar.ValueKind == JsonValueKind.Array)
            {
                foreach (var sertifika in sertifikalar.EnumerateArray())
                {
                    var adaySertifika = new AdaySertifika
                    {
                        AdayId = adayId,
                        SertifikaAd = sertifika.TryGetProperty("sertifikaAd", out var sertifikaAd) ? (sertifikaAd.ValueKind == JsonValueKind.Null ? "" : sertifikaAd.GetString()) : "",
                        VerenKurum = sertifika.TryGetProperty("verenKurum", out var verenKurum) ? (verenKurum.ValueKind == JsonValueKind.Null ? "" : verenKurum.GetString()) : "",
                        Tarih = sertifika.TryGetProperty("tarih", out var tarih) && tarih.ValueKind != JsonValueKind.Null ? DateTime.SpecifyKind(tarih.GetDateTime(), DateTimeKind.Utc) : DateTime.UtcNow,
                        GecerlilikTarihi = sertifika.TryGetProperty("gecerlilikTarihi", out var gecerlilikTarihi) && !gecerlilikTarihi.ValueKind.Equals(JsonValueKind.Null) ? gecerlilikTarihi.GetDateTime() : null,
                        SertifikaNo = sertifika.TryGetProperty("sertifikaNo", out var sertifikaNo) ? (sertifikaNo.ValueKind == JsonValueKind.Null ? null : sertifikaNo.GetString()) : null,
                        Aciklama = sertifika.TryGetProperty("aciklama", out var aciklama) ? (aciklama.ValueKind == JsonValueKind.Null ? null : aciklama.GetString()) : null
                    };
                    _context.AdaySertifikalari.Add(adaySertifika);
                }
            }

            // Add Diller
            if (adayData.TryGetProperty("diller", out var diller) && diller.ValueKind == JsonValueKind.Array)
            {
                foreach (var dil in diller.EnumerateArray())
                {
                    var adayDil = new AdayDil
                    {
                        AdayId = adayId,
                        Dil = dil.TryGetProperty("dil", out var dilAd) ? (dilAd.ValueKind == JsonValueKind.Null ? "" : dilAd.GetString()) : "",
                        OkumaSeviyesi = dil.TryGetProperty("okumaSeviyesi", out var okumaSeviyesi) ? okumaSeviyesi.GetInt32() : 1,
                        YazmaSeviyesi = dil.TryGetProperty("yazmaSeviyesi", out var yazmaSeviyesi) ? yazmaSeviyesi.GetInt32() : 1,
                        KonusmaSeviyesi = dil.TryGetProperty("konusmaSeviyesi", out var konusmaSeviyesi) ? konusmaSeviyesi.GetInt32() : 1,
                        Sertifika = dil.TryGetProperty("sertifika", out var sertifika) ? (sertifika.ValueKind == JsonValueKind.Null ? null : sertifika.GetString()) : null,
                        SertifikaPuani = dil.TryGetProperty("sertifikaPuani", out var sertifikaPuani) ? (sertifikaPuani.ValueKind == JsonValueKind.Null ? null : sertifikaPuani.GetString()) : null
                    };
                    _context.AdayDilleri.Add(adayDil);
                }
            }

            // Add Referanslar
            if (adayData.TryGetProperty("referanslar", out var referanslar) && referanslar.ValueKind == JsonValueKind.Array)
            {
                foreach (var referans in referanslar.EnumerateArray())
                {
                    var adayReferans = new AdayReferans
                    {
                        AdayId = adayId,
                        AdSoyad = referans.TryGetProperty("adSoyad", out var adSoyad) ? (adSoyad.ValueKind == JsonValueKind.Null ? "" : adSoyad.GetString()) : "",
                        Sirket = referans.TryGetProperty("sirket", out var sirket) ? (sirket.ValueKind == JsonValueKind.Null ? "" : sirket.GetString()) : "",
                        Pozisyon = referans.TryGetProperty("pozisyon", out var pozisyon) ? (pozisyon.ValueKind == JsonValueKind.Null ? "" : pozisyon.GetString()) : "",
                        Telefon = referans.TryGetProperty("telefon", out var telefon) ? (telefon.ValueKind == JsonValueKind.Null ? "" : telefon.GetString()) : "",
                        Email = referans.TryGetProperty("email", out var email) ? (email.ValueKind == JsonValueKind.Null ? "" : email.GetString()) : "",
                        IliskiTuru = referans.TryGetProperty("iliskiTuru", out var iliskiTuru) ? (iliskiTuru.ValueKind == JsonValueKind.Null ? null : iliskiTuru.GetString()) : null,
                        Aciklama = referans.TryGetProperty("aciklama", out var aciklama) ? (aciklama.ValueKind == JsonValueKind.Null ? null : aciklama.GetString()) : null
                    };
                    _context.AdayReferanslari.Add(adayReferans);
                }
            }

            // Add Projeler
            if (adayData.TryGetProperty("projeler", out var projeler) && projeler.ValueKind == JsonValueKind.Array)
            {
                foreach (var proje in projeler.EnumerateArray())
                {
                    var adayProje = new AdayProje
                    {
                        AdayId = adayId,
                        ProjeAd = proje.TryGetProperty("projeAd", out var projeAd) ? (projeAd.ValueKind == JsonValueKind.Null ? "" : projeAd.GetString()) : "",
                        Rol = proje.TryGetProperty("rol", out var rol) ? (rol.ValueKind == JsonValueKind.Null ? null : rol.GetString()) : null,
                        BaslangicTarihi = proje.TryGetProperty("baslangicTarihi", out var baslangicTarihi) && !baslangicTarihi.ValueKind.Equals(JsonValueKind.Null) ? baslangicTarihi.GetDateTime() : null,
                        BitisTarihi = proje.TryGetProperty("bitisTarihi", out var bitisTarihi) && !bitisTarihi.ValueKind.Equals(JsonValueKind.Null) ? bitisTarihi.GetDateTime() : null,
                        DevamEdiyor = proje.TryGetProperty("devamEdiyor", out var devamEdiyor) && devamEdiyor.GetBoolean(),
                        Teknolojiler = proje.TryGetProperty("teknolojiler", out var teknolojiler) ? (teknolojiler.ValueKind == JsonValueKind.Null ? null : teknolojiler.GetString()) : null,
                        Aciklama = proje.TryGetProperty("aciklama", out var aciklama) ? (aciklama.ValueKind == JsonValueKind.Null ? null : aciklama.GetString()) : null,
                        ProjeUrl = proje.TryGetProperty("projeUrl", out var projeUrl) ? (projeUrl.ValueKind == JsonValueKind.Null ? null : projeUrl.GetString()) : null
                    };
                    _context.AdayProjeleri.Add(adayProje);
                }
            }

            // Add Hobiler
            if (adayData.TryGetProperty("hobiler", out var hobiler) && hobiler.ValueKind == JsonValueKind.Array)
            {
                foreach (var hobi in hobiler.EnumerateArray())
                {
                    var adayHobi = new AdayHobi
                    {
                        AdayId = adayId,
                        Hobi = hobi.TryGetProperty("hobi", out var hobiAd) ? (hobiAd.ValueKind == JsonValueKind.Null ? "" : hobiAd.GetString()) : "",
                        Kategori = hobi.TryGetProperty("kategori", out var kategori) ? (kategori.ValueKind == JsonValueKind.Null ? null : kategori.GetString()) : null,
                        Seviye = hobi.TryGetProperty("seviye", out var seviye) ? (seviye.ValueKind == JsonValueKind.Null ? null : seviye.GetString()) : null,
                        Aciklama = hobi.TryGetProperty("aciklama", out var aciklama) ? (aciklama.ValueKind == JsonValueKind.Null ? null : aciklama.GetString()) : null
                    };
                    _context.AdayHobileri.Add(adayHobi);
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task UpdateRelatedEntities(int adayId, JsonElement adayData)
        {
            // Remove existing related entities
            var existingEgitimler = await _context.AdayEgitimleri.Where(e => e.AdayId == adayId).ToListAsync();
            _context.AdayEgitimleri.RemoveRange(existingEgitimler);

            var existingDeneyimler = await _context.AdayDeneyimleri.Where(d => d.AdayId == adayId).ToListAsync();
            _context.AdayDeneyimleri.RemoveRange(existingDeneyimler);

            var existingYetenekler = await _context.AdayYetenekleri.Where(y => y.AdayId == adayId).ToListAsync();
            _context.AdayYetenekleri.RemoveRange(existingYetenekler);

            var existingSertifikalar = await _context.AdaySertifikalari.Where(s => s.AdayId == adayId).ToListAsync();
            _context.AdaySertifikalari.RemoveRange(existingSertifikalar);

            var existingDiller = await _context.AdayDilleri.Where(d => d.AdayId == adayId).ToListAsync();
            _context.AdayDilleri.RemoveRange(existingDiller);

            var existingReferanslar = await _context.AdayReferanslari.Where(r => r.AdayId == adayId).ToListAsync();
            _context.AdayReferanslari.RemoveRange(existingReferanslar);

            var existingProjeler = await _context.AdayProjeleri.Where(p => p.AdayId == adayId).ToListAsync();
            _context.AdayProjeleri.RemoveRange(existingProjeler);

            var existingHobiler = await _context.AdayHobileri.Where(h => h.AdayId == adayId).ToListAsync();
            _context.AdayHobileri.RemoveRange(existingHobiler);

            await _context.SaveChangesAsync();

            // Add new related entities
            await AddRelatedEntities(adayId, adayData);
        }

        private static string GetDeneyimFormattedFromMonths(int totalMonths)
        {
            var years = totalMonths / 12;
            var remainingMonths = totalMonths % 12;

            if (years == 0 && remainingMonths == 0) return "Yeni mezun";
            if (years == 0) return $"{remainingMonths} Ay";
            if (remainingMonths == 0) return years == 1 ? "1 Yıl" : $"{years} Yıl";
            return $"{years} Yıl, {remainingMonths} Ay";
        }
    }
}