using IconIK.API.Models;

namespace IconIK.API.Services
{
    public interface IUserService
    {
        Task<Kullanici> CreateUserForPersonelAsync(Personel personel);
        Task<string> GenerateKullaniciAdiAsync(string ad, string soyad);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
        Task<bool> IsFirstLoginAsync(int kullaniciId);
        Task<bool> ChangePasswordAsync(int kullaniciId, string newPassword);
        string ConvertTurkishToEnglish(string input);
    }
}