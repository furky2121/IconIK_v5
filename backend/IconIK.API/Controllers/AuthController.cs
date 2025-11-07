using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IconIK.API.Data;
using IconIK.API.Models;
using IconIK.API.Services;

namespace IconIK.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IconIKContext _context;
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;

        public AuthController(IconIKContext context, IUserService userService, IConfiguration configuration)
        {
            _context = context;
            _userService = userService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.KullaniciAdi) || string.IsNullOrEmpty(request.Sifre))
                {
                    return BadRequest(new { success = false, message = "Kullanıcı adı ve şifre gereklidir." });
                }

                // Kullanıcıyı bul - Kullanıcı adı veya sicil numarası ile
                var kullanici = await _context.Kullanicilar
                    .Include(k => k.Personel)
                        .ThenInclude(p => p.Pozisyon)
                            .ThenInclude(pos => pos.Departman)
                    .Include(k => k.Personel)
                        .ThenInclude(p => p.Pozisyon)
                            .ThenInclude(pos => pos.Kademe)
                    .FirstOrDefaultAsync(k => 
                        (k.KullaniciAdi.ToLower() == request.KullaniciAdi.ToLower() || 
                         k.Personel.Id.ToString() == request.KullaniciAdi) && k.Aktif);

                if (kullanici == null)
                {
                    return Unauthorized(new { success = false, message = "Geçersiz kullanıcı adı veya şifre." });
                }

                // Şifre kontrolü
                if (!_userService.VerifyPassword(request.Sifre, kullanici.SifreHash))
                {
                    return Unauthorized(new { success = false, message = "Geçersiz kullanıcı adı veya şifre." });
                }

                // Personel aktif mi kontrolü
                if (!kullanici.Personel.Aktif)
                {
                    return Unauthorized(new { success = false, message = "Hesabınız pasif durumda. Lütfen yöneticinize başvurun." });
                }

                // Son giriş tarihini güncelle
                kullanici.SonGirisTarihi = DateTime.UtcNow;
                kullanici.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                Console.WriteLine($"=== LOGIN SUCCESS DEBUG ===");
                Console.WriteLine($"KullaniciId: {kullanici.Id}");
                Console.WriteLine($"KullaniciAdi: {kullanici.KullaniciAdi}");
                Console.WriteLine($"PersonelId: {kullanici.PersonelId}");
                Console.WriteLine($"PersonelAd: {kullanici.Personel.Ad}");
                Console.WriteLine($"PersonelSoyad: {kullanici.Personel.Soyad}");

                // JWT token oluştur
                var token = GenerateJwtToken(kullanici);

                var response = new
                {
                    success = true,
                    message = "Giriş başarılı.",
                    data = new
                    {
                        token,
                        kullanici = new
                        {
                            kullanici.Id,
                            kullanici.KullaniciAdi,
                            kullanici.IlkGiris,
                            kullanici.KVKKOnaylandi,
                            kullanici.KVKKOnayTarihi,
                            Personel = new
                            {
                                kullanici.Personel.Id,
                                kullanici.Personel.Ad,
                                kullanici.Personel.Soyad,
                                AdSoyad = kullanici.Personel.Ad + " " + kullanici.Personel.Soyad,
                                kullanici.Personel.Email,
                                kullanici.Personel.TcKimlik,
                                kullanici.Personel.Telefon,
                                kullanici.Personel.IseBaslamaTarihi,
                                kullanici.Personel.FotografUrl,
                                Pozisyon = new
                                {
                                    kullanici.Personel.Pozisyon.Id,
                                    kullanici.Personel.Pozisyon.Ad,
                                    Departman = new
                                    {
                                        kullanici.Personel.Pozisyon.Departman.Id,
                                        kullanici.Personel.Pozisyon.Departman.Ad
                                    },
                                    Kademe = new
                                    {
                                        kullanici.Personel.Pozisyon.Kademe.Id,
                                        kullanici.Personel.Pozisyon.Kademe.Ad,
                                        kullanici.Personel.Pozisyon.Kademe.Seviye
                                    }
                                }
                            }
                        }
                    }
                };

                Console.WriteLine($"=== RESPONSE DEBUG ===");
                Console.WriteLine($"Token: {token}");
                Console.WriteLine($"Response Success: {response.success}");
                Console.WriteLine($"Response Message: {response.message}");
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Giriş işlemi sırasında bir hata oluştu.", error = ex.Message });
            }
        }

        [HttpPost("change-password")]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.MevcutSifre) || string.IsNullOrEmpty(request.YeniSifre))
                {
                    return BadRequest(new { success = false, message = "Mevcut şifre ve yeni şifre gereklidir." });
                }

                if (request.YeniSifre.Length < 6)
                {
                    return BadRequest(new { success = false, message = "Yeni şifre en az 6 karakter olmalıdır." });
                }

                // Kullanıcıyı bul
                var kullanici = await _context.Kullanicilar.FindAsync(request.KullaniciId);
                if (kullanici == null || !kullanici.Aktif)
                {
                    return NotFound(new { success = false, message = "Kullanıcı bulunamadı." });
                }

                // Mevcut şifre kontrolü
                if (!_userService.VerifyPassword(request.MevcutSifre, kullanici.SifreHash))
                {
                    return BadRequest(new { success = false, message = "Mevcut şifre yanlış." });
                }

                // Yeni şifre mevcut şifre ile aynı mı kontrolü
                if (_userService.VerifyPassword(request.YeniSifre, kullanici.SifreHash))
                {
                    return BadRequest(new { success = false, message = "Yeni şifre mevcut şifre ile aynı olamaz." });
                }

                // Şifreyi güncelle
                var success = await _userService.ChangePasswordAsync(request.KullaniciId, request.YeniSifre);
                if (success)
                {
                    return Ok(new { success = true, message = "Şifre başarıyla değiştirildi." });
                }
                else
                {
                    return StatusCode(500, new { success = false, message = "Şifre değiştirilemedi." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Şifre değiştirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        [HttpPost("first-login-change-password")]
        public async Task<ActionResult> FirstLoginChangePassword([FromBody] FirstLoginChangePasswordRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.KullaniciAdi) || string.IsNullOrEmpty(request.MevcutSifre) || string.IsNullOrEmpty(request.YeniSifre))
                {
                    return BadRequest(new { success = false, message = "Tüm alanlar gereklidir." });
                }

                if (request.YeniSifre.Length < 6)
                {
                    return BadRequest(new { success = false, message = "Yeni şifre en az 6 karakter olmalıdır." });
                }

                // Kullanıcıyı bul
                var kullanici = await _context.Kullanicilar
                    .Include(k => k.Personel)
                    .FirstOrDefaultAsync(k => k.KullaniciAdi.ToLower() == request.KullaniciAdi.ToLower() && k.Aktif);

                if (kullanici == null)
                {
                    return Unauthorized(new { success = false, message = "Geçersiz kullanıcı adı." });
                }

                // İlk giriş kontrolü
                if (!kullanici.IlkGiris)
                {
                    return BadRequest(new { success = false, message = "Bu işlem sadece ilk giriş için geçerlidir." });
                }

                // Mevcut şifre kontrolü (TC'nin son 4 hanesi)
                if (!_userService.VerifyPassword(request.MevcutSifre, kullanici.SifreHash))
                {
                    return BadRequest(new { success = false, message = "Mevcut şifre yanlış." });
                }

                // Yeni şifre mevcut şifre ile aynı mı kontrolü
                if (_userService.VerifyPassword(request.YeniSifre, kullanici.SifreHash))
                {
                    return BadRequest(new { success = false, message = "Yeni şifre mevcut şifre ile aynı olamaz." });
                }

                // Şifreyi güncelle ve ilk giriş bayrağını kapat
                var success = await _userService.ChangePasswordAsync(kullanici.Id, request.YeniSifre);
                if (success)
                {
                    return Ok(new { success = true, message = "Şifre başarıyla değiştirildi. Yeni şifreniz ile giriş yapabilirsiniz." });
                }
                else
                {
                    return StatusCode(500, new { success = false, message = "Şifre değiştirilemedi." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Şifre değiştirilirken bir hata oluştu.", error = ex.Message });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.KullaniciAdi))
                {
                    return BadRequest(new { success = false, message = "Kullanıcı adı gereklidir." });
                }

                // Kullanıcıyı bul
                var kullanici = await _context.Kullanicilar
                    .Include(k => k.Personel)
                    .FirstOrDefaultAsync(k => k.KullaniciAdi.ToLower() == request.KullaniciAdi.ToLower() && k.Aktif);

                if (kullanici == null)
                {
                    // Güvenlik nedeniyle kullanıcının var olup olmadığını belirtmiyoruz
                    return Ok(new { 
                        success = true, 
                        message = "Eğer bu kullanıcı adı sistemde kayıtlıysa, yeni şifre TC kimlik numaranızın son 4 hanesi olarak sıfırlanmıştır." 
                    });
                }

                // Personel aktif mi kontrolü
                if (!kullanici.Personel.Aktif)
                {
                    return Ok(new { 
                        success = true, 
                        message = "Eğer bu kullanıcı adı sistemde kayıtlıysa, yeni şifre TC kimlik numaranızın son 4 hanesi olarak sıfırlanmıştır." 
                    });
                }

                // Şifreyi TC kimlik son 4 hane olarak sıfırla
                var newPassword = kullanici.Personel.TcKimlik.Substring(kullanici.Personel.TcKimlik.Length - 4);
                var hashedPassword = _userService.HashPassword(newPassword);

                kullanici.SifreHash = hashedPassword;
                kullanici.IlkGiris = true; // Şifre sıfırlama sonrası ilk giriş yap
                kullanici.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { 
                    success = true, 
                    message = "Şifreniz TC kimlik numaranızın son 4 hanesi olarak sıfırlanmıştır. Sisteme giriş yaptıktan sonra şifrenizi değiştirmeniz gerekecektir." 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Şifre sıfırlanırken bir hata oluştu.", error = ex.Message });
            }
        }

        [HttpGet("debug-user/{kullaniciAdi}")]
        public async Task<ActionResult> DebugUser(string kullaniciAdi)
        {
            try
            {
                var kullanici = await _context.Kullanicilar
                    .Include(k => k.Personel)
                    .FirstOrDefaultAsync(k => k.KullaniciAdi.ToLower() == kullaniciAdi.ToLower());

                if (kullanici == null)
                {
                    return NotFound(new { message = "Kullanıcı bulunamadı." });
                }

                return Ok(new
                {
                    KullaniciId = kullanici.Id,
                    KullaniciAdi = kullanici.KullaniciAdi,
                    PersonelId = kullanici.PersonelId,
                    PersonelAd = kullanici.Personel?.Ad,
                    PersonelSoyad = kullanici.Personel?.Soyad,
                    PersonelTcKimlik = kullanici.Personel?.TcKimlik,
                    IlkGiris = kullanici.IlkGiris,
                    Aktif = kullanici.Aktif
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private string GenerateJwtToken(Kullanici kullanici)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            // Production için environment variable'dan oku, yoksa config'den al
            var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expirationHours = int.Parse(jwtSettings["ExpirationHours"] ?? "8");

            var key = Encoding.ASCII.GetBytes(secretKey!);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, kullanici.Id.ToString()),
                    new Claim(ClaimTypes.Name, kullanici.KullaniciAdi),
                    new Claim("PersonelId", kullanici.PersonelId.ToString()),
                    new Claim("KademeId", kullanici.Personel.Pozisyon.KademeId.ToString()),
                    new Claim("KademeSeviye", kullanici.Personel.Pozisyon.Kademe.Seviye.ToString()),
                    new Claim("DepartmanId", kullanici.Personel.Pozisyon.DepartmanId.ToString()),
                    new Claim("IlkGiris", kullanici.IlkGiris.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(expirationHours),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

    public class LoginRequest
    {
        public string KullaniciAdi { get; set; } = string.Empty;
        public string Sifre { get; set; } = string.Empty;
    }

    public class ChangePasswordRequest
    {
        public int KullaniciId { get; set; }
        public string MevcutSifre { get; set; } = string.Empty;
        public string YeniSifre { get; set; } = string.Empty;
    }

    public class FirstLoginChangePasswordRequest
    {
        public string KullaniciAdi { get; set; } = string.Empty;
        public string MevcutSifre { get; set; } = string.Empty;
        public string YeniSifre { get; set; } = string.Empty;
    }

    public class ForgotPasswordRequest
    {
        public string KullaniciAdi { get; set; } = string.Empty;
    }
}