using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IconIK.API.Data;
using IconIK.API.Models;
using System.Text.Json;

namespace IconIK.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ZimmetController : ControllerBase
    {
        private readonly IconIKContext _context;

        public ZimmetController(IconIKContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var zimmetler = await _context.Zimmetler
                    .Include(z => z.Personel)
                        .ThenInclude(p => p!.Pozisyon)
                            .ThenInclude(pos => pos!.Departman)
                    .Include(z => z.Personel)
                        .ThenInclude(p => p!.Pozisyon)
                            .ThenInclude(pos => pos!.Kademe)
                    .OrderByDescending(z => z.OlusturmaTarihi)
                    .Select(z => new
                    {
                        z.Id,
                        z.PersonelId,
                        PersonelAdSoyad = z.Personel!.Ad + " " + z.Personel.Soyad,
                        DepartmanAd = z.Personel.Pozisyon!.Departman!.Ad,
                        KademeAd = z.Personel.Pozisyon.Kademe!.Ad,
                        z.DokumanNo,
                        z.ZimmetTarihi,
                        z.GsmHat,
                        z.GsmHatDetay,
                        z.Monitor,
                        z.MonitorDetay,
                        z.OfisTelefonu,
                        z.OfisTelefonuDetay,
                        z.CepTelefonu,
                        z.CepTelefonuDetay,
                        z.DizustuBilgisayar,
                        z.DizustuBilgisayarDetay,
                        z.YemekKarti,
                        z.YemekKartiDetay,
                        z.Klavye,
                        z.Mouse,
                        z.BilgisayarCantasi,
                        z.BilgisayarCantasiDetay,
                        z.ErisimYetkileri,
                        z.TeslimAlmaNotlari,
                        z.TeslimEden,
                        z.Hazirlayan,
                        z.Onaylayan,
                        z.TeslimDurumu,
                        z.TeslimTarihi,
                        z.Aktif,
                        z.OlusturmaTarihi,
                        zimmetMalzemeleri = _context.ZimmetMalzemeleri
                            .Include(zm => zm.ZimmetStok)
                            .Where(zm => zm.ZimmetId == z.Id)
                            .Select(zm => new {
                                zm.Id,
                                zm.ZimmetStokId,
                                zm.Miktar,
                                MalzemeAdi = zm.ZimmetStok!.MalzemeAdi,
                                Kategori = zm.ZimmetStok.Kategori,
                                Marka = zm.ZimmetStok.Marka,
                                Model = zm.ZimmetStok.Model
                            })
                            .ToList()
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = zimmetler });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("Aktif")]
        public async Task<IActionResult> GetAktif()
        {
            try
            {
                var zimmetler = await _context.Zimmetler
                    .Include(z => z.Personel)
                        .ThenInclude(p => p!.Pozisyon)
                            .ThenInclude(pos => pos!.Departman)
                    .Include(z => z.Personel)
                        .ThenInclude(p => p!.Pozisyon)
                            .ThenInclude(pos => pos!.Kademe)
                    .Where(z => z.Aktif)
                    .OrderByDescending(z => z.OlusturmaTarihi)
                    .Select(z => new
                    {
                        z.Id,
                        z.PersonelId,
                        PersonelAdSoyad = z.Personel!.Ad + " " + z.Personel.Soyad,
                        DepartmanAd = z.Personel.Pozisyon!.Departman!.Ad,
                        KademeAd = z.Personel.Pozisyon.Kademe!.Ad,
                        z.DokumanNo,
                        z.ZimmetTarihi,
                        z.GsmHat,
                        z.GsmHatDetay,
                        z.Monitor,
                        z.MonitorDetay,
                        z.OfisTelefonu,
                        z.OfisTelefonuDetay,
                        z.CepTelefonu,
                        z.CepTelefonuDetay,
                        z.DizustuBilgisayar,
                        z.DizustuBilgisayarDetay,
                        z.YemekKarti,
                        z.YemekKartiDetay,
                        z.Klavye,
                        z.Mouse,
                        z.BilgisayarCantasi,
                        z.BilgisayarCantasiDetay,
                        z.ErisimYetkileri,
                        z.TeslimAlmaNotlari,
                        z.TeslimEden,
                        z.Hazirlayan,
                        z.Onaylayan,
                        z.TeslimDurumu,
                        z.TeslimTarihi,
                        z.Aktif,
                        z.OlusturmaTarihi
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = zimmetler });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var zimmet = await _context.Zimmetler
                    .Include(z => z.Personel)
                        .ThenInclude(p => p!.Pozisyon)
                            .ThenInclude(pos => pos!.Departman)
                    .Include(z => z.Personel)
                        .ThenInclude(p => p!.Pozisyon)
                            .ThenInclude(pos => pos!.Kademe)
                    .Where(z => z.Id == id)
                    .FirstOrDefaultAsync();

                if (zimmet == null)
                    return NotFound(new { success = false, message = "Zimmet kaydı bulunamadı" });

                // Get zimmet malzemeleri
                var zimmetMalzemeleri = await _context.ZimmetMalzemeleri
                    .Include(zm => zm.ZimmetStok)
                    .Where(zm => zm.ZimmetId == id)
                    .Select(zm => new
                    {
                        zm.Id,
                        zm.ZimmetStokId,
                        zm.Miktar,
                        MalzemeAdi = zm.ZimmetStok!.MalzemeAdi,
                        Kategori = zm.ZimmetStok.Kategori,
                        Marka = zm.ZimmetStok.Marka,
                        Model = zm.ZimmetStok.Model
                    })
                    .ToListAsync();

                var result = new
                {
                    zimmet.Id,
                    zimmet.PersonelId,
                    PersonelAdSoyad = zimmet.Personel!.Ad + " " + zimmet.Personel.Soyad,
                    DepartmanAd = zimmet.Personel.Pozisyon!.Departman!.Ad,
                    KademeAd = zimmet.Personel.Pozisyon.Kademe!.Ad,
                    zimmet.DokumanNo,
                    zimmet.ZimmetTarihi,
                    zimmet.GsmHat,
                    zimmet.GsmHatDetay,
                    zimmet.Monitor,
                    zimmet.MonitorDetay,
                    zimmet.OfisTelefonu,
                    zimmet.OfisTelefonuDetay,
                    zimmet.CepTelefonu,
                    zimmet.CepTelefonuDetay,
                    zimmet.DizustuBilgisayar,
                    zimmet.DizustuBilgisayarDetay,
                    zimmet.YemekKarti,
                    zimmet.YemekKartiDetay,
                    zimmet.Klavye,
                    zimmet.Mouse,
                    zimmet.BilgisayarCantasi,
                    zimmet.BilgisayarCantasiDetay,
                    zimmet.ErisimYetkileri,
                    zimmet.TeslimAlmaNotlari,
                    zimmet.TeslimEden,
                    zimmet.Hazirlayan,
                    zimmet.Onaylayan,
                    zimmet.TeslimDurumu,
                    zimmet.TeslimTarihi,
                    zimmet.Aktif,
                    zimmet.OlusturmaTarihi,
                    ZimmetMalzemeleri = zimmetMalzemeleri
                };

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] JsonElement requestData)
        {
            try
            {
                var zimmet = new Zimmet
                {
                    PersonelId = requestData.GetProperty("personelId").GetInt32(),
                    DokumanNo = requestData.TryGetProperty("dokumanNo", out var dokumanNo) 
                        ? dokumanNo.GetString() ?? "BLG.ZM.001" 
                        : "BLG.ZM.001",
                    ZimmetTarihi = requestData.TryGetProperty("zimmetTarihi", out var zimmetTarihi) 
                        ? DateTime.Parse(zimmetTarihi.GetString()!).ToUniversalTime() 
                        : DateTime.UtcNow,
                    GsmHat = requestData.TryGetProperty("gsmHat", out var gsmHat) && gsmHat.GetBoolean(),
                    GsmHatDetay = requestData.TryGetProperty("gsmHatDetay", out var gsmHatDetay) 
                        ? gsmHatDetay.GetString() 
                        : null,
                    Monitor = requestData.TryGetProperty("monitor", out var monitor) && monitor.GetBoolean(),
                    MonitorDetay = requestData.TryGetProperty("monitorDetay", out var monitorDetay) 
                        ? monitorDetay.GetString() 
                        : null,
                    OfisTelefonu = requestData.TryGetProperty("ofisTelefonu", out var ofisTelefonu) && ofisTelefonu.GetBoolean(),
                    OfisTelefonuDetay = requestData.TryGetProperty("ofisTelefonuDetay", out var ofisTelefonuDetay) 
                        ? ofisTelefonuDetay.GetString() 
                        : null,
                    CepTelefonu = requestData.TryGetProperty("cepTelefonu", out var cepTelefonu) && cepTelefonu.GetBoolean(),
                    CepTelefonuDetay = requestData.TryGetProperty("cepTelefonuDetay", out var cepTelefonuDetay) 
                        ? cepTelefonuDetay.GetString() 
                        : null,
                    DizustuBilgisayar = requestData.TryGetProperty("dizustuBilgisayar", out var dizustuBilgisayar) && dizustuBilgisayar.GetBoolean(),
                    DizustuBilgisayarDetay = requestData.TryGetProperty("dizustuBilgisayarDetay", out var dizustuBilgisayarDetay) 
                        ? dizustuBilgisayarDetay.GetString() 
                        : null,
                    YemekKarti = requestData.TryGetProperty("yemekKarti", out var yemekKarti) && yemekKarti.GetBoolean(),
                    YemekKartiDetay = requestData.TryGetProperty("yemekKartiDetay", out var yemekKartiDetay) 
                        ? yemekKartiDetay.GetString() 
                        : null,
                    Klavye = requestData.TryGetProperty("klavye", out var klavye) && klavye.GetBoolean(),
                    Mouse = requestData.TryGetProperty("mouse", out var mouseProp) && mouseProp.GetBoolean(),
                    BilgisayarCantasi = requestData.TryGetProperty("bilgisayarCantasi", out var bilgisayarCantasi) && bilgisayarCantasi.GetBoolean(),
                    BilgisayarCantasiDetay = requestData.TryGetProperty("bilgisayarCantasiDetay", out var bilgisayarCantasiDetay) 
                        ? bilgisayarCantasiDetay.GetString() 
                        : null,
                    ErisimYetkileri = requestData.TryGetProperty("erisimYetkileri", out var erisimYetkileri) 
                        ? erisimYetkileri.GetString() 
                        : null,
                    TeslimAlmaNotlari = requestData.TryGetProperty("teslimAlmaNotlari", out var teslimAlmaNotlari) 
                        ? teslimAlmaNotlari.GetString() 
                        : null,
                    TeslimEden = requestData.TryGetProperty("teslimEden", out var teslimEden) 
                        ? teslimEden.GetString() 
                        : null,
                    Hazirlayan = requestData.TryGetProperty("hazirlayan", out var hazirlayan) 
                        ? hazirlayan.GetString() ?? "ÇAĞLA YILDIRIM"
                        : "ÇAĞLA YILDIRIM",
                    Onaylayan = requestData.TryGetProperty("onaylayan", out var onaylayan) 
                        ? onaylayan.GetString() ?? "EMRE HACIEVLİYAGİL"
                        : "EMRE HACIEVLİYAGİL",
                    TeslimDurumu = requestData.TryGetProperty("teslimDurumu", out var teslimDurumu) && teslimDurumu.GetBoolean(),
                    TeslimTarihi = requestData.TryGetProperty("teslimTarihi", out var teslimTarihi) && !teslimTarihi.ValueKind.Equals(JsonValueKind.Null)
                        ? DateTime.Parse(teslimTarihi.GetString()!).ToUniversalTime()
                        : null,
                    Aktif = requestData.TryGetProperty("aktif", out var aktif) ? aktif.GetBoolean() : true
                };

                _context.Zimmetler.Add(zimmet);
                await _context.SaveChangesAsync();

                // Handle selected stocks
                if (requestData.TryGetProperty("selectedStoklar", out var selectedStoklar))
                {
                    var stockConsumptions = new List<(int stokId, int miktar)>();

                    foreach (var stockProperty in selectedStoklar.EnumerateObject())
                    {
                        if (int.TryParse(stockProperty.Name, out int stokId) && stockProperty.Value.GetInt32() > 0)
                        {
                            int miktar = stockProperty.Value.GetInt32();
                            
                            // Create ZimmetMalzeme record
                            var zimmetMalzeme = new ZimmetMalzeme
                            {
                                ZimmetId = zimmet.Id,
                                ZimmetStokId = stokId,
                                Miktar = miktar
                            };
                            _context.ZimmetMalzemeleri.Add(zimmetMalzeme);
                            
                            stockConsumptions.Add((stokId, miktar));
                        }
                    }

                    // Update stock quantities
                    foreach (var (stokId, miktar) in stockConsumptions)
                    {
                        var zimmetStok = await _context.ZimmetStoklar.FindAsync(stokId);
                        if (zimmetStok != null)
                        {
                            zimmetStok.KalanMiktar -= miktar;
                            if (zimmetStok.KalanMiktar < 0) zimmetStok.KalanMiktar = 0;
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                return Ok(new { success = true, data = zimmet, message = "Zimmet kaydı başarıyla oluşturuldu" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] JsonElement requestData)
        {
            try
            {
                var existingZimmet = await _context.Zimmetler.FindAsync(id);
                if (existingZimmet == null)
                    return NotFound(new { success = false, message = "Zimmet kaydı bulunamadı" });

                // Get existing zimmet malzemeleri for stock restoration if needed
                var existingMalzemeler = await _context.ZimmetMalzemeleri
                    .Where(zm => zm.ZimmetId == id)
                    .ToListAsync();

                existingZimmet.PersonelId = requestData.GetProperty("personelId").GetInt32();
                existingZimmet.DokumanNo = requestData.TryGetProperty("dokumanNo", out var dokumanNo) 
                    ? dokumanNo.GetString() ?? "BLG.ZM.001" 
                    : "BLG.ZM.001";
                existingZimmet.ZimmetTarihi = requestData.TryGetProperty("zimmetTarihi", out var zimmetTarihi) 
                    ? DateTime.Parse(zimmetTarihi.GetString()!).ToUniversalTime() 
                    : existingZimmet.ZimmetTarihi;
                existingZimmet.GsmHat = requestData.TryGetProperty("gsmHat", out var gsmHat) && gsmHat.GetBoolean();
                existingZimmet.GsmHatDetay = requestData.TryGetProperty("gsmHatDetay", out var gsmHatDetay) 
                    ? gsmHatDetay.GetString() 
                    : null;
                existingZimmet.Monitor = requestData.TryGetProperty("monitor", out var monitor) && monitor.GetBoolean();
                existingZimmet.MonitorDetay = requestData.TryGetProperty("monitorDetay", out var monitorDetay) 
                    ? monitorDetay.GetString() 
                    : null;
                existingZimmet.OfisTelefonu = requestData.TryGetProperty("ofisTelefonu", out var ofisTelefonu) && ofisTelefonu.GetBoolean();
                existingZimmet.OfisTelefonuDetay = requestData.TryGetProperty("ofisTelefonuDetay", out var ofisTelefonuDetay) 
                    ? ofisTelefonuDetay.GetString() 
                    : null;
                existingZimmet.CepTelefonu = requestData.TryGetProperty("cepTelefonu", out var cepTelefonu) && cepTelefonu.GetBoolean();
                existingZimmet.CepTelefonuDetay = requestData.TryGetProperty("cepTelefonuDetay", out var cepTelefonuDetay) 
                    ? cepTelefonuDetay.GetString() 
                    : null;
                existingZimmet.DizustuBilgisayar = requestData.TryGetProperty("dizustuBilgisayar", out var dizustuBilgisayar) && dizustuBilgisayar.GetBoolean();
                existingZimmet.DizustuBilgisayarDetay = requestData.TryGetProperty("dizustuBilgisayarDetay", out var dizustuBilgisayarDetay) 
                    ? dizustuBilgisayarDetay.GetString() 
                    : null;
                existingZimmet.YemekKarti = requestData.TryGetProperty("yemekKarti", out var yemekKarti) && yemekKarti.GetBoolean();
                existingZimmet.YemekKartiDetay = requestData.TryGetProperty("yemekKartiDetay", out var yemekKartiDetay) 
                    ? yemekKartiDetay.GetString() 
                    : null;
                existingZimmet.Klavye = requestData.TryGetProperty("klavye", out var klavye) && klavye.GetBoolean();
                existingZimmet.Mouse = requestData.TryGetProperty("mouse", out var mouseProp) && mouseProp.GetBoolean();
                existingZimmet.BilgisayarCantasi = requestData.TryGetProperty("bilgisayarCantasi", out var bilgisayarCantasi) && bilgisayarCantasi.GetBoolean();
                existingZimmet.BilgisayarCantasiDetay = requestData.TryGetProperty("bilgisayarCantasiDetay", out var bilgisayarCantasiDetay) 
                    ? bilgisayarCantasiDetay.GetString() 
                    : null;
                existingZimmet.ErisimYetkileri = requestData.TryGetProperty("erisimYetkileri", out var erisimYetkileri) 
                    ? erisimYetkileri.GetString() 
                    : null;
                existingZimmet.TeslimAlmaNotlari = requestData.TryGetProperty("teslimAlmaNotlari", out var teslimAlmaNotlari) 
                    ? teslimAlmaNotlari.GetString() 
                    : null;
                existingZimmet.TeslimEden = requestData.TryGetProperty("teslimEden", out var teslimEden) 
                    ? teslimEden.GetString() 
                    : null;
                existingZimmet.Hazirlayan = requestData.TryGetProperty("hazirlayan", out var hazirlayan) 
                    ? hazirlayan.GetString() ?? "ÇAĞLA YILDIRIM"
                    : "ÇAĞLA YILDIRIM";
                existingZimmet.Onaylayan = requestData.TryGetProperty("onaylayan", out var onaylayan) 
                    ? onaylayan.GetString() ?? "EMRE HACIEVLİYAGİL"
                    : "EMRE HACIEVLİYAGİL";
                existingZimmet.TeslimDurumu = requestData.TryGetProperty("teslimDurumu", out var teslimDurumu) && teslimDurumu.GetBoolean();
                existingZimmet.TeslimTarihi = requestData.TryGetProperty("teslimTarihi", out var teslimTarihi) && !teslimTarihi.ValueKind.Equals(JsonValueKind.Null)
                    ? DateTime.Parse(teslimTarihi.GetString()!).ToUniversalTime()
                    : null;
                existingZimmet.Aktif = requestData.TryGetProperty("aktif", out var aktif) ? aktif.GetBoolean() : existingZimmet.Aktif;
                existingZimmet.GuncellemeTarihi = DateTime.UtcNow;

                // Handle "Teslim Edildi" checkbox - restore stock if checked
                bool wasTeslimDurumu = existingZimmet.TeslimDurumu;
                bool newTeslimDurumu = requestData.TryGetProperty("teslimDurumu", out var teslimDurumuProp) && teslimDurumuProp.GetBoolean();
                
                if (!wasTeslimDurumu && newTeslimDurumu)
                {
                    // Restore stock quantities for all zimmet malzemeleri
                    foreach (var malzeme in existingMalzemeler)
                    {
                        var zimmetStok = await _context.ZimmetStoklar.FindAsync(malzeme.ZimmetStokId);
                        if (zimmetStok != null)
                        {
                            zimmetStok.KalanMiktar += malzeme.Miktar;
                        }
                    }
                }

                // Handle selected stocks changes
                if (requestData.TryGetProperty("selectedStoklar", out var selectedStoklar))
                {
                    // Remove existing zimmet malzemeleri and restore their stock
                    foreach (var existingMalzeme in existingMalzemeler)
                    {
                        if (!newTeslimDurumu) // Only restore if not delivered
                        {
                            var zimmetStok = await _context.ZimmetStoklar.FindAsync(existingMalzeme.ZimmetStokId);
                            if (zimmetStok != null)
                            {
                                zimmetStok.KalanMiktar += existingMalzeme.Miktar;
                            }
                        }
                    }
                    _context.ZimmetMalzemeleri.RemoveRange(existingMalzemeler);

                    // Add new zimmet malzemeleri
                    var stockConsumptions = new List<(int stokId, int miktar)>();

                    foreach (var stockProperty in selectedStoklar.EnumerateObject())
                    {
                        if (int.TryParse(stockProperty.Name, out int stokId) && stockProperty.Value.GetInt32() > 0)
                        {
                            int miktar = stockProperty.Value.GetInt32();
                            
                            var zimmetMalzeme = new ZimmetMalzeme
                            {
                                ZimmetId = existingZimmet.Id,
                                ZimmetStokId = stokId,
                                Miktar = miktar
                            };
                            _context.ZimmetMalzemeleri.Add(zimmetMalzeme);
                            
                            if (!newTeslimDurumu) // Only consume if not delivered
                            {
                                stockConsumptions.Add((stokId, miktar));
                            }
                        }
                    }

                    // Update stock quantities
                    foreach (var (stokId, miktar) in stockConsumptions)
                    {
                        var zimmetStok = await _context.ZimmetStoklar.FindAsync(stokId);
                        if (zimmetStok != null)
                        {
                            zimmetStok.KalanMiktar -= miktar;
                            if (zimmetStok.KalanMiktar < 0) zimmetStok.KalanMiktar = 0;
                        }
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new { success = true, data = existingZimmet, message = "Zimmet kaydı başarıyla güncellendi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var zimmet = await _context.Zimmetler.FindAsync(id);
                if (zimmet == null)
                    return NotFound(new { success = false, message = "Zimmet kaydı bulunamadı" });

                _context.Zimmetler.Remove(zimmet);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Zimmet kaydı başarıyla silindi" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPatch("{id}/aktiflik")]
        public async Task<IActionResult> ToggleAktiflik(int id)
        {
            try
            {
                var zimmet = await _context.Zimmetler.FindAsync(id);
                if (zimmet == null)
                    return NotFound(new { success = false, message = "Zimmet kaydı bulunamadı" });

                zimmet.Aktif = !zimmet.Aktif;
                zimmet.GuncellemeTarihi = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new { 
                    success = true, 
                    data = zimmet, 
                    message = $"Zimmet kaydı {(zimmet.Aktif ? "aktif" : "pasif")} duruma geçirildi" 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}