using Microsoft.EntityFrameworkCore;
using IconIK.API.Data;
using IconIK.API.Models;

namespace IconIK.API.Services
{
    public interface IMasrafService
    {
        Task<bool> CheckMasrafLimit(int personelId, MasrafTipi masrafTipi, decimal tutar);
        Task<decimal> GetMasrafLimit(int personelId, MasrafTipi masrafTipi);
        Task<decimal> GetAylikMasrafToplami(int personelId, int ay, int yil);
    }

    public class MasrafService : IMasrafService
    {
        private readonly IconIKContext _context;

        public MasrafService(IconIKContext context)
        {
            _context = context;
        }

        public async Task<bool> CheckMasrafLimit(int personelId, MasrafTipi masrafTipi, decimal tutar)
        {
            var personel = await _context.Personeller
                .Include(p => p.Pozisyon)
                .FirstOrDefaultAsync(p => p.Id == personelId);

            if (personel == null) return false;

            var limit = await GetMasrafLimit(personelId, masrafTipi);
            
            // Bu ay yapılan masrafları hesaba kat
            var mevcutMasraflar = await _context.MasrafTalepleri
                .Where(m => m.PersonelId == personelId 
                    && m.MasrafTipi == masrafTipi
                    && (m.OnayDurumu == "Beklemede" || m.OnayDurumu == "Onaylandı")
                    && m.TalepTarihi.Month == DateTime.Now.Month
                    && m.TalepTarihi.Year == DateTime.Now.Year)
                .SumAsync(m => m.Tutar);

            return (mevcutMasraflar + tutar) <= limit;
        }

        public async Task<decimal> GetMasrafLimit(int personelId, MasrafTipi masrafTipi)
        {
            var personel = await _context.Personeller
                .Include(p => p.Pozisyon)
                    .ThenInclude(pos => pos.Kademe)
                .FirstOrDefaultAsync(p => p.Id == personelId);

            if (personel == null) return 0;

            var maas = personel.Maas ?? 0;
            var kademeSeviye = personel.Pozisyon?.Kademe?.Seviye ?? 5;

            // Kademe seviyesine göre masraf limitleri (maaş üzerinden yüzde)
            decimal limitYuzdesi = masrafTipi switch
            {
                MasrafTipi.Yemek => GetYemekLimitYuzdesi(kademeSeviye),
                MasrafTipi.Ulasim => GetUlasimLimitYuzdesi(kademeSeviye),
                MasrafTipi.Konaklama => GetKonaklamaLimitYuzdesi(kademeSeviye),
                MasrafTipi.Egitim => GetEgitimLimitYuzdesi(kademeSeviye),
                MasrafTipi.Diger => GetDigerLimitYuzdesi(kademeSeviye),
                _ => 0.05m
            };

            return Math.Round(maas * limitYuzdesi, 2);
        }

        public async Task<decimal> GetAylikMasrafToplami(int personelId, int ay, int yil)
        {
            return await _context.MasrafTalepleri
                .Where(m => m.PersonelId == personelId 
                    && m.OnayDurumu == "Onaylandı"
                    && m.TalepTarihi.Month == ay
                    && m.TalepTarihi.Year == yil)
                .SumAsync(m => m.Tutar);
        }

        private decimal GetYemekLimitYuzdesi(int kademeSeviye)
        {
            return kademeSeviye switch
            {
                1 => 0.15m, // %15
                2 => 0.12m, // %12
                3 => 0.10m, // %10
                4 => 0.08m, // %8
                _ => 0.05m  // %5
            };
        }

        private decimal GetUlasimLimitYuzdesi(int kademeSeviye)
        {
            return kademeSeviye switch
            {
                1 => 0.10m, // %10
                2 => 0.08m, // %8
                3 => 0.06m, // %6
                4 => 0.05m, // %5
                _ => 0.03m  // %3
            };
        }

        private decimal GetKonaklamaLimitYuzdesi(int kademeSeviye)
        {
            return kademeSeviye switch
            {
                1 => 0.20m, // %20
                2 => 0.15m, // %15
                3 => 0.10m, // %10
                4 => 0.08m, // %8
                _ => 0.05m  // %5
            };
        }

        private decimal GetEgitimLimitYuzdesi(int kademeSeviye)
        {
            return kademeSeviye switch
            {
                1 => 0.25m, // %25
                2 => 0.20m, // %20
                3 => 0.15m, // %15
                4 => 0.10m, // %10
                _ => 0.08m  // %8
            };
        }

        private decimal GetDigerLimitYuzdesi(int kademeSeviye)
        {
            return kademeSeviye switch
            {
                1 => 0.08m, // %8
                2 => 0.06m, // %6
                3 => 0.05m, // %5
                4 => 0.04m, // %4
                _ => 0.03m  // %3
            };
        }
    }
}