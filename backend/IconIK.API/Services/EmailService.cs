using IconIK.API.Data;
using IconIK.API.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System.Security.Cryptography;
using System.Text;

namespace IconIK.API.Services
{
    public class EmailService : IEmailService
    {
        private readonly IconIKContext _context;
        private readonly ILogger<EmailService> _logger;
        private const string EncryptionKey = "IconIK2024SecretKey!!!!!!"; // Exactly 32 bytes for AES-256

        public EmailService(IconIKContext context, ILogger<EmailService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                var smtpAyarlari = await GetAktifSmtpAyarlari();
                if (smtpAyarlari == null)
                {
                    _logger.LogError("Aktif SMTP ayarları bulunamadı");
                    return false;
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(smtpAyarlari.FromName, smtpAyarlari.FromEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;

                // Spam filtreleri için önemli başlıklar
                message.ReplyTo.Add(new MailboxAddress(smtpAyarlari.FromName, smtpAyarlari.FromEmail));
                message.Headers.Add("X-Mailer", "iconIK İnsan Kaynakları Yönetim Sistemi");
                message.Headers.Add("X-Priority", "3"); // Normal priority
                message.MessageId = MimeKit.Utils.MimeUtils.GenerateMessageId();

                // HTML body ve plain text alternatifi
                var plainTextBody = System.Text.RegularExpressions.Regex.Replace(htmlBody, "<[^>]*>", "");
                plainTextBody = System.Net.WebUtility.HtmlDecode(plainTextBody);

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = htmlBody,
                    TextBody = plainTextBody
                };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();

                // SMTP bağlantısı
                var secureSocketOptions = smtpAyarlari.EnableSsl
                    ? SecureSocketOptions.StartTls
                    : SecureSocketOptions.None;

                await client.ConnectAsync(smtpAyarlari.SmtpHost, smtpAyarlari.SmtpPort, secureSocketOptions);

                // Şifre çözme
                var decryptedPassword = DecryptString(smtpAyarlari.SmtpPassword);
                await client.AuthenticateAsync(smtpAyarlari.SmtpUsername, decryptedPassword);

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Email başarıyla gönderildi: {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Email gönderme hatası: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendMulakatBildirimAsync(DateTime tarih, string recipientEmail)
        {
            try
            {
                // Tarih aralığını UTC olarak belirle (gün başlangıcı ve bitişi)
                var gunBaslangic = DateTime.SpecifyKind(tarih.Date, DateTimeKind.Utc);
                var gunBitis = gunBaslangic.AddDays(1);

                // Sadece o günün "Planlandı" durumundaki mülakatları getir
                var mulakatlar = await _context.Mulakatlar
                    .Include(m => m.Basvuru)
                        .ThenInclude(b => b.Aday)
                    .Include(m => m.Basvuru)
                        .ThenInclude(b => b.Ilan)
                    .Include(m => m.MulakatYapan)
                        .ThenInclude(p => p.Pozisyon)
                            .ThenInclude(poz => poz.Departman)
                    .Where(m => m.Tarih >= gunBaslangic && m.Tarih < gunBitis && m.Durum == "Planlandı")
                    .OrderBy(m => m.Tarih)
                    .ToListAsync();

                if (!mulakatlar.Any())
                {
                    _logger.LogInformation($"{tarih:dd.MM.yyyy} tarihinde planlanmış mülakat bulunamadı");
                    return true; // Hata değil, sadece mülakat yok
                }

                // HTML email oluştur
                var htmlBody = GenerateMulakatEmailHtml(mulakatlar, tarih);
                var subject = $"Mülakat Planlama - {tarih:dd MMMM yyyy} ({mulakatlar.Count} Mülakat)";

                return await SendEmailAsync(recipientEmail, subject, htmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Mülakat bildirimi gönderme hatası: {ex.Message}");
                return false;
            }
        }

        public async Task<(bool success, string message, string? details)> TestEmailConnectionAsync()
        {
            try
            {
                var smtpAyarlari = await GetAktifSmtpAyarlari();
                if (smtpAyarlari == null)
                {
                    _logger.LogWarning("SMTP testi yapılamadı: Aktif SMTP ayarları bulunamadı");
                    return (false, "Aktif SMTP ayarları bulunamadı. Lütfen önce SMTP ayarlarını kaydedin.", null);
                }

                // Bağlantı ayarlarını logla
                var sslMode = smtpAyarlari.EnableSsl ? "StartTLS" : "Şifreleme Yok";
                _logger.LogInformation($"SMTP bağlantı testi başlatılıyor - Host: {smtpAyarlari.SmtpHost}, Port: {smtpAyarlari.SmtpPort}, SSL Modu: {sslMode}, Kullanıcı: {smtpAyarlari.SmtpUsername}");

                using var client = new SmtpClient();

                var secureSocketOptions = smtpAyarlari.EnableSsl
                    ? SecureSocketOptions.StartTls
                    : SecureSocketOptions.None;

                // Bağlantı adımı
                try
                {
                    await client.ConnectAsync(smtpAyarlari.SmtpHost, smtpAyarlari.SmtpPort, secureSocketOptions);
                    _logger.LogInformation("SMTP sunucusuna bağlantı başarılı");
                }
                catch (Exception connEx)
                {
                    var errorMsg = $"SMTP sunucusuna bağlanılamadı: {connEx.Message}";
                    var detailedError = $"Host: {smtpAyarlari.SmtpHost}\nPort: {smtpAyarlari.SmtpPort}\nSSL Modu: {sslMode}\n\nHata Detayı:\n{connEx.Message}";

                    if (connEx.InnerException != null)
                    {
                        detailedError += $"\n\nİç Hata:\n{connEx.InnerException.Message}";
                    }

                    _logger.LogError(connEx, "SMTP bağlantı hatası - {ErrorDetails}", detailedError);

                    // Port bazlı öneriler
                    if (smtpAyarlari.SmtpPort == 465 && smtpAyarlari.EnableSsl)
                    {
                        detailedError += "\n\n💡 ÖNERİ: Port 465 kullanıyorsanız, SSL/TLS seçeneğini kapatmayı veya Port 587 ile SSL/TLS açık kombinasyonunu deneyebilirsiniz.";
                    }

                    return (false, errorMsg, detailedError);
                }

                // Kimlik doğrulama adımı
                try
                {
                    var decryptedPassword = DecryptString(smtpAyarlari.SmtpPassword);
                    await client.AuthenticateAsync(smtpAyarlari.SmtpUsername, decryptedPassword);
                    _logger.LogInformation("SMTP kimlik doğrulama başarılı");
                }
                catch (Exception authEx)
                {
                    var errorMsg = $"SMTP kimlik doğrulama başarısız: {authEx.Message}";
                    var detailedError = $"Kullanıcı: {smtpAyarlari.SmtpUsername}\n\nHata Detayı:\n{authEx.Message}";

                    if (authEx.InnerException != null)
                    {
                        detailedError += $"\n\nİç Hata:\n{authEx.InnerException.Message}";
                    }

                    _logger.LogError(authEx, "SMTP kimlik doğrulama hatası - {ErrorDetails}", detailedError);
                    detailedError += "\n\n💡 ÖNERİ: Kullanıcı adı ve şifrenizi kontrol edin.";

                    return (false, errorMsg, detailedError);
                }

                await client.DisconnectAsync(true);
                _logger.LogInformation("SMTP bağlantı testi başarıyla tamamlandı");

                return (true, "SMTP bağlantı testi başarılı! E-posta ayarlarınız doğru şekilde yapılandırılmış.", null);
            }
            catch (Exception ex)
            {
                var errorMsg = $"Beklenmeyen hata: {ex.Message}";
                var detailedError = $"Hata Türü: {ex.GetType().Name}\n\nHata Mesajı:\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}";

                if (ex.InnerException != null)
                {
                    detailedError += $"\n\nİç Hata:\n{ex.InnerException.Message}";
                }

                _logger.LogError(ex, "SMTP test hatası - Beklenmeyen bir hata oluştu: {ErrorDetails}", detailedError);
                return (false, errorMsg, detailedError);
            }
        }

        private async Task<EPostaAyarlari?> GetAktifSmtpAyarlari()
        {
            return await _context.EPostaAyarlari
                .Where(e => e.Aktif)
                .OrderByDescending(e => e.CreatedAt)
                .FirstOrDefaultAsync();
        }

        private string GenerateMulakatEmailHtml(List<Mulakat> mulakatlar, DateTime tarih)
        {
            var turkceTarih = tarih.ToString("dd MMMM yyyy dddd", new System.Globalization.CultureInfo("tr-TR"));
            var turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");

            var sb = new StringBuilder();
            sb.AppendLine(@"
<!DOCTYPE html>
<html lang='tr'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Mülakat Planlama</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            line-height: 1.5;
            color: #2c3e50;
            background-color: #f5f7fa;
            padding: 20px;
        }
        .email-wrapper {
            max-width: 900px;
            margin: 0 auto;
            background-color: #ffffff;
            border-radius: 8px;
            box-shadow: 0 4px 6px rgba(0,0,0,0.07);
            overflow: hidden;
        }
        .header {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            padding: 20px 30px;
            display: flex;
            align-items: center;
            gap: 12px;
        }
        .header-title-wrapper {
            flex: 1;
        }
        .header-brand {
            font-size: 18px;
            font-weight: 700;
            color: white;
            letter-spacing: 0.5px;
        }
        .header-content {
            flex: 1;
            color: white;
        }
        .header-subtitle {
            font-size: 13px;
            opacity: 0.85;
            margin-top: 3px;
        }
        .summary-bar {
            background: linear-gradient(to right, #667eea15, #764ba215);
            border-left: 4px solid #667eea;
            padding: 15px 30px;
            display: flex;
            align-items: center;
            gap: 12px;
        }
        .summary-bar strong {
            color: #667eea;
            font-size: 18px;
            font-weight: 700;
        }
        .summary-bar span {
            color: #6c757d;
            font-size: 14px;
        }
        .content {
            padding: 25px 30px;
        }
        .table-wrapper {
            overflow-x: auto;
        }
        table {
            width: 100%;
            border-collapse: collapse;
            font-size: 13px;
        }
        thead {
            background-color: #f8f9fa;
            border-bottom: 2px solid #e9ecef;
        }
        th {
            padding: 12px;
            text-align: left;
            font-weight: 600;
            color: #495057;
            font-size: 11px;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }
        td {
            padding: 14px 12px;
            border-bottom: 1px solid #f1f3f5;
            color: #2c3e50;
        }
        tbody tr:hover {
            background-color: #f8f9fa;
        }
        tbody tr:last-child td {
            border-bottom: none;
        }
        .time-cell {
            font-weight: 700;
            color: #667eea;
            font-size: 16px;
            white-space: nowrap;
        }
        .badge {
            display: inline-block;
            padding: 4px 10px;
            border-radius: 12px;
            font-size: 10px;
            font-weight: 700;
            text-transform: uppercase;
            letter-spacing: 0.3px;
        }
        .badge-ik { background: #3498db; color: white; }
        .badge-teknik { background: #9b59b6; color: white; }
        .badge-yonetici { background: #f39c12; color: white; }
        .badge-genel { background: #e74c3c; color: white; }
        .badge-video { background: #1abc9c; color: white; }
        .info-primary {
            font-weight: 600;
            color: #2c3e50;
            margin-bottom: 2px;
        }
        .info-secondary {
            font-size: 12px;
            color: #6c757d;
        }
        .notes-badge {
            display: inline-flex;
            align-items: center;
            gap: 4px;
            background-color: #fff3cd;
            color: #856404;
            padding: 3px 8px;
            border-radius: 8px;
            font-size: 11px;
            font-weight: 600;
        }
        .footer {
            background-color: #f8f9fa;
            padding: 20px;
            text-align: center;
            border-top: 1px solid #e9ecef;
        }
        .footer-logo {
            max-width: 8px;
            max-height: 8px;
            width: 8px;
            height: 8px;
            object-fit: contain;
            margin: 0 auto 3px;
            display: block;
        }
        .footer-text {
            font-size: 12px;
            color: #6c757d;
        }
        @media only screen and (max-width: 600px) {
            .email-wrapper { margin: 0; border-radius: 0; }
            body { padding: 0; }
            .content { padding: 15px; }
            table { font-size: 12px; }
            th, td { padding: 10px 8px; }
        }
    </style>
</head>
<body>
    <div class='email-wrapper'>
        <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 20px 30px; display: flex; align-items: center; gap: 12px;'>
            <div style='flex: 1;'>
                <div style='font-size: 18px; font-weight: 700; color: white; letter-spacing: 0.5px;'>İnsan Kaynakları Yönetimi</div>
            </div>
            <div style='flex: 1; color: white;'>
                <div style='font-size: 13px; opacity: 0.85; margin-top: 3px;'>Günlük Mülakat Takvimi • " + turkceTarih + @"</div>
            </div>
        </div>
        <div class='summary-bar'>
            <strong>" + mulakatlar.Count + @" Mülakat</strong>
            <span>Bugün için planlanmış görüşmeler</span>
        </div>
        <div class='content'>
            <div class='table-wrapper'>
                <table>
                    <thead>
                        <tr>
                            <th>SAAT</th>
                            <th>TÜR</th>
                            <th>ADAY</th>
                            <th>POZİSYON</th>
                            <th>GÖRÜŞMECI</th>
                            <th>LOKASYON</th>
                            <th>SÜRE</th>
                            <th>NOT</th>
                        </tr>
                    </thead>
                    <tbody>");

            foreach (var mulakat in mulakatlar)
            {
                // UTC'den Türkiye saatine çevir
                var turkiyeSaati = TimeZoneInfo.ConvertTimeFromUtc(mulakat.Tarih, turkeyTimeZone);
                var saat = turkiyeSaati.ToString("HH:mm");

                var adayAdi = $"{mulakat.Basvuru.Aday.Ad} {mulakat.Basvuru.Aday.Soyad}";
                var pozisyon = mulakat.Basvuru.Ilan?.Baslik ?? "Belirtilmemiş";
                var turText = GetMulakatTuruText(mulakat.Tur.ToString());
                var sure = $"{mulakat.Sure}dk";
                var lokasyon = mulakat.Lokasyon ?? "-";
                var gorusmeci = $"{mulakat.MulakatYapan.Ad} {mulakat.MulakatYapan.Soyad}";
                var gorusmecDept = mulakat.MulakatYapan.Pozisyon?.Departman?.Ad ?? "";
                var notlar = !string.IsNullOrWhiteSpace(mulakat.Notlar) ? mulakat.Notlar : "-";

                sb.AppendLine($@"
                        <tr>
                            <td class='time-cell'>{saat}</td>
                            <td>{turText}</td>
                            <td>
                                <div class='info-primary'>{adayAdi}</div>
                            </td>
                            <td>{pozisyon}</td>
                            <td>
                                <div class='info-primary'>{gorusmeci}</div>
                                <div class='info-secondary'>{gorusmecDept}</div>
                            </td>
                            <td>{lokasyon}</td>
                            <td>{sure}</td>
                            <td>{notlar}</td>
                        </tr>");
            }

            sb.AppendLine(@"
                    </tbody>
                </table>
            </div>
        </div>
        <div class='footer'>
            <img src='http://localhost:5000/images/icon_ik.png' alt='Logo' width='90' height='50' style='width: 90px; height: 50px; max-width: 90px; max-height: 50px; display: block; margin: 0 auto 10px;' />
            <div class='footer-text'>Bu bilgilendirme e-postası sistem tarafından gönderilmiştir.</div>
        </div>
    </div>
</body>
</html>");

            return sb.ToString();
        }

        private string GetMulakatTuruText(string tur)
        {
            return tur switch
            {
                "HR" => "İK",
                "Teknik" => "Teknik",
                "Yonetici" => "Yönetici",
                "GenelMudur" => "Genel Müdür",
                "Video" => "Video",
                _ => tur
            };
        }

        private string GetMulakatTuruBadge(string tur)
        {
            return tur switch
            {
                "IK" => "<span class='badge badge-ik'>İK</span>",
                "Teknik" => "<span class='badge badge-teknik'>Teknik</span>",
                "Yonetici" => "<span class='badge badge-yonetici'>Yönetici</span>",
                "GenelMudur" => "<span class='badge badge-genel'>Genel Müdür</span>",
                "Video" => "<span class='badge badge-video'>Video</span>",
                _ => $"<span class='badge'>{tur}</span>"
            };
        }

        // Şifreleme metodları
        public static string EncryptString(string plainText)
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
