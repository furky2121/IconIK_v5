using IconIK.API.Models;

namespace IconIK.API.Services
{
    public interface IBildirimService
    {
        Task<Bildirim> CreateBildirimAsync(Bildirim bildirim);
        Task<List<Bildirim>> GetBildirimlerByPersonelAsync(int personelId);
        Task<List<Bildirim>> GetOkunmamisBildirimlerAsync(int personelId);
        Task<int> GetOkunmamisSayisiAsync(int personelId);
        Task<bool> MarkAsReadAsync(int bildirimId);
        Task<bool> MarkAllAsReadAsync(int personelId);
        Task<bool> DeleteBildirimAsync(int bildirimId);
    }
}
