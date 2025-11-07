using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IconIK.API.Data;
using IconIK.API.Services;

namespace IconIK.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IconIKContext _context;
        private readonly IIzinService _izinService;

        public AdminController(IconIKContext context, IIzinService izinService)
        {
            _context = context;
            _izinService = izinService;
        }

        // GET: api/Admin/FixDayCalculations
        [HttpGet("FixDayCalculations")]
        public async Task<ActionResult<object>> FixDayCalculations()
        {
            try
            {
                var allRecords = await _context.IzinTalepleri.ToListAsync();
                var updatedRecords = new List<object>();
                var updatedCount = 0;
                
                Console.WriteLine($"DEBUG FIX: Found {allRecords.Count} total records");
                
                foreach (var record in allRecords)
                {
                    var correctGunSayisi = _izinService.CalculateGunSayisi(record.IzinBaslamaTarihi, record.IsbasiTarihi);
                    
                    if (record.GunSayisi != correctGunSayisi)
                    {
                        Console.WriteLine($"DEBUG FIX: Record ID {record.Id}: {record.IzinBaslamaTarihi:yyyy-MM-dd} to {record.IsbasiTarihi:yyyy-MM-dd}");
                        Console.WriteLine($"DEBUG FIX: Old: {record.GunSayisi} days, Correct: {correctGunSayisi} days");
                        
                        updatedRecords.Add(new {
                            id = record.Id,
                            dates = $"{record.IzinBaslamaTarihi:yyyy-MM-dd} to {record.IsbasiTarihi:yyyy-MM-dd}",
                            oldDays = record.GunSayisi,
                            newDays = correctGunSayisi
                        });
                        
                        record.GunSayisi = correctGunSayisi;
                        record.UpdatedAt = DateTime.UtcNow;
                        updatedCount++;
                    }
                }
                
                if (updatedCount > 0)
                {
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"DEBUG FIX: Successfully updated {updatedCount} records");
                }
                
                return Ok(new { 
                    success = true, 
                    data = new {
                        totalRecords = allRecords.Count,
                        updatedCount = updatedCount,
                        updatedRecords = updatedRecords
                    },
                    message = $"Day calculation fix completed. {updatedCount} out of {allRecords.Count} records were corrected." 
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG FIX: Error - {ex.Message}");
                return StatusCode(500, new { success = false, message = "Fix failed.", error = ex.Message });
            }
        }
    }
}