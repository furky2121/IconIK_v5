using IconIK.API.Models;

namespace IconIK.API.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody);
        Task<bool> SendMulakatBildirimAsync(DateTime tarih, string recipientEmail);
        Task<(bool success, string message, string? details)> TestEmailConnectionAsync();
    }
}
