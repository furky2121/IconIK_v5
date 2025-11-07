# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

IconIK İK Yönetim Sistemi - Enterprise HR management system with:
- **Backend**: ASP.NET Core 8.0 Web API with Entity Framework Core and PostgreSQL
- **Frontend**: Next.js 14.2 with React 18 and PrimeReact UI components (Sakai Theme v10.1.0)
- **Database**: PostgreSQL with relational data model and comprehensive sample data
- **Authentication**: JWT-based security with SHA256 password hashing

## Development Commands

### Backend (.NET Core API)
```bash
cd backend/IconIK.API
dotnet restore                  # Restore packages
dotnet build                     # Build the project
dotnet run                       # Runs on http://localhost:5000 (default)

# Entity Framework operations
dotnet ef migrations add [MigrationName]
dotnet ef database update
dotnet ef migrations remove      # Remove last migration

# Run with specific environment
dotnet run --environment Development
dotnet run --environment Production
```

### Frontend (Next.js/React)
```bash
cd frontend
npm install                      # Install dependencies
npm run dev                      # Development server on http://localhost:3000
npm run build                    # Production build
npm run start                    # Start production server
npm run lint                     # ESLint check
npm run format                   # Prettier formatting
```

### Database Setup
```bash
# Create database
psql -U postgres
CREATE DATABASE "IconIKdb";
\q

# Apply schema and sample data
psql -d IconIKdb -f backend/IconIK.API/SQL/IconIKdb_Setup.sql

# For migrations (alternative to SQL script)
cd backend/IconIK.API
dotnet ef database update
```

## System Architecture

### Backend Architecture Patterns

**Controller Patterns**:
- **Raw JSON Parsing**: Controllers use `System.Text.Json.JsonElement` for PUT/POST to handle frontend display fields not in domain model
- **Consistent Response Format**: All endpoints return `{ success: bool, data: T, message: string }`
- **Dual Endpoint Pattern**: Main endpoints return all records, `/Aktif` endpoints return only active records for dropdowns
- **PostgreSQL Exception Handling**: Specific handling for unique constraint violations (SqlState 23505)

**Service Layer**:
- **Interface-Based Design**: All services have corresponding interfaces registered with DI
- **Business Logic Separation**: Complex rules in services (UserService, IzinService, VideoEgitimService, CVService, StatusService, MasrafService)
- **Turkish Localization**: Character conversion, timezone handling (Turkey Standard Time)
- **Document Generation**: CVService handles automatic CV generation from candidate data

**Data Access Patterns**:
- **Code-First EF Core**: Snake_case columns for PostgreSQL, explicit foreign keys
- **Soft Delete Pattern**: `Aktif` field instead of hard deletion, audit fields (CreatedAt, UpdatedAt)
- **Eager Loading**: Heavy use of `Include()` for related data
- **Restrict Delete**: Prevents cascade deletion, enforces referential integrity

### Frontend Architecture Patterns

**Next.js Structure**:
- **App Directory**: Route groups `(main)` for authenticated, `(full-page)` for public pages
- **Page-Component Bridge**: Next.js pages in `/app` are thin wrappers, business logic in `/src/pages`
- **Mixed TypeScript/JavaScript**: Route files use TypeScript (.tsx), business logic uses JavaScript (.js)

**Service Layer**:
- **Base ApiService**: Centralized HTTP client with JWT token management and error handling
- **Entity Services**: Each domain has dedicated service extending ApiService
- **Dual Endpoints**: Services implement both standard and `/Aktif` methods for filtering

**State Management**:
- **React Context**: LayoutContext for UI state (menu, sidebar)
- **Local State**: Each page manages own data with useState
- **No Redux/MobX**: Relies on React's built-in state and service layer

**UI Patterns**:
- **PrimeReact Components**: Consistent use of DataTable, Dialog, Toast, Toolbar
- **Turkish Localization**: Complete UI localization with PrimeReact locale
- **Permission-Based Rendering**: Components check permissions via yetkiService

### Active/Passive Record Management
- **Dual State**: Records marked active/passive via `Aktif` field
- **Hard Delete**: Delete operations permanently remove records
- **Endpoint Strategy**: Management pages show all records, dropdowns use `/Aktif` endpoints
- **UI Indicators**: Status badges show active (green) / passive (orange) state

### Entity Relationships
```
Kademe (1) → (N) Pozisyon
Kademe (1) → (N) KademeEkranYetkisi
Departman (1) → (N) Pozisyon
Pozisyon (1) → (N) Personel
Personel (1) → (1) Kullanici
Personel (1) → (N) IzinTalebi
Personel (N) → (M) Egitim (via PersonelEgitimi)
Personel (1) → (N) Bordro
Personel (1) → (N) VideoEgitimAtamasi
Personel (1) → (N) AvansTalebi
Personel (1) → (N) IstifaTalebi
Personel (1) → (N) PersonelGirisCikis
Personel (1) → (N) PersonelZimmet
Personel (1) → (N) MasrafTalebi
EkranYetkisi (1) → (N) KademeEkranYetkisi

# Recruitment Module Relationships
Sehir (1) → (N) Aday
IlanKategorisi (1) → (N) IsIlani
IsIlani (1) → (N) Basvuru
Aday (1) → (N) Basvuru
Basvuru (1) → (N) Mulakat
Basvuru (1) → (N) TeklifMektubu
Aday (1) → (N) AdayEgitim
Aday (1) → (N) AdayDeneyim
Aday (1) → (N) AdayYetenek
Aday (1) → (N) AdaySertifika
Aday (1) → (N) AdayReferans
Aday (1) → (N) AdayDil
Aday (1) → (N) AdayProje
Aday (1) → (N) AdayHobi
```

## Critical Business Rules

### Leave Management
- Annual allocation: `(CurrentYear - StartYear) × 14 days`
- Weekends excluded from leave day calculations
- Multi-level approval: Employee → Manager → Director → General Manager
- Database constraints prevent overlapping leave dates
- Timezone-aware calculations (Turkey Standard Time)

### Payroll Calculations
- SGK deduction: 14% of gross salary
- Tax calculation: Progressive brackets (15%-35%)
- Net salary: `GrossSalary - SGK - Tax`
- Unique constraint on (PersonelId, Year, Month)
- Salary must be within position min-max range

### Training Management
- Capacity enforcement via `MaxKatilimci` field
- Score range: 1-100 points
- Status workflow: Planned → Active → Completed
- Certificate generation upon completion

### Authentication & Security
- JWT tokens with 8-hour expiration
- Claims include: UserId, PersonelId, KademeId, DepartmanId
- SHA256 password hashing
- Username generation: Turkish to English conversion ("Özcan Gülüş" → "ozcan.gulus")
- Default password: Last 4 digits of TC Kimlik number

### Advance Payment (Avans) Management
- Maximum advance: 50% of monthly salary
- Approval workflow: Employee → Manager → Finance
- Monthly installment deductions from salary
- Active advance check prevents multiple requests

### Resignation (İstifa) Management
- Multi-step resignation process with approval workflow
- Exit interview documentation
- Asset return tracking
- Notice period calculation based on employment duration

### Personnel Time Tracking
- Daily entry/exit time recording
- Late arrival and early departure tracking
- Working hours calculation (minutes)
- Overtime tracking with different entry types (Normal, Overtime, Weekend)

### Recruitment Management
- Candidate status workflow: CV Pool → Applied → Under Review → Interview Planned → Interview Completed → Reference Check → Offer Prepared → Offer Sent → Offer Pending → Hired/Rejected
- Automatic CV generation from comprehensive candidate profiles
- Multi-stage interview process (HR, Technical, Management, General Manager)
- Offer letter management with expiration dates
- Job posting with categories and requirements
- Application tracking and candidate pool management

### Expense Management
- Expense categories: Food (Yemek), Transportation (Ulasim), Accommodation (Konaklama), Training (Egitim), Other (Diger)
- Receipt attachment requirements
- Multi-level approval workflow: Employee → Manager → Finance
- Expense amount limits and validation

### City Management
- Complete Turkish cities database with plate codes
- Integration with candidate address information
- Used for demographic analysis and reporting

## Configuration

### Backend Configuration (`appsettings.json`)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=IconIKdb;Username=postgres;Password=yourpassword"
  },
  "Jwt": {
    "Key": "your-secret-key-min-32-characters",
    "Issuer": "IconIKIK",
    "ExpireHours": 8
  }
}
```

### Frontend Configuration
- API base URL: `http://localhost:5000/api` in development
- Environment variables for production deployment
- PrimeReact Lara Light Blue theme
- Turkish locale for all components

### CORS Configuration
- Development: `http://localhost:3000`
- Production: Environment variable `FRONTEND_URL`
- Vercel/Render deployment support

## Current Implementation Status

### Completed Modules
- ✅ **Departman**: Full CRUD with active/passive management
- ✅ **Kademe**: Full CRUD with active/passive management
- ✅ **Pozisyon**: Basic CRUD (active/passive pending)
- ✅ **Personel**: Full management with photo upload and detailed profile fields
- ✅ **İzin Yönetimi**: Multi-level approval workflow with leave balance tracking
- ✅ **Eğitim**: Training management with assignments and certificates
- ✅ **Bordro**: Payroll with auto-calculations (SGK, tax brackets)
- ✅ **Dashboard**: Analytics and reporting with charts
- ✅ **Yetki Sistemi**: Screen-level permissions matrix
- ✅ **Video Eğitim**: Video training module with progress tracking and assignments
- ✅ **Avans Talepleri**: Advance payment requests with approval workflow
- ✅ **İstifa İşlemleri**: Resignation process management
- ✅ **Personel Giriş/Çıkış**: Time tracking and attendance management
- ✅ **Zimmet Yönetimi**: Asset management and tracking
- ✅ **İşe Alım Modülü**: Complete recruitment management system
  - Job posting and categories (İlan Kategorileri)
  - Job listings management (İş İlanları)
  - Candidate pool with detailed profiles (Özgeçmiş Havuzu)
  - Application tracking (Başvuru Yönetimi)
  - Interview scheduling (Mülakat Takvimi)
  - Offer letter management (Teklif Yönetimi)
  - Hiring process workflow (İşe Alım Süreçleri)
- ✅ **Masraf Yönetimi**: Employee expense management with approval workflow
  - Expense requests (Masraf Talepleri)
  - Expense approval (Masraf Onay)
- ✅ **Şehir Yönetimi**: Turkish cities database with plate codes
- ✅ **CV Otomatik Oluşturma**: Automatic CV generation from candidate data

### Recently Added Modules
- 🆕 **İşe Alım Sistemi**: Complete recruitment and hiring management (Major Release)
- 🆕 **Masraf Yönetimi**: Employee expense request and approval workflow
- 🆕 **CV Oluşturma Servisi**: Automatic CV generation with professional formatting
- 🆕 **Şehir Veritabanı**: Turkish cities with plate codes for demographic data
- 🆕 **Gelişmiş Aday Profilleri**: Comprehensive candidate profiles with education, experience, skills, certifications

### Pending Enhancements
- ⏳ Pozisyon active/passive implementation
- ⏳ Unit test coverage
- ⏳ API documentation (Swagger)
- ⏳ Email notification system
- ✅ **Report export (Excel/PDF)**: Implemented with jspdf, xlsx libraries

## Demo Accounts
| Role | Username | Password | Access Level |
|------|----------|----------|--------------|
| Genel Müdür | `ahmet.yilmaz` | `8901` | Full system access |
| İK Direktörü | `mehmet.kaya` | `8902` | HR modules access |
| BIT Direktörü | `ali.demir` | `8903` | IT department management |
| İK Uzmanı | `ozcan.bulut` | `8912` | Limited HR operations |

## File Upload Configuration
- Avatar path: `wwwroot/uploads/avatars/`
- CV files path: `wwwroot/uploads/cvs/`
- Candidate photos path: `wwwroot/uploads/candidates/`
- Expense receipts path: `wwwroot/uploads/receipts/`
- Max file size: 10MB
- Supported formats: JPG, PNG, GIF, PDF, DOC, DOCX
- Static file serving configured in `Program.cs`

## Testing Guidelines
When testing CRUD operations:
1. Verify management pages show both active and passive records
2. Confirm dropdowns only display active records via `/Aktif` endpoints
3. Test hard delete removes records permanently
4. Validate unique constraints (department code, level number)
5. Check cascade delete restrictions
6. Verify JWT token expiration handling
7. Test Turkish character conversion in usernames

## Common Development Tasks

### Adding New Entity
1. Create model in `Models/` with audit fields
2. Add DbSet to `IconIKContext`
3. Create controller with standard CRUD and `/Aktif` endpoint
4. Add service in frontend `src/services/`
5. Create page component in `src/pages/`
6. Add Next.js route in `app/(main)/`
7. Update navigation in `AppMenu.tsx`

### Implementing Active/Passive Pattern
1. Ensure entity has `Aktif` boolean field
2. Add `/Aktif` endpoint returning filtered records
3. Update service to include `getAktif()` method
4. Use `getAktif()` for dropdown data sources
5. Show status badge in management tables

### Database Migration
```bash
cd backend/IconIK.API
dotnet ef migrations add [DescriptiveName]
dotnet ef database update
# If rollback needed:
dotnet ef database update [PreviousMigration]
dotnet ef migrations remove
```

## Troubleshooting

### Common Issues
- **Port conflicts**: Backend defaults to 5000, frontend to 3000
- **Database connection**: Check PostgreSQL service and connection string
- **CORS errors**: Verify allowed origins in backend configuration
- **Token expiration**: Frontend should redirect to login on 401 responses
- **Turkish characters**: Ensure UTF-8 encoding throughout stack

## Controller Response Pattern
All controllers follow a consistent JSON response format:
```json
{
  "success": true,
  "data": {...},
  "message": "İşlem başarılı"
}
```

Error responses include descriptive messages with PostgreSQL exception handling for unique constraints (SqlState 23505).

## Service Registration Pattern
All services use interface-based dependency injection:
```csharp
// In Program.cs
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IIzinService, IzinService>();
builder.Services.AddScoped<ICVService, CVService>();
builder.Services.AddScoped<IStatusService, StatusService>();
builder.Services.AddScoped<IMasrafService, MasrafService>();
```

## Frontend API Call Pattern
Services extend base ApiService with JWT token management:
```javascript
// Example service method
async getAll() {
    return await this.get('/personel');
}

async getAktif() {
    return await this.get('/personel/aktif');
}

// Recruitment module example
async getAdaylar() {
    return await this.get('/aday');
}

async generateCV(adayId) {
    return await this.get(`/aday/${adayId}/cv-generate`);
}