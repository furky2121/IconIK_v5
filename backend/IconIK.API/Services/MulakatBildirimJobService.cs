using IconIK.API.Data;
using Microsoft.EntityFrameworkCore;

namespace IconIK.API.Services
{
    public class MulakatBildirimJobService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MulakatBildirimJobService> _logger;

        public MulakatBildirimJobService(IServiceProvider serviceProvider, ILogger<MulakatBildirimJobService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task CheckAndSendMulakatBildirimAsync()
        {
            try
            {
                _logger.LogInformation("Mülakat bildirimi kontrolü başlatıldı");

                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<IconIKContext>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                // Türkiye saat dilimi
                var turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
                var currentTimeInTurkey = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, turkeyTimeZone);
                var currentTime = currentTimeInTurkey.TimeOfDay;

                _logger.LogInformation($"Şu anki Türkiye saati: {currentTimeInTurkey:HH:mm:ss}");

                // Aktif mülakat yönlendirme ayarlarını getir
                var yonlendirmeler = await context.EPostaYonlendirme
                    .Where(y => y.Aktif && y.YonlendirmeTuru == "MulakatPlanlama")
                    .ToListAsync();

                _logger.LogInformation($"Bulunan aktif yönlendirme sayısı: {yonlendirmeler.Count}");

                if (!yonlendirmeler.Any())
                {
                    _logger.LogInformation("Aktif mülakat yönlendirme ayarı bulunamadı");
                    return;
                }

                foreach (var yonlendirme in yonlendirmeler)
                {
                    _logger.LogInformation($"Yönlendirme kontrol ediliyor: {yonlendirme.AliciEmail}, GonderimSaati: {yonlendirme.GonderimSaati}");

                    // Gönderim saatine gelindi mi kontrol et (±5 dakika tolerans)
                    var timeDifference = Math.Abs((currentTime - yonlendirme.GonderimSaati).TotalMinutes);

                    _logger.LogInformation($"Zaman farkı: {timeDifference:F2} dakika (Tolerans: 5 dakika)");

                    if (timeDifference <= 5)
                    {
                        // Bugün daha önce gönderildi mi kontrol et (UTC karşılaştırması)
                        if (yonlendirme.SonGonderimTarihi.HasValue)
                        {
                            var sonGonderimTurkeyTime = TimeZoneInfo.ConvertTimeFromUtc(yonlendirme.SonGonderimTarihi.Value, turkeyTimeZone);
                            if (sonGonderimTurkeyTime.Date == currentTimeInTurkey.Date)
                            {
                                _logger.LogInformation($"Bu yönlendirme bugün zaten gönderilmiş: {yonlendirme.AliciEmail}");
                                continue;
                            }
                        }

                        _logger.LogInformation($"Mülakat bildirimi gönderiliyor: {yonlendirme.AliciEmail}");

                        // Email gönder (bugünkü mülakatlar)
                        var result = await emailService.SendMulakatBildirimAsync(
                            currentTimeInTurkey.Date,
                            yonlendirme.AliciEmail
                        );

                        if (result)
                        {
                            // Son gönderim tarihini güncelle (UTC olarak kaydet)
                            yonlendirme.SonGonderimTarihi = DateTime.UtcNow;
                            await context.SaveChangesAsync();

                            _logger.LogInformation($"Mülakat bildirimi başarıyla gönderildi: {yonlendirme.AliciEmail}");
                        }
                        else
                        {
                            _logger.LogError($"Mülakat bildirimi gönderilemedi: {yonlendirme.AliciEmail}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Mülakat bildirimi job hatası: {ex.Message}", ex);
            }
        }
    }
}
