using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IconIK.API.Data;
using IconIK.API.Models;

namespace IconIK.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class YetkiController : ControllerBase
    {
        private readonly IconIKContext _context;

        public YetkiController(IconIKContext context)
        {
            _context = context;
        }

        // GET: api/Yetki/EkranYetkileri
        [HttpGet("EkranYetkileri")]
        public async Task<ActionResult<IEnumerable<EkranYetkisi>>> GetEkranYetkileri()
        {
            var count = await _context.EkranYetkileri.CountAsync();
            Console.WriteLine($"=== Total EkranYetkileri count: {count} ===");
            
            return await _context.EkranYetkileri
                .Where(e => e.Aktif)
                .OrderBy(e => e.EkranAdi)
                .ToListAsync();
        }

        // GET: api/Yetki/KademeYetkileri
        [HttpGet("KademeYetkileri")]
        public async Task<ActionResult<IEnumerable<object>>> GetKademeYetkileri()
        {
            var kademeYetkileri = await _context.KademeEkranYetkileri
                .Include(k => k.Kademe)
                .Include(k => k.EkranYetkisi)
                .Where(k => k.Aktif)
                .Select(k => new
                {
                    k.Id,
                    k.KademeId,
                    KademeAdi = k.Kademe.Ad,
                    KademeSeviye = k.Kademe.Seviye,
                    k.EkranYetkisiId,
                    EkranAdi = k.EkranYetkisi.EkranAdi,
                    EkranKodu = k.EkranYetkisi.EkranKodu,
                    k.OkumaYetkisi,
                    k.YazmaYetkisi,
                    k.SilmeYetkisi,
                    k.GuncellemeYetkisi
                })
                .OrderBy(k => k.KademeSeviye)
                .ThenBy(k => k.EkranAdi)
                .ToListAsync();

            return Ok(kademeYetkileri);
        }

        // GET: api/Yetki/KademeYetkileri/{kademeId}
        [HttpGet("KademeYetkileri/{kademeId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetKademeYetkileriByKademe(int kademeId)
        {
            var kademeYetkileri = await _context.KademeEkranYetkileri
                .Include(k => k.Kademe)
                .Include(k => k.EkranYetkisi)
                .Where(k => k.KademeId == kademeId && k.Aktif)
                .Select(k => new
                {
                    k.Id,
                    k.KademeId,
                    KademeAdi = k.Kademe.Ad,
                    KademeSeviye = k.Kademe.Seviye,
                    k.EkranYetkisiId,
                    EkranAdi = k.EkranYetkisi.EkranAdi,
                    EkranKodu = k.EkranYetkisi.EkranKodu,
                    k.OkumaYetkisi,
                    k.YazmaYetkisi,
                    k.SilmeYetkisi,
                    k.GuncellemeYetkisi
                })
                .OrderBy(k => k.EkranAdi)
                .ToListAsync();

            return Ok(kademeYetkileri);
        }

        // POST: api/Yetki/InitializeOrganizasyonSemasiPermission
        [HttpPost("InitializeOrganizasyonSemasiPermission")]
        public async Task<ActionResult<object>> InitializeOrganizasyonSemasiPermission()
        {
            try
            {
                // Check if organizasyon-semasi permission already exists
                var existingEkran = await _context.EkranYetkileri
                    .FirstOrDefaultAsync(e => e.EkranKodu == "organizasyon-semasi");
                
                if (existingEkran == null)
                {
                    // Create screen permission
                    var ekranYetkisi = new EkranYetkisi
                    {
                        EkranAdi = "Organizasyon Şeması",
                        EkranKodu = "organizasyon-semasi",
                        Aciklama = "Şirketin organizasyon şemasını görüntüleme yetkisi",
                        Aktif = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.EkranYetkileri.Add(ekranYetkisi);
                    await _context.SaveChangesAsync();
                    existingEkran = ekranYetkisi;
                }

                // Get all active kademeler
                var kademeler = await _context.Kademeler.Where(k => k.Aktif).ToListAsync();
                int permissionsAdded = 0;

                // Add read permission for all kademeler
                foreach (var kademe in kademeler)
                {
                    var existingPermission = await _context.KademeEkranYetkileri
                        .FirstOrDefaultAsync(k => k.KademeId == kademe.Id && k.EkranYetkisiId == existingEkran.Id);

                    if (existingPermission == null)
                    {
                        var kademeYetki = new KademeEkranYetkisi
                        {
                            KademeId = kademe.Id,
                            EkranYetkisiId = existingEkran.Id,
                            OkumaYetkisi = true,   // Everyone can read
                            YazmaYetkisi = false,  // No one can write
                            SilmeYetkisi = false,  // No one can delete
                            GuncellemeYetkisi = false, // No one can update
                            Aktif = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        _context.KademeEkranYetkileri.Add(kademeYetki);
                        permissionsAdded++;
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new { 
                    success = true, 
                    message = $"Organizasyon şeması yetkileri başarıyla oluşturuldu. {permissionsAdded} kademe için yetki eklendi.",
                    ekranId = existingEkran.Id,
                    permissionsAdded = permissionsAdded
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Yetki oluşturulurken hata oluştu.", error = ex.Message });
            }
        }

        // POST: api/Yetki/EkranYetkisi
        [HttpPost("EkranYetkisi")]
        public async Task<ActionResult<EkranYetkisi>> CreateEkranYetkisi(EkranYetkisi ekranYetkisi)
        {
            try
            {
                // Check if screen code already exists
                var existingEkran = await _context.EkranYetkileri
                    .FirstOrDefaultAsync(e => e.EkranKodu == ekranYetkisi.EkranKodu);
                
                if (existingEkran != null)
                {
                    return BadRequest($"Ekran kodu '{ekranYetkisi.EkranKodu}' zaten mevcut.");
                }

                _context.EkranYetkileri.Add(ekranYetkisi);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetEkranYetkileri), new { id = ekranYetkisi.Id }, ekranYetkisi);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ekran yetkisi oluşturulurken hata: {ex.Message}");
            }
        }

        // POST: api/Yetki/KademeYetkisi
        [HttpPost("KademeYetkisi")]
        public async Task<ActionResult<KademeEkranYetkisi>> CreateKademeYetkisi(KademeEkranYetkisi kademeYetkisi)
        {
            try
            {
                // Check if permission already exists for this kademe-screen combination
                var existingYetki = await _context.KademeEkranYetkileri
                    .FirstOrDefaultAsync(k => k.KademeId == kademeYetkisi.KademeId && 
                                           k.EkranYetkisiId == kademeYetkisi.EkranYetkisiId);
                
                if (existingYetki != null)
                {
                    return BadRequest("Bu kademe için bu ekran yetkisi zaten mevcut.");
                }

                _context.KademeEkranYetkileri.Add(kademeYetkisi);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetKademeYetkileri), new { id = kademeYetkisi.Id }, kademeYetkisi);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Kademe yetkisi oluşturulurken hata: {ex.Message}");
            }
        }

        // PUT: api/Yetki/KademeYetkisi/{id}
        [HttpPut("KademeYetkisi/{id}")]
        public async Task<IActionResult> UpdateKademeYetkisi(int id, KademeEkranYetkisi kademeYetkisi)
        {
            if (id != kademeYetkisi.Id)
            {
                return BadRequest("ID uyuşmazlığı.");
            }

            try
            {
                var existingYetki = await _context.KademeEkranYetkileri.FindAsync(id);
                if (existingYetki == null)
                {
                    return NotFound();
                }

                existingYetki.OkumaYetkisi = kademeYetkisi.OkumaYetkisi;
                existingYetki.YazmaYetkisi = kademeYetkisi.YazmaYetkisi;
                existingYetki.SilmeYetkisi = kademeYetkisi.SilmeYetkisi;
                existingYetki.GuncellemeYetkisi = kademeYetkisi.GuncellemeYetkisi;
                existingYetki.Aktif = kademeYetkisi.Aktif;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Kademe yetkisi güncellenirken hata: {ex.Message}");
            }
        }

        // DELETE: api/Yetki/KademeYetkisi/{id}
        [HttpDelete("KademeYetkisi/{id}")]
        public async Task<IActionResult> DeleteKademeYetkisi(int id)
        {
            try
            {
                var kademeYetkisi = await _context.KademeEkranYetkileri.FindAsync(id);
                if (kademeYetkisi == null)
                {
                    return NotFound();
                }

                // Soft delete
                kademeYetkisi.Aktif = false;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Kademe yetkisi silinirken hata: {ex.Message}");
            }
        }

        // POST: api/Yetki/DefaultEkranYetkileri
        [HttpPost("DefaultEkranYetkileri")]
        public async Task<ActionResult> CreateDefaultEkranYetkileri()
        {
            try
            {
                Console.WriteLine("=== CreateDefaultEkranYetkileri started ===");
                
                // Check if screen permissions already exist
                var existingScreens = await _context.EkranYetkileri.CountAsync();
                Console.WriteLine($"Existing screens count: {existingScreens}");
                
                if (existingScreens > 0)
                {
                    Console.WriteLine("Screens already exist, returning BadRequest");
                    return BadRequest(new { success = false, message = "Ekran yetkileri zaten mevcut.", existingCount = existingScreens });
                }

                // Create default screen permissions
                var ekranYetkileri = new List<EkranYetkisi>
                {
                    new EkranYetkisi { EkranAdi = "Dashboard", EkranKodu = "dashboard", Aciklama = "Ana sayfa ve raporlar" },
                    new EkranYetkisi { EkranAdi = "Personeller", EkranKodu = "personeller", Aciklama = "Personel yönetimi" },
                    new EkranYetkisi { EkranAdi = "Departmanlar", EkranKodu = "departmanlar", Aciklama = "Departman yönetimi" },
                    new EkranYetkisi { EkranAdi = "Pozisyonlar", EkranKodu = "pozisyonlar", Aciklama = "Pozisyon yönetimi" },
                    new EkranYetkisi { EkranAdi = "Kademeler", EkranKodu = "kademeler", Aciklama = "Kademe yönetimi" },
                    new EkranYetkisi { EkranAdi = "İzin Talepleri", EkranKodu = "izin-talepleri", Aciklama = "İzin talebi yönetimi" },
                    new EkranYetkisi { EkranAdi = "İzin Takvimi", EkranKodu = "izin-takvimi", Aciklama = "İzin takvimi görüntüleme" },
                    new EkranYetkisi { EkranAdi = "Eğitimler", EkranKodu = "egitimler", Aciklama = "Eğitim yönetimi" },
                    new EkranYetkisi { EkranAdi = "Bordrolar", EkranKodu = "bordrolar", Aciklama = "Bordro yönetimi" },
                    new EkranYetkisi { EkranAdi = "Profil", EkranKodu = "profil", Aciklama = "Kullanıcı profili" }
                };

                _context.EkranYetkileri.AddRange(ekranYetkileri);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Varsayılan ekran yetkileri oluşturuldu.", count = ekranYetkileri.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Varsayılan ekran yetkileri oluşturulurken hata: {ex.Message}" });
            }
        }

        // POST: api/Yetki/DefaultKademeYetkileri
        [HttpPost("DefaultKademeYetkileri")]
        public async Task<ActionResult> CreateDefaultKademeYetkileri()
        {
            try
            {
                // Get all kademeler and screen permissions
                var kademeler = await _context.Kademeler.ToListAsync();
                var ekranYetkileri = await _context.EkranYetkileri.ToListAsync();

                if (!kademeler.Any() || !ekranYetkileri.Any())
                {
                    return BadRequest(new { success = false, message = "Önce kademeler ve ekran yetkileri oluşturulmalı." });
                }

                var kademeYetkileri = new List<KademeEkranYetkisi>();

                foreach (var kademe in kademeler)
                {
                    foreach (var ekran in ekranYetkileri)
                    {
                        // Check if permission already exists
                        var existingYetki = await _context.KademeEkranYetkileri
                            .FirstOrDefaultAsync(k => k.KademeId == kademe.Id && k.EkranYetkisiId == ekran.Id);

                        if (existingYetki == null)
                        {
                            var yetki = new KademeEkranYetkisi
                            {
                                KademeId = kademe.Id,
                                EkranYetkisiId = ekran.Id,
                                OkumaYetkisi = true, // Everyone can read by default
                                YazmaYetkisi = kademe.Seviye <= 3, // Only top 3 levels can write
                                SilmeYetkisi = kademe.Seviye <= 2, // Only top 2 levels can delete
                                GuncellemeYetkisi = kademe.Seviye <= 3 // Only top 3 levels can update
                            };

                            kademeYetkileri.Add(yetki);
                        }
                    }
                }

                if (kademeYetkileri.Any())
                {
                    _context.KademeEkranYetkileri.AddRange(kademeYetkileri);
                    await _context.SaveChangesAsync();
                }

                return Ok(new { success = true, message = $"Varsayılan kademe yetkileri oluşturuldu. Toplam {kademeYetkileri.Count} yetki.", count = kademeYetkileri.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Varsayılan kademe yetkileri oluşturulurken hata: {ex.Message}" });
            }
        }

        // POST: api/Yetki/UpdateIzinYetkileri
        [HttpPost("UpdateIzinYetkileri")]
        public async Task<ActionResult> UpdateIzinYetkileri()
        {
            try
            {
                // İzin Talepleri ve İzin Takvimi ekranları için tüm kademelere tam yetki ver
                var izinEkranKodlari = new[] { "izin-talepleri", "izin-takvimi" };
                
                var izinEkranlari = await _context.EkranYetkileri
                    .Where(e => izinEkranKodlari.Contains(e.EkranKodu))
                    .ToListAsync();

                if (!izinEkranlari.Any())
                {
                    return BadRequest(new { success = false, message = "İzin ekranları bulunamadı." });
                }

                var izinYetkileri = await _context.KademeEkranYetkileri
                    .Where(k => izinEkranlari.Select(e => e.Id).Contains(k.EkranYetkisiId))
                    .ToListAsync();

                foreach (var yetki in izinYetkileri)
                {
                    yetki.OkumaYetkisi = true;
                    yetki.YazmaYetkisi = true;
                    yetki.SilmeYetkisi = true;
                    yetki.GuncellemeYetkisi = true;
                    yetki.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = $"İzin ekranları için {izinYetkileri.Count} yetki güncellendi.", count = izinYetkileri.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"İzin yetkileri güncellenirken hata: {ex.Message}" });
            }
        }

        // POST: api/Yetki/AddZimmetEkrani
        [HttpPost("AddZimmetEkrani")]
        public async Task<ActionResult> AddZimmetEkrani()
        {
            try
            {
                // Zimmet İşlemleri ekranının zaten var olup olmadığını kontrol et
                var mevcutEkran = await _context.EkranYetkileri
                    .FirstOrDefaultAsync(e => e.EkranKodu == "zimmet");

                if (mevcutEkran != null)
                {
                    // Ekran mevcut, ama Genel Müdür yetkisi var mı kontrol et
                    var genelMudurKademe2 = await _context.Kademeler
                        .FirstOrDefaultAsync(k => k.Aktif && k.Seviye == 1);

                    if (genelMudurKademe2 != null)
                    {
                        var mevcutYetki = await _context.KademeEkranYetkileri
                            .FirstOrDefaultAsync(k => k.KademeId == genelMudurKademe2.Id && k.EkranYetkisiId == mevcutEkran.Id);

                        if (mevcutYetki == null)
                        {
                            // Genel Müdür yetkisi yoksa oluştur
                            var yetki = new KademeEkranYetkisi
                            {
                                KademeId = genelMudurKademe2.Id,
                                EkranYetkisiId = mevcutEkran.Id,
                                OkumaYetkisi = true,
                                YazmaYetkisi = true,
                                GuncellemeYetkisi = true,
                                SilmeYetkisi = true,
                                Aktif = true,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };

                            _context.KademeEkranYetkileri.Add(yetki);
                            await _context.SaveChangesAsync();

                            return Ok(new { success = true, message = "Zimmet İşlemleri ekranı zaten mevcut. Genel Müdür yetkisi eklendi." });
                        }
                    }
                    return Ok(new { success = true, message = "Zimmet İşlemleri ekranı zaten mevcut." });
                }

                // Yeni ekran yetkisi oluştur
                var yeniEkran = new EkranYetkisi
                {
                    EkranAdi = "Zimmet İşlemleri",
                    EkranKodu = "zimmet",
                    Aciklama = "Personel zimmet yönetimi ve takibi",
                    Aktif = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.EkranYetkileri.Add(yeniEkran);
                await _context.SaveChangesAsync();

                // Sadece Genel Müdür (seviye 1) için bu ekrana tam yetki ver
                var genelMudurKademe = await _context.Kademeler
                    .FirstOrDefaultAsync(k => k.Aktif && k.Seviye == 1);

                if (genelMudurKademe != null)
                {
                    var yetki = new KademeEkranYetkisi
                    {
                        KademeId = genelMudurKademe.Id,
                        EkranYetkisiId = yeniEkran.Id,
                        OkumaYetkisi = true,
                        YazmaYetkisi = true,
                        GuncellemeYetkisi = true,
                        SilmeYetkisi = true,
                        Aktif = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.KademeEkranYetkileri.Add(yetki);
                    await _context.SaveChangesAsync();

                    return Ok(new { 
                        success = true, 
                        message = $"Zimmet İşlemleri ekranı oluşturuldu ve Genel Müdür yetkisi atandı.",
                        ekranId = yeniEkran.Id
                    });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Genel Müdür kademesi bulunamadı." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Zimmet İşlemleri ekranı oluşturulurken hata: {ex.Message}" });
            }
        }

        // POST: api/Yetki/AddBekleyenIzinEkrani
        [HttpPost("AddBekleyenIzinEkrani")]
        public async Task<ActionResult> AddBekleyenIzinEkrani()
        {
            try
            {
                // Bekleyen İzin Talepleri ekranının zaten var olup olmadığını kontrol et
                var mevcutEkran = await _context.EkranYetkileri
                    .FirstOrDefaultAsync(e => e.EkranKodu == "bekleyen-izin-talepleri");

                if (mevcutEkran != null)
                {
                    return Ok(new { success = true, message = "Bekleyen İzin Talepleri ekranı zaten mevcut." });
                }

                // Yeni ekran yetkisi oluştur
                var yeniEkran = new EkranYetkisi
                {
                    EkranAdi = "Bekleyen İzin Talepleri",
                    EkranKodu = "bekleyen-izin-talepleri",
                    Aciklama = "Onay bekleyen izin taleplerinin görüntülendiği ve onaylandığı ekran",
                    Aktif = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.EkranYetkileri.Add(yeniEkran);
                await _context.SaveChangesAsync();

                // Tüm kademeler için bu ekrana yetki ver (sadece Yönetici ve üstü kademeler için)
                var kademeler = await _context.Kademeler
                    .Where(k => k.Aktif && k.Seviye <= 6) // Şef (6) ve üstü kademeler
                    .ToListAsync();

                var kademeYetkileri = new List<KademeEkranYetkisi>();

                foreach (var kademe in kademeler)
                {
                    // Yetkiler kademe seviyesine göre ayarlanacak
                    // Genel Müdür (1) -> Tam yetki
                    // Diğer kademeler -> Okuma ve güncelleme yetkisi (onay/red için)
                    var yetki = new KademeEkranYetkisi
                    {
                        KademeId = kademe.Id,
                        EkranYetkisiId = yeniEkran.Id,
                        OkumaYetkisi = true,
                        YazmaYetkisi = false, // Yeni talep oluşturamaz
                        GuncellemeYetkisi = true, // Onay/red yapabilir
                        SilmeYetkisi = kademe.Seviye == 1, // Sadece Genel Müdür silebilir
                        Aktif = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    kademeYetkileri.Add(yetki);
                }

                _context.KademeEkranYetkileri.AddRange(kademeYetkileri);
                await _context.SaveChangesAsync();

                return Ok(new { 
                    success = true, 
                    message = $"Bekleyen İzin Talepleri ekranı oluşturuldu ve {kademeYetkileri.Count} kademe yetkisi atandı.",
                    ekranId = yeniEkran.Id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Bekleyen İzin Talepleri ekranı oluşturulurken hata: {ex.Message}" });
            }
        }
    }
}