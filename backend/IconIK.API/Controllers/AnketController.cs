using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using IconIK.API.Services;
using IconIK.API.Models;
using System.Text.Json;
using Npgsql;

namespace IconIK.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AnketController : ControllerBase
    {
        private readonly IAnketService _anketService;

        public AnketController(IAnketService anketService)
        {
            _anketService = anketService;
        }

        // ===== ANKET CRUD =====

        [HttpGet]
        public async Task<IActionResult> GetAllAnketler()
        {
            try
            {
                var anketler = await _anketService.GetAllAnketlerAsync();
                return Ok(new { success = true, data = anketler });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("Aktif")]
        public async Task<IActionResult> GetAktifAnketler()
        {
            try
            {
                var anketler = await _anketService.GetAktifAnketlerAsync();
                return Ok(new { success = true, data = anketler });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAnketById(int id)
        {
            try
            {
                var anket = await _anketService.GetAnketByIdAsync(id);
                if (anket == null)
                    return NotFound(new { success = false, message = "Anket bulunamadı" });

                return Ok(new { success = true, data = anket });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateAnket([FromBody] JsonElement anketData)
        {
            try
            {
                var anket = new Anket
                {
                    Baslik = anketData.GetProperty("baslik").GetString() ?? "",
                    Aciklama = anketData.TryGetProperty("aciklama", out var aciklama) ? aciklama.GetString() : null,
                    BaslangicTarihi = anketData.GetProperty("baslangicTarihi").GetDateTime(),
                    BitisTarihi = anketData.GetProperty("bitisTarihi").GetDateTime(),
                    AnketDurumu = anketData.GetProperty("anketDurumu").GetString() ?? "Taslak",
                    AnonymousMu = anketData.TryGetProperty("anonymousMu", out var anonymous) && anonymous.GetBoolean(),
                    OlusturanPersonelId = anketData.GetProperty("olusturanPersonelId").GetInt32(),
                    Aktif = true
                };

                // Soruları ekle
                if (anketData.TryGetProperty("sorular", out var sorularElement))
                {
                    anket.Sorular = new List<AnketSoru>();
                    var sorular = sorularElement.EnumerateArray();

                    foreach (var soruElement in sorular)
                    {
                        var soru = new AnketSoru
                        {
                            SoruMetni = soruElement.GetProperty("soruMetni").GetString() ?? "",
                            SoruTipi = soruElement.GetProperty("soruTipi").GetString() ?? "TekSecim",
                            Sira = soruElement.TryGetProperty("sira", out var sira) ? sira.GetInt32() : 0,
                            ZorunluMu = soruElement.TryGetProperty("zorunluMu", out var zorunlu) && zorunlu.GetBoolean(),
                            Aktif = true
                        };

                        // Seçenekleri ekle (çoktan seçmeli sorular için)
                        if (soruElement.TryGetProperty("secenekler", out var seceneklerElement))
                        {
                            soru.Secenekler = new List<AnketSecenek>();
                            var secenekler = seceneklerElement.EnumerateArray();

                            foreach (var secenekElement in secenekler)
                            {
                                var secenek = new AnketSecenek
                                {
                                    SecenekMetni = secenekElement.GetProperty("secenekMetni").GetString() ?? "",
                                    Sira = secenekElement.TryGetProperty("sira", out var secSira) ? secSira.GetInt32() : 0,
                                    Aktif = true
                                };
                                soru.Secenekler.Add(secenek);
                            }
                        }

                        anket.Sorular.Add(soru);
                    }
                }

                var createdAnket = await _anketService.CreateAnketAsync(anket);
                return Ok(new { success = true, data = createdAnket, message = "Anket başarıyla oluşturuldu" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAnket(int id, [FromBody] JsonElement anketData)
        {
            try
            {
                var existingAnket = await _anketService.GetAnketByIdAsync(id);
                if (existingAnket == null)
                    return NotFound(new { success = false, message = "Anket bulunamadı" });

                existingAnket.Baslik = anketData.GetProperty("baslik").GetString() ?? existingAnket.Baslik;
                existingAnket.Aciklama = anketData.TryGetProperty("aciklama", out var aciklama) ? aciklama.GetString() : existingAnket.Aciklama;
                existingAnket.BaslangicTarihi = anketData.GetProperty("baslangicTarihi").GetDateTime();
                existingAnket.BitisTarihi = anketData.GetProperty("bitisTarihi").GetDateTime();
                existingAnket.AnketDurumu = anketData.GetProperty("anketDurumu").GetString() ?? existingAnket.AnketDurumu;
                existingAnket.AnonymousMu = anketData.TryGetProperty("anonymousMu", out var anonymous) && anonymous.GetBoolean();
                existingAnket.Aktif = anketData.TryGetProperty("aktif", out var aktif) && aktif.GetBoolean();

                var updatedAnket = await _anketService.UpdateAnketAsync(existingAnket);
                return Ok(new { success = true, data = updatedAnket, message = "Anket başarıyla güncellendi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnket(int id)
        {
            try
            {
                var result = await _anketService.DeleteAnketAsync(id);
                if (!result)
                    return NotFound(new { success = false, message = "Anket bulunamadı" });

                return Ok(new { success = true, message = "Anket başarıyla silindi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // ===== ANKET ATAMA =====

        [HttpGet("{anketId}/Atamalar")]
        public async Task<IActionResult> GetAnketAtamalari(int anketId)
        {
            try
            {
                var atamalar = await _anketService.GetAnketAtamalariAsync(anketId);
                return Ok(new { success = true, data = atamalar });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("Atama")]
        public async Task<IActionResult> CreateAtama([FromBody] JsonElement atamaData)
        {
            try
            {
                var atama = new AnketAtama
                {
                    AnketId = atamaData.GetProperty("anketId").GetInt32(),
                    PersonelId = atamaData.TryGetProperty("personelId", out var personelId) && personelId.ValueKind != JsonValueKind.Null
                        ? personelId.GetInt32() : null,
                    DepartmanId = atamaData.TryGetProperty("departmanId", out var departmanId) && departmanId.ValueKind != JsonValueKind.Null
                        ? departmanId.GetInt32() : null,
                    PozisyonId = atamaData.TryGetProperty("pozisyonId", out var pozisyonId) && pozisyonId.ValueKind != JsonValueKind.Null
                        ? pozisyonId.GetInt32() : null,
                    AtayanPersonelId = atamaData.GetProperty("atayanPersonelId").GetInt32(),
                    Not = atamaData.TryGetProperty("not", out var not) ? not.GetString() : null
                };

                var createdAtama = await _anketService.CreateAtamaAsync(atama);
                return Ok(new { success = true, data = createdAtama, message = "Atama başarıyla oluşturuldu" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("Atama/{atamaId}")]
        public async Task<IActionResult> DeleteAtama(int atamaId)
        {
            try
            {
                var result = await _anketService.DeleteAtamaAsync(atamaId);
                if (!result)
                    return NotFound(new { success = false, message = "Atama bulunamadı" });

                return Ok(new { success = true, message = "Atama başarıyla silindi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("BanaAtananlar/{personelId}")]
        public async Task<IActionResult> GetBanaAtananAnketler(int personelId)
        {
            try
            {
                var anketler = await _anketService.GetBanaAtananAnketlerAsync(personelId);
                return Ok(new { success = true, data = anketler });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // ===== ANKET CEVAPLAMA =====

        [HttpGet("{anketId}/Katilim/{personelId}")]
        public async Task<IActionResult> GetKatilim(int anketId, int personelId)
        {
            try
            {
                var katilim = await _anketService.GetKatilimAsync(anketId, personelId);
                return Ok(new { success = true, data = katilim });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("{anketId}/Baslat/{personelId}")]
        public async Task<IActionResult> BaslatAnket(int anketId, int personelId)
        {
            try
            {
                var katilim = await _anketService.BaslatAsync(anketId, personelId);
                return Ok(new { success = true, data = katilim, message = "Anket başlatıldı" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("{anketId}/Cevapla/{personelId}")]
        public async Task<IActionResult> CevapKaydet(int anketId, int personelId, [FromBody] JsonElement cevaplarData)
        {
            try
            {
                var cevaplar = new List<AnketCevap>();
                var cevaplarArray = cevaplarData.GetProperty("cevaplar").EnumerateArray();

                foreach (var cevapElement in cevaplarArray)
                {
                    var cevap = new AnketCevap
                    {
                        SoruId = cevapElement.GetProperty("soruId").GetInt32(),
                        SecenekId = cevapElement.TryGetProperty("secenekId", out var secenekId) && secenekId.ValueKind != JsonValueKind.Null
                            ? secenekId.GetInt32() : null,
                        AcikCevap = cevapElement.TryGetProperty("acikCevap", out var acikCevap) ? acikCevap.GetString() : null
                    };
                    cevaplar.Add(cevap);
                }

                var savedCevaplar = await _anketService.CevapKaydetAsync(anketId, personelId, cevaplar);
                return Ok(new { success = true, data = savedCevaplar, message = "Cevaplar kaydedildi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("{anketId}/Tamamla/{personelId}")]
        public async Task<IActionResult> TamamlaAnket(int anketId, int personelId)
        {
            try
            {
                var result = await _anketService.TamamlaAsync(anketId, personelId);
                if (!result)
                    return NotFound(new { success = false, message = "Katılım bulunamadı" });

                return Ok(new { success = true, message = "Anket başarıyla tamamlandı" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // ===== ANKET SONUÇLARI =====

        [HttpGet("{anketId}/Sonuclar")]
        public async Task<IActionResult> GetAnketSonuclari(int anketId)
        {
            try
            {
                var sonuclar = await _anketService.GetAnketSonuclariAsync(anketId);
                if (sonuclar == null)
                    return NotFound(new { success = false, message = "Anket bulunamadı" });

                return Ok(new { success = true, data = sonuclar });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{anketId}/Cevaplar")]
        public async Task<IActionResult> GetAnketCevaplari(int anketId)
        {
            try
            {
                var cevaplar = await _anketService.GetAnketCevaplariAsync(anketId);
                return Ok(new { success = true, data = cevaplar });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{anketId}/KatilimIstatistikleri")]
        public async Task<IActionResult> GetKatilimIstatistikleri(int anketId)
        {
            try
            {
                var istatistikler = await _anketService.GetKatilimIstatistikleriAsync(anketId);
                return Ok(new { success = true, data = istatistikler });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
