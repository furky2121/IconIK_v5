using Microsoft.AspNetCore.Mvc;
using IconIK.API.Models;
using IconIK.API.Services;
using System.Text.Json;

namespace IconIK.API.Controllers
{
    [Route("api/izintipi")]
    [ApiController]
    public class IzinTipiController : ControllerBase
    {
        private readonly IIzinKonfigurasyonService _izinKonfigurasyonService;
        private readonly ILogger<IzinTipiController> _logger;

        public IzinTipiController(IIzinKonfigurasyonService izinKonfigurasyonService, ILogger<IzinTipiController> logger)
        {
            _izinKonfigurasyonService = izinKonfigurasyonService;
            _logger = logger;
        }

        // GET: api/izintipi
        [HttpGet]
        public async Task<ActionResult> GetIzinTipleri()
        {
            try
            {
                var izinTipleri = await _izinKonfigurasyonService.GetAllIzinTipleri();
                return Ok(new { success = true, data = izinTipleri, message = "İzin tipleri başarıyla listelendi." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving leave types");
                return StatusCode(500, new { success = false, message = "İzin tipleri listelenirken bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/izintipi/aktif
        [HttpGet("aktif")]
        public async Task<ActionResult> GetAktifIzinTipleri([FromQuery] string? cinsiyet = null)
        {
            try
            {
                IEnumerable<IzinTipi> izinTipleri;

                if (!string.IsNullOrEmpty(cinsiyet))
                {
                    izinTipleri = await _izinKonfigurasyonService.GetAktifIzinTipleriByGender(cinsiyet);
                }
                else
                {
                    izinTipleri = await _izinKonfigurasyonService.GetAktifIzinTipleri();
                }

                return Ok(new { success = true, data = izinTipleri, message = "Aktif izin tipleri başarıyla listelendi." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active leave types");
                return StatusCode(500, new { success = false, message = "Aktif izin tipleri listelenirken bir hata oluştu.", error = ex.Message });
            }
        }

        // GET: api/izintipi/5
        [HttpGet("{id}")]
        public async Task<ActionResult> GetIzinTipi(int id)
        {
            try
            {
                var izinTipi = await _izinKonfigurasyonService.GetIzinTipiById(id);

                if (izinTipi == null)
                {
                    return NotFound(new { success = false, message = "İzin tipi bulunamadı." });
                }

                return Ok(new { success = true, data = izinTipi, message = "İzin tipi başarıyla getirildi." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving leave type by ID: {Id}", id);
                return StatusCode(500, new { success = false, message = "İzin tipi getirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        // POST: api/izintipi
        [HttpPost]
        public async Task<ActionResult> CreateIzinTipi([FromBody] JsonElement requestBody)
        {
            try
            {
                var izinTipi = new IzinTipi
                {
                    IzinTipiAdi = requestBody.GetProperty("izinTipiAdi").GetString() ?? string.Empty,
                    StandartGunSayisi = requestBody.TryGetProperty("standartGunSayisi", out var standartGun) && standartGun.ValueKind != JsonValueKind.Null
                        ? standartGun.GetInt32() : null,
                    MinimumGunSayisi = requestBody.TryGetProperty("minimumGunSayisi", out var minGun) && minGun.ValueKind != JsonValueKind.Null
                        ? minGun.GetInt32() : null,
                    MaksimumGunSayisi = requestBody.TryGetProperty("maksimumGunSayisi", out var maxGun) && maxGun.ValueKind != JsonValueKind.Null
                        ? maxGun.GetInt32() : null,
                    CinsiyetKisiti = requestBody.TryGetProperty("cinsiyetKisiti", out var cinsiyet) && cinsiyet.ValueKind != JsonValueKind.Null
                        ? cinsiyet.GetString() : null,
                    RaporGerekli = requestBody.TryGetProperty("raporGerekli", out var rapor) && rapor.GetBoolean(),
                    UcretliMi = requestBody.TryGetProperty("ucretliMi", out var ucretli) ? ucretli.GetBoolean() : true,
                    Renk = requestBody.TryGetProperty("renk", out var renk) && renk.ValueKind != JsonValueKind.Null
                        ? renk.GetString() : null,
                    Aciklama = requestBody.TryGetProperty("aciklama", out var aciklama) && aciklama.ValueKind != JsonValueKind.Null
                        ? aciklama.GetString() : null,
                    Sira = requestBody.TryGetProperty("sira", out var sira) ? sira.GetInt32() : 0,
                    Aktif = requestBody.TryGetProperty("aktif", out var aktif) ? aktif.GetBoolean() : true
                };

                var createdIzinTipi = await _izinKonfigurasyonService.CreateIzinTipi(izinTipi);
                return CreatedAtAction(nameof(GetIzinTipi), new { id = createdIzinTipi.Id },
                    new { success = true, data = createdIzinTipi, message = "İzin tipi başarıyla oluşturuldu." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating leave type");
                return StatusCode(500, new { success = false, message = "İzin tipi oluşturulurken bir hata oluştu.", error = ex.Message });
            }
        }

        // PUT: api/izintipi/5
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateIzinTipi(int id, [FromBody] JsonElement requestBody)
        {
            try
            {
                var izinTipi = new IzinTipi
                {
                    Id = id,
                    IzinTipiAdi = requestBody.GetProperty("izinTipiAdi").GetString() ?? string.Empty,
                    StandartGunSayisi = requestBody.TryGetProperty("standartGunSayisi", out var standartGun) && standartGun.ValueKind != JsonValueKind.Null
                        ? standartGun.GetInt32() : null,
                    MinimumGunSayisi = requestBody.TryGetProperty("minimumGunSayisi", out var minGun) && minGun.ValueKind != JsonValueKind.Null
                        ? minGun.GetInt32() : null,
                    MaksimumGunSayisi = requestBody.TryGetProperty("maksimumGunSayisi", out var maxGun) && maxGun.ValueKind != JsonValueKind.Null
                        ? maxGun.GetInt32() : null,
                    CinsiyetKisiti = requestBody.TryGetProperty("cinsiyetKisiti", out var cinsiyet) && cinsiyet.ValueKind != JsonValueKind.Null
                        ? cinsiyet.GetString() : null,
                    RaporGerekli = requestBody.TryGetProperty("raporGerekli", out var rapor) && rapor.GetBoolean(),
                    UcretliMi = requestBody.TryGetProperty("ucretliMi", out var ucretli) ? ucretli.GetBoolean() : true,
                    Renk = requestBody.TryGetProperty("renk", out var renk) && renk.ValueKind != JsonValueKind.Null
                        ? renk.GetString() : null,
                    Aciklama = requestBody.TryGetProperty("aciklama", out var aciklama) && aciklama.ValueKind != JsonValueKind.Null
                        ? aciklama.GetString() : null,
                    Sira = requestBody.TryGetProperty("sira", out var sira) ? sira.GetInt32() : 0,
                    Aktif = requestBody.TryGetProperty("aktif", out var aktif) ? aktif.GetBoolean() : true
                };

                var updatedIzinTipi = await _izinKonfigurasyonService.UpdateIzinTipi(izinTipi);
                return Ok(new { success = true, data = updatedIzinTipi, message = "İzin tipi başarıyla güncellendi." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating leave type ID: {Id}", id);
                return StatusCode(500, new { success = false, message = "İzin tipi güncellenirken bir hata oluştu.", error = ex.Message });
            }
        }

        // DELETE: api/izintipi/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteIzinTipi(int id)
        {
            try
            {
                var result = await _izinKonfigurasyonService.DeleteIzinTipi(id);

                if (!result)
                {
                    return NotFound(new { success = false, message = "İzin tipi bulunamadı." });
                }

                return Ok(new { success = true, message = "İzin tipi başarıyla silindi." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting leave type ID: {Id}", id);
                return StatusCode(500, new { success = false, message = "İzin tipi silinirken bir hata oluştu.", error = ex.Message });
            }
        }
    }
}
