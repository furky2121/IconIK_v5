using Microsoft.EntityFrameworkCore;
using IconIK.API.Data;
using IconIK.API.Models;

namespace IconIK.API.Services
{
    public class IzinKonfigurasyonService : IIzinKonfigurasyonService
    {
        private readonly IconIKContext _context;
        private readonly ILogger<IzinKonfigurasyonService> _logger;

        public IzinKonfigurasyonService(IconIKContext context, ILogger<IzinKonfigurasyonService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<IzinTipi>> GetAllIzinTipleri()
        {
            try
            {
                return await _context.IzinTipleri
                    .OrderBy(it => it.Sira)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all leave types");
                throw;
            }
        }

        public async Task<IEnumerable<IzinTipi>> GetAktifIzinTipleri()
        {
            try
            {
                return await _context.IzinTipleri
                    .Where(it => it.Aktif)
                    .OrderBy(it => it.Sira)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active leave types");
                throw;
            }
        }

        public async Task<IEnumerable<IzinTipi>> GetAktifIzinTipleriByGender(string? cinsiyet)
        {
            try
            {
                var query = _context.IzinTipleri
                    .Where(it => it.Aktif);

                // Filter by gender if specified
                if (!string.IsNullOrEmpty(cinsiyet))
                {
                    query = query.Where(it => it.CinsiyetKisiti == null || it.CinsiyetKisiti == cinsiyet);
                }

                return await query
                    .OrderBy(it => it.Sira)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active leave types by gender: {Gender}", cinsiyet);
                throw;
            }
        }

        public async Task<IzinTipi?> GetIzinTipiById(int id)
        {
            try
            {
                return await _context.IzinTipleri
                    .FirstOrDefaultAsync(it => it.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving leave type by ID: {Id}", id);
                throw;
            }
        }

        public async Task<IzinTipi?> GetIzinTipiByName(string name)
        {
            try
            {
                return await _context.IzinTipleri
                    .FirstOrDefaultAsync(it => it.IzinTipiAdi == name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving leave type by name: {Name}", name);
                throw;
            }
        }

        public async Task<IzinTipi> CreateIzinTipi(IzinTipi izinTipi)
        {
            try
            {
                // Check if leave type with same name already exists
                if (await IzinTipiExists(izinTipi.IzinTipiAdi))
                {
                    throw new InvalidOperationException($"İzin tipi '{izinTipi.IzinTipiAdi}' zaten mevcut.");
                }

                izinTipi.CreatedAt = DateTime.UtcNow;
                _context.IzinTipleri.Add(izinTipi);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Leave type created: {LeaveType}", izinTipi.IzinTipiAdi);
                return izinTipi;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating leave type: {LeaveType}", izinTipi.IzinTipiAdi);
                throw;
            }
        }

        public async Task<IzinTipi> UpdateIzinTipi(IzinTipi izinTipi)
        {
            try
            {
                var existing = await _context.IzinTipleri.FindAsync(izinTipi.Id);
                if (existing == null)
                {
                    throw new InvalidOperationException($"İzin tipi ID {izinTipi.Id} bulunamadı.");
                }

                // Check if name is being changed to an existing name
                if (existing.IzinTipiAdi != izinTipi.IzinTipiAdi && await IzinTipiExists(izinTipi.IzinTipiAdi, izinTipi.Id))
                {
                    throw new InvalidOperationException($"İzin tipi '{izinTipi.IzinTipiAdi}' zaten mevcut.");
                }

                existing.IzinTipiAdi = izinTipi.IzinTipiAdi;
                existing.StandartGunSayisi = izinTipi.StandartGunSayisi;
                existing.MinimumGunSayisi = izinTipi.MinimumGunSayisi;
                existing.MaksimumGunSayisi = izinTipi.MaksimumGunSayisi;
                existing.CinsiyetKisiti = izinTipi.CinsiyetKisiti;
                existing.RaporGerekli = izinTipi.RaporGerekli;
                existing.UcretliMi = izinTipi.UcretliMi;
                existing.Renk = izinTipi.Renk;
                existing.Aciklama = izinTipi.Aciklama;
                existing.Sira = izinTipi.Sira;
                existing.Aktif = izinTipi.Aktif;
                existing.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Leave type updated: {LeaveType}", existing.IzinTipiAdi);
                return existing;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating leave type ID: {Id}", izinTipi.Id);
                throw;
            }
        }

        public async Task<bool> DeleteIzinTipi(int id)
        {
            try
            {
                var izinTipi = await _context.IzinTipleri.FindAsync(id);
                if (izinTipi == null)
                {
                    return false;
                }

                // Soft delete - just mark as inactive
                izinTipi.Aktif = false;
                izinTipi.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Leave type deactivated: {LeaveType}", izinTipi.IzinTipiAdi);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting leave type ID: {Id}", id);
                throw;
            }
        }

        public async Task<bool> IzinTipiExists(string name, int? excludeId = null)
        {
            try
            {
                var query = _context.IzinTipleri.Where(it => it.IzinTipiAdi == name);

                if (excludeId.HasValue)
                {
                    query = query.Where(it => it.Id != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if leave type exists: {Name}", name);
                throw;
            }
        }
    }
}
