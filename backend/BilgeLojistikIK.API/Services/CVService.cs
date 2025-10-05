using Microsoft.EntityFrameworkCore;
using BilgeLojistikIK.API.Data;
using BilgeLojistikIK.API.Models;
using System.Text;

namespace BilgeLojistikIK.API.Services
{
    public interface ICVService
    {
        Task<string> OtomatikCVOlustur(int adayId);
        Task<AdayCV> CVYukle(int adayId, IFormFile file);
        Task<string> CVHtmlGetir(int adayId, string cvTipi);
        Task<bool> CVSil(int cvId);
        Task<List<AdayCV>> AdayinCVleriniGetir(int adayId);
    }

    public class CVService : ICVService
    {
        private readonly BilgeLojistikIKContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CVService(BilgeLojistikIKContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string> OtomatikCVOlustur(int adayId)
        {
            var aday = await _context.Adaylar
                .Include(a => a.Deneyimler.OrderByDescending(d => d.BaslangicTarihi))
                .Include(a => a.Yetenekler.OrderByDescending(y => y.Seviye))
                .Include(a => a.Egitimler.OrderByDescending(e => e.MezuniyetYili ?? e.BaslangicYili))
                .Include(a => a.Sertifikalar.OrderByDescending(s => s.Tarih))
                .Include(a => a.Referanslar)
                .Include(a => a.Diller.OrderBy(d => d.Dil))
                .Include(a => a.Projeler.OrderByDescending(p => p.BaslangicTarihi))
                .Include(a => a.Hobiler.OrderBy(h => h.Kategori))
                .FirstOrDefaultAsync(a => a.Id == adayId);

            if (aday == null)
                throw new ArgumentException("Aday bulunamadı");

            var htmlContent = GenerateCVHtml(aday);

            var existingCV = await _context.AdayCVleri
                .FirstOrDefaultAsync(cv => cv.AdayId == adayId && cv.CVTipi == "Otomatik");

            if (existingCV != null)
            {
                existingCV.OtomatikCVHtml = htmlContent;
                existingCV.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                var yeniCV = new AdayCV
                {
                    AdayId = adayId,
                    CVTipi = "Otomatik",
                    OtomatikCVHtml = htmlContent,
                    DosyaAdi = $"{aday.Ad}_{aday.Soyad}_CV.html",
                    Aktif = true
                };
                _context.AdayCVleri.Add(yeniCV);
            }

            aday.OtomatikCVOlusturuldu = true;
            await _context.SaveChangesAsync();

            return htmlContent;
        }

        public async Task<AdayCV> CVYukle(int adayId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Dosya seçilmedi");

            var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
                throw new ArgumentException("Desteklenmeyen dosya formatı. PDF, DOC veya DOCX dosyası seçiniz.");

            var aday = await _context.Adaylar.FindAsync(adayId);
            if (aday == null)
                throw new ArgumentException("Aday bulunamadı");

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "cv");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{aday.Id}_{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            var relativePath = Path.Combine("uploads", "cv", uniqueFileName).Replace("\\", "/");

            var adayCV = new AdayCV
            {
                AdayId = adayId,
                CVTipi = "Yuklenmiş",
                DosyaAdi = file.FileName,
                DosyaYolu = relativePath,
                DosyaBoyutu = file.Length,
                MimeTipi = file.ContentType,
                Aktif = true
            };

            _context.AdayCVleri.Add(adayCV);
            await _context.SaveChangesAsync();

            return adayCV;
        }

        public async Task<string> CVHtmlGetir(int adayId, string cvTipi)
        {
            var cv = await _context.AdayCVleri
                .FirstOrDefaultAsync(cv => cv.AdayId == adayId && cv.CVTipi == cvTipi && cv.Aktif);

            if (cv == null)
                return string.Empty;

            if (cvTipi == "Otomatik")
            {
                return cv.OtomatikCVHtml ?? string.Empty;
            }
            else
            {
                return $"<p>Yüklenmiş CV: <a href=\"/{cv.DosyaYolu}\" target=\"_blank\">{cv.DosyaAdi}</a></p>";
            }
        }

        public async Task<bool> CVSil(int cvId)
        {
            var cv = await _context.AdayCVleri.FindAsync(cvId);
            if (cv == null)
                return false;

            if (cv.CVTipi == "Yuklenmiş" && !string.IsNullOrEmpty(cv.DosyaYolu))
            {
                var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, cv.DosyaYolu);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }

            _context.AdayCVleri.Remove(cv);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<AdayCV>> AdayinCVleriniGetir(int adayId)
        {
            return await _context.AdayCVleri
                .Where(cv => cv.AdayId == adayId && cv.Aktif)
                .OrderByDescending(cv => cv.CreatedAt)
                .ToListAsync();
        }

        private string GenerateCVHtml(Aday aday)
        {
            var html = new StringBuilder();

            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang='tr'>");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset='UTF-8'>");
            html.AppendLine("    <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            html.AppendLine($"    <title>{aday.Ad} {aday.Soyad} - Özgeçmiş</title>");
            html.AppendLine("    <style>");
            html.AppendLine(GetCVStyle());
            html.AppendLine("    </style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");

            html.AppendLine("    <div class='cv-container'>");

            // Sol Sütun
            html.AppendLine("        <div class='left-column'>");

            // Profil Fotoğrafı
            html.AppendLine("            <div class='profile-section'>");
            if (!string.IsNullOrEmpty(aday.FotografYolu))
            {
                // Fotoğraf yolunu backend server'dan erişilebilir hale getir
                var fotografUrl = aday.FotografYolu.StartsWith("http")
                    ? aday.FotografYolu
                    : $"http://localhost:5000/{aday.FotografYolu.TrimStart('/')}";
                html.AppendLine($"                <img src='{fotografUrl}' alt='Profil Fotoğrafı' class='profile-photo' />");
            }
            else
            {
                html.AppendLine("                <div class='profile-placeholder'>");
                html.AppendLine($"                    <span>{aday.Ad.Substring(0, 1)}{aday.Soyad.Substring(0, 1)}</span>");
                html.AppendLine("                </div>");
            }
            html.AppendLine($"                <h1>{aday.Ad} {aday.Soyad}</h1>");
            html.AppendLine("            </div>");

            // İletişim Bilgileri
            html.AppendLine("            <div class='contact-section'>");
            html.AppendLine("                <h3><i class='icon'>✉</i> İletişim</h3>");
            html.AppendLine($"                <p><strong>Email:</strong> {aday.Email}</p>");
            if (!string.IsNullOrEmpty(aday.Telefon))
                html.AppendLine($"                <p><strong>Telefon:</strong> {aday.Telefon}</p>");
            if (!string.IsNullOrEmpty(aday.Sehir))
                html.AppendLine($"                <p><strong>Şehir:</strong> {aday.Sehir}</p>");
            if (!string.IsNullOrEmpty(aday.Adres))
                html.AppendLine($"                <p><strong>Adres:</strong> {aday.Adres}</p>");
            if (!string.IsNullOrEmpty(aday.LinkedinUrl))
                html.AppendLine($"                <p><strong>LinkedIn:</strong> <a href='{aday.LinkedinUrl}' target='_blank'>{aday.LinkedinUrl}</a></p>");
            html.AppendLine("            </div>");

            // Kişisel Bilgiler
            html.AppendLine("            <div class='personal-section'>");
            html.AppendLine("                <h3><i class='icon'>👤</i> Kişisel Bilgiler</h3>");
            if (aday.DogumTarihi.HasValue)
            {
                // Timezone sorunlarından kaçınmak için sadece tarih bileşenlerini kullan
                var dogumTarihi = aday.DogumTarihi.Value;
                var dogumTarihiStr = $"{dogumTarihi.Day:D2}.{dogumTarihi.Month:D2}.{dogumTarihi.Year}";

                // Debug: Veritabanından gelen doğum tarihi
                Console.WriteLine($"CV DEBUG - Doğum Tarihi DB: {dogumTarihi}, Formatlanmış: {dogumTarihiStr}");

                html.AppendLine($"                <p><strong>Doğum Tarihi:</strong> {dogumTarihiStr}</p>");
            }
            if (!string.IsNullOrEmpty(aday.Cinsiyet))
                html.AppendLine($"                <p><strong>Cinsiyet:</strong> {aday.Cinsiyet}</p>");
            if (!string.IsNullOrEmpty(aday.MedeniDurum))
                html.AppendLine($"                <p><strong>Medeni Durum:</strong> {aday.MedeniDurum}</p>");
            if (!string.IsNullOrEmpty(aday.AskerlikDurumu))
                html.AppendLine($"                <p><strong>Askerlik:</strong> {aday.AskerlikDurumu}</p>");
            html.AppendLine("            </div>");

            // Diller
            if (aday.Diller.Any())
            {
                html.AppendLine("            <div class='languages-section'>");
                html.AppendLine("                <h3><i class='icon'>🌐</i> Diller</h3>");
                foreach (var dil in aday.Diller)
                {
                    html.AppendLine("                <div class='language-item'>");
                    html.AppendLine($"                    <p><strong>{dil.Dil}</strong></p>");
                    html.AppendLine($"                    <div class='language-skills'>");
                    html.AppendLine($"                        <span>Okuma: {GetLanguageLevel(dil.OkumaSeviyesi)}</span>");
                    html.AppendLine($"                        <span>Yazma: {GetLanguageLevel(dil.YazmaSeviyesi)}</span>");
                    html.AppendLine($"                        <span>Konuşma: {GetLanguageLevel(dil.KonusmaSeviyesi)}</span>");
                    html.AppendLine($"                    </div>");
                    html.AppendLine("                </div>");
                }
                html.AppendLine("            </div>");
            }

            // Yetenekler
            if (aday.Yetenekler.Any())
            {
                html.AppendLine("            <div class='skills-section'>");
                html.AppendLine("                <h3><i class='icon'>🎯</i> Yetenekler</h3>");
                foreach (var yetenek in aday.Yetenekler)
                {
                    var seviyeText = yetenek.Seviye switch
                    {
                        1 => "Başlangıç",
                        2 => "Temel",
                        3 => "Orta",
                        4 => "İyi",
                        5 => "Uzman",
                        _ => ""
                    };
                    html.AppendLine("                <div class='skill-item'>");
                    html.AppendLine($"                    <div class='skill-header'>");
                    html.AppendLine($"                        <span class='skill-name'>{yetenek.Yetenek}</span>");
                    html.AppendLine($"                        <span class='skill-level'>{seviyeText}</span>");
                    html.AppendLine($"                    </div>");
                    html.AppendLine("                    <div class='skill-bar'>");
                    html.AppendLine($"                        <div class='skill-progress' style='width: {yetenek.Seviye * 20}%'></div>");
                    html.AppendLine("                    </div>");
                    html.AppendLine("                </div>");
                }
                html.AppendLine("            </div>");
            }

            // Hobiler
            if (aday.Hobiler.Any())
            {
                html.AppendLine("            <div class='hobbies-section'>");
                html.AppendLine("                <h3><i class='icon'>🎨</i> Hobiler</h3>");
                var kategoriGruplari = aday.Hobiler.GroupBy(h => h.Kategori ?? "Diğer");
                foreach (var grup in kategoriGruplari)
                {
                    if (!string.IsNullOrEmpty(grup.Key) && grup.Key != "Diğer")
                        html.AppendLine($"                <p><strong>{grup.Key}:</strong></p>");
                    html.AppendLine("                <ul class='hobby-list'>");
                    foreach (var hobi in grup)
                    {
                        html.AppendLine($"                    <li>{hobi.Hobi}{(!string.IsNullOrEmpty(hobi.Seviye) ? $" ({hobi.Seviye})" : "")}</li>");
                    }
                    html.AppendLine("                </ul>");
                }
                html.AppendLine("            </div>");
            }

            html.AppendLine("        </div>"); // Sol sütun sonu

            // Sağ Sütun
            html.AppendLine("        <div class='right-column'>");

            // Hakkımda/Özet
            if (!string.IsNullOrEmpty(aday.Notlar))
            {
                html.AppendLine("            <div class='summary-section'>");
                html.AppendLine("                <h2><i class='icon'>📝</i> Hakkımda</h2>");
                html.AppendLine($"                <p>{aday.Notlar}</p>");
                html.AppendLine("            </div>");
            }

            // İş Deneyimi
            if (aday.Deneyimler.Any())
            {
                html.AppendLine("            <div class='experience-section'>");
                html.AppendLine("                <h2><i class='icon'>💼</i> İş Deneyimi</h2>");
                foreach (var deneyim in aday.Deneyimler)
                {
                    html.AppendLine("                <div class='experience-item'>");
                    html.AppendLine($"                    <h3>{deneyim.Pozisyon}</h3>");
                    html.AppendLine($"                    <h4>{deneyim.SirketAd}</h4>");
                    var baslangic = deneyim.BaslangicTarihi?.ToString("MM/yyyy") ?? "";
                    var bitis = deneyim.HalenCalisiyor ? "Devam ediyor" : deneyim.BitisTarihi?.ToString("MM/yyyy") ?? "";
                    html.AppendLine($"                    <p class='date-range'>{baslangic} - {bitis}</p>");
                    if (!string.IsNullOrEmpty(deneyim.Aciklama))
                    {
                        var aciklamaSatirlari = deneyim.Aciklama.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                        if (aciklamaSatirlari.Length > 1)
                        {
                            html.AppendLine("                    <ul class='description-list'>");
                            foreach (var satir in aciklamaSatirlari)
                            {
                                if (!string.IsNullOrWhiteSpace(satir))
                                    html.AppendLine($"                        <li>{satir.Trim()}</li>");
                            }
                            html.AppendLine("                    </ul>");
                        }
                        else
                        {
                            html.AppendLine($"                    <p class='description'>{deneyim.Aciklama}</p>");
                        }
                    }
                    html.AppendLine("                </div>");
                }
                html.AppendLine("            </div>");
            }

            // Eğitim Bilgileri
            if (aday.Egitimler.Any() || !string.IsNullOrEmpty(aday.Universite))
            {
                html.AppendLine("            <div class='education-section'>");
                html.AppendLine("                <h2><i class='icon'>🎓</i> Eğitim</h2>");

                // Detaylı eğitim bilgileri varsa onları göster
                if (aday.Egitimler.Any())
                {
                    foreach (var egitim in aday.Egitimler)
                    {
                        html.AppendLine("                <div class='education-item'>");
                        html.AppendLine($"                    <h3>{egitim.Bolum}</h3>");
                        var okulAd = egitim.OkulAd;
                        if (!string.IsNullOrEmpty(egitim.Derece))
                        {
                            html.AppendLine($"                    <h4>{okulAd} <span class='degree-text'>({egitim.Derece})</span></h4>");
                        }
                        else
                        {
                            html.AppendLine($"                    <h4>{okulAd}</h4>");
                        }
                        var egitimTarih = "";
                        if (egitim.BaslangicYili > 0 && egitim.MezuniyetYili.HasValue)
                            egitimTarih = $"{egitim.BaslangicYili} - {egitim.MezuniyetYili}";
                        else if (egitim.MezuniyetYili.HasValue)
                            egitimTarih = egitim.MezuniyetYili.ToString();
                        else if (egitim.BaslangicYili > 0)
                            egitimTarih = $"{egitim.BaslangicYili} - Devam ediyor";
                        if (!string.IsNullOrEmpty(egitimTarih))
                            html.AppendLine($"                    <p class='date-range'>{egitimTarih}</p>");
                        if (egitim.NotOrtalamasi.HasValue)
                            html.AppendLine($"                    <p class='gpa'>Not Ortalaması: {egitim.NotOrtalamasi:F2}</p>");
                        html.AppendLine("                </div>");
                    }
                }
                else
                {
                    // Sadece temel üniversite bilgisi varsa
                    html.AppendLine("                <div class='education-item'>");
                    if (!string.IsNullOrEmpty(aday.Bolum))
                        html.AppendLine($"                    <h3>{aday.Bolum}</h3>");
                    html.AppendLine($"                    <h4>{aday.Universite}</h4>");
                    if (aday.MezuniyetYili.HasValue)
                        html.AppendLine($"                    <p class='date-range'>{aday.MezuniyetYili}</p>");
                    html.AppendLine("                </div>");
                }
                html.AppendLine("            </div>");
            }

            // Sertifikalar
            if (aday.Sertifikalar.Any())
            {
                html.AppendLine("            <div class='certificates-section'>");
                html.AppendLine("                <h2><i class='icon'>🏆</i> Sertifikalar</h2>");
                html.AppendLine("                <!-- GÜNCEL: 3 Sütun Grid Sertifikalar -->");
                html.AppendLine("                <div class='certificates-grid'>");
                foreach (var sertifika in aday.Sertifikalar)
                {
                    html.AppendLine("                    <div class='certificate-item'>");
                    html.AppendLine($"                        <h3>{sertifika.SertifikaAd}</h3>");
                    html.AppendLine($"                        <h4>{sertifika.VerenKurum}</h4>");
                    html.AppendLine($"                        <p class='date-range'>{sertifika.Tarih:MM/yyyy}</p>");
                    if (sertifika.GecerlilikTarihi.HasValue)
                        html.AppendLine($"                        <p class='validity'>Geçerlilik: {sertifika.GecerlilikTarihi:MM/yyyy}</p>");
                    if (!string.IsNullOrEmpty(sertifika.SertifikaNo))
                        html.AppendLine($"                        <p class='cert-no'>Sertifika No: {sertifika.SertifikaNo}</p>");
                    html.AppendLine("                    </div>");
                }
                html.AppendLine("                </div>");
                html.AppendLine("            </div>");
            }

            // Projeler
            if (aday.Projeler.Any())
            {
                html.AppendLine("            <div class='projects-section'>");
                html.AppendLine("                <h2><i class='icon'>🚀</i> Projeler</h2>");
                foreach (var proje in aday.Projeler)
                {
                    html.AppendLine("                <div class='project-item'>");
                    html.AppendLine($"                    <h3>{proje.ProjeAd}</h3>");
                    if (!string.IsNullOrEmpty(proje.Rol))
                        html.AppendLine($"                    <h4>{proje.Rol}</h4>");
                    var projeTarih = "";
                    if (proje.BaslangicTarihi.HasValue && proje.BitisTarihi.HasValue)
                        projeTarih = $"{proje.BaslangicTarihi:MM/yyyy} - {proje.BitisTarihi:MM/yyyy}";
                    else if (proje.BaslangicTarihi.HasValue)
                        projeTarih = $"{proje.BaslangicTarihi:MM/yyyy} - Devam ediyor";
                    if (!string.IsNullOrEmpty(projeTarih))
                        html.AppendLine($"                    <p class='date-range'>{projeTarih}</p>");
                    if (!string.IsNullOrEmpty(proje.Aciklama))
                    {
                        var aciklamaSatirlari = proje.Aciklama.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                        if (aciklamaSatirlari.Length > 1)
                        {
                            html.AppendLine("                    <ul class='description-list'>");
                            foreach (var satir in aciklamaSatirlari)
                            {
                                if (!string.IsNullOrWhiteSpace(satir))
                                    html.AppendLine($"                        <li>{satir.Trim()}</li>");
                            }
                            html.AppendLine("                    </ul>");
                        }
                        else
                        {
                            html.AppendLine($"                    <p class='description'>{proje.Aciklama}</p>");
                        }
                    }
                    if (!string.IsNullOrEmpty(proje.Teknolojiler))
                        html.AppendLine($"                    <p class='technologies'>Teknolojiler: {proje.Teknolojiler}</p>");
                    if (!string.IsNullOrEmpty(proje.ProjeUrl))
                        html.AppendLine($"                    <p class='project-url'><a href='{proje.ProjeUrl}' target='_blank'>Projeyi Görüntüle</a></p>");
                    html.AppendLine("                </div>");
                }
                html.AppendLine("            </div>");
            }

            // Referanslar
            if (aday.Referanslar.Any())
            {
                html.AppendLine("            <div class='references-section'>");
                html.AppendLine("                <h2><i class='icon'>📞</i> Referanslar</h2>");
                html.AppendLine("                <!-- GÜNCEL: 3 Sütun Grid Referanslar -->");
                html.AppendLine("                <div class='references-grid'>");
                foreach (var referans in aday.Referanslar)
                {
                    html.AppendLine("                    <div class='reference-item'>");
                    html.AppendLine($"                        <h3>{referans.AdSoyad}</h3>");
                    html.AppendLine($"                        <h4>{referans.Sirket}</h4>");
                    html.AppendLine($"                        <p class='position'>{referans.Pozisyon}</p>");
                    if (!string.IsNullOrEmpty(referans.Telefon))
                        html.AppendLine($"                        <p class='reference-contact'>{referans.Telefon}</p>");
                    if (!string.IsNullOrEmpty(referans.Email))
                        html.AppendLine($"                        <p class='reference-contact'>{referans.Email}</p>");
                    if (!string.IsNullOrEmpty(referans.IliskiTuru))
                        html.AppendLine($"                        <p class='relation'>İlişki: {referans.IliskiTuru}</p>");
                    html.AppendLine("                    </div>");
                }
                html.AppendLine("                </div>");
                html.AppendLine("            </div>");
            }

            html.AppendLine("        </div>"); // Sağ sütun sonu
            html.AppendLine("    </div>"); // Container sonu
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }

        private string GetLanguageLevel(int seviye)
        {
            return seviye switch
            {
                1 => "Başlangıç",
                2 => "Temel",
                3 => "Orta",
                4 => "İyi",
                5 => "Mükemmel",
                _ => "Belirtilmemiş"
            };
        }

        private string GetCVStyle()
        {
            return @"
                * {
                    margin: 0;
                    padding: 0;
                    box-sizing: border-box;
                }

                body {
                    font-family: 'Segoe UI', 'Arial', sans-serif;
                    line-height: 1.1;
                    color: #2C3E50;
                    background-color: #FFFFFF;
                    font-size: 10px;
                }

                .cv-container {
                    display: flex;
                    max-width: 1200px;
                    margin: 0 auto;
                    background: white;
                    box-shadow: 0 0 20px rgba(0,0,0,0.1);
                    min-height: 100vh;
                }

                /* SOL SÜTUN - 30% */
                .left-column {
                    width: 30%;
                    background: linear-gradient(135deg, #2C3E50 0%, #34495E 100%);
                    color: white;
                    padding: 10px 10px;
                    position: relative;
                }

                .left-column h3 {
                    color: #ECF0F1;
                    font-size: 14px;
                    margin-bottom: 8px;
                    padding-bottom: 4px;
                    border-bottom: 2px solid #3498DB;
                    display: flex;
                    align-items: center;
                    gap: 5px;
                }

                .left-column .icon {
                    font-size: 16px;
                }

                /* Profil Bölümü */
                .profile-section {
                    text-align: center;
                    margin-bottom: 8px;
                }

                .profile-photo {
                    width: 100px;
                    height: 100px;
                    border-radius: 50%;
                    object-fit: cover;
                    border: 3px solid #3498DB;
                    margin-bottom: 10px;
                }

                .profile-placeholder {
                    width: 100px;
                    height: 100px;
                    border-radius: 50%;
                    background: #3498DB;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    margin: 0 auto 10px;
                    border: 3px solid #ECF0F1;
                }

                .profile-placeholder span {
                    font-size: 30px;
                    font-weight: bold;
                    color: white;
                }

                .profile-section h1 {
                    font-size: 20px;
                    font-weight: 600;
                    margin-bottom: 8px;
                    color: #ECF0F1;
                }

                /* Sol Sütun Bölümleri */
                .contact-section,
                .personal-section,
                .languages-section,
                .skills-section,
                .hobbies-section {
                    margin-bottom: 8px;
                }

                .left-column p {
                    margin-bottom: 5px;
                    font-size: 10px;
                    line-height: 1.2;
                }

                .left-column strong {
                    color: #3498DB;
                    font-size: 11px;
                    display: block;
                    margin-bottom: 2px;
                }

                .left-column a {
                    color: #3498DB;
                    text-decoration: none;
                    word-break: break-all;
                }

                /* Dil Yetenekleri */
                .language-item {
                    margin-bottom: 15px;
                }

                .language-skills {
                    display: flex;
                    flex-direction: column;
                    gap: 3px;
                    margin-top: 5px;
                }

                .language-skills span {
                    font-size: 10px;
                    color: #BDC3C7;
                }

                /* Yetenekler */
                .skill-item {
                    margin-bottom: 12px;
                }

                .skill-header {
                    display: flex;
                    justify-content: space-between;
                    align-items: center;
                    margin-bottom: 5px;
                }

                .skill-name {
                    font-size: 12px;
                    font-weight: 500;
                }

                .skill-level {
                    font-size: 10px;
                    color: #BDC3C7;
                }

                .skill-bar {
                    height: 6px;
                    background: rgba(255,255,255,0.2);
                    border-radius: 3px;
                    overflow: hidden;
                }

                .skill-progress {
                    height: 100%;
                    background: #3498DB;
                    border-radius: 3px;
                    transition: width 0.3s ease;
                }

                /* Hobiler */
                .hobby-list {
                    list-style: none;
                    padding-left: 0;
                }

                .hobby-list li {
                    font-size: 12px;
                    margin-bottom: 5px;
                    color: #BDC3C7;
                    position: relative;
                    padding-left: 15px;
                }

                .hobby-list li:before {
                    content: '•';
                    color: #3498DB;
                    position: absolute;
                    left: 0;
                }

                /* SAĞ SÜTUN - 70% */
                .right-column {
                    width: 70%;
                    padding: 10px 15px;
                    background: white;
                }

                .right-column h2 {
                    color: #2C3E50;
                    font-size: 14px;
                    margin-bottom: 8px;
                    padding-bottom: 4px;
                    border-bottom: 2px solid #3498DB;
                    display: flex;
                    align-items: center;
                    gap: 6px;
                }

                .right-column .icon {
                    font-size: 16px;
                }

                /* Sağ Sütun Bölümleri */
                .summary-section,
                .experience-section,
                .education-section,
                .certificates-section,
                .projects-section,
                .references-section {
                    margin-bottom: 8px;
                }

                /* Hakkımda */
                .summary-section p {
                    font-size: 11px;
                    line-height: 1.3;
                    color: #34495E;
                    text-align: justify;
                }

                /* İş Deneyimi, Eğitim, vs. */
                .experience-item,
                .education-item,
                .certificate-item,
                .project-item,
                .reference-item {
                    margin-bottom: 5px;
                    padding-bottom: 4px;
                    border-bottom: 1px solid #ECF0F1;
                    position: relative;
                }

                .experience-item:last-child,
                .education-item:last-child,
                .certificate-item:last-child,
                .project-item:last-child,
                .reference-item:last-child {
                    border-bottom: none;
                    margin-bottom: 8px;
                }

                .experience-item h3,
                .education-item h3,
                .project-item h3 {
                    color: #2C3E50;
                    font-size: 14px;
                    font-weight: 600;
                    margin-top: 0;
                    margin-bottom: 0;
                    line-height: 1.1;
                }

                .experience-item h4,
                .education-item h4,
                .project-item h4 {
                    color: #3498DB;
                    font-size: 12px;
                    font-weight: 500;
                    margin-top: 0;
                    margin-bottom: 0;
                    line-height: 1.1;
                }

                .date-range {
                    color: #7F8C8D;
                    font-size: 11px;
                    font-style: italic;
                    margin-bottom: 1px;
                    margin-top: 0;
                    font-weight: 500;
                    line-height: 1.1;
                }

                .description {
                    color: #34495E;
                    font-size: 11px;
                    line-height: 1.3;
                    text-align: justify;
                    margin-top: 2px;
                }

                .description-list {
                    color: #34495E;
                    font-size: 11px;
                    line-height: 1.3;
                    margin-top: 2px;
                    margin-bottom: 0;
                    padding-left: 15px;
                }

                .description-list li {
                    margin-bottom: 2px;
                    list-style-type: disc;
                    list-style-position: outside;
                }

                .degree,
                .validity,
                .cert-no,
                .gpa,
                .technologies,
                .project-url,
                .company {
                    font-size: 10px;
                    color: #7F8C8D;
                    margin-top: 0;
                    margin-bottom: 0;
                    line-height: 1.1;
                }

                .degree-text {
                    color: #7F8C8D;
                    font-weight: normal;
                    font-size: inherit;
                }

                .project-url a,
                .right-column a {
                    color: #3498DB;
                    text-decoration: none;
                }

                .project-url a:hover,
                .right-column a:hover {
                    text-decoration: underline;
                }

                /* Referans İletişim Bilgileri */
                .reference-contact {
                    font-size: 9px;
                    color: #7F8C8D;
                    margin-top: 1px;
                    margin-bottom: 1px;
                    line-height: 1.1;
                }

                /* Grid Düzenleri */
                .certificates-grid {
                    display: grid;
                    grid-template-columns: repeat(3, 1fr);
                    gap: 10px;
                    margin-top: 8px;
                }

                .references-grid {
                    display: grid;
                    grid-template-columns: repeat(3, 1fr);
                    gap: 10px;
                    margin-top: 8px;
                }

                /* Grid İçindeki Öğeler */
                .certificates-grid .certificate-item,
                .references-grid .reference-item {
                    background: #FAFAFA;
                    padding: 8px;
                    border: 1px solid #E8E8E8;
                    border-radius: 4px;
                    margin-bottom: 0;
                    border-bottom: none;
                }

                /* Sertifikalar İçin Özel Düzenleme */
                .certificate-item h3 {
                    color: #2C3E50;
                    font-size: 12px;
                    font-weight: 600;
                    margin-bottom: 1px;
                    line-height: 1.2;
                }

                .certificate-item h4 {
                    color: #3498DB;
                    font-size: 11px;
                    font-weight: 500;
                    margin-bottom: 1px;
                    line-height: 1.2;
                }

                /* Referanslar İçin Özel Düzenleme */
                .reference-item h3 {
                    color: #2C3E50;
                    font-size: 12px;
                    font-weight: 600;
                    margin-bottom: 1px;
                    line-height: 1.2;
                }

                .reference-item h4 {
                    color: #3498DB;
                    font-size: 11px;
                    font-weight: 500;
                    margin-bottom: 1px;
                    line-height: 1.2;
                }

                .reference-item .position {
                    font-size: 10px;
                    color: #7F8C8D;
                    margin-bottom: 1px;
                    margin-top: 1px;
                }

                .reference-item .relation {
                    font-size: 9px;
                    color: #95A5A6;
                    margin-top: 2px;
                    margin-bottom: 1px;
                }

                /* Responsive Grid - Tablet için */
                @media (max-width: 1024px) {
                    .certificates-grid,
                    .references-grid {
                        grid-template-columns: repeat(2, 1fr);
                    }
                }

                /* Responsive Grid - Mobil için */
                @media (max-width: 768px) {
                    .certificates-grid,
                    .references-grid {
                        grid-template-columns: 1fr;
                    }
                }

                /* Yazdırma Desteği */
                @media print {
                    body {
                        background: white;
                        font-size: 12px;
                    }

                    .cv-container {
                        box-shadow: none;
                        max-width: none;
                    }

                    .left-column {
                        background: #2C3E50 !important;
                        -webkit-print-color-adjust: exact;
                        print-color-adjust: exact;
                    }

                    .profile-photo {
                        width: 100px;
                        height: 100px;
                    }

                    .right-column h2 {
                        font-size: 16px;
                    }

                    .left-column h3 {
                        font-size: 14px;
                    }
                }

                /* Mobil Uyumluluk */
                @media (max-width: 768px) {
                    .cv-container {
                        flex-direction: column;
                    }

                    .left-column,
                    .right-column {
                        width: 100%;
                    }

                    .left-column {
                        padding: 20px;
                    }

                    .right-column {
                        padding: 20px;
                    }

                    .profile-photo,
                    .profile-placeholder {
                        width: 100px;
                        height: 100px;
                    }

                    .profile-section h1 {
                        font-size: 20px;
                    }

                    .right-column h2 {
                        font-size: 18px;
                    }
                }
            ";
        }
    }
}