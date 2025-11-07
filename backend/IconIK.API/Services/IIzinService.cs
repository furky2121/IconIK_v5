using IconIK.API.Models;

namespace IconIK.API.Services
{
    public interface IIzinService
    {
        Task<int> CalculateYillikIzinHakki(int personelId);
        Task<int> CalculateKullanilmisIzin(int personelId, int yil);
        Task<int> CalculateKalanIzin(int personelId);
        Task<bool> CheckIzinCakismasi(int personelId, DateTime baslangic, DateTime bitis, int? excludeIzinId = null);
        Task<List<Personel>> GetOnaylamaYetkilisiOlanPersoneller(int talepEdenPersonelId);
        Task<bool> CanPersonelApproveIzin(int onaylayanPersonelId, int talepEdenPersonelId);
        int CalculateGunSayisi(DateTime baslangic, DateTime bitis);
        Task<Dictionary<string, object>> GetPersonelIzinOzeti(int personelId);
        Task<IzinValidationResult> ValidateIzinTalebi(string izinTipiAdi, int gunSayisi, int personelId, bool hasRapor = false);
    }

    public class IzinValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public IzinTipi? IzinTipi { get; set; }
    }
}