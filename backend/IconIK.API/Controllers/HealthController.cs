using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IconIK.API.Data;

namespace IconIK.API.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        private readonly IconIKContext _context;
        private readonly ILogger<HealthController> _logger;

        public HealthController(IconIKContext context, ILogger<HealthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Check()
        {
            try
            {
                var healthCheck = new
                {
                    status = "healthy",
                    timestamp = DateTime.UtcNow,
                    version = GetType().Assembly.GetName().Version?.ToString() ?? "unknown",
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "unknown",
                    database = await CheckDatabaseAsync(),
                    uptime = GetUptime()
                };

                _logger.LogInformation("Health check completed successfully");
                return Ok(healthCheck);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                
                var healthCheck = new
                {
                    status = "unhealthy",
                    timestamp = DateTime.UtcNow,
                    error = ex.Message,
                    version = GetType().Assembly.GetName().Version?.ToString() ?? "unknown",
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "unknown"
                };

                return StatusCode(503, healthCheck);
            }
        }

        [HttpGet("ready")]
        public async Task<IActionResult> ReadinessCheck()
        {
            try
            {
                // Database bağlantısını kontrol et
                await _context.Database.CanConnectAsync();
                
                // Kritik tabloları kontrol et
                var userCount = await _context.Kullanicilar.CountAsync();
                
                var readinessCheck = new
                {
                    status = "ready",
                    timestamp = DateTime.UtcNow,
                    database = "connected",
                    tables = "accessible"
                };

                return Ok(readinessCheck);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Readiness check failed");
                
                var readinessCheck = new
                {
                    status = "not_ready",
                    timestamp = DateTime.UtcNow,
                    error = ex.Message
                };

                return StatusCode(503, readinessCheck);
            }
        }

        [HttpGet("live")]
        public IActionResult LivenessCheck()
        {
            var livenessCheck = new
            {
                status = "alive",
                timestamp = DateTime.UtcNow,
                process_id = Environment.ProcessId,
                machine_name = Environment.MachineName
            };

            return Ok(livenessCheck);
        }

        private async Task<object> CheckDatabaseAsync()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                var connectionString = _context.Database.GetConnectionString();
                
                // Connection string'den sensitive bilgileri temizle
                var sanitizedConnectionString = connectionString?.Split(';')
                    .Where(part => !part.ToLower().Contains("password") && !part.ToLower().Contains("pwd"))
                    .Aggregate((a, b) => $"{a};{b}") ?? "unknown";

                return new
                {
                    status = canConnect ? "connected" : "disconnected",
                    provider = _context.Database.ProviderName,
                    connection = sanitizedConnectionString
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    status = "error",
                    error = ex.Message
                };
            }
        }

        private string GetUptime()
        {
            try
            {
                var uptime = Environment.TickCount64;
                var uptimeSpan = TimeSpan.FromMilliseconds(uptime);
                return $"{uptimeSpan.Days}d {uptimeSpan.Hours}h {uptimeSpan.Minutes}m {uptimeSpan.Seconds}s";
            }
            catch
            {
                return "unknown";
            }
        }
    }
}