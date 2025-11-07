using IconIK.API.Models;

namespace IconIK.API.Services
{
    public interface ILucaBordroService
    {
        Task<List<LucaBordro>> GetBenimBordrolarimAsync(string tcKimlik);
        Task<LucaBordro?> GetByIdAsync(int id);
        Task<(bool success, string message, int count)> SenkronizeEtAsync();
        Task<(bool success, string message, int count)> DosyadanYukleAsync(IFormFile file);
        Task<(bool success, string message)> MaileGonderAsync(int lucaBordroId, int kullaniciId);
    }
}
