using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IconIK.API.Data;
using IconIK.API.Models;

namespace IconIK.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EgitimController : ControllerBase
    {
        private readonly IconIKContext _context;

        public EgitimController(IconIKContext context)
        {
            _context = context;
        }

        // GET: api/Egitim
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetEgitimler()
        {
            try
            {
                var egitimler = await _context.Egitimler
                    .Include(e => e.PersonelEgitimleri)
                        .ThenInclude(pe => pe.Personel)
                    .OrderByDescending(e => e.CreatedAt)
                    .Select(e => new
                    {
                        e.Id,
                        e.Baslik,
                        e.Aciklama,
                        e.BaslangicTarihi,
                        e.BitisTarihi,
                        e.SureSaat,
                        e.Egitmen,
                        e.Konum,
                        e.Kapasite,
                        e.Durum,
                        e.CreatedAt,
                        e.UpdatedAt,
                        KatilimciSayisi = e.PersonelEgitimleri.Count,
                        Katilimcilar = e.PersonelEgitimleri.Select(pe => new
                        {
                            pe.PersonelId,
                            PersonelAd = pe.Personel.Ad + " " + pe.Personel.Soyad,
                            pe.KatilimDurumu,
                            pe.Puan
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = egitimler, message = "Eğitimler başarıyla listelendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Eğitimler listelenirken bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/Egitim/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetEgitim(int id)
        {
            try
            {
                var egitim = await _context.Egitimler
                    .Include(e => e.PersonelEgitimleri)
                        .ThenInclude(pe => pe.Personel)
                            .ThenInclude(p => p.Pozisyon)
                                .ThenInclude(pos => pos.Departman)
                    .Where(e => e.Id == id)
                    .Select(e => new
                    {
                        e.Id,
                        e.Baslik,
                        e.Aciklama,
                        e.BaslangicTarihi,
                        e.BitisTarihi,
                        e.SureSaat,
                        e.Egitmen,
                        e.Konum,
                        e.Kapasite,
                        e.Durum,
                        e.CreatedAt,
                        e.UpdatedAt,
                        Katilimcilar = e.PersonelEgitimleri.Select(pe => new
                        {
                            pe.Id,
                            pe.PersonelId,
                            PersonelAd = pe.Personel.Ad + " " + pe.Personel.Soyad,
                            PersonelDepartman = pe.Personel.Pozisyon.Departman.Ad,
                            PersonelEmail = pe.Personel.Email,
                            pe.KatilimDurumu,
                            pe.Puan,
                            pe.SertifikaUrl
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (egitim == null)
                {
                    return NotFound(new { success = false, message = "Eğitim bulunamadı." });
                }

                return Ok(new { success = true, data = egitim, message = "Eğitim başarıyla getirildi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Eğitim getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/Egitim/PersonelEgitimleri/5
        [HttpGet("PersonelEgitimleri/{personelId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetPersonelEgitimleri(int personelId)
        {
            try
            {
                var personelEgitimleri = await _context.PersonelEgitimleri
                    .Include(pe => pe.Egitim)
                    .Where(pe => pe.PersonelId == personelId)
                    .OrderByDescending(pe => pe.Egitim.BaslangicTarihi)
                    .Select(pe => new
                    {
                        pe.Id,
                        EgitimId = pe.Egitim.Id,
                        EgitimBaslik = pe.Egitim.Baslik,
                        EgitimTarih = pe.Egitim.BaslangicTarihi,
                        EgitimSure = pe.Egitim.SureSaat,
                        EgitimDurum = pe.Egitim.Durum,
                        pe.KatilimDurumu,
                        pe.Puan,
                        pe.SertifikaUrl,
                        pe.CreatedAt
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = personelEgitimleri, message = "Personel eğitimleri başarıyla listelendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Personel eğitimleri listelenirken bir hata oluştu.", error = ex.Message });
            }
        }

        // POST: api/Egitim
        [HttpPost]
        public async Task<ActionResult<object>> PostEgitim(Egitim egitim)
        {
            try
            {
                // Tarih kontrolü
                if (egitim.BaslangicTarihi >= egitim.BitisTarihi)
                {
                    return BadRequest(new { success = false, message = "Bitiş tarihi, başlangıç tarihinden sonra olmalıdır." });
                }

                egitim.CreatedAt = DateTime.UtcNow;
                egitim.UpdatedAt = DateTime.UtcNow;

                _context.Egitimler.Add(egitim);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetEgitim", new { id = egitim.Id },
                    new { success = true, data = new { egitim.Id }, message = "Eğitim başarıyla oluşturuldu." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Eğitim oluşturulurken bir hata oluştu.", error = ex.Message });
            }
        }

        // PUT: api/Egitim/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEgitim(int id, Egitim egitim)
        {
            if (id != egitim.Id)
            {
                return BadRequest(new { success = false, message = "Geçersiz eğitim ID." });
            }

            try
            {
                // Tarih kontrolü
                if (egitim.BaslangicTarihi >= egitim.BitisTarihi)
                {
                    return BadRequest(new { success = false, message = "Bitiş tarihi, başlangıç tarihinden sonra olmalıdır." });
                }

                egitim.UpdatedAt = DateTime.UtcNow;
                _context.Entry(egitim).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Eğitim başarıyla güncellendi." });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EgitimExists(id))
                {
                    return NotFound(new { success = false, message = "Eğitim bulunamadı." });
                }
                throw;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Eğitim güncellenirken bir hata oluştu.", error = ex.Message });
            }
        }

        // POST: api/Egitim/PersonelAta/5
        [HttpPost("PersonelAta/{egitimId}")]
        public async Task<IActionResult> PersonelAta(int egitimId, [FromBody] PersonelAtaRequest request)
        {
            try
            {
                var egitim = await _context.Egitimler
                    .Include(e => e.PersonelEgitimleri)
                    .FirstOrDefaultAsync(e => e.Id == egitimId);

                if (egitim == null)
                {
                    return NotFound(new { success = false, message = "Eğitim bulunamadı." });
                }

                // Kapasite kontrolü
                if (egitim.Kapasite.HasValue && egitim.PersonelEgitimleri.Count >= egitim.Kapasite)
                {
                    return BadRequest(new { success = false, message = "Eğitim kapasitesi dolmuştur." });
                }

                var atamaList = new List<PersonelEgitimi>();

                foreach (var personelId in request.PersonelIdler)
                {
                    // Zaten atanmış mı kontrol et
                    var mevcutAtama = await _context.PersonelEgitimleri
                        .FirstOrDefaultAsync(pe => pe.EgitimId == egitimId && pe.PersonelId == personelId);

                    if (mevcutAtama == null)
                    {
                        // Personel var mı kontrol et
                        var personelExists = await _context.Personeller.AnyAsync(p => p.Id == personelId && p.Aktif);
                        if (personelExists)
                        {
                            atamaList.Add(new PersonelEgitimi
                            {
                                EgitimId = egitimId,
                                PersonelId = personelId,
                                KatilimDurumu = "Atandı",
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            });
                        }
                    }
                }

                if (atamaList.Any())
                {
                    _context.PersonelEgitimleri.AddRange(atamaList);
                    await _context.SaveChangesAsync();

                    return Ok(new { success = true, message = $"{atamaList.Count} personel eğitime başarıyla atandı." });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Atanacak yeni personel bulunamadı." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Personel atanırken bir hata oluştu.", error = ex.Message });
            }
        }

        // PUT: api/Egitim/KatilimGuncelle/5
        [HttpPut("KatilimGuncelle/{personelEgitimId}")]
        public async Task<IActionResult> KatilimGuncelle(int personelEgitimId, [FromBody] KatilimGuncelleRequest request)
        {
            try
            {
                var personelEgitimi = await _context.PersonelEgitimleri.FindAsync(personelEgitimId);
                if (personelEgitimi == null)
                {
                    return NotFound(new { success = false, message = "Personel eğitimi bulunamadı." });
                }

                personelEgitimi.KatilimDurumu = request.KatilimDurumu;
                personelEgitimi.Puan = request.Puan;
                personelEgitimi.SertifikaUrl = request.SertifikaUrl;
                personelEgitimi.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Katılım durumu başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Katılım durumu güncellenirken bir hata oluştu.", error = ex.Message });
            }
        }

        // DELETE: api/Egitim/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEgitim(int id)
        {
            try
            {
                var egitim = await _context.Egitimler
                    .Include(e => e.PersonelEgitimleri)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (egitim == null)
                {
                    return NotFound(new { success = false, message = "Eğitim bulunamadı." });
                }

                // Eğer eğitim başlamışsa silinmesine izin verme
                if (egitim.Durum == "Devam Ediyor" || egitim.Durum == "Tamamlandı")
                {
                    return BadRequest(new { success = false, message = "Başlamış veya tamamlanmış eğitimler silinemez." });
                }

                // İlişkili personel eğitimlerini de sil
                _context.PersonelEgitimleri.RemoveRange(egitim.PersonelEgitimleri);
                _context.Egitimler.Remove(egitim);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Eğitim başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Eğitim silinirken bir hata oluştu.", error = ex.Message });
            }
        }

        // DELETE: api/Egitim/PersonelCikar/5
        [HttpDelete("PersonelCikar/{personelEgitimId}")]
        public async Task<IActionResult> PersonelCikar(int personelEgitimId)
        {
            try
            {
                var personelEgitimi = await _context.PersonelEgitimleri
                    .Include(pe => pe.Egitim)
                    .FirstOrDefaultAsync(pe => pe.Id == personelEgitimId);

                if (personelEgitimi == null)
                {
                    return NotFound(new { success = false, message = "Personel eğitimi bulunamadı." });
                }

                // Eğitim tamamlandıysa çıkarılamaz
                if (personelEgitimi.Egitim.Durum == "Tamamlandı" && personelEgitimi.KatilimDurumu == "Tamamladı")
                {
                    return BadRequest(new { success = false, message = "Tamamlanmış eğitimlerden personel çıkarılamaz." });
                }

                _context.PersonelEgitimleri.Remove(personelEgitimi);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Personel eğitimden başarıyla çıkarıldı." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Personel çıkarılırken bir hata oluştu.", error = ex.Message });
            }
        }

        private bool EgitimExists(int id)
        {
            return _context.Egitimler.Any(e => e.Id == id);
        }
    }

    public class PersonelAtaRequest
    {
        public List<int> PersonelIdler { get; set; } = new List<int>();
    }

    public class KatilimGuncelleRequest
    {
        public string KatilimDurumu { get; set; } = string.Empty;
        public int? Puan { get; set; }
        public string? SertifikaUrl { get; set; }
    }
}