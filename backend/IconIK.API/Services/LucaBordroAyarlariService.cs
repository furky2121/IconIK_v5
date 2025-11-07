using IconIK.API.Data;
using IconIK.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace IconIK.API.Services
{
    public class LucaBordroAyarlariService : ILucaBordroAyarlariService
    {
        private readonly IconIKContext _context;
        private readonly ILogger<LucaBordroAyarlariService> _logger;
        private const string EncryptionKey = "IconIK2024SecretKey!!!!!!"; // 32 bytes for AES-256

        public LucaBordroAyarlariService(
            IconIKContext context,
            ILogger<LucaBordroAyarlariService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<LucaBordroAyarlari>> GetAllAsync()
        {
            return await _context.LucaBordroAyarlari
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<LucaBordroAyarlari?> GetByIdAsync(int id)
        {
            var ayar = await _context.LucaBordroAyarlari.FindAsync(id);

            if (ayar != null)
            {
                // Şifreleri maskeleme (frontend'e gönderirken)
                if (!string.IsNullOrEmpty(ayar.ApiKey))
                    ayar.ApiKey = "********";
                if (!string.IsNullOrEmpty(ayar.ApiPassword))
                    ayar.ApiPassword = "********";
            }

            return ayar;
        }

        public async Task<LucaBordroAyarlari?> GetAktifAyarAsync()
        {
            return await _context.LucaBordroAyarlari
                .Where(a => a.Aktif)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<LucaBordroAyarlari> CreateAsync(LucaBordroAyarlari ayarlar)
        {
            // Sadece bir aktif ayar olacak - diğerlerini pasif yap
            if (ayarlar.Aktif)
            {
                var mevcutAktifler = await _context.LucaBordroAyarlari
                    .Where(a => a.Aktif)
                    .ToListAsync();

                foreach (var aktif in mevcutAktifler)
                {
                    aktif.Aktif = false;
                }
            }

            // Şifreleri şifrele
            if (!string.IsNullOrEmpty(ayarlar.ApiKey) && ayarlar.ApiKey != "********")
            {
                ayarlar.ApiKey = EncryptString(ayarlar.ApiKey);
            }

            if (!string.IsNullOrEmpty(ayarlar.ApiPassword) && ayarlar.ApiPassword != "********")
            {
                ayarlar.ApiPassword = EncryptString(ayarlar.ApiPassword);
            }

            ayarlar.CreatedAt = DateTime.UtcNow;
            ayarlar.UpdatedAt = DateTime.UtcNow;

            _context.LucaBordroAyarlari.Add(ayarlar);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Luca bordro ayarı oluşturuldu: ID {ayarlar.Id}");

            return ayarlar;
        }

        public async Task<LucaBordroAyarlari?> UpdateAsync(int id, LucaBordroAyarlari ayarlar)
        {
            var mevcutAyar = await _context.LucaBordroAyarlari.FindAsync(id);
            if (mevcutAyar == null)
                return null;

            // Sadece bir aktif ayar olacak
            if (ayarlar.Aktif && !mevcutAyar.Aktif)
            {
                var digerAktifler = await _context.LucaBordroAyarlari
                    .Where(a => a.Aktif && a.Id != id)
                    .ToListAsync();

                foreach (var aktif in digerAktifler)
                {
                    aktif.Aktif = false;
                }
            }

            // Güncelleme
            mevcutAyar.BaglantiTipi = ayarlar.BaglantiTipi;
            mevcutAyar.ApiUrl = ayarlar.ApiUrl;
            mevcutAyar.ApiUsername = ayarlar.ApiUsername;
            mevcutAyar.DosyaYolu = ayarlar.DosyaYolu;
            mevcutAyar.OtomatikSenkron = ayarlar.OtomatikSenkron;
            mevcutAyar.SenkronSaati = ayarlar.SenkronSaati;
            mevcutAyar.Aktif = ayarlar.Aktif;
            mevcutAyar.UpdatedAt = DateTime.UtcNow;

            // Şifreleri güncelle (sadece değişmişse)
            if (!string.IsNullOrEmpty(ayarlar.ApiKey) && ayarlar.ApiKey != "********")
            {
                mevcutAyar.ApiKey = EncryptString(ayarlar.ApiKey);
            }

            if (!string.IsNullOrEmpty(ayarlar.ApiPassword) && ayarlar.ApiPassword != "********")
            {
                mevcutAyar.ApiPassword = EncryptString(ayarlar.ApiPassword);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Luca bordro ayarı güncellendi: ID {id}");

            return mevcutAyar;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var ayar = await _context.LucaBordroAyarlari.FindAsync(id);
            if (ayar == null)
                return false;

            _context.LucaBordroAyarlari.Remove(ayar);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Luca bordro ayarı silindi: ID {id}");

            return true;
        }

        public async Task<(bool success, string message)> TestBaglantiAsync(int id)
        {
            try
            {
                var ayar = await _context.LucaBordroAyarlari.FindAsync(id);
                if (ayar == null)
                    return (false, "Ayar bulunamadı");

                if (ayar.BaglantiTipi == "API" || ayar.BaglantiTipi == "Ikisi")
                {
                    if (string.IsNullOrEmpty(ayar.ApiUrl))
                        return (false, "API URL belirtilmemiş");

                    // HTTP bağlantı testi
                    using var client = new HttpClient();
                    client.Timeout = TimeSpan.FromSeconds(10);

                    // API Key veya Basic Auth header ekle
                    if (!string.IsNullOrEmpty(ayar.ApiKey))
                    {
                        var decryptedKey = DecryptString(ayar.ApiKey);
                        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {decryptedKey}");
                    }
                    else if (!string.IsNullOrEmpty(ayar.ApiUsername) && !string.IsNullOrEmpty(ayar.ApiPassword))
                    {
                        var decryptedPassword = DecryptString(ayar.ApiPassword);
                        var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ayar.ApiUsername}:{decryptedPassword}"));
                        client.DefaultRequestHeaders.Add("Authorization", $"Basic {authToken}");
                    }

                    // Test endpoint çağrısı (genellikle /health veya /api/status gibi)
                    var response = await client.GetAsync($"{ayar.ApiUrl.TrimEnd('/')}/health");

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation($"Luca API bağlantı testi başarılı: {ayar.ApiUrl}");
                        return (true, "Luca API bağlantısı başarılı!");
                    }
                    else
                    {
                        _logger.LogWarning($"Luca API bağlantı testi başarısız: Status {response.StatusCode}");
                        return (false, $"API bağlantı hatası: HTTP {response.StatusCode}");
                    }
                }
                else if (ayar.BaglantiTipi == "Dosya")
                {
                    if (string.IsNullOrEmpty(ayar.DosyaYolu))
                        return (false, "Dosya yolu belirtilmemiş");

                    // Dizin var mı kontrol et
                    if (Directory.Exists(ayar.DosyaYolu))
                    {
                        _logger.LogInformation($"Luca dosya yolu erişilebilir: {ayar.DosyaYolu}");
                        return (true, "Dosya yolu erişilebilir!");
                    }
                    else
                    {
                        _logger.LogWarning($"Luca dosya yolu bulunamadı: {ayar.DosyaYolu}");
                        return (false, "Belirtilen dosya yolu bulunamadı");
                    }
                }

                return (false, "Geçersiz bağlantı tipi");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Luca API bağlantı hatası");
                return (false, $"Bağlantı hatası: {ex.Message}");
            }
            catch (TaskCanceledException)
            {
                _logger.LogError("Luca API bağlantı zaman aşımı");
                return (false, "Bağlantı zaman aşımına uğradı (10 saniye)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Luca bağlantı testi hatası");
                return (false, $"Beklenmeyen hata: {ex.Message}");
            }
        }

        // Şifreleme metodları (EmailService ile aynı)
        private static string EncryptString(string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(EncryptionKey);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        private static string DecryptString(string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(EncryptionKey);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader(cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
