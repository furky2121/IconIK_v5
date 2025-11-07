using IconIK.API.Data;
using IconIK.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace IconIK.API.Services
{
    public class LucaBordroService : ILucaBordroService
    {
        private readonly IconIKContext _context;
        private readonly ILucaBordroAyarlariService _ayarlariService;
        private readonly IEmailService _emailService;
        private readonly ILogger<LucaBordroService> _logger;

        public LucaBordroService(
            IconIKContext context,
            ILucaBordroAyarlariService ayarlariService,
            IEmailService emailService,
            ILogger<LucaBordroService> logger)
        {
            _context = context;
            _ayarlariService = ayarlariService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<List<LucaBordro>> GetBenimBordrolarimAsync(string tcKimlik)
        {
            return await _context.LucaBordrolar
                .Include(b => b.Personel)
                .Where(b => b.TcKimlik == tcKimlik)
                .OrderByDescending(b => b.DonemYil)
                .ThenByDescending(b => b.DonemAy)
                .ToListAsync();
        }

        public async Task<LucaBordro?> GetByIdAsync(int id)
        {
            return await _context.LucaBordrolar
                .Include(b => b.Personel)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<(bool success, string message, int count)> SenkronizeEtAsync()
        {
            try
            {
                var ayar = await _ayarlariService.GetAktifAyarAsync();
                if (ayar == null)
                    return (false, "Aktif Luca ayarı bulunamadı", 0);

                List<LucaBordroDto> bordrolar = new();

                if (ayar.BaglantiTipi == "API" || ayar.BaglantiTipi == "Ikisi")
                {
                    var apiResult = await FetchFromApiAsync(ayar);
                    if (apiResult.success)
                        bordrolar.AddRange(apiResult.data);
                }

                if (bordrolar.Count == 0)
                    return (false, "Luca'dan bordro verisi alınamadı", 0);

                // Veritabanına kaydet
                int eklenenSayisi = 0;
                foreach (var dto in bordrolar)
                {
                    // Aynı dönem ve TC kimlik için zaten var mı kontrol et
                    var mevcut = await _context.LucaBordrolar
                        .FirstOrDefaultAsync(b => b.TcKimlik == dto.TcKimlik &&
                                                  b.DonemYil == dto.DonemYil &&
                                                  b.DonemAy == dto.DonemAy);

                    if (mevcut != null)
                    {
                        // Güncelle
                        mevcut.BordroNo = dto.BordroNo;
                        mevcut.BrutMaas = dto.BrutMaas;
                        mevcut.NetUcret = dto.NetUcret;
                        mevcut.ToplamOdeme = dto.ToplamOdeme;
                        mevcut.ToplamKesinti = dto.ToplamKesinti;
                        mevcut.SgkIsci = dto.SgkIsci;
                        mevcut.GelirVergisi = dto.GelirVergisi;
                        mevcut.DamgaVergisi = dto.DamgaVergisi;
                        mevcut.DetayJson = dto.DetayJson;
                        mevcut.SenkronTarihi = DateTime.UtcNow;
                    }
                    else
                    {
                        // Yeni ekle
                        var yeniBordro = new LucaBordro
                        {
                            TcKimlik = dto.TcKimlik,
                            SicilNo = dto.SicilNo,
                            AdSoyad = dto.AdSoyad,
                            DonemYil = dto.DonemYil,
                            DonemAy = dto.DonemAy,
                            BordroNo = dto.BordroNo,
                            BrutMaas = dto.BrutMaas,
                            NetUcret = dto.NetUcret,
                            ToplamOdeme = dto.ToplamOdeme,
                            ToplamKesinti = dto.ToplamKesinti,
                            SgkIsci = dto.SgkIsci,
                            GelirVergisi = dto.GelirVergisi,
                            DamgaVergisi = dto.DamgaVergisi,
                            DetayJson = dto.DetayJson,
                            SenkronTarihi = DateTime.UtcNow,
                            Durum = "Aktif",
                            Aktif = true,
                            CreatedAt = DateTime.UtcNow
                        };

                        // Personel eşleştir
                        var personel = await _context.Personeller
                            .FirstOrDefaultAsync(p => p.TcKimlik == dto.TcKimlik);
                        if (personel != null)
                            yeniBordro.PersonelId = personel.Id;

                        _context.LucaBordrolar.Add(yeniBordro);
                        eklenenSayisi++;
                    }
                }

                await _context.SaveChangesAsync();

                // Ayarın son senkron tarihini güncelle
                if (ayar != null)
                {
                    ayar.SonSenkronTarihi = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation($"Luca senkronizasyonu tamamlandı: {eklenenSayisi} yeni bordro");

                return (true, $"Senkronizasyon başarılı: {eklenenSayisi} bordro eklendi/güncellendi", eklenenSayisi);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Luca senkronizasyon hatası");
                return (false, $"Senkronizasyon hatası: {ex.Message}", 0);
            }
        }

        public async Task<(bool success, string message, int count)> DosyadanYukleAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return (false, "Dosya seçilmedi", 0);

                var extension = Path.GetExtension(file.FileName).ToLower();
                List<LucaBordroDto> bordrolar = new();

                if (extension == ".xlsx" || extension == ".xls")
                {
                    bordrolar = await ReadExcelFileAsync(file);
                }
                else if (extension == ".csv")
                {
                    bordrolar = await ReadCsvFileAsync(file);
                }
                else
                {
                    return (false, "Desteklenmeyen dosya formatı. Sadece Excel (.xlsx) veya CSV dosyaları yükleyebilirsiniz.", 0);
                }

                if (bordrolar.Count == 0)
                    return (false, "Dosyada geçerli bordro verisi bulunamadı", 0);

                // Veritabanına kaydet (senkronizeEt ile aynı mantık)
                int eklenenSayisi = 0;
                foreach (var dto in bordrolar)
                {
                    var mevcut = await _context.LucaBordrolar
                        .FirstOrDefaultAsync(b => b.TcKimlik == dto.TcKimlik &&
                                                  b.DonemYil == dto.DonemYil &&
                                                  b.DonemAy == dto.DonemAy);

                    if (mevcut == null)
                    {
                        var yeniBordro = new LucaBordro
                        {
                            TcKimlik = dto.TcKimlik,
                            SicilNo = dto.SicilNo,
                            AdSoyad = dto.AdSoyad,
                            DonemYil = dto.DonemYil,
                            DonemAy = dto.DonemAy,
                            BordroNo = dto.BordroNo,
                            BrutMaas = dto.BrutMaas,
                            NetUcret = dto.NetUcret,
                            ToplamOdeme = dto.ToplamOdeme,
                            ToplamKesinti = dto.ToplamKesinti,
                            SgkIsci = dto.SgkIsci,
                            GelirVergisi = dto.GelirVergisi,
                            DamgaVergisi = dto.DamgaVergisi,
                            DetayJson = dto.DetayJson,
                            SenkronTarihi = DateTime.UtcNow,
                            Durum = "Aktif",
                            Aktif = true,
                            CreatedAt = DateTime.UtcNow
                        };

                        var personel = await _context.Personeller
                            .FirstOrDefaultAsync(p => p.TcKimlik == dto.TcKimlik);
                        if (personel != null)
                            yeniBordro.PersonelId = personel.Id;

                        _context.LucaBordrolar.Add(yeniBordro);
                        eklenenSayisi++;
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Dosyadan {eklenenSayisi} bordro yüklendi");

                return (true, $"Dosya yükleme başarılı: {eklenenSayisi} bordro eklendi", eklenenSayisi);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dosya yükleme hatası");
                return (false, $"Dosya yükleme hatası: {ex.Message}", 0);
            }
        }

        public async Task<(bool success, string message)> MaileGonderAsync(int lucaBordroId, int kullaniciId)
        {
            try
            {
                var bordro = await GetByIdAsync(lucaBordroId);
                if (bordro == null)
                    return (false, "Bordro bulunamadı");

                var kullanici = await _context.Kullanicilar
                    .Include(k => k.Personel)
                    .FirstOrDefaultAsync(k => k.Id == kullaniciId);

                if (kullanici == null || kullanici.Personel == null)
                    return (false, "Kullanıcı bilgileri bulunamadı");

                // E-posta kontrolü
                var email = kullanici.Personel.Email;
                if (string.IsNullOrEmpty(email))
                    return (false, "E-posta adresi bulunamadı");

                // Bordro PDF/HTML oluştur
                var htmlBody = GenerateBordroEmailHtml(bordro);
                var subject = $"Bordro - {bordro.DonemYil}/{bordro.DonemAy:D2}";

                var emailGonderildi = await _emailService.SendEmailAsync(email, subject, htmlBody);

                if (emailGonderildi)
                {
                    _logger.LogInformation($"Bordro e-posta gönderildi: {lucaBordroId} -> {email}");
                    return (true, "Bordro e-posta adresinize gönderildi");
                }
                else
                {
                    return (false, "E-posta gönderilemedi");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Bordro e-posta gönderme hatası: {lucaBordroId}");
                return (false, $"E-posta gönderme hatası: {ex.Message}");
            }
        }

        private async Task<(bool success, List<LucaBordroDto> data)> FetchFromApiAsync(LucaBordroAyarlari ayar)
        {
            try
            {
                if (string.IsNullOrEmpty(ayar.ApiUrl))
                    return (false, new List<LucaBordroDto>());

                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(30);

                // Authorization header ekle
                if (!string.IsNullOrEmpty(ayar.ApiKey))
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {ayar.ApiKey}");
                }

                // Mock endpoint - gerçek Luca API dokümantasyonuna göre güncellenecek
                var response = await client.GetAsync($"{ayar.ApiUrl}/api/bordrolar");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var bordrolar = JsonSerializer.Deserialize<List<LucaBordroDto>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return (true, bordrolar ?? new List<LucaBordroDto>());
                }

                _logger.LogWarning($"Luca API hatası: {response.StatusCode}");
                return (false, new List<LucaBordroDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Luca API çağrısı hatası");
                return (false, new List<LucaBordroDto>());
            }
        }

        private async Task<List<LucaBordroDto>> ReadExcelFileAsync(IFormFile file)
        {
            // EPPlus paketi yüklü değil - gerekirse sonra eklenecek
            // Şimdilik sadece CSV desteği var
            _logger.LogWarning("Excel dosya okuma desteği henüz aktif değil. Lütfen CSV formatı kullanın.");
            return await Task.FromResult(new List<LucaBordroDto>());
        }

        private async Task<List<LucaBordroDto>> ReadCsvFileAsync(IFormFile file)
        {
            var bordrolar = new List<LucaBordroDto>();

            using (var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8))
            {
                // İlk satırı atla (başlık)
                await reader.ReadLineAsync();

                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var values = line.Split(',');
                    if (values.Length < 8)
                        continue;

                    try
                    {
                        var bordro = new LucaBordroDto
                        {
                            TcKimlik = values[0].Trim(),
                            SicilNo = values[1].Trim(),
                            AdSoyad = values[2].Trim(),
                            DonemYil = int.TryParse(values[3], out var yil) ? yil : DateTime.Now.Year,
                            DonemAy = int.TryParse(values[4], out var ay) ? ay : DateTime.Now.Month,
                            BordroNo = values[5].Trim(),
                            BrutMaas = decimal.TryParse(values[6], out var brut) ? brut : 0,
                            NetUcret = decimal.TryParse(values[7], out var net) ? net : 0
                        };

                        if (values.Length > 8)
                            bordro.ToplamOdeme = decimal.TryParse(values[8], out var odeme) ? odeme : 0;
                        if (values.Length > 9)
                            bordro.ToplamKesinti = decimal.TryParse(values[9], out var kesinti) ? kesinti : 0;

                        bordrolar.Add(bordro);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"CSV satır okuma hatası: {ex.Message}");
                    }
                }
            }

            return bordrolar;
        }

        private string GenerateBordroEmailHtml(LucaBordro bordro)
        {
            var ayAdi = new System.Globalization.CultureInfo("tr-TR")
                .DateTimeFormat.GetMonthName(bordro.DonemAy);

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Bordro</title>
</head>
<body style='font-family: Arial, sans-serif; padding: 20px;'>
    <div style='max-width: 600px; margin: 0 auto; border: 1px solid #ddd; padding: 20px;'>
        <h2 style='color: #667eea; border-bottom: 2px solid #667eea; padding-bottom: 10px;'>
            Bordro Fişi - {bordro.DonemYil} {ayAdi}
        </h2>

        <div style='margin: 20px 0;'>
            <p><strong>Ad Soyad:</strong> {bordro.AdSoyad}</p>
            <p><strong>TC Kimlik:</strong> {bordro.TcKimlik}</p>
            <p><strong>Sicil No:</strong> {bordro.SicilNo}</p>
            <p><strong>Bordro No:</strong> {bordro.BordroNo}</p>
        </div>

        <table style='width: 100%; border-collapse: collapse; margin: 20px 0;'>
            <tr style='background-color: #f0f0f0;'>
                <th style='padding: 10px; text-align: left; border: 1px solid #ddd;'>Açıklama</th>
                <th style='padding: 10px; text-align: right; border: 1px solid #ddd;'>Tutar (TL)</th>
            </tr>
            <tr>
                <td style='padding: 10px; border: 1px solid #ddd;'>Brüt Maaş</td>
                <td style='padding: 10px; text-align: right; border: 1px solid #ddd;'>{bordro.BrutMaas:N2}</td>
            </tr>
            <tr>
                <td style='padding: 10px; border: 1px solid #ddd;'>Toplam Ödeme</td>
                <td style='padding: 10px; text-align: right; border: 1px solid #ddd;'>{bordro.ToplamOdeme:N2}</td>
            </tr>
            <tr style='background-color: #fff3cd;'>
                <td style='padding: 10px; border: 1px solid #ddd;'><strong>Kesintiler</strong></td>
                <td style='padding: 10px; text-align: right; border: 1px solid #ddd;'></td>
            </tr>
            <tr>
                <td style='padding: 10px; padding-left: 30px; border: 1px solid #ddd;'>SGK İşçi Payı</td>
                <td style='padding: 10px; text-align: right; border: 1px solid #ddd;'>{bordro.SgkIsci:N2}</td>
            </tr>
            <tr>
                <td style='padding: 10px; padding-left: 30px; border: 1px solid #ddd;'>Gelir Vergisi</td>
                <td style='padding: 10px; text-align: right; border: 1px solid #ddd;'>{bordro.GelirVergisi:N2}</td>
            </tr>
            <tr>
                <td style='padding: 10px; padding-left: 30px; border: 1px solid #ddd;'>Damga Vergisi</td>
                <td style='padding: 10px; text-align: right; border: 1px solid #ddd;'>{bordro.DamgaVergisi:N2}</td>
            </tr>
            <tr>
                <td style='padding: 10px; border: 1px solid #ddd;'>Toplam Kesinti</td>
                <td style='padding: 10px; text-align: right; border: 1px solid #ddd;'>{bordro.ToplamKesinti:N2}</td>
            </tr>
            <tr style='background-color: #d4edda; font-weight: bold; font-size: 18px;'>
                <td style='padding: 15px; border: 1px solid #ddd;'>NET ÜCRET</td>
                <td style='padding: 15px; text-align: right; border: 1px solid #ddd;'>{bordro.NetUcret:N2}</td>
            </tr>
        </table>

        <div style='margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; font-size: 12px; color: #666;'>
            <p>Bu belge elektronik ortamda oluşturulmuştur.</p>
            <p>Senkronizasyon Tarihi: {bordro.SenkronTarihi:dd.MM.yyyy HH:mm}</p>
        </div>
    </div>
</body>
</html>";
        }
    }

    // DTO class
    public class LucaBordroDto
    {
        public string TcKimlik { get; set; } = string.Empty;
        public string? SicilNo { get; set; }
        public string? AdSoyad { get; set; }
        public int DonemYil { get; set; }
        public int DonemAy { get; set; }
        public string? BordroNo { get; set; }
        public decimal? BrutMaas { get; set; }
        public decimal? NetUcret { get; set; }
        public decimal? ToplamOdeme { get; set; }
        public decimal? ToplamKesinti { get; set; }
        public decimal? SgkIsci { get; set; }
        public decimal? GelirVergisi { get; set; }
        public decimal? DamgaVergisi { get; set; }
        public string? DetayJson { get; set; }
    }
}
