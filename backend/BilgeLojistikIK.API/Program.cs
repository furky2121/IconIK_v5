using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BilgeLojistikIK.API.Data;
using BilgeLojistikIK.API.Services;

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

    connectionString = $"Host={databaseUri.Host};Port={port};Database={databaseUri.LocalPath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";

    Console.WriteLine($"Database connection configured: Host={databaseUri.Host}, Port={port}, Database={databaseUri.LocalPath.TrimStart('/')}");
}

builder.Services.AddDbContext<BilgeLojistikIKContext>(options =>
    options.UseNpgsql(connectionString,
        o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

// Add Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IIzinService, IzinService>();
builder.Services.AddScoped<IVideoEgitimService, VideoEgitimService>();
builder.Services.AddScoped<IAvansService, AvansService>();
builder.Services.AddScoped<IMasrafService, MasrafService>();
builder.Services.AddScoped<ICVService, CVService>();
builder.Services.AddScoped<IStatusService, StatusService>();

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

            Console.WriteLine($"CORS Allowed Origins: {string.Join(", ", allowedOrigins)}");

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

// Request logging middleware - Development ve Production'da
app.Use(async (context, next) =>
{
    Console.WriteLine($"=== REQUEST: {context.Request.Method} {context.Request.Path} from {context.Request.Headers.Origin} ===");
    await next.Invoke();
    Console.WriteLine($"=== RESPONSE: {context.Response.StatusCode} ===");
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Apply migrations automatically in production
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<BilgeLojistikIKContext>();
        dbContext.Database.Migrate();
        Console.WriteLine("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database migration error: {ex.Message}");
        // Log but don't fail startup
    }
}

app.Run();
