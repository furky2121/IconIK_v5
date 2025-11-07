using Microsoft.EntityFrameworkCore;
using IconIK.API.Data;
using IconIK.API.Models;

namespace IconIK.API.Services
{
    public class BildirimService : IBildirimService
    {
        private readonly IconIKContext _context;

        public BildirimService(IconIKContext context)
        {
            _context = context;
        }

        public async Task<Bildirim> CreateBildirimAsync(Bildirim bildirim)
        {
            bildirim.OlusturulmaTarihi = DateTime.UtcNow;
            bildirim.Okundu = false;

            _context.Bildirimler.Add(bildirim);
            await _context.SaveChangesAsync();

            return bildirim;
        }

        public async Task<List<Bildirim>> GetBildirimlerByPersonelAsync(int personelId)
        {
            return await _context.Bildirimler
                .Where(b => b.AliciId == personelId)
                .OrderByDescending(b => b.OlusturulmaTarihi)
                .ToListAsync();
        }

        public async Task<List<Bildirim>> GetOkunmamisBildirimlerAsync(int personelId)
        {
            return await _context.Bildirimler
                .Where(b => b.AliciId == personelId && !b.Okundu)
                .OrderByDescending(b => b.OlusturulmaTarihi)
                .ToListAsync();
        }

        public async Task<int> GetOkunmamisSayisiAsync(int personelId)
        {
            return await _context.Bildirimler
                .Where(b => b.AliciId == personelId && !b.Okundu)
                .CountAsync();
        }

        public async Task<bool> MarkAsReadAsync(int bildirimId)
        {
            var bildirim = await _context.Bildirimler.FindAsync(bildirimId);
            if (bildirim == null) return false;

            bildirim.Okundu = true;
            bildirim.OkunmaTarihi = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(int personelId)
        {
            var okunmamisBildirimler = await _context.Bildirimler
                .Where(b => b.AliciId == personelId && !b.Okundu)
                .ToListAsync();

            foreach (var bildirim in okunmamisBildirimler)
            {
                bildirim.Okundu = true;
                bildirim.OkunmaTarihi = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteBildirimAsync(int bildirimId)
        {
            var bildirim = await _context.Bildirimler.FindAsync(bildirimId);
            if (bildirim == null) return false;

            _context.Bildirimler.Remove(bildirim);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
