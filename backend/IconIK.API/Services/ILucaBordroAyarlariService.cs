using IconIK.API.Models;

namespace IconIK.API.Services
{
    public interface ILucaBordroAyarlariService
    {
        Task<List<LucaBordroAyarlari>> GetAllAsync();
        Task<LucaBordroAyarlari?> GetByIdAsync(int id);
        Task<LucaBordroAyarlari?> GetAktifAyarAsync();
        Task<LucaBordroAyarlari> CreateAsync(LucaBordroAyarlari ayarlar);
        Task<LucaBordroAyarlari?> UpdateAsync(int id, LucaBordroAyarlari ayarlar);
        Task<bool> DeleteAsync(int id);
        Task<(bool success, string message)> TestBaglantiAsync(int id);
    }
}
