using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using IconIK.API.Data;
using IconIK.API.Models;

namespace IconIK.API.Services
{
    public class UserService : IUserService
    {
        private readonly IconIKContext _context;

        public UserService(IconIKContext context)
        {
            _context = context;
        }

        public async Task<Kullanici> CreateUserForPersonelAsync(Personel personel)
        {
            try
            {
                // Kullanıcı adı oluştur
                var kullaniciAdi = await GenerateKullaniciAdiAsync(personel.Ad, personel.Soyad);
                
                // TC kimlik numarasının son 4 hanesi şifre olacak
                var defaultPassword = personel.TcKimlik.Substring(personel.TcKimlik.Length - 4);
                var hashedPassword = HashPassword(defaultPassword);

                var kullanici = new Kullanici
                {
                    PersonelId = personel.Id,
                    KullaniciAdi = kullaniciAdi,
                    SifreHash = hashedPassword,
                    IlkGiris = true,
                    Aktif = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Kullanicilar.Add(kullanici);
                await _context.SaveChangesAsync();

                return kullanici;
            }
            catch (Exception ex)
            {
                throw new Exception($"Kullanıcı oluşturulurken hata: {ex.Message}");
            }
        }

        public async Task<string> GenerateKullaniciAdiAsync(string ad, string soyad)
        {
            // Türkçe karakterleri İngilizce'ye çevir ve küçük harfe çevir
            var baseUsername = $"{ConvertTurkishToEnglish(ad.ToLowerInvariant())}.{ConvertTurkishToEnglish(soyad.ToLowerInvariant())}";
            
            var kullaniciAdi = baseUsername;
            var counter = 1;

            // Eğer aynı kullanıcı adı varsa sonuna numara ekle
            while (await _context.Kullanicilar.AnyAsync(k => k.KullaniciAdi == kullaniciAdi))
            {
                kullaniciAdi = $"{baseUsername}{counter}";
                counter++;
            }

            return kullaniciAdi;
        }

        public string ConvertTurkishToEnglish(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var turkishChars = "çĞğıİöşüÇĞİÖŞÜ";
            var englishChars = "cGgiIosuCGIOSU";

            var result = new StringBuilder();
            foreach (char c in input)
            {
                var index = turkishChars.IndexOf(c);
                if (index >= 0)
                {
                    result.Append(englishChars[index]);
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }

        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(hashedBytes).ToLowerInvariant();
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            var hashedInput = HashPassword(password);
            return hashedInput.Equals(hashedPassword, StringComparison.OrdinalIgnoreCase);
        }

        public async Task<bool> IsFirstLoginAsync(int kullaniciId)
        {
            var kullanici = await _context.Kullanicilar.FindAsync(kullaniciId);
            return kullanici?.IlkGiris ?? false;
        }

        public async Task<bool> ChangePasswordAsync(int kullaniciId, string newPassword)
        {
            try
            {
                var kullanici = await _context.Kullanicilar.FindAsync(kullaniciId);
                if (kullanici == null)
                    return false;

                kullanici.SifreHash = HashPassword(newPassword);
                kullanici.IlkGiris = false;
                kullanici.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}