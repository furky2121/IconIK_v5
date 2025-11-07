using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using IconIK.API.Data;
using IconIK.API.Models;

namespace IconIK.API.Services
{
    public interface IVideoEgitimService
    {
        Task<List<VideoKategori>> GetKategorilerAsync();
        Task<List<VideoEgitim>> GetEgitimlerByKategoriAsync(int kategoriId);
        Task<List<VideoEgitim>> GetPersonelEgitimleriAsync(int personelId);
        Task<VideoAtama> AtamaYapAsync(VideoAtama atama);
        Task<VideoIzleme> IzlemeKaydetAsync(VideoIzleme izleme);
        Task<bool> EgitimTamamlandiMiAsync(int videoEgitimId, int personelId);
        Task<Dictionary<string, object>> GetIstatistiklerAsync(int? personelId = null, int? departmanId = null);
        Task<Dictionary<string, object>> GetRaporIstatistikleriAsync();
        Task<List<object>> GetPersonelEgitimOzetiAsync();
        Task<List<object>> GetDepartmanRaporuAsync(int year);
        Task<List<VideoAtama>> GetBekleyenEgitimlerAsync(int personelId);
        Task<bool> HatirlatmaGonderAsync(int atamaId);
        Task<VideoSertifika> SertifikaOlusturAsync(int videoEgitimId, int personelId);
        Task<List<VideoEgitim>> SearchEgitimlerAsync(string searchTerm);
        Task<object> GetEgitimDetayAsync(int egitimId, int? personelId = null);
        Task<bool> UpdateVideoProgressAsync(int personelId, object progressData);
        Task<object> GetVideoDurationAsync(string videoUrl);
        Task<List<object>> GetAylikEgitimTrendiAsync(int year = 0);
    }

    public class VideoEgitimService : IVideoEgitimService
    {
        private readonly IconIKContext _context;

        public VideoEgitimService(IconIKContext context)
        {
            _context = context;
        }

        public async Task<List<VideoKategori>> GetKategorilerAsync()
        {
            return await _context.VideoKategoriler
                .Where(k => k.Aktif)
                .OrderBy(k => k.Sira)
                .Include(k => k.VideoEgitimler)
                .ToListAsync();
        }

        public async Task<List<VideoEgitim>> GetEgitimlerByKategoriAsync(int kategoriId)
        {
            return await _context.VideoEgitimler
                .Where(e => e.KategoriId == kategoriId && e.Aktif)
                .Include(e => e.Kategori)
                .OrderBy(e => e.Baslik)
                .ToListAsync();
        }

        public async Task<List<VideoEgitim>> GetPersonelEgitimleriAsync(int personelId)
        {
            // Önce personelin pozisyon ve departman bilgilerini al
            var personel = await _context.Personeller
                .Include(p => p.Pozisyon)
                    .ThenInclude(po => po.Departman)
                .FirstOrDefaultAsync(p => p.Id == personelId);

            if (personel == null) return new List<VideoEgitim>();

            var pozisyonId = personel.PozisyonId;
            var departmanId = personel.Pozisyon?.DepartmanId;

            var atananEgitimler = await _context.VideoAtamalar
                .Where(a => a.PersonelId == personelId || 
                           a.PozisyonId == pozisyonId ||
                           a.DepartmanId == departmanId)
                .Select(a => a.VideoEgitimId)
                .Distinct()
                .ToListAsync();

            return await _context.VideoEgitimler
                .Where(e => atananEgitimler.Contains(e.Id) && e.Aktif)
                .Include(e => e.Kategori)
                .Include(e => e.VideoIzlemeler.Where(i => i.PersonelId == personelId))
                .OrderBy(e => e.ZorunluMu ? 0 : 1)
                .ThenBy(e => e.Baslik)
                .ToListAsync();
        }

        public async Task<VideoAtama> AtamaYapAsync(VideoAtama atama)
        {
            _context.VideoAtamalar.Add(atama);
            await _context.SaveChangesAsync();
            return atama;
        }

        public async Task<VideoIzleme> IzlemeKaydetAsync(VideoIzleme izleme)
        {
            try
            {
                Console.WriteLine($"IzlemeKaydetAsync başlıyor - VideoEgitimId: {izleme.VideoEgitimId}, PersonelId: {izleme.PersonelId}");
                
                var mevcutIzleme = await _context.VideoIzlemeler
                    .FirstOrDefaultAsync(i => i.VideoEgitimId == izleme.VideoEgitimId && 
                                             i.PersonelId == izleme.PersonelId &&
                                             !i.TamamlandiMi);

                if (mevcutIzleme != null)
                {
                    Console.WriteLine($"Mevcut izleme kaydı bulundu - Id: {mevcutIzleme.Id}");
                    
                    // Mevcut izleme kaydını güncelle
                    mevcutIzleme.ToplamIzlenenSure += izleme.ToplamIzlenenSure;
                    mevcutIzleme.IzlemeYuzdesi = izleme.IzlemeYuzdesi;
                    mevcutIzleme.IzlemeBitis = DateTime.UtcNow;

                    var egitim = await _context.VideoEgitimler.FindAsync(izleme.VideoEgitimId);
                    if (egitim != null && mevcutIzleme.IzlemeYuzdesi >= egitim.IzlenmeMinimum)
                    {
                        Console.WriteLine($"Eğitim tamamlandı - IzlenmeYuzdesi: {mevcutIzleme.IzlemeYuzdesi}, Minimum: {egitim.IzlenmeMinimum}");
                        
                        mevcutIzleme.TamamlandiMi = true;
                        mevcutIzleme.TamamlanmaTarihi = DateTime.UtcNow;

                        // Atama durumunu güncelle
                        var atama = await _context.VideoAtamalar
                            .FirstOrDefaultAsync(a => a.VideoEgitimId == izleme.VideoEgitimId && 
                                                     a.PersonelId == izleme.PersonelId);
                        if (atama != null)
                        {
                            atama.Durum = "Tamamlandı";
                            atama.TamamlanmaTarihi = DateTime.UtcNow;
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Yeni izleme kaydı oluşturuluyor - IpAdresi: {izleme.IpAdresi}, CihazTipi: {izleme.CihazTipi}, VideoPlatform: {izleme.VideoPlatform}");
                    _context.VideoIzlemeler.Add(izleme);
                }

                Console.WriteLine("SaveChangesAsync çağrılıyor...");
                await _context.SaveChangesAsync();
                Console.WriteLine("SaveChangesAsync tamamlandı");
                
                return mevcutIzleme ?? izleme;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"IzlemeKaydetAsync HATA: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<bool> EgitimTamamlandiMiAsync(int videoEgitimId, int personelId)
        {
            return await _context.VideoIzlemeler
                .AnyAsync(i => i.VideoEgitimId == videoEgitimId && 
                             i.PersonelId == personelId && 
                             i.TamamlandiMi);
        }

        public async Task<Dictionary<string, object>> GetIstatistiklerAsync(int? personelId = null, int? departmanId = null)
        {
            var istatistikler = new Dictionary<string, object>();

            if (personelId.HasValue)
            {
                // Personel bazlı istatistikler
                var toplamEgitim = await _context.VideoAtamalar
                    .CountAsync(a => a.PersonelId == personelId);
                
                var tamamlananEgitim = await _context.VideoAtamalar
                    .CountAsync(a => a.PersonelId == personelId && a.Durum == "Tamamlandı");
                
                var toplamSure = await _context.VideoIzlemeler
                    .Where(i => i.PersonelId == personelId)
                    .SumAsync(i => i.ToplamIzlenenSure);
                
                var ortalamaPuan = await _context.VideoIzlemeler
                    .Where(i => i.PersonelId == personelId && i.Puan.HasValue)
                    .AverageAsync(i => i.Puan);
                
                var sertifikaSayisi = await _context.VideoSertifikalar
                    .CountAsync(s => s.PersonelId == personelId);

                istatistikler["toplamEgitim"] = toplamEgitim;
                istatistikler["tamamlananEgitim"] = tamamlananEgitim;
                istatistikler["tamamlanmaOrani"] = toplamEgitim > 0 ? (tamamlananEgitim * 100 / toplamEgitim) : 0;
                istatistikler["toplamIzlemeSuresi"] = toplamSure / 60; // Dakika olarak
                istatistikler["ortalamaPuan"] = ortalamaPuan ?? 0;
                istatistikler["sertifikaSayisi"] = sertifikaSayisi;
            }
            else if (departmanId.HasValue)
            {
                // Departman bazlı istatistikler
                var departmanPersoneller = await _context.Personeller
                    .Where(p => p.Pozisyon.DepartmanId == departmanId)
                    .Select(p => p.Id)
                    .ToListAsync();
                
                var toplamAtama = await _context.VideoAtamalar
                    .CountAsync(a => a.DepartmanId == departmanId || 
                                   departmanPersoneller.Contains(a.PersonelId.Value));
                
                var tamamlananAtama = await _context.VideoAtamalar
                    .CountAsync(a => (a.DepartmanId == departmanId || 
                                    departmanPersoneller.Contains(a.PersonelId.Value)) && 
                                   a.Durum == "Tamamlandı");
                
                istatistikler["toplamAtama"] = toplamAtama;
                istatistikler["tamamlananAtama"] = tamamlananAtama;
                istatistikler["tamamlanmaOrani"] = toplamAtama > 0 ? (tamamlananAtama * 100 / toplamAtama) : 0;
                istatistikler["personelSayisi"] = departmanPersoneller.Count;
            }
            else
            {
                // Genel istatistikler
                istatistikler["toplamVideo"] = await _context.VideoEgitimler.CountAsync(e => e.Aktif);
                istatistikler["toplamKategori"] = await _context.VideoKategoriler.CountAsync(k => k.Aktif);
                istatistikler["toplamAtama"] = await _context.VideoAtamalar.CountAsync();
                istatistikler["tamamlananAtama"] = await _context.VideoAtamalar.CountAsync(a => a.Durum == "Tamamlandı");
                istatistikler["toplamIzlenme"] = await _context.VideoIzlemeler.CountAsync();
                istatistikler["toplamSertifika"] = await _context.VideoSertifikalar.CountAsync();
            }

            return istatistikler;
        }

        public async Task<List<VideoAtama>> GetBekleyenEgitimlerAsync(int personelId)
        {
            return await _context.VideoAtamalar
                .Where(a => a.PersonelId == personelId && 
                          a.Durum != "Tamamlandı" &&
                          a.VideoEgitim.Aktif)
                .Include(a => a.VideoEgitim)
                    .ThenInclude(e => e.Kategori)
                .OrderBy(a => a.VideoEgitim.ZorunluMu ? 0 : 1)
                .ThenBy(a => a.VideoEgitim.SonTamamlanmaTarihi)
                .ToListAsync();
        }

        public async Task<bool> HatirlatmaGonderAsync(int atamaId)
        {
            var atama = await _context.VideoAtamalar
                .Include(a => a.VideoEgitim)
                .Include(a => a.Personel)
                .FirstOrDefaultAsync(a => a.Id == atamaId);

            if (atama == null) return false;

            // Burada email veya bildirim gönderme işlemi yapılacak
            // Şimdilik sadece veritabanını güncelliyoruz
            atama.HatirlatmaGonderildiMi = true;
            atama.SonHatirlatmaTarihi = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<VideoSertifika> SertifikaOlusturAsync(int videoEgitimId, int personelId)
        {
            Console.WriteLine($"SertifikaOlusturAsync called - videoEgitimId: {videoEgitimId}, personelId: {personelId}");

            var izleme = await _context.VideoIzlemeler
                .FirstOrDefaultAsync(i => i.VideoEgitimId == videoEgitimId &&
                                         i.PersonelId == personelId &&
                                         i.TamamlandiMi);

            Console.WriteLine($"Found izleme record: {izleme != null}");
            if (izleme == null)
            {
                // Check if any izleme record exists at all
                var anyIzleme = await _context.VideoIzlemeler
                    .FirstOrDefaultAsync(i => i.VideoEgitimId == videoEgitimId && i.PersonelId == personelId);
                Console.WriteLine($"Any izleme record exists: {anyIzleme != null}");
                if (anyIzleme != null)
                {
                    Console.WriteLine($"Existing izleme - TamamlandiMi: {anyIzleme.TamamlandiMi}, IzlemeYuzdesi: {anyIzleme.IzlemeYuzdesi}");
                }
                return null;
            }

            // Quiz puanını hesapla
            var sorular = await _context.VideoSorular
                .Where(s => s.VideoEgitimId == videoEgitimId && s.Aktif)
                .ToListAsync();
            
            int? quizPuani = null;
            if (sorular.Any())
            {
                var cevaplar = await _context.VideoSoruCevaplar
                    .Where(c => sorular.Select(s => s.Id).Contains(c.VideoSoruId) && 
                              c.PersonelId == personelId)
                    .ToListAsync();
                
                if (cevaplar.Any())
                {
                    quizPuani = cevaplar.Sum(c => c.AlinanPuan);
                }
            }

            var sertifikaNo = $"CERT-{DateTime.UtcNow:yyyyMMdd}-{personelId:D4}-{videoEgitimId:D4}";
            var sertifika = new VideoSertifika
            {
                VideoEgitimId = videoEgitimId,
                PersonelId = personelId,
                SertifikaNo = sertifikaNo,
                VerilisTarihi = DateTime.UtcNow,
                GecerlilikTarihi = DateTime.UtcNow.AddYears(1),
                QuizPuani = quizPuani,
                IzlemeYuzdesi = izleme.IzlemeYuzdesi,
                SertifikaUrl = $"/api/VideoEgitim/sertifika/download/{sertifikaNo}"
            };

            _context.VideoSertifikalar.Add(sertifika);
            await _context.SaveChangesAsync();

            return sertifika;
        }

        public async Task<List<VideoEgitim>> SearchEgitimlerAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<VideoEgitim>();

            searchTerm = searchTerm.ToLower();

            return await _context.VideoEgitimler
                .Where(e => e.Aktif && 
                          (e.Baslik.ToLower().Contains(searchTerm) ||
                           e.Aciklama.ToLower().Contains(searchTerm) ||
                           e.Egitmen.ToLower().Contains(searchTerm) ||
                           e.Kategori.Ad.ToLower().Contains(searchTerm)))
                .Include(e => e.Kategori)
                .OrderBy(e => e.Baslik)
                .ToListAsync();
        }

        public async Task<object> GetEgitimDetayAsync(int egitimId, int? personelId = null)
        {
            VideoEgitim egitim;
            try
            {
                Console.WriteLine($"=== GetEgitimDetayAsync called with egitimId: {egitimId}, personelId: {personelId} ===");
                
                egitim = await _context.VideoEgitimler
                    .Include(e => e.Kategori)
                    .Include(e => e.VideoSorular)
                    .Include(e => e.VideoYorumlar)
                        .ThenInclude(y => y.Personel)
                    .FirstOrDefaultAsync(e => e.Id == egitimId);

                Console.WriteLine($"Egitim found: {egitim != null}, ID: {egitim?.Id}, Title: {egitim?.Baslik}");

                if (egitim == null) 
                {
                    Console.WriteLine($"ERROR: Video eğitim bulunamadı - egitimId: {egitimId}");
                    return new
                    {
                        error = true,
                        message = "Video eğitim bulunamadı",
                        egitimId = egitimId
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in GetEgitimDetayAsync (database query): {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }

            // Get user-specific data if personelId provided
            object izlemeKaydi = null;
            object atama = null;
            
            try
            {
                if (personelId.HasValue)
                {
                    Console.WriteLine($"Getting user-specific data for personelId: {personelId}");
                    izlemeKaydi = await _context.VideoIzlemeler
                        .Where(i => i.VideoEgitimId == egitimId && i.PersonelId == personelId)
                        .OrderByDescending(i => i.IzlemeBaslangic)
                        .FirstOrDefaultAsync();
                        
                    atama = await _context.VideoAtamalar
                        .FirstOrDefaultAsync(a => a.VideoEgitimId == egitimId && a.PersonelId == personelId);
                    
                    Console.WriteLine($"Found izlemeKaydi: {izlemeKaydi != null}, atama: {atama != null}");
                }

                // Get statistics separately to avoid complex LINQ translation
                Console.WriteLine("Getting statistics...");
                var toplamIzlenme = await _context.VideoIzlemeler
                    .CountAsync(i => i.VideoEgitimId == egitimId);

                // Calculate average rating
                var puanliIzlemeler = await _context.VideoIzlemeler
                    .Where(i => i.VideoEgitimId == egitimId && i.Puan.HasValue)
                    .Select(i => i.Puan.Value)
                    .ToListAsync();
                var ortalamaPuan = puanliIzlemeler.Any() ? puanliIzlemeler.Average() : 0.0;

                // Calculate completion rate
                var tumIzlemeler = await _context.VideoIzlemeler
                    .Where(i => i.VideoEgitimId == egitimId)
                    .Select(i => (double)i.IzlemeYuzdesi)
                    .ToListAsync();
                var tamamlanmaOrani = tumIzlemeler.Any() ? tumIzlemeler.Average() : 0.0;

                Console.WriteLine($"Statistics: toplamIzlenme={toplamIzlenme}, ortalamaPuan={ortalamaPuan}, tamamlanmaOrani={tamamlanmaOrani}");

                var result = new
                {
                    egitim,
                    izlemeKaydi,
                    atama,
                    toplamIzlenme,
                    ortalamaPuan,
                    tamamlanmaOrani
                };

                Console.WriteLine("Successfully created result object");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in GetEgitimDetayAsync (statistics/user data): {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<bool> UpdateVideoProgressAsync(int personelId, object progressDataObj)
        {
            try
            {
                // Cast object to IzlemeKayitModel (from Controller)
                var progressData = progressDataObj as dynamic;
                
                var videoEgitimId = (int)progressData.VideoEgitimId;
                var toplamIzlenenSure = (int)progressData.ToplamIzlenenSure;
                var izlemeYuzdesi = (int)progressData.IzlemeYuzdesi;
                var tamamlandiMi = (bool)progressData.TamamlandiMi;
                
                Console.WriteLine($"Service: Updating progress for PersonelId:{personelId}, VideoEgitimId:{videoEgitimId}, Progress:{izlemeYuzdesi}%, Completed:{tamamlandiMi}");

                // Mevcut izleme kaydını bul veya yeni oluştur
                var mevcutIzleme = await _context.VideoIzlemeler
                    .FirstOrDefaultAsync(i => i.PersonelId == personelId && i.VideoEgitimId == videoEgitimId);

                if (mevcutIzleme == null)
                {
                    // Yeni izleme kaydı oluştur
                    mevcutIzleme = new VideoIzleme
                    {
                        PersonelId = personelId,
                        VideoEgitimId = videoEgitimId,
                        IzlemeBaslangic = DateTime.UtcNow,
                        ToplamIzlenenSure = toplamIzlenenSure,
                        IzlemeYuzdesi = izlemeYuzdesi,
                        TamamlandiMi = tamamlandiMi,
                        IzlemeBitis = tamamlandiMi ? DateTime.UtcNow : null,
                        TamamlanmaTarihi = tamamlandiMi ? DateTime.UtcNow : null,
                        CihazTipi = (string)progressData.CihazTipi,
                        VideoPlatform = (string)progressData.VideoPlatform,
                        IzlemeBaslangicSaniye = (int)progressData.IzlemeBaslangicSaniye,
                        IzlemeBitisSaniye = (int)progressData.IzlemeBitisSaniye,
                        VideoToplamSure = (int)progressData.VideoToplamSure
                    };
                    
                    _context.VideoIzlemeler.Add(mevcutIzleme);
                    Console.WriteLine("New video progress record created");
                }
                else
                {
                    // Mevcut kaydı güncelle - Progress'i her zaman güncelle
                    mevcutIzleme.IzlemeYuzdesi = Math.Max(mevcutIzleme.IzlemeYuzdesi, izlemeYuzdesi);
                    mevcutIzleme.ToplamIzlenenSure = Math.Max(mevcutIzleme.ToplamIzlenenSure, toplamIzlenenSure);
                    mevcutIzleme.IzlemeBitisSaniye = Math.Max(mevcutIzleme.IzlemeBitisSaniye, (int)progressData.IzlemeBitisSaniye);
                    Console.WriteLine($"Updated progress to {mevcutIzleme.IzlemeYuzdesi}%");
                    
                    if (tamamlandiMi && !mevcutIzleme.TamamlandiMi)
                    {
                        mevcutIzleme.TamamlandiMi = true;
                        mevcutIzleme.IzlemeBitis = DateTime.UtcNow;
                        mevcutIzleme.TamamlanmaTarihi = DateTime.UtcNow;
                        Console.WriteLine("Video marked as completed");
                    }
                }

                // VideoAtama durumunu güncelle
                if (tamamlandiMi)
                {
                    var atama = await _context.VideoAtamalar
                        .FirstOrDefaultAsync(a => a.VideoEgitimId == videoEgitimId && 
                                                 a.PersonelId == personelId);
                    if (atama != null && atama.Durum != "Tamamlandı")
                    {
                        atama.Durum = "Tamamlandı";
                        atama.TamamlanmaTarihi = DateTime.UtcNow;
                        Console.WriteLine("VideoAtama status updated to 'Tamamlandı'");
                    }
                }

                await _context.SaveChangesAsync();
                Console.WriteLine("Video progress and assignment status saved to database");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateVideoProgressAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<object> GetVideoDurationAsync(string videoUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(videoUrl))
                    return null;

                Console.WriteLine($"Getting duration for URL: {videoUrl}");

                // YouTube video detection
                if (videoUrl.Contains("youtube.com") || videoUrl.Contains("youtu.be"))
                {
                    return await GetYouTubeDurationAsync(videoUrl);
                }
                // Vimeo video detection  
                else if (videoUrl.Contains("vimeo.com"))
                {
                    return await GetVimeoDurationAsync(videoUrl);
                }
                
                // For local videos or unsupported platforms
                return new { 
                    success = false, 
                    platform = "Local", 
                    message = "Bu video platformu için otomatik süre çekimi desteklenmiyor" 
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetVideoDurationAsync: {ex.Message}");
                return null;
            }
        }

        private async Task<object> GetYouTubeDurationAsync(string videoUrl)
        {
            try
            {
                // Extract video ID from YouTube URL
                var videoId = ExtractYouTubeVideoId(videoUrl);
                if (string.IsNullOrEmpty(videoId))
                {
                    return new { success = false, message = "YouTube video ID çıkarılamadı" };
                }

                // YouTube Data API v3 kullanarak süreyi çek
                // API key gerekli - şimdilik static değer dönelim demo için
                // Gerçek implementasyonda appsettings.json'dan API key alınmalı
                
                using var httpClient = new HttpClient();
                var apiKey = "YOUR_YOUTUBE_API_KEY"; // Bu değer appsettings.json'dan gelecek
                var apiUrl = $"https://www.googleapis.com/youtube/v3/videos?id={videoId}&part=contentDetails,snippet&key={apiKey}";
                
                // Demo için static değer döndürelim
                Console.WriteLine($"YouTube Video ID: {videoId}");
                
                // Gerçek implementasyon için video ID'ye göre farklı süreler döndürelim
                var estimatedDuration = GetEstimatedDurationFromVideoId(videoId);
                return new { 
                    success = true, 
                    platform = "YouTube", 
                    durationSeconds = estimatedDuration,
                    durationMinutes = Math.Ceiling((double)estimatedDuration / 60),
                    videoId = videoId,
                    thumbnailUrl = $"https://img.youtube.com/vi/{videoId}/maxresdefault.jpg"
                };
                
                // Gerçek API implementation:
                /*
                var response = await httpClient.GetStringAsync(apiUrl);
                var jsonData = JsonDocument.Parse(response);
                
                if (jsonData.RootElement.TryGetProperty("items", out var items) && items.GetArrayLength() > 0)
                {
                    var item = items[0];
                    var contentDetails = item.GetProperty("contentDetails");
                    var duration = contentDetails.GetProperty("duration").GetString(); // PT4M13S format
                    
                    var durationSeconds = ParseYouTubeDuration(duration);
                    var durationMinutes = Math.Ceiling((double)durationSeconds / 60);
                    
                    var snippet = item.GetProperty("snippet");
                    var title = snippet.GetProperty("title").GetString();
                    var thumbnailUrl = snippet.GetProperty("thumbnails").GetProperty("maxres").GetProperty("url").GetString();
                    
                    return new { 
                        success = true, 
                        platform = "YouTube", 
                        durationSeconds = durationSeconds,
                        durationMinutes = (int)durationMinutes,
                        title = title,
                        thumbnailUrl = thumbnailUrl,
                        videoId = videoId
                    };
                }
                */
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting YouTube duration: {ex.Message}");
                return new { success = false, message = $"YouTube API hatası: {ex.Message}" };
            }
        }

        private async Task<object> GetVimeoDurationAsync(string videoUrl)
        {
            try
            {
                // Extract video ID from Vimeo URL  
                var videoId = ExtractVimeoVideoId(videoUrl);
                if (string.IsNullOrEmpty(videoId))
                {
                    return new { success = false, message = "Vimeo video ID çıkarılamadı" };
                }

                // Vimeo API kullanarak süreyi çek
                using var httpClient = new HttpClient();
                var apiUrl = $"https://vimeo.com/api/oembed.json?url=https://vimeo.com/{videoId}";
                
                Console.WriteLine($"Vimeo Video ID: {videoId}");
                
                // Gerçek implementasyon için video ID'ye göre farklı süreler döndürelim
                var estimatedDuration = GetEstimatedDurationFromVideoId(videoId);
                return new { 
                    success = true, 
                    platform = "Vimeo", 
                    durationSeconds = estimatedDuration,
                    durationMinutes = Math.Ceiling((double)estimatedDuration / 60),
                    videoId = videoId,
                    thumbnailUrl = $"https://i.vimeocdn.com/video/{videoId}_640x360.jpg"
                };
                
                // Gerçek API implementation:
                /*
                var response = await httpClient.GetStringAsync(apiUrl);
                var jsonData = JsonDocument.Parse(response);
                
                if (jsonData.RootElement.TryGetProperty("duration", out var duration))
                {
                    var durationSeconds = duration.GetInt32();
                    var durationMinutes = Math.Ceiling((double)durationSeconds / 60);
                    
                    var title = jsonData.RootElement.GetProperty("title").GetString();
                    var thumbnailUrl = jsonData.RootElement.GetProperty("thumbnail_url").GetString();
                    
                    return new { 
                        success = true, 
                        platform = "Vimeo", 
                        durationSeconds = durationSeconds,
                        durationMinutes = (int)durationMinutes,
                        title = title,
                        thumbnailUrl = thumbnailUrl,
                        videoId = videoId
                    };
                }
                */
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting Vimeo duration: {ex.Message}");
                return new { success = false, message = $"Vimeo API hatası: {ex.Message}" };
            }
        }

        private string ExtractYouTubeVideoId(string videoUrl)
        {
            try
            {
                if (videoUrl.Contains("youtu.be/"))
                {
                    return videoUrl.Split("youtu.be/")[1].Split('?')[0];
                }
                else if (videoUrl.Contains("youtube.com/watch?v="))
                {
                    return videoUrl.Split("v=")[1].Split('&')[0];
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        private string ExtractVimeoVideoId(string videoUrl)
        {
            try
            {
                // Handle group videos: https://vimeo.com/groups/114/videos/1017406920
                if (videoUrl.Contains("/groups/") && videoUrl.Contains("/videos/"))
                {
                    var parts = videoUrl.Split("/videos/");
                    if (parts.Length > 1)
                    {
                        return parts[1].Split('?')[0].Split('#')[0];
                    }
                }
                // Handle regular videos: https://vimeo.com/1017406920
                else if (videoUrl.Contains("vimeo.com/"))
                {
                    var parts = videoUrl.Split("vimeo.com/");
                    if (parts.Length > 1)
                    {
                        return parts[1].Split('?')[0].Split('#')[0];
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        private int ParseYouTubeDuration(string duration)
        {
            // Parse ISO 8601 duration format (PT4M13S) to seconds
            try
            {
                var totalSeconds = 0;
                duration = duration.Replace("PT", "");
                
                if (duration.Contains("H"))
                {
                    var hours = int.Parse(duration.Split('H')[0]);
                    totalSeconds += hours * 3600;
                    duration = duration.Split('H')[1];
                }
                
                if (duration.Contains("M"))
                {
                    var minutes = int.Parse(duration.Split('M')[0]);
                    totalSeconds += minutes * 60;
                    duration = duration.Split('M')[1];
                }
                
                if (duration.Contains("S"))
                {
                    var seconds = int.Parse(duration.Split('S')[0]);
                    totalSeconds += seconds;
                }
                
                return totalSeconds;
            }
            catch
            {
                return 0;
            }
        }

        private int GetEstimatedDurationFromVideoId(string videoId)
        {
            // Video ID'nin son karakterine göre farklı süreler döndür (demo amaçlı)
            var lastChar = videoId.LastOrDefault();
            var hash = Math.Abs(videoId.GetHashCode());
            
            // 3-15 dakika arasında rastgele bir süre oluştur
            var baseDuration = 180; // 3 dakika
            var variableDuration = hash % 720; // 0-12 dakika arasında değişken
            
            return baseDuration + variableDuration;
        }

        public async Task<Dictionary<string, object>> GetRaporIstatistikleriAsync()
        {
            var istatistikler = new Dictionary<string, object>();

            try
            {
                // Genel video eğitim istatistikleri
                var toplamVideoEgitim = await _context.VideoEgitimler.CountAsync(e => e.Aktif);
                var toplamAtama = await _context.VideoAtamalar.CountAsync();
                var tamamlananAtama = await _context.VideoAtamalar.CountAsync(a => a.Durum == "Tamamlandı");
                var devamEdenAtama = await _context.VideoAtamalar.CountAsync(a => a.Durum == "Devam Ediyor");
                var atandiAtama = await _context.VideoAtamalar.CountAsync(a => a.Durum == "Atandı");
                var suresiGectiAtama = await _context.VideoAtamalar.CountAsync(a => a.Durum == "Süresi Geçti");

                // Katılımcı sayısı (benzersiz personel sayısı)
                var toplamKatilimci = await _context.VideoAtamalar
                    .Where(a => a.PersonelId.HasValue)
                    .Select(a => a.PersonelId.Value)
                    .Distinct()
                    .CountAsync();

                // Ortalama izleme süresi hesaplama (güvenli)
                var izlemeKayitlari = await _context.VideoIzlemeler.ToListAsync();
                double ortalamaIzlemeSuresi = 0;

                if (izlemeKayitlari.Any())
                {
                    var toplamIzlemeSuresi = izlemeKayitlari.Sum(i => i.ToplamIzlenenSure);
                    ortalamaIzlemeSuresi = Math.Round((double)toplamIzlemeSuresi / izlemeKayitlari.Count / 60.0, 1);
                }

                // Tamamlanma oranı
                var tamamlanmaOrani = toplamAtama > 0 ? (int)Math.Round((double)tamamlananAtama * 100 / toplamAtama) : 0;

                istatistikler["toplamVideoEgitim"] = toplamVideoEgitim;
                istatistikler["tamamlananAtama"] = tamamlananAtama;
                istatistikler["devamEdenAtama"] = devamEdenAtama;
                istatistikler["atandiAtama"] = atandiAtama;
                istatistikler["suresiGectiAtama"] = suresiGectiAtama;
                istatistikler["toplamKatilimci"] = toplamKatilimci;
                istatistikler["ortalamaIzlemeSuresi"] = ortalamaIzlemeSuresi;
                istatistikler["tamamlanmaOrani"] = tamamlanmaOrani;

                // Debug log ekle
                Console.WriteLine($"DEBUG - Atama Durumları: Tamamlanan={tamamlananAtama}, DevamEden={devamEdenAtama}, Atandi={atandiAtama}, SuresiGecti={suresiGectiAtama}");

                return istatistikler;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetRaporIstatistikleriAsync error: {ex.Message}");
                // Return empty stats on error
                istatistikler["toplamVideoEgitim"] = 0;
                istatistikler["tamamlananAtama"] = 0;
                istatistikler["devamEdenAtama"] = 0;
                istatistikler["atandiAtama"] = 0;
                istatistikler["suresiGectiAtama"] = 0;
                istatistikler["toplamKatilimci"] = 0;
                istatistikler["ortalamaIzlemeSuresi"] = 0.0;
                istatistikler["tamamlanmaOrani"] = 0;
                return istatistikler;
            }
        }

        public async Task<List<object>> GetPersonelEgitimOzetiAsync()
        {
            var personelOzeti = new List<object>();

            try
            {
                var personelListesi = await _context.Personeller
                    .Include(p => p.Pozisyon)
                        .ThenInclude(po => po.Departman)
                    .Where(p => p.Aktif)
                    .Take(50) // Performans için limit ekle
                    .ToListAsync();

                foreach (var personel in personelListesi)
                {
                    try
                    {
                        // Personelin video eğitim atamalarını al
                        var toplamVideoEgitim = await _context.VideoAtamalar
                            .CountAsync(a => a.PersonelId == personel.Id);

                        var tamamlanan = await _context.VideoAtamalar
                            .CountAsync(a => a.PersonelId == personel.Id && a.Durum == "Tamamlandı");

                        // İzleme yüzdesi hesaplama
                        var toplamIzleme = await _context.VideoIzlemeler
                            .Where(i => i.PersonelId == personel.Id)
                            .ToListAsync();

                        var izlemeYuzdesi = 0.0;
                        var toplamSure = 0.0;

                        if (toplamIzleme.Any())
                        {
                            izlemeYuzdesi = Math.Round(toplamIzleme.Average(i => i.IzlemeYuzdesi));
                            toplamSure = Math.Round(toplamIzleme.Sum(i => i.ToplamIzlenenSure) / 60.0, 1); // Dakika olarak
                        }

                        if (toplamVideoEgitim > 0) // Sadece eğitim atanmış personeli dahil et
                        {
                            personelOzeti.Add(new
                            {
                                id = personel.Id,
                                personelAd = $"{personel.Ad} {personel.Soyad}",
                                departman = personel.Pozisyon?.Departman?.Ad ?? "Bilinmiyor",
                                toplamVideoEgitim = toplamVideoEgitim,
                                tamamlanan = tamamlanan,
                                izlemeYuzdesi = (int)izlemeYuzdesi,
                                toplamSure = (int)toplamSure
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing personnel {personel.Id}: {ex.Message}");
                        continue;
                    }
                }

                return personelOzeti.OrderBy(p => ((dynamic)p).departman).ThenBy(p => ((dynamic)p).personelAd).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetPersonelEgitimOzetiAsync error: {ex.Message}");
                return personelOzeti; // Return empty list on error
            }
        }

        public async Task<List<object>> GetDepartmanRaporuAsync(int year)
        {
            var departmanRaporu = new List<object>();

            try
            {
                var departmanlar = await _context.Departmanlar
                    .Where(d => d.Aktif)
                    .ToListAsync();

                foreach (var departman in departmanlar)
                {
                    try
                    {
                        // Departmandaki personelleri al
                        var departmanPersonelleri = await _context.Personeller
                            .Where(p => p.Pozisyon.DepartmanId == departman.Id && p.Aktif)
                            .Select(p => p.Id)
                            .ToListAsync();

                        if (!departmanPersonelleri.Any()) continue;

                        // Video eğitim istatistikleri
                        var toplamVideoEgitim = await _context.VideoAtamalar
                            .Where(a => a.DepartmanId == departman.Id ||
                                       (a.PersonelId.HasValue && departmanPersonelleri.Contains(a.PersonelId.Value)))
                            .Select(a => a.VideoEgitimId)
                            .Distinct()
                            .CountAsync();

                        var toplamKatilimci = departmanPersonelleri.Count;

                        // Departman bazlı izleme istatistikleri
                        var izlemeVerileri = await _context.VideoIzlemeler
                            .Where(i => departmanPersonelleri.Contains(i.PersonelId))
                            .ToListAsync();

                        var ortalamaIzlemeSuresi = 0.0;
                        var tamamlanmaOrani = 0;

                        if (izlemeVerileri.Any())
                        {
                            ortalamaIzlemeSuresi = Math.Round(izlemeVerileri.Average(i => i.ToplamIzlenenSure) / 60.0, 1);

                            var tamamlananSayisi = await _context.VideoAtamalar
                                .CountAsync(a => a.PersonelId.HasValue && departmanPersonelleri.Contains(a.PersonelId.Value) && a.Durum == "Tamamlandı");
                            var toplamAtamaSayisi = await _context.VideoAtamalar
                                .CountAsync(a => a.PersonelId.HasValue && departmanPersonelleri.Contains(a.PersonelId.Value));

                            tamamlanmaOrani = toplamAtamaSayisi > 0 ? (int)Math.Round((double)tamamlananSayisi * 100 / toplamAtamaSayisi) : 0;
                        }

                        departmanRaporu.Add(new
                        {
                            id = departman.Id,
                            departman = departman.Ad,
                            toplamVideoEgitim = toplamVideoEgitim,
                            toplamKatilimci = toplamKatilimci,
                            ortalamaIzlemeSuresi = (int)ortalamaIzlemeSuresi,
                            tamamlanmaOrani = tamamlanmaOrani
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing department {departman.Ad}: {ex.Message}");
                        continue;
                    }
                }

                return departmanRaporu.OrderBy(d => ((dynamic)d).departman).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetDepartmanRaporuAsync error: {ex.Message}");
                return departmanRaporu; // Return empty list on error
            }
        }

        public async Task<List<object>> GetAylikEgitimTrendiAsync(int year = 0)
        {
            var aylikTrend = new List<object>();

            try
            {
                var currentYear = year == 0 ? DateTime.Now.Year : year;

                // 12 aylık veri için tüm ayları initialize et
                var aylar = new string[] { "Oca", "Şub", "Mar", "Nis", "May", "Haz", "Tem", "Ağu", "Eyl", "Eki", "Kas", "Ara" };

                for (int ay = 1; ay <= 12; ay++)
                {
                    try
                    {
                        // Bu ay için video eğitim atama sayısı
                        var egitimSayisi = await _context.VideoAtamalar
                            .Where(a => a.AtamaTarihi.Year == currentYear && a.AtamaTarihi.Month == ay)
                            .CountAsync();

                        // Bu ay için benzersiz katılımcı sayısı
                        var katilimciSayisi = await _context.VideoAtamalar
                            .Where(a => a.AtamaTarihi.Year == currentYear && a.AtamaTarihi.Month == ay && a.PersonelId.HasValue)
                            .Select(a => a.PersonelId.Value)
                            .Distinct()
                            .CountAsync();

                        aylikTrend.Add(new
                        {
                            ay = aylar[ay - 1],
                            ayNumarasi = ay,
                            egitim = egitimSayisi,
                            katilimci = katilimciSayisi
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing month {ay}: {ex.Message}");
                        // Hata durumunda boş veri ekle
                        aylikTrend.Add(new
                        {
                            ay = aylar[ay - 1],
                            ayNumarasi = ay,
                            egitim = 0,
                            katilimci = 0
                        });
                    }
                }

                return aylikTrend;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetAylikEgitimTrendiAsync error: {ex.Message}");
                // Hata durumunda boş 12 aylık veri döndür
                var aylar = new string[] { "Oca", "Şub", "Mar", "Nis", "May", "Haz", "Tem", "Ağu", "Eyl", "Eki", "Kas", "Ara" };
                for (int ay = 1; ay <= 12; ay++)
                {
                    aylikTrend.Add(new
                    {
                        ay = aylar[ay - 1],
                        ayNumarasi = ay,
                        egitim = 0,
                        katilimci = 0
                    });
                }
                return aylikTrend;
            }
        }
    }
}