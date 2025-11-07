using IconIK.API.Data;
using IconIK.API.Models;
using Microsoft.EntityFrameworkCore;

namespace IconIK.API.Services
{
    public class OtpService : IOtpService
    {
        private readonly IconIKContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<OtpService> _logger;
        private const int MAX_DENEME = 3;

        public OtpService(
            IconIKContext context,
            IEmailService emailService,
            ILogger<OtpService> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<(bool success, string otpKodu, string message)> OlusturVeGonderAsync(
            int kullaniciId,
            string email,
            int? lucaBordroId = null)
        {
            try
            {
                // 6 haneli rastgele OTP kodu oluştur
                var random = new Random();
                var otpKodu = random.Next(100000, 999999).ToString();

                // Veritabanına kaydet
                var otp = new OtpKod
                {
                    KullaniciId = kullaniciId,
                    Email = email,
                    OtpKodu = otpKodu,
                    LucaBordroId = lucaBordroId,
                    OlusturmaTarihi = DateTime.UtcNow,
                    GecerlilikSuresi = 5, // 5 dakika
                    Kullanildi = false,
                    DenemeSayisi = 0
                };

                _context.OtpKodlar.Add(otp);
                await _context.SaveChangesAsync();

                // E-posta gönder
                var htmlBody = GenerateOtpEmailHtml(otpKodu);
                var subject = "Bordro Görüntüleme - OTP Doğrulama Kodu";

                var emailGonderildi = await _emailService.SendEmailAsync(email, subject, htmlBody);

                if (emailGonderildi)
                {
                    _logger.LogInformation($"OTP kodu oluşturuldu ve e-posta gönderildi: Kullanıcı {kullaniciId}, Email: {email}");
                    return (true, otpKodu, "OTP kodu e-posta adresinize gönderildi. Kod 5 dakika geçerlidir.");
                }
                else
                {
                    _logger.LogError($"OTP e-postası gönderilemedi: Kullanıcı {kullaniciId}, Email: {email}");
                    return (false, "", "E-posta gönderilemedi. Lütfen e-posta ayarlarınızı kontrol edin.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"OTP oluşturma hatası: Kullanıcı {kullaniciId}");
                return (false, "", $"OTP oluşturma hatası: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> DogrulaAsync(int kullaniciId, string otpKodu)
        {
            try
            {
                // En son oluşturulan ve kullanılmamış OTP'yi getir
                var otp = await _context.OtpKodlar
                    .Where(o => o.KullaniciId == kullaniciId &&
                               o.OtpKodu == otpKodu &&
                               !o.Kullanildi)
                    .OrderByDescending(o => o.OlusturmaTarihi)
                    .FirstOrDefaultAsync();

                if (otp == null)
                {
                    _logger.LogWarning($"OTP bulunamadı veya kullanılmış: Kullanıcı {kullaniciId}, Kod: {otpKodu}");
                    return (false, "Geçersiz OTP kodu. Lütfen doğru kodu girdiğinizden emin olun.");
                }

                // Geçerlilik süresi kontrolü (5 dakika)
                var gecerlilikSuresi = DateTime.UtcNow - otp.OlusturmaTarihi;
                if (gecerlilikSuresi.TotalMinutes > otp.GecerlilikSuresi)
                {
                    _logger.LogWarning($"OTP süresi dolmuş: Kullanıcı {kullaniciId}, Geçen süre: {gecerlilikSuresi.TotalMinutes:F2} dk");
                    return (false, "OTP kodunun süresi dolmuş. Lütfen yeni bir kod talep edin.");
                }

                // Deneme sayısı kontrolü
                otp.DenemeSayisi++;
                if (otp.DenemeSayisi > MAX_DENEME)
                {
                    otp.Kullanildi = true; // Kod geçersiz hale gelir
                    await _context.SaveChangesAsync();

                    _logger.LogWarning($"OTP maksimum deneme sayısı aşıldı: Kullanıcı {kullaniciId}");
                    return (false, "Maksimum deneme sayısına ulaştınız. Lütfen yeni bir kod talep edin.");
                }

                // OTP doğru - kullanıldı olarak işaretle
                otp.Kullanildi = true;
                otp.KullanimTarihi = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"OTP başarıyla doğrulandı: Kullanıcı {kullaniciId}");
                return (true, "OTP kodu başarıyla doğrulandı.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"OTP doğrulama hatası: Kullanıcı {kullaniciId}");
                return (false, $"OTP doğrulama hatası: {ex.Message}");
            }
        }

        public async Task<bool> GecersizOtpleriTemizleAsync()
        {
            try
            {
                // 24 saatten eski veya kullanılmış OTP'leri sil
                var gecersizOtpler = await _context.OtpKodlar
                    .Where(o => o.Kullanildi ||
                               DateTime.UtcNow > o.OlusturmaTarihi.AddHours(24))
                    .ToListAsync();

                if (gecersizOtpler.Any())
                {
                    _context.OtpKodlar.RemoveRange(gecersizOtpler);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"{gecersizOtpler.Count} adet geçersiz OTP temizlendi");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OTP temizleme hatası");
                return false;
            }
        }

        private string GenerateOtpEmailHtml(string otpKodu)
        {
            return $@"
<!DOCTYPE html>
<html lang='tr'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>OTP Doğrulama Kodu</title>
</head>
<body style='margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;'>
    <table width='100%' cellpadding='0' cellspacing='0' style='background-color: #f4f4f4; padding: 20px;'>
        <tr>
            <td align='center'>
                <table width='600' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1);'>
                    <!-- Header -->
                    <tr>
                        <td style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center;'>
                            <h1 style='color: #ffffff; margin: 0; font-size: 28px; font-weight: 700;'>Bordro Görüntüleme</h1>
                            <p style='color: #ffffff; margin: 10px 0 0 0; font-size: 14px; opacity: 0.9;'>Güvenlik Doğrulama Kodu</p>
                        </td>
                    </tr>

                    <!-- Content -->
                    <tr>
                        <td style='padding: 40px 30px;'>
                            <p style='color: #333333; font-size: 16px; line-height: 1.6; margin: 0 0 20px 0;'>
                                Merhaba,
                            </p>
                            <p style='color: #333333; font-size: 16px; line-height: 1.6; margin: 0 0 30px 0;'>
                                Bordronuzu e-posta adresinize göndermek için aşağıdaki doğrulama kodunu kullanın:
                            </p>

                            <!-- OTP Code -->
                            <table width='100%' cellpadding='0' cellspacing='0' style='margin: 0 0 30px 0;'>
                                <tr>
                                    <td align='center' style='background: linear-gradient(to right, #667eea15, #764ba215); padding: 30px; border-radius: 8px; border-left: 4px solid #667eea;'>
                                        <div style='font-size: 42px; font-weight: 700; letter-spacing: 8px; color: #667eea; font-family: Courier New, monospace;'>
                                            {otpKodu}
                                        </div>
                                    </td>
                                </tr>
                            </table>

                            <!-- Warning -->
                            <table width='100%' cellpadding='0' cellspacing='0' style='margin: 0 0 20px 0;'>
                                <tr>
                                    <td style='background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; border-radius: 4px;'>
                                        <p style='color: #856404; margin: 0; font-size: 14px; line-height: 1.5;'>
                                            <strong>⚠ Önemli:</strong><br>
                                            • Bu kod <strong>5 dakika</strong> boyunca geçerlidir<br>
                                            • Kodu kimseyle paylaşmayın<br>
                                            • Bu işlemi siz yapmadıysanız, lütfen hemen BT departmanını bilgilendirin
                                        </p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style='background-color: #f8f9fa; padding: 20px; text-align: center; border-top: 1px solid #e9ecef;'>
                            <p style='color: #6c757d; font-size: 12px; margin: 0; line-height: 1.5;'>
                                Bu e-posta otomatik olarak gönderilmiştir. Lütfen yanıtlamayın.<br>
                                <strong>İnsan Kaynakları Yönetim Sistemi</strong>
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
        }
    }
}
