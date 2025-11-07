using IconIK.API.Models;

namespace IconIK.API.Services
{
    public interface IBordroService
    {
        /// <summary>
        /// Tek personel için bordro hesaplama
        /// </summary>
        Task<BordroAna> HesaplaBordro(int personelId, int donemYil, int donemAy, int? puantajId = null);

        /// <summary>
        /// Toplu bordro hesaplama (tüm aktif personeller veya departman/pozisyon bazlı)
        /// </summary>
        Task<List<BordroAna>> HesaplaTopluBordro(int donemYil, int donemAy, int? departmanId = null, int? pozisyonId = null);

        /// <summary>
        /// Brüt maaştan net maaş hesaplama (Türkiye mevzuatı)
        /// </summary>
        Task<BordroHesaplamaDetay> HesaplaBrutNet(decimal brutMaas, string medeniDurum, int cocukSayisi, bool engelliDurumu, int donemYil, int donemAy, decimal? kumulatifGelir = null);

        /// <summary>
        /// AGI (Asgari Geçim İndirimi) hesaplama
        /// </summary>
        Task<decimal> HesaplaAGI(string medeniDurum, int cocukSayisi, bool engelliDurumu, int donemYil, int donemAy);

        /// <summary>
        /// Gelir vergisi hesaplama (dilimli vergi sistemi)
        /// </summary>
        Task<decimal> HesaplaGelirVergisi(decimal vergiMatrahi, decimal kumulatifGelir, int donemYil, int donemAy);

        /// <summary>
        /// Bordro onaylama
        /// </summary>
        Task<bool> OnayBordro(int bordroId, int onaylayanPersonelId, string onayNotu = "");

        /// <summary>
        /// Bordro reddetme
        /// </summary>
        Task<bool> RedBordro(int bordroId, int onaylayanPersonelId, string redNedeni);

        /// <summary>
        /// Bordro silme (taslak veya iptal durumundaysa)
        /// </summary>
        Task<bool> SilBordro(int bordroId);

        /// <summary>
        /// Bordro detayları getirme (ödemeler ve kesintilerle birlikte)
        /// </summary>
        Task<BordroAna?> GetBordroDetay(int bordroId);

        /// <summary>
        /// Personelin tüm bordro kayıtlarını getirme
        /// </summary>
        Task<List<BordroAna>> GetPersonelBordrolar(int personelId, int? yil = null);

        /// <summary>
        /// Kümülatif gelir hesaplama (yıl başından belirli bir aya kadar)
        /// </summary>
        Task<decimal> HesaplaKumulatifGelir(int personelId, int yil, int ay);
    }

    /// <summary>
    /// Bordro hesaplama detay modeli (ara hesaplamalar için)
    /// </summary>
    public class BordroHesaplamaDetay
    {
        public decimal BrutMaas { get; set; }
        public decimal SgkMatrahi { get; set; }
        public decimal SgkIsciPayi { get; set; }
        public decimal SgkIsverenPayi { get; set; }
        public decimal IssizlikIsciPayi { get; set; }
        public decimal IssizlikIsverenPayi { get; set; }
        public decimal GelirVergisiMatrahi { get; set; }
        public decimal GelirVergisi { get; set; }
        public decimal DamgaVergisi { get; set; }
        public decimal AgiTutari { get; set; }
        public decimal AgiOrani { get; set; }
        public decimal NetMaas { get; set; }
        public decimal IsverenMaliyeti { get; set; }
        public decimal ToplamKesinti { get; set; }
        public Dictionary<string, decimal> HesaplamaAdimlari { get; set; } = new();
    }
}
