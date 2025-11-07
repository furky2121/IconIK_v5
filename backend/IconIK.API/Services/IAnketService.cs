using IconIK.API.Models;

namespace IconIK.API.Services
{
    public interface IAnketService
    {
        // Anket CRUD
        Task<List<Anket>> GetAllAnketlerAsync();
        Task<List<Anket>> GetAktifAnketlerAsync();
        Task<Anket?> GetAnketByIdAsync(int id);
        Task<Anket> CreateAnketAsync(Anket anket);
        Task<Anket> UpdateAnketAsync(Anket anket);
        Task<bool> DeleteAnketAsync(int id);

        // Anket Atama
        Task<List<AnketAtama>> GetAnketAtamalariAsync(int anketId);
        Task<AnketAtama> CreateAtamaAsync(AnketAtama atama);
        Task<bool> DeleteAtamaAsync(int atamaId);
        Task<List<Anket>> GetBanaAtananAnketlerAsync(int personelId);

        // Anket Cevaplama
        Task<AnketKatilim?> GetKatilimAsync(int anketId, int personelId);
        Task<AnketKatilim> BaslatAsync(int anketId, int personelId);
        Task<List<AnketCevap>> CevapKaydetAsync(int anketId, int personelId, List<AnketCevap> cevaplar);
        Task<bool> TamamlaAsync(int anketId, int personelId);

        // Anket Sonuçları
        Task<object> GetAnketSonuclariAsync(int anketId);
        Task<object> GetAnketCevaplariAsync(int anketId);
        Task<object> GetKatilimIstatistikleriAsync(int anketId);
    }
}
