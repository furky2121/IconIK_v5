using Microsoft.EntityFrameworkCore;
using IconIK.API.Data;
using IconIK.API.Models;

namespace IconIK.API.Services
{
    public interface IStatusService
    {
        Task AdayDurumDegistir(int adayId, AdayDurumu yeniDurum, string? not = null, int? degistirenPersonelId = null, bool otomatik = false, int? ilgiliBasvuruId = null, int? ilgiliMulakatId = null);
        Task BasvuruOlusturuldu(int adayId, int basvuruId);
        Task MulakatPlanlandi(int adayId, int mulakatId);
        Task MulakatTamamlandi(int adayId, int mulakatId, string sonuc);
        Task TeklifGonderildi(int adayId, int basvuruId);
        Task TeklifKabulEdildi(int adayId, int basvuruId);
        Task TeklifReddedildi(int adayId, int basvuruId);
        Task<List<AdayDurumGecmisi>> AdayDurumGecmisiniGetir(int adayId);
        Task KaraListeyeEkle(int adayId, int? degistirenPersonelId = null, string? not = null);
    }

    public class StatusService : IStatusService
    {
        private readonly IconIKContext _context;

        public StatusService(IconIKContext context)
        {
            _context = context;
        }

        public async Task AdayDurumDegistir(int adayId, AdayDurumu yeniDurum, string? not = null, int? degistirenPersonelId = null, bool otomatik = false, int? ilgiliBasvuruId = null, int? ilgiliMulakatId = null)
        {
            var aday = await _context.Adaylar.FindAsync(adayId);
            if (aday == null)
                throw new ArgumentException("Aday bulunamadı");

            var eskiDurum = aday.Durum;

            if (eskiDurum == yeniDurum)
                return;

            var durumGecmisi = new AdayDurumGecmisi
            {
                AdayId = adayId,
                EskiDurum = eskiDurum,
                YeniDurum = yeniDurum,
                DegisiklikTarihi = DateTime.UtcNow,
                DegisiklikNotu = not,
                DegistirenPersonelId = degistirenPersonelId,
                OtomatikDegisiklik = otomatik,
                IlgiliBasvuruId = ilgiliBasvuruId,
                IlgiliMulakatId = ilgiliMulakatId
            };

            _context.AdayDurumGecmisleri.Add(durumGecmisi);

            aday.Durum = yeniDurum;
            aday.DurumGuncellenmeTarihi = DateTime.UtcNow;
            aday.DurumGuncellemeNotu = not;

            await _context.SaveChangesAsync();
        }

        public async Task BasvuruOlusturuldu(int adayId, int basvuruId)
        {
            await AdayDurumDegistir(
                adayId,
                AdayDurumu.BasvuruYapildi,
                "Aday bir iş ilanına başvuru yaptı",
                null,
                true,
                basvuruId
            );

            var basvuru = await _context.Basvurular.FindAsync(basvuruId);
            if (basvuru != null)
            {
                basvuru.Durum = BasvuruDurumu.YeniBasvuru;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MulakatPlanlandi(int adayId, int mulakatId)
        {
            await AdayDurumDegistir(
                adayId,
                AdayDurumu.MulakatPlanlandi,
                "Aday için mülakat planlandı",
                null,
                true,
                null,
                mulakatId
            );

            var mulakat = await _context.Mulakatlar
                .Include(m => m.Basvuru)
                .FirstOrDefaultAsync(m => m.Id == mulakatId);

            if (mulakat?.Basvuru != null)
            {
                mulakat.Basvuru.Durum = BasvuruDurumu.MulakatBekleniyor;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MulakatTamamlandi(int adayId, int mulakatId, string sonuc)
        {
            var mulakatlar = await _context.Mulakatlar
                .Include(m => m.Basvuru)
                .Where(m => m.Basvuru.AdayId == adayId)
                .OrderByDescending(m => m.Tarih)
                .ToListAsync();

            var tamamlananMulakat = mulakatlar.FirstOrDefault(m => m.Id == mulakatId);
            if (tamamlananMulakat == null)
                return;

            var yeniDurum = sonuc == "Başarılı" ? AdayDurumu.MulakatTamamlandi : AdayDurumu.Reddedildi;
            var not = $"Mülakat tamamlandı - Sonuç: {sonuc}";

            await AdayDurumDegistir(
                adayId,
                yeniDurum,
                not,
                null,
                true,
                tamamlananMulakat.BasvuruId,
                mulakatId
            );

            if (tamamlananMulakat.Basvuru != null)
            {
                tamamlananMulakat.Basvuru.Durum = sonuc == "Başarılı"
                    ? BasvuruDurumu.MulakatTamamlandi
                    : BasvuruDurumu.Reddedildi;
                await _context.SaveChangesAsync();
            }
        }

        public async Task TeklifGonderildi(int adayId, int basvuruId)
        {
            await AdayDurumDegistir(
                adayId,
                AdayDurumu.TeklifGonderildi,
                "Adaya teklif mektubu gönderildi",
                null,
                true,
                basvuruId
            );

            var basvuru = await _context.Basvurular.FindAsync(basvuruId);
            if (basvuru != null)
            {
                basvuru.Durum = BasvuruDurumu.TeklifVerildi;
                await _context.SaveChangesAsync();
            }
        }

        public async Task TeklifKabulEdildi(int adayId, int basvuruId)
        {
            await AdayDurumDegistir(
                adayId,
                AdayDurumu.IseBasladi,
                "Teklif kabul edildi - İşe başladı",
                null,
                true,
                basvuruId
            );

            var basvuru = await _context.Basvurular.FindAsync(basvuruId);
            if (basvuru != null)
            {
                basvuru.Durum = BasvuruDurumu.IseAlindi;
                await _context.SaveChangesAsync();
            }
        }

        public async Task TeklifReddedildi(int adayId, int basvuruId)
        {
            await AdayDurumDegistir(
                adayId,
                AdayDurumu.AdayVazgecti,
                "Teklif reddedildi - Aday vazgeçti",
                null,
                true,
                basvuruId
            );

            var basvuru = await _context.Basvurular.FindAsync(basvuruId);
            if (basvuru != null)
            {
                basvuru.Durum = BasvuruDurumu.AdayVazgecti;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<AdayDurumGecmisi>> AdayDurumGecmisiniGetir(int adayId)
        {
            return await _context.AdayDurumGecmisleri
                .Include(adg => adg.DegistirenPersonel)
                .Include(adg => adg.IlgiliBasvuru)
                    .ThenInclude(b => b.Ilan)
                .Include(adg => adg.IlgiliMulakat)
                .Where(adg => adg.AdayId == adayId)
                .OrderByDescending(adg => adg.DegisiklikTarihi)
                .ToListAsync();
        }

        public async Task KaraListeyeEkle(int adayId, int? degistirenPersonelId = null, string? not = null)
        {
            await AdayDurumDegistir(
                adayId,
                AdayDurumu.KaraListe,
                not ?? "Aday kara listeye eklendi",
                degistirenPersonelId,
                false
            );

            var aday = await _context.Adaylar.FindAsync(adayId);
            if (aday != null)
            {
                aday.KaraListe = true;
                await _context.SaveChangesAsync();
            }
        }

        public static string GetAdayDurumText(AdayDurumu durum)
        {
            return durum switch
            {
                AdayDurumu.CVHavuzunda => "CV Havuzunda",
                AdayDurumu.BasvuruYapildi => "Başvuru Yapıldı",
                AdayDurumu.CVInceleniyor => "CV İnceleniyor",
                AdayDurumu.MulakatPlanlandi => "Mülakat Planlandı",
                AdayDurumu.MulakatTamamlandi => "Mülakat Tamamlandı",
                AdayDurumu.ReferansKontrolu => "Referans Kontrolü",
                AdayDurumu.TeklifHazirlaniyor => "Teklif Hazırlanıyor",
                AdayDurumu.TeklifGonderildi => "Teklif Gönderildi",
                AdayDurumu.TeklifOnayiBekleniyor => "Teklif Onayı Bekleniyor",
                AdayDurumu.IseBasladi => "İşe Başladı",
                AdayDurumu.Reddedildi => "Reddedildi",
                AdayDurumu.AdayVazgecti => "Aday Vazgeçti",
                AdayDurumu.KaraListe => "Kara Liste",
                _ => "Bilinmiyor"
            };
        }

        public static string GetBasvuruDurumText(BasvuruDurumu durum)
        {
            return durum switch
            {
                BasvuruDurumu.YeniBasvuru => "Yeni Başvuru",
                BasvuruDurumu.CVInceleniyor => "CV İnceleniyor",
                BasvuruDurumu.Degerlendiriliyor => "Değerlendiriliyor",
                BasvuruDurumu.MulakatBekleniyor => "Mülakat Bekliyor",
                BasvuruDurumu.MulakatTamamlandi => "Mülakat Tamamlandı",
                BasvuruDurumu.ReferansKontrolu => "Referans Kontrolü",
                BasvuruDurumu.TeklifVerildi => "Teklif Verildi",
                BasvuruDurumu.IseAlindi => "İşe Alındı",
                BasvuruDurumu.Reddedildi => "Reddedildi",
                BasvuruDurumu.AdayVazgecti => "Aday Vazgeçti",
                _ => "Bilinmiyor"
            };
        }
    }
}