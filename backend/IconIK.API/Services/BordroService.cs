using IconIK.API.Data;
using IconIK.API.Models;
using Microsoft.EntityFrameworkCore;

namespace IconIK.API.Services
{
    public class BordroService : IBordroService
    {
        private readonly IconIKContext _context;

        public BordroService(IconIKContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tek personel için bordro hesaplama
        /// </summary>
        public async Task<BordroAna> HesaplaBordro(int personelId, int donemYil, int donemAy, int? puantajId = null)
        {
            // Personel bilgilerini getir
            var personel = await _context.Personeller
                .Include(p => p.Pozisyon)
                    .ThenInclude(pos => pos.Departman)
                .FirstOrDefaultAsync(p => p.Id == personelId && p.Aktif);

            if (personel == null)
                throw new Exception("Personel bulunamadı veya aktif değil.");

            if (personel.Maas == null || personel.Maas == 0)
                throw new Exception("Personel maaş bilgisi tanımlı değil.");

            // Kümülatif gelir hesapla (yıl başından önceki aya kadar)
            var kumulatifGelir = await HesaplaKumulatifGelir(personelId, donemYil, donemAy - 1);

            // Brüt -> Net hesaplama
            var hesapDetay = await HesaplaBrutNet(
                personel.Maas.Value,
                personel.MedeniHal ?? "Bekar",
                0, // Çocuk sayısı - personel tablosuna eklenebilir
                false, // Engelli durumu - personel tablosuna eklenebilir
                donemYil,
                donemAy,
                kumulatifGelir
            );

            // Bordro numarası oluştur
            var bordroNo = $"BR-{donemYil}{donemAy:D2}-{personelId:D5}";

            // Bordro kaydı oluştur
            var bordro = new BordroAna
            {
                PersonelId = personelId,
                PuantajId = puantajId,
                DonemYil = donemYil,
                DonemAy = donemAy,
                BordroNo = bordroNo,
                PozisyonAdi = personel.Pozisyon.Ad,
                DepartmanAdi = personel.Pozisyon.Departman.Ad,
                MedeniDurum = personel.MedeniHal ?? "Bekar",
                CocukSayisi = 0,
                EngelliDurumu = false,
                BrutMaas = hesapDetay.BrutMaas,
                ToplamOdeme = hesapDetay.BrutMaas,
                ToplamKesinti = hesapDetay.ToplamKesinti,
                NetUcret = hesapDetay.NetMaas,
                SgkMatrahi = hesapDetay.SgkMatrahi,
                SgkIsciPayi = hesapDetay.SgkIsciPayi,
                SgkIsverenPayi = hesapDetay.SgkIsverenPayi,
                IssizlikIsciPayi = hesapDetay.IssizlikIsciPayi,
                IssizlikIsverenPayi = hesapDetay.IssizlikIsverenPayi,
                GelirVergisiMatrahi = hesapDetay.GelirVergisiMatrahi,
                GelirVergisi = hesapDetay.GelirVergisi,
                DamgaVergisi = hesapDetay.DamgaVergisi,
                AgiTutari = hesapDetay.AgiTutari,
                AgiOrani = hesapDetay.AgiOrani,
                KumulatifGelir = kumulatifGelir + hesapDetay.BrutMaas,
                KumulatifVergi = kumulatifGelir > 0 ? await HesaplaKumulatifVergi(personelId, donemYil, donemAy) : hesapDetay.GelirVergisi,
                IsverenMaliyeti = hesapDetay.IsverenMaliyeti,
                OdemeSekli = "Banka",
                OdemeDurumu = "Beklemede",
                BordroDurumu = "Taslak",
                OnayDurumu = "Beklemede",
                Aktif = true,
                HesaplamaTarihi = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.BordroAna.Add(bordro);
            await _context.SaveChangesAsync();

            // Ödeme kalemleri ekle
            await EkleOdemeKalemleri(bordro.Id, hesapDetay);

            // Kesinti kalemleri ekle
            await EkleKesintiKalemleri(bordro.Id, hesapDetay);

            return bordro;
        }

        /// <summary>
        /// Toplu bordro hesaplama
        /// </summary>
        public async Task<List<BordroAna>> HesaplaTopluBordro(int donemYil, int donemAy, int? departmanId = null, int? pozisyonId = null)
        {
            var query = _context.Personeller
                .Include(p => p.Pozisyon)
                    .ThenInclude(pos => pos.Departman)
                .Where(p => p.Aktif && p.Maas != null && p.Maas > 0);

            if (departmanId.HasValue)
                query = query.Where(p => p.Pozisyon.DepartmanId == departmanId.Value);

            if (pozisyonId.HasValue)
                query = query.Where(p => p.PozisyonId == pozisyonId.Value);

            var personeller = await query.ToListAsync();

            var bordrolar = new List<BordroAna>();

            foreach (var personel in personeller)
            {
                try
                {
                    // Aynı dönem için bordro var mı kontrol et
                    var mevcutBordro = await _context.BordroAna
                        .FirstOrDefaultAsync(b => b.PersonelId == personel.Id &&
                                                  b.DonemYil == donemYil &&
                                                  b.DonemAy == donemAy);

                    if (mevcutBordro == null)
                    {
                        var bordro = await HesaplaBordro(personel.Id, donemYil, donemAy);
                        bordrolar.Add(bordro);
                    }
                }
                catch (Exception)
                {
                    // Hata olan personeli atla, diğerlerine devam et
                    continue;
                }
            }

            return bordrolar;
        }

        /// <summary>
        /// Brüt maaştan net maaş hesaplama (Türkiye mevzuatı - 2025)
        /// </summary>
        public async Task<BordroHesaplamaDetay> HesaplaBrutNet(
            decimal brutMaas,
            string medeniDurum,
            int cocukSayisi,
            bool engelliDurumu,
            int donemYil,
            int donemAy,
            decimal? kumulatifGelir = null)
        {
            var parametreler = await GetBordroParametreleri(donemYil, donemAy);
            var detay = new BordroHesaplamaDetay { BrutMaas = brutMaas };

            // 1. SGK Matrahı Hesaplama (Tavan ve Taban kontrolü)
            detay.SgkMatrahi = brutMaas;
            if (detay.SgkMatrahi > parametreler.SgkTavanBrut)
                detay.SgkMatrahi = parametreler.SgkTavanBrut;
            if (detay.SgkMatrahi < parametreler.SgkTabanBrut)
                detay.SgkMatrahi = parametreler.SgkTabanBrut;

            detay.HesaplamaAdimlari["1_SgkMatrahi"] = detay.SgkMatrahi;

            // 2. SGK İşçi Payı (%14)
            detay.SgkIsciPayi = Math.Round(detay.SgkMatrahi * parametreler.SgkIsciOrani / 100, 2);
            detay.HesaplamaAdimlari["2_SgkIsciPayi"] = detay.SgkIsciPayi;

            // 3. SGK İşveren Payı (%20.5)
            detay.SgkIsverenPayi = Math.Round(detay.SgkMatrahi * parametreler.SgkIsverenOrani / 100, 2);
            detay.HesaplamaAdimlari["3_SgkIsverenPayi"] = detay.SgkIsverenPayi;

            // 4. İşsizlik Sigortası İşçi Payı (%1)
            detay.IssizlikIsciPayi = Math.Round(detay.SgkMatrahi * parametreler.IssizlikIsciOrani / 100, 2);
            detay.HesaplamaAdimlari["4_IssizlikIsciPayi"] = detay.IssizlikIsciPayi;

            // 5. İşsizlik Sigortası İşveren Payı (%2)
            detay.IssizlikIsverenPayi = Math.Round(detay.SgkMatrahi * parametreler.IssizlikIsverenOrani / 100, 2);
            detay.HesaplamaAdimlari["5_IssizlikIsverenPayi"] = detay.IssizlikIsverenPayi;

            // 6. Gelir Vergisi Matrahı = Brüt Maaş - SGK İşçi - İşsizlik İşçi
            detay.GelirVergisiMatrahi = brutMaas - detay.SgkIsciPayi - detay.IssizlikIsciPayi;
            detay.HesaplamaAdimlari["6_GelirVergisiMatrahi"] = detay.GelirVergisiMatrahi;

            // 7. Gelir Vergisi Hesaplama (Dilimli sistem + Kümülatif)
            var toplamKumulatif = (kumulatifGelir ?? 0) + brutMaas;
            detay.GelirVergisi = await HesaplaGelirVergisi(detay.GelirVergisiMatrahi, toplamKumulatif, donemYil, donemAy);
            detay.HesaplamaAdimlari["7_GelirVergisi"] = detay.GelirVergisi;

            // 8. AGI (Asgari Geçim İndirimi) Hesaplama
            detay.AgiTutari = await HesaplaAGI(medeniDurum, cocukSayisi, engelliDurumu, donemYil, donemAy);

            // AGI gelir vergisinden düşülür
            if (detay.AgiTutari > detay.GelirVergisi)
                detay.AgiTutari = detay.GelirVergisi;

            detay.GelirVergisi = detay.GelirVergisi - detay.AgiTutari;
            detay.HesaplamaAdimlari["8_AgiTutari"] = detay.AgiTutari;
            detay.HesaplamaAdimlari["8b_GelirVergisiAgiSonrasi"] = detay.GelirVergisi;

            // AGI oranını hesapla
            if (medeniDurum == "Evli")
            {
                if (cocukSayisi >= 3) detay.AgiOrani = parametreler.AgiCocuk3Oran;
                else if (cocukSayisi == 2) detay.AgiOrani = parametreler.AgiCocuk2Oran;
                else if (cocukSayisi == 1) detay.AgiOrani = parametreler.AgiCocuk1Oran;
                else detay.AgiOrani = parametreler.AgiEvliOran;
            }
            else
            {
                detay.AgiOrani = parametreler.AgiBekarOran;
            }

            // 9. Damga Vergisi (%0.759)
            detay.DamgaVergisi = Math.Round(brutMaas * parametreler.DamgaVergisiOrani / 100, 2);
            detay.HesaplamaAdimlari["9_DamgaVergisi"] = detay.DamgaVergisi;

            // 10. Toplam Kesintiler
            detay.ToplamKesinti = detay.SgkIsciPayi + detay.IssizlikIsciPayi + detay.GelirVergisi + detay.DamgaVergisi;
            detay.HesaplamaAdimlari["10_ToplamKesinti"] = detay.ToplamKesinti;

            // 11. Net Maaş = Brüt Maaş - Toplam Kesintiler
            detay.NetMaas = brutMaas - detay.ToplamKesinti;
            detay.HesaplamaAdimlari["11_NetMaas"] = detay.NetMaas;

            // 12. İşveren Maliyeti = Brüt Maaş + SGK İşveren + İşsizlik İşveren
            detay.IsverenMaliyeti = brutMaas + detay.SgkIsverenPayi + detay.IssizlikIsverenPayi;
            detay.HesaplamaAdimlari["12_IsverenMaliyeti"] = detay.IsverenMaliyeti;

            return detay;
        }

        /// <summary>
        /// AGI (Asgari Geçim İndirimi) Hesaplama
        /// </summary>
        public async Task<decimal> HesaplaAGI(string medeniDurum, int cocukSayisi, bool engelliDurumu, int donemYil, int donemAy)
        {
            var parametreler = await GetBordroParametreleri(donemYil, donemAy);

            decimal agiOrani = 0;

            // Medeni durum ve çocuk sayısına göre AGI oranı
            if (medeniDurum == "Evli")
            {
                if (cocukSayisi >= 3)
                    agiOrani = parametreler.AgiCocuk3Oran; // %90
                else if (cocukSayisi == 2)
                    agiOrani = parametreler.AgiCocuk2Oran; // %80
                else if (cocukSayisi == 1)
                    agiOrani = parametreler.AgiCocuk1Oran; // %70
                else
                    agiOrani = parametreler.AgiEvliOran; // %60
            }
            else
            {
                agiOrani = parametreler.AgiBekarOran; // %50
            }

            // Engelli ise %10 ek indirim (toplam %100'ü geçmemek üzere)
            if (engelliDurumu && agiOrani < 100)
            {
                agiOrani = Math.Min(agiOrani + 10, 100);
            }

            // AGI tutarı = AGI base tutarı * Oran / 100
            var agiTutari = Math.Round(parametreler.AgiTutari * agiOrani / 100, 2);

            return agiTutari;
        }

        /// <summary>
        /// Gelir Vergisi Hesaplama (Dilimli Sistem - 2025)
        /// </summary>
        public async Task<decimal> HesaplaGelirVergisi(decimal vergiMatrahi, decimal kumulatifGelir, int donemYil, int donemAy)
        {
            var parametreler = await GetBordroParametreleri(donemYil, donemAy);
            decimal vergi = 0;
            decimal kalanMatrah = vergiMatrahi;

            // 2025 Gelir Vergisi Dilimleri
            // Dilim 1: 0 - 110,000 TL → %15
            // Dilim 2: 110,000 - 230,000 TL → %20
            // Dilim 3: 230,000 - 870,000 TL → %27
            // Dilim 4: 870,000 - 3,000,000 TL → %35
            // Dilim 5: 3,000,000 TL üzeri → %40

            // Dilim 1: %15
            if (kalanMatrah > 0)
            {
                var dilim1Tutar = Math.Min(kalanMatrah, parametreler.VergiDilim1UstSinir);
                vergi += dilim1Tutar * parametreler.VergiDilim1Oran / 100;
                kalanMatrah -= dilim1Tutar;
            }

            // Dilim 2: %20
            if (kalanMatrah > 0)
            {
                var dilim2Tutar = Math.Min(kalanMatrah, parametreler.VergiDilim2UstSinir - parametreler.VergiDilim1UstSinir);
                vergi += dilim2Tutar * parametreler.VergiDilim2Oran / 100;
                kalanMatrah -= dilim2Tutar;
            }

            // Dilim 3: %27
            if (kalanMatrah > 0)
            {
                var dilim3Tutar = Math.Min(kalanMatrah, parametreler.VergiDilim3UstSinir - parametreler.VergiDilim2UstSinir);
                vergi += dilim3Tutar * parametreler.VergiDilim3Oran / 100;
                kalanMatrah -= dilim3Tutar;
            }

            // Dilim 4: %35
            if (kalanMatrah > 0)
            {
                var dilim4Tutar = Math.Min(kalanMatrah, parametreler.VergiDilim4UstSinir - parametreler.VergiDilim3UstSinir);
                vergi += dilim4Tutar * parametreler.VergiDilim4Oran / 100;
                kalanMatrah -= dilim4Tutar;
            }

            // Dilim 5: %40 (3M üzeri)
            if (kalanMatrah > 0)
            {
                vergi += kalanMatrah * parametreler.VergiDilim5Oran / 100;
            }

            return Math.Round(vergi, 2);
        }

        /// <summary>
        /// Kümülatif gelir hesaplama
        /// </summary>
        public async Task<decimal> HesaplaKumulatifGelir(int personelId, int yil, int ay)
        {
            if (ay < 1) return 0;

            var kumulatif = await _context.BordroAna
                .Where(b => b.PersonelId == personelId &&
                           b.DonemYil == yil &&
                           b.DonemAy <= ay &&
                           b.BordroDurumu != "Iptal")
                .SumAsync(b => b.BrutMaas);

            return kumulatif;
        }

        /// <summary>
        /// Kümülatif vergi hesaplama
        /// </summary>
        private async Task<decimal> HesaplaKumulatifVergi(int personelId, int yil, int ay)
        {
            var kumulatif = await _context.BordroAna
                .Where(b => b.PersonelId == personelId &&
                           b.DonemYil == yil &&
                           b.DonemAy <= ay &&
                           b.BordroDurumu != "Iptal")
                .SumAsync(b => b.GelirVergisi);

            return kumulatif;
        }

        /// <summary>
        /// Bordro parametrelerini getir (yıl/ay için, yoksa en güncel)
        /// </summary>
        private async Task<BordroParametreleri> GetBordroParametreleri(int yil, int ay)
        {
            var parametreler = await _context.BordroParametreleri
                .FirstOrDefaultAsync(p => p.Yil == yil && p.Donem == ay && p.Aktif);

            if (parametreler == null)
            {
                // Belirtilen ay için parametre yoksa, en güncel parametreleri kullan
                parametreler = await _context.BordroParametreleri
                    .Where(p => p.Aktif)
                    .OrderByDescending(p => p.Yil)
                    .ThenByDescending(p => p.Donem)
                    .FirstOrDefaultAsync();
            }

            if (parametreler == null)
            {
                // Hiç parametre yoksa default 2025 parametreleri oluştur
                parametreler = await OlusturVarsayilanParametreler(yil, ay);
            }

            return parametreler;
        }

        /// <summary>
        /// Varsayılan bordro parametreleri oluştur (2025)
        /// </summary>
        private async Task<BordroParametreleri> OlusturVarsayilanParametreler(int yil, int ay)
        {
            var parametreler = new BordroParametreleri
            {
                Yil = yil,
                Donem = ay,
                AsgariUcretBrut = 20002.50m,
                AsgariUcretNet = 17002.12m,
                AgiOrani = 15.00m,
                AgiTutari = 2140.20m,
                SgkIsciOrani = 14.00m,
                SgkIsverenOrani = 20.50m,
                SgkTavanBrut = 147073.50m,
                SgkTabanBrut = 20002.50m,
                IssizlikIsciOrani = 1.00m,
                IssizlikIsverenOrani = 2.00m,
                DamgaVergisiOrani = 0.759m,
                VergiDilim1UstSinir = 110000m,
                VergiDilim1Oran = 15.00m,
                VergiDilim2UstSinir = 230000m,
                VergiDilim2Oran = 20.00m,
                VergiDilim3UstSinir = 870000m,
                VergiDilim3Oran = 27.00m,
                VergiDilim4UstSinir = 3000000m,
                VergiDilim4Oran = 35.00m,
                VergiDilim5Oran = 40.00m,
                AgiBekarOran = 50.00m,
                AgiEvliOran = 60.00m,
                AgiCocuk1Oran = 70.00m,
                AgiCocuk2Oran = 80.00m,
                AgiCocuk3Oran = 90.00m,
                NormalMesaiCarpan = 1.00m,
                HaftaIciMesaiCarpan = 1.50m,
                HaftaSonuMesaiCarpan = 2.00m,
                GeceMesaiCarpan = 1.25m,
                ResmiTatilCarpan = 2.00m,
                YillikIzin1_5Yil = 14,
                YillikIzin5_15Yil = 20,
                YillikIzin15YilUstu = 26,
                KidemTavan = 52734.72m,
                Aktif = true,
                Aciklama = "Otomatik oluşturulmuş varsayılan parametreler (2025)",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.BordroParametreleri.Add(parametreler);
            await _context.SaveChangesAsync();

            return parametreler;
        }

        /// <summary>
        /// Ödeme kalemlerini ekle
        /// </summary>
        private async Task EkleOdemeKalemleri(int bordroId, BordroHesaplamaDetay detay)
        {
            // Temel maaş ödeme kalemi
            var odemeTanimi = await _context.OdemeTanimlari.FirstOrDefaultAsync(o => o.Kod == "MAAS");
            if (odemeTanimi == null)
            {
                // Ödeme tanımı yoksa oluştur
                odemeTanimi = await OlusturTemelOdemeTanimlari();
            }

            var odeme = new BordroOdeme
            {
                BordroId = bordroId,
                OdemeTanimiId = odemeTanimi.Id,
                OdemeKodu = "MAAS",
                OdemeAdi = "Aylık Maaş",
                OdemeTuru = "Sabit",
                Tutar = detay.BrutMaas,
                SgkMatrahinaDahil = true,
                VergiMatrahinaDahil = true,
                AgiUygulanir = true,
                DamgaVergisiDahil = true,
                SiraNo = 1,
                CreatedAt = DateTime.UtcNow
            };

            _context.BordroOdemeler.Add(odeme);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Kesinti kalemlerini ekle
        /// </summary>
        private async Task EkleKesintiKalemleri(int bordroId, BordroHesaplamaDetay detay)
        {
            var kesintiler = new List<BordroKesinti>();
            int siraNo = 1;

            // SGK İşçi Payı
            var sgkTanim = await _context.KesintiTanimlari.FirstOrDefaultAsync(k => k.Kod == "SGK");
            if (sgkTanim == null)
                sgkTanim = await OlusturTemelKesintiTanimlari();

            kesintiler.Add(new BordroKesinti
            {
                BordroId = bordroId,
                KesintiTanimiId = sgkTanim.Id,
                KesintiKodu = "SGK",
                KesintiAdi = "SGK İşçi Payı",
                KesintiTuru = "Yasal",
                Tutar = detay.SgkIsciPayi,
                Matrah = detay.SgkMatrahi,
                OtomatikHesaplama = true,
                SiraNo = siraNo++,
                CreatedAt = DateTime.UtcNow
            });

            // İşsizlik Sigortası İşçi Payı
            var issizlikTanim = await _context.KesintiTanimlari.FirstOrDefaultAsync(k => k.Kod == "ISSIZLIK");
            kesintiler.Add(new BordroKesinti
            {
                BordroId = bordroId,
                KesintiTanimiId = issizlikTanim?.Id ?? sgkTanim.Id,
                KesintiKodu = "ISSIZLIK",
                KesintiAdi = "İşsizlik Sigortası",
                KesintiTuru = "Yasal",
                Tutar = detay.IssizlikIsciPayi,
                Matrah = detay.SgkMatrahi,
                OtomatikHesaplama = true,
                SiraNo = siraNo++,
                CreatedAt = DateTime.UtcNow
            });

            // Gelir Vergisi
            var vergiTanim = await _context.KesintiTanimlari.FirstOrDefaultAsync(k => k.Kod == "VERGI");
            kesintiler.Add(new BordroKesinti
            {
                BordroId = bordroId,
                KesintiTanimiId = vergiTanim?.Id ?? sgkTanim.Id,
                KesintiKodu = "VERGI",
                KesintiAdi = "Gelir Vergisi",
                KesintiTuru = "Yasal",
                Tutar = detay.GelirVergisi,
                Matrah = detay.GelirVergisiMatrahi,
                OtomatikHesaplama = true,
                SiraNo = siraNo++,
                CreatedAt = DateTime.UtcNow
            });

            // Damga Vergisi
            var damgaTanim = await _context.KesintiTanimlari.FirstOrDefaultAsync(k => k.Kod == "DAMGA");
            kesintiler.Add(new BordroKesinti
            {
                BordroId = bordroId,
                KesintiTanimiId = damgaTanim?.Id ?? sgkTanim.Id,
                KesintiKodu = "DAMGA",
                KesintiAdi = "Damga Vergisi",
                KesintiTuru = "Yasal",
                Tutar = detay.DamgaVergisi,
                OtomatikHesaplama = true,
                SiraNo = siraNo++,
                CreatedAt = DateTime.UtcNow
            });

            _context.BordroKesintiler.AddRange(kesintiler);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Temel ödeme tanımlarını oluştur
        /// </summary>
        private async Task<OdemeTanimi> OlusturTemelOdemeTanimlari()
        {
            var odemeler = new List<OdemeTanimi>
            {
                new OdemeTanimi
                {
                    Kod = "MAAS",
                    Ad = "Aylık Maaş",
                    OdemeTuru = "Sabit",
                    SgkMatrahinaDahil = true,
                    VergiMatrahinaDahil = true,
                    AgiUygulanir = true,
                    DamgaVergisiDahil = true,
                    SiraNo = 1,
                    Aktif = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new OdemeTanimi
                {
                    Kod = "YEMEK",
                    Ad = "Yemek Yardımı",
                    OdemeTuru = "Sabit",
                    SgkMatrahinaDahil = false,
                    VergiMatrahinaDahil = false,
                    AgiUygulanir = false,
                    DamgaVergisiDahil = false,
                    SiraNo = 2,
                    Aktif = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new OdemeTanimi
                {
                    Kod = "YOL",
                    Ad = "Yol Yardımı",
                    OdemeTuru = "Sabit",
                    SgkMatrahinaDahil = false,
                    VergiMatrahinaDahil = false,
                    AgiUygulanir = false,
                    DamgaVergisiDahil = false,
                    SiraNo = 3,
                    Aktif = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            _context.OdemeTanimlari.AddRange(odemeler);
            await _context.SaveChangesAsync();

            return odemeler[0]; // MAAS
        }

        /// <summary>
        /// Temel kesinti tanımlarını oluştur
        /// </summary>
        private async Task<KesintiTanimi> OlusturTemelKesintiTanimlari()
        {
            var kesintiler = new List<KesintiTanimi>
            {
                new KesintiTanimi
                {
                    Kod = "SGK",
                    Ad = "SGK İşçi Payı",
                    KesintiTuru = "Yasal",
                    OtomatikHesaplama = true,
                    Oran = 14.00m,
                    SiraNo = 1,
                    Aktif = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new KesintiTanimi
                {
                    Kod = "ISSIZLIK",
                    Ad = "İşsizlik Sigortası",
                    KesintiTuru = "Yasal",
                    OtomatikHesaplama = true,
                    Oran = 1.00m,
                    SiraNo = 2,
                    Aktif = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new KesintiTanimi
                {
                    Kod = "VERGI",
                    Ad = "Gelir Vergisi",
                    KesintiTuru = "Yasal",
                    OtomatikHesaplama = true,
                    SiraNo = 3,
                    Aktif = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new KesintiTanimi
                {
                    Kod = "DAMGA",
                    Ad = "Damga Vergisi",
                    KesintiTuru = "Yasal",
                    OtomatikHesaplama = true,
                    Oran = 0.759m,
                    SiraNo = 4,
                    Aktif = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            _context.KesintiTanimlari.AddRange(kesintiler);
            await _context.SaveChangesAsync();

            return kesintiler[0]; // SGK
        }

        /// <summary>
        /// Bordro onaylama
        /// </summary>
        public async Task<bool> OnayBordro(int bordroId, int onaylayanPersonelId, string onayNotu = "")
        {
            var bordro = await _context.BordroAna.FindAsync(bordroId);
            if (bordro == null)
                return false;

            if (bordro.BordroDurumu == "Iptal" || bordro.OnayDurumu == "Onaylandi")
                return false;

            bordro.OnayDurumu = "Onaylandi";
            bordro.BordroDurumu = "Onaylandi";
            bordro.UpdatedAt = DateTime.UtcNow;

            var onayKaydi = new BordroOnay
            {
                BordroId = bordroId,
                OnaySeviyesi = 1,
                OnaySeviyeAdi = "Bordro Onayı",
                OnaylayanId = onaylayanPersonelId,
                OnayDurumu = "Onaylandi",
                OnayTarihi = DateTime.UtcNow,
                OnayNotu = onayNotu,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.BordroOnaylar.Add(onayKaydi);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Bordro reddetme
        /// </summary>
        public async Task<bool> RedBordro(int bordroId, int onaylayanPersonelId, string redNedeni)
        {
            var bordro = await _context.BordroAna.FindAsync(bordroId);
            if (bordro == null)
                return false;

            if (bordro.BordroDurumu == "Iptal")
                return false;

            bordro.OnayDurumu = "Reddedildi";
            bordro.BordroDurumu = "Taslak";
            bordro.UpdatedAt = DateTime.UtcNow;

            var onayKaydi = new BordroOnay
            {
                BordroId = bordroId,
                OnaySeviyesi = 1,
                OnaySeviyeAdi = "Bordro Onayı",
                OnaylayanId = onaylayanPersonelId,
                OnayDurumu = "Reddedildi",
                OnayTarihi = DateTime.UtcNow,
                RedNedeni = redNedeni,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.BordroOnaylar.Add(onayKaydi);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Bordro silme
        /// </summary>
        public async Task<bool> SilBordro(int bordroId)
        {
            var bordro = await _context.BordroAna
                .Include(b => b.BordroOdemeler)
                .Include(b => b.BordroKesintiler)
                .Include(b => b.BordroOnaylar)
                .FirstOrDefaultAsync(b => b.Id == bordroId);

            if (bordro == null)
                return false;

            // Sadece Taslak veya Reddedildi durumundaki bordrolar silinebilir
            if (bordro.BordroDurumu != "Taslak" && bordro.OnayDurumu != "Reddedildi")
                return false;

            _context.BordroOdemeler.RemoveRange(bordro.BordroOdemeler);
            _context.BordroKesintiler.RemoveRange(bordro.BordroKesintiler);
            _context.BordroOnaylar.RemoveRange(bordro.BordroOnaylar);
            _context.BordroAna.Remove(bordro);

            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Bordro detaylarını getir
        /// </summary>
        public async Task<BordroAna?> GetBordroDetay(int bordroId)
        {
            return await _context.BordroAna
                .Include(b => b.Personel)
                .Include(b => b.Puantaj)
                .Include(b => b.BordroOdemeler).ThenInclude(o => o.OdemeTanimi)
                .Include(b => b.BordroKesintiler).ThenInclude(k => k.KesintiTanimi)
                .Include(b => b.BordroOnaylar).ThenInclude(o => o.Onaylayan)
                .FirstOrDefaultAsync(b => b.Id == bordroId);
        }

        /// <summary>
        /// Personelin tüm bordrolarını getir
        /// </summary>
        public async Task<List<BordroAna>> GetPersonelBordrolar(int personelId, int? yil = null)
        {
            var query = _context.BordroAna
                .Include(b => b.Personel)
                .Where(b => b.PersonelId == personelId);

            if (yil.HasValue)
                query = query.Where(b => b.DonemYil == yil.Value);

            return await query
                .OrderByDescending(b => b.DonemYil)
                .ThenByDescending(b => b.DonemAy)
                .ToListAsync();
        }
    }
}
