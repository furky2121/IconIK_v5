# Render Environment Variables Setup

## Backend (API Service) - Environment Variables

Render.com'da backend service için aşağıdaki environment variable'ları tanımlayın:

### Database
```
DATABASE_URL=postgresql://username:password@hostname:port/database_name
```

### JWT Settings
```
JWT_SECRET_KEY=IconIKIK-JWT-Secret-Key-2024-Very-Strong-Key
JWT_ISSUER=IconIK.API
JWT_AUDIENCE=IconIKIK.Client
JWT_EXPIRATION_HOURS=8
```

### Frontend URL (CORS için)
```
FRONTEND_URL=https://hr-management-v1.vercel.app
```

## Vercel Environment Variables

Vercel'de frontend için:

```
NEXT_PUBLIC_API_BASE_URL=https://IconIK-api.onrender.com/api
NEXT_PUBLIC_FILE_BASE_URL=https://IconIK-api.onrender.com
```

## Hata Çözümü

"Bearer error="invalid_token", error_description="The signature key was not found"" hatası JWT secret key'in Render'da tanımlanmamasından kaynaklanıyor.

1. Render dashboard'a git
2. Backend service'ini seç
3. Environment tab'ına git
4. Yukarıdaki JWT environment variable'larını ekle
5. Deploy'u tetikle