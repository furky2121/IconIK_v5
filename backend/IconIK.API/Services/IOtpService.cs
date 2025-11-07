namespace IconIK.API.Services
{
    public interface IOtpService
    {
        Task<(bool success, string otpKodu, string message)> OlusturVeGonderAsync(int kullaniciId, string email, int? lucaBordroId = null);
        Task<(bool success, string message)> DogrulaAsync(int kullaniciId, string otpKodu);
        Task<bool> GecersizOtpleriTemizleAsync();
    }
}
