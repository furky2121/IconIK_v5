using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using IconIK.API.Data;
using IconIK.API.Services;
using Hangfire;
using Hangfire.PostgreSql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Circular reference handling
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.MaxDepth = 64;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;

        // UTF-8 encoding support for Turkish characters
        options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = false;
        options.JsonSerializerOptions.AllowTrailingCommas = true;
        options.JsonSerializerOptions.ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            // Navigation property hatalarını filtrele
            var filteredErrors = context.ModelState
                .Where(x => !x.Key.Equals("Kademe", StringComparison.OrdinalIgnoreCase) && 
                           !x.Key.Equals("Departman", StringComparison.OrdinalIgnoreCase) &&
                           !x.Key.Equals("Pozisyonlar", StringComparison.OrdinalIgnoreCase) &&
                           !x.Key.Equals("Personeller", StringComparison.OrdinalIgnoreCase))
                .SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage))
                .ToList();
            
            var response = new
            {
                success = false,
                message = "Validation hatası",
                errors = filteredErrors
            };
            
            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(response);
        };
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Parse DATABASE_URL for Render.com deployment (PostgreSQL URI format)
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(databaseUrl))
{
    var databaseUri = new Uri(databaseUrl);
    var userInfo = databaseUri.UserInfo.Split(':');
    var port = databaseUri.Port > 0 ? databaseUri.Port : 5432; // Default PostgreSQL port

    connectionString = $"Host={databaseUri.Host};Port={port};Database={databaseUri.LocalPath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true;Encoding=UTF8";
}

builder.Services.AddDbContext<IconIKContext>(options =>
    options.UseNpgsql(connectionString,
        o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

// Add Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IIzinService, IzinService>();
builder.Services.AddScoped<IIzinKonfigurasyonService, IzinKonfigurasyonService>();
builder.Services.AddScoped<IVideoEgitimService, VideoEgitimService>();
builder.Services.AddScoped<IAvansService, AvansService>();
builder.Services.AddScoped<IMasrafService, MasrafService>();
builder.Services.AddScoped<ICVService, CVService>();
builder.Services.AddScoped<IStatusService, StatusService>();
builder.Services.AddScoped<IBildirimService, BildirimService>();
builder.Services.AddScoped<IAnketService, AnketService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<MulakatBildirimJobService>();
builder.Services.AddScoped<IBordroService, BordroService>();

// Luca Bordro Entegrasyonu Servisleri
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<ILucaBordroAyarlariService, LucaBordroAyarlariService>();
builder.Services.AddScoped<ILucaBordroService, LucaBordroService>();

// Add Hangfire
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(connectionString)));

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 1; // Tek worker yeterli
});

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? jwtSettings["SecretKey"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey!))
        };
    });

// CORS - Production için dinamik origin ayarı
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedOrigins",
        corsBuilder =>
        {
            var allowedOrigins = new List<string> {
                "http://localhost:3000",
                "http://localhost:3001",
                "http://localhost:3002"
            };

            // Production ortamında environment variable'dan frontend URL'leri ekle
            var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL");
            if (!string.IsNullOrEmpty(frontendUrl))
            {
                allowedOrigins.Add(frontendUrl);
            }

            // Vercel deployment URL'lerini environment variable'dan ekle (virgülle ayrılmış)
            var additionalOrigins = Environment.GetEnvironmentVariable("ADDITIONAL_CORS_ORIGINS");
            if (!string.IsNullOrEmpty(additionalOrigins))
            {
                var origins = additionalOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(o => o.Trim())
                    .Where(o => !string.IsNullOrWhiteSpace(o));
                allowedOrigins.AddRange(origins);
            }

            corsBuilder.WithOrigins(allowedOrigins.ToArray())
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
        });
});

// Static files support for file uploads
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
});

// Configure request localization for UTF-8 support
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("tr-TR");
    options.SupportedCultures = new[] { new System.Globalization.CultureInfo("tr-TR") };
    options.SupportedUICultures = new[] { new System.Globalization.CultureInfo("tr-TR") };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTPS Redirection - Production'da aktif
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseHsts(); // HTTP Strict Transport Security
}

// Static files
app.UseStaticFiles();

// Aday fotoğrafları için özel static file mapping
var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
if (!Directory.Exists(uploadsPath))
    Directory.CreateDirectory(uploadsPath);

var adayFotografPath = Path.Combine(uploadsPath, "aday-fotograflar");
if (!Directory.Exists(adayFotografPath))
    Directory.CreateDirectory(adayFotografPath);

app.UseCors("AllowedOrigins");

// Use request localization for UTF-8 support
app.UseRequestLocalization();

app.UseAuthentication();
app.UseAuthorization();

// Hangfire Dashboard - /hangfire URL'inden erişilebilir
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

app.MapControllers();

// Apply migrations automatically in production
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<IconIKContext>();
        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database migration error: {ex.Message}");
        // Log but don't fail startup
    }
}

// Hangfire Recurring Jobs - Her 5 dakikada bir kontrol eder
RecurringJob.AddOrUpdate<MulakatBildirimJobService>(
    "mulakat-bildirim-job",
    service => service.CheckAndSendMulakatBildirimAsync(),
    "*/5 * * * *"); // Her 5 dakikada bir çalışır

app.Run();

// Hangfire Authorization Filter - Dashboard erişim kontrolü
public class HangfireAuthorizationFilter : Hangfire.Dashboard.IDashboardAuthorizationFilter
{
    public bool Authorize(Hangfire.Dashboard.DashboardContext context)
    {
        // Development ortamında herkes erişebilir
        // Production'da JWT token kontrolü eklenebilir
        return true;
    }
}
