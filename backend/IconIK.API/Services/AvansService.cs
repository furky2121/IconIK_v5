using Microsoft.EntityFrameworkCore;
using IconIK.API.Data;
using IconIK.API.Models;

namespace IconIK.API.Services
{
    public interface IAvansService
    {
        Task<bool> CheckAvansLimit(int personelId, decimal talepTutari);
        Task<decimal> GetMaxAvansLimit(int personelId);
    }

    public class AvansService : IAvansService
    {
        private readonly IconIKContext _context;

        public AvansService(IconIKContext context)
        {
            _context = context;
        }

        public async Task<bool> CheckAvansLimit(int personelId, decimal talepTutari)
        {
            var personel = await _context.Personeller
                .Include(p => p.Pozisyon)
                .FirstOrDefaultAsync(p => p.Id == personelId);

            if (personel == null) return false;

            var maxLimit = await GetMaxAvansLimit(personelId);

            // Bekleyen ve onaylanan avansları hesaba kat (yeni talep dahil)
            var mevcutAvanslar = await _context.AvansTalepleri
                .Where(a => a.PersonelId == personelId
                    && (a.OnayDurumu == "Beklemede" || a.OnayDurumu == "Onaylandı")
                    && a.TalepTarihi.Month == DateTime.Now.Month
                    && a.TalepTarihi.Year == DateTime.Now.Year)
                .SumAsync(a => a.TalepTutari);

            return (mevcutAvanslar + talepTutari) <= maxLimit;
        }

        public async Task<decimal> GetMaxAvansLimit(int personelId)
        {
            var personel = await _context.Personeller
                .Include(p => p.Pozisyon)
                .FirstOrDefaultAsync(p => p.Id == personelId);

            if (personel == null) return 0;

            // Maaşın 1/3'ü kadar avans verilebilir
            return Math.Round((personel.Maas ?? 0) / 3, 2);
        }
    }
}