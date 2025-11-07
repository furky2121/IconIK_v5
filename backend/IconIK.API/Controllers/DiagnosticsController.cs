using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IconIK.API.Data;
using IconIK.API.Models;

namespace IconIK.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiagnosticsController : ControllerBase
    {
        private readonly IconIKContext _context;
        private readonly IConfiguration _configuration;

        public DiagnosticsController(IconIKContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            bool canConnect = false;
            var tables = new List<string>();
            int userCount = 0;
            string error = "";

            try
            {
                // Test database connection
                canConnect = await _context.Database.CanConnectAsync();
                
                if (canConnect)
                {
                    // Count users
                    userCount = await _context.Kullanicilar.CountAsync();
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            var diagnostics = new
            {
                timestamp = DateTime.UtcNow,
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                database = new
                {
                    canConnect = canConnect,
                    tables = tables,
                    userCount = userCount,
                    error = error
                },
                jwt = new
                {
                    hasSecretKey = !string.IsNullOrEmpty(_configuration["JwtSettings:SecretKey"] ?? 
                                                          Environment.GetEnvironmentVariable("JwtSettings__SecretKey")),
                    issuer = _configuration["JwtSettings:Issuer"] ?? 
                            Environment.GetEnvironmentVariable("JwtSettings__Issuer"),
                    audience = _configuration["JwtSettings:Audience"] ?? 
                              Environment.GetEnvironmentVariable("JwtSettings__Audience")
                }
            };

            return Ok(diagnostics);
        }

        [HttpGet("simple")]
        public IActionResult GetSimpleStatus()
        {
            return Ok(new { 
                status = "API is running",
                timestamp = DateTime.UtcNow,
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                connectionString = _configuration.GetConnectionString("DefaultConnection")?.Substring(0, 50) + "..."
            });
        }
    }
}