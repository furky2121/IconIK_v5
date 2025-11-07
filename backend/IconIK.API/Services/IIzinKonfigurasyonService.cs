using IconIK.API.Models;

namespace IconIK.API.Services
{
    public interface IIzinKonfigurasyonService
    {
        Task<IEnumerable<IzinTipi>> GetAllIzinTipleri();
        Task<IEnumerable<IzinTipi>> GetAktifIzinTipleri();
        Task<IEnumerable<IzinTipi>> GetAktifIzinTipleriByGender(string? cinsiyet);
        Task<IzinTipi?> GetIzinTipiById(int id);
        Task<IzinTipi?> GetIzinTipiByName(string name);
        Task<IzinTipi> CreateIzinTipi(IzinTipi izinTipi);
        Task<IzinTipi> UpdateIzinTipi(IzinTipi izinTipi);
        Task<bool> DeleteIzinTipi(int id);
        Task<bool> IzinTipiExists(string name, int? excludeId = null);
    }
}
