# 🚀 IconIK İK Yönetim Sistemi - Production Deployment Guide

Bu rehber, IconIK İK Yönetim Sistemi'ni production ortamına deploy etmek için gerekli tüm adımları içerir.

## 📋 Sistem Gereksinimleri

### Minimum Sistem Gereksinimleri
- **CPU**: 2 vCore
- **RAM**: 4 GB
- **Disk**: 50 GB SSD
- **Network**: 100 Mbps
- **OS**: Ubuntu 20.04+ / CentOS 8+ / Debian 11+

### Önerilen Sistem Gereksinimleri
- **CPU**: 4 vCore
- **RAM**: 8 GB
- **Disk**: 100 GB SSD
- **Network**: 1 Gbps
- **OS**: Ubuntu 22.04 LTS

## 🛠️ Kurulum Gereksinimleri

- Docker 24.0+
- Docker Compose 2.0+
- Git
- SSL Sertifikası (Let's Encrypt önerilir)

## 🚀 Hızlı Kurulum

### 1. Projeyi İndirin
```bash
git clone <repository-url>
cd IconIK-IK
```

### 2. Environment Ayarları
```bash
cp .env.example .env
nano .env  # Ayarlarınızı düzenleyin
```

### 3. Deployment Çalıştırın
```bash
chmod +x scripts/*.sh
./scripts/deploy.sh
```

## ⚙️ Detaylı Konfigürasyon

### Environment Variables (.env)

```bash
# Database Configuration
DB_HOST=postgres
DB_NAME=IconIKdb
DB_USER=bilgeik
DB_PASSWORD=GÜÇLÜ_ŞİFRE_BURAYA
DB_PORT=5432

# JWT Configuration  
JWT_SECRET_KEY=64_KARAKTER_GÜÇLÜ_ANAHTAR_BURAYA
JWT_ISSUER=YourCompanyName
JWT_AUDIENCE=YourCompanyName-Users

# Application URLs
FRONTEND_URL=https://yourdomain.com
BACKEND_URL=https://api.yourdomain.com
```

### SSL Sertifikası Kurulumu

#### Let's Encrypt (Ücretsiz - Önerilir)
```bash
./scripts/setup-ssl.sh
# Seçenek 1'i seçin ve domain/email bilgilerinizi girin
```

#### Self-Signed (Test Ortamı)
```bash
./scripts/setup-ssl.sh
# Seçenek 2'yi seçin
```

## 🗄️ Database Yönetimi

### İlk Kurulum
```bash
# Container'ları başlatın
docker-compose up -d postgres

# Veritabanı hazır olana kadar bekleyin
docker-compose logs -f postgres
```

### Migration İşlemleri
```bash
# Migration çalıştır
docker-compose exec backend dotnet ef database update

# Yeni migration oluştur
docker-compose exec backend dotnet ef migrations add MigrationName
```

### Backup İşlemleri
```bash
# Manuel backup
./scripts/backup.sh

# Otomatik backup (crontab)
echo "0 2 * * * /path/to/project/scripts/backup.sh" | crontab -
```

## 📊 Monitoring ve Health Checks

### Health Check Endpoints
- **API Health**: `GET /health`
- **Readiness**: `GET /health/ready`
- **Liveness**: `GET /health/live`

### Log İzleme
```bash
# Tüm servis logları
docker-compose logs -f

# Belirli servis logları
docker-compose logs -f backend
docker-compose logs -f frontend
docker-compose logs -f postgres
```

## 🔒 Güvenlik Konfigürasyonu

### Firewall Ayarları
```bash
# UFW (Ubuntu)
sudo ufw allow 22/tcp    # SSH
sudo ufw allow 80/tcp    # HTTP
sudo ufw allow 443/tcp   # HTTPS
sudo ufw enable

# firewalld (CentOS)
sudo firewall-cmd --permanent --add-service=ssh
sudo firewall-cmd --permanent --add-service=http
sudo firewall-cmd --permanent --add-service=https
sudo firewall-cmd --reload
```

### SSL/TLS Konfigürasyonu
- TLS 1.2+ zorunlu
- HSTS enabled
- Secure headers aktif
- Mixed content koruması

### Database Güvenliği
- Güçlü şifreler
- Database erişimi sadece uygulama networku
- Regular backup
- Şifreleme at rest

## 📈 Performance Optimizasyonu

### Backend Optimizasyonları
- Connection pooling
- Response caching
- Query optimization
- Static file compression

### Frontend Optimizasyonları
- Code splitting
- Image optimization
- CDN kullanımı
- Browser caching

### Database Optimizasyonları
- Index optimizasyonu
- Query performance monitoring
- Connection pooling
- Regular VACUUM

## 🔄 Update İşlemleri

### Uygulama Güncellemesi
```bash
# Kodları güncelle
git pull origin main

# Zero-downtime deployment
./scripts/deploy.sh

# Rollback (gerekirse)
git checkout previous-version
./scripts/deploy.sh
```

### Database Migration
```bash
# Migration'ları çalıştır
docker-compose exec backend dotnet ef database update

# Migration durumunu kontrol et
docker-compose exec backend dotnet ef migrations list
```

## 🚨 Troubleshooting

### Yaygın Problemler

#### Container Başlamıyor
```bash
# Container durumunu kontrol et
docker-compose ps

# Logları incele
docker-compose logs <service-name>

# Container'ı yeniden başlat
docker-compose restart <service-name>
```

#### Database Bağlantı Hatası
```bash
# PostgreSQL durumunu kontrol et
docker-compose exec postgres pg_isready

# Database loglarını incele
docker-compose logs postgres

# Database'e manuel bağlan
docker-compose exec postgres psql -U bilgeik -d IconIKdb
```

#### SSL Sertifika Problemleri
```bash
# Sertifika geçerlilik kontrolü
openssl x509 -in certificates/fullchain.pem -text -noout

# Let's Encrypt yenileme
sudo certbot renew
```

## 📞 Destek ve Bakım

### Log Dosyaları
- Backend logs: `docker-compose logs backend`
- Frontend logs: `docker-compose logs frontend`
- Database logs: `docker-compose logs postgres`

### Performance İzleme
- Health check endpoints
- Database performance monitoring
- Resource usage monitoring

### Bakım Takvimi
- **Günlük**: Log kontrolü, health check
- **Haftalık**: Performance review, backup kontrolü
- **Aylık**: Security updates, dependency updates
- **3 Aylık**: Full system review, capacity planning

## 🔧 Gelişmiş Konfigürasyon

### Load Balancer Kurulumu (Nginx)
```nginx
upstream bilgeik_backend {
    server backend:80;
}

upstream bilgeik_frontend {
    server frontend:3000;
}

server {
    listen 443 ssl http2;
    server_name yourdomain.com;
    
    location /api/ {
        proxy_pass http://bilgeik_backend;
    }
    
    location / {
        proxy_pass http://bilgeik_frontend;
    }
}
```

### Redis Cache (Opsiyonel)
```bash
# Redis'i etkinleştir
docker-compose --profile redis up -d
```

## 📋 Checklist

### Deployment Öncesi
- [ ] Server gereksinimleri karşılandı
- [ ] Domain/DNS ayarları tamamlandı
- [ ] SSL sertifikası hazırlandı
- [ ] .env dosyası konfigüre edildi
- [ ] Firewall ayarları yapıldı

### Deployment Sonrası
- [ ] Health check endpoints çalışıyor
- [ ] SSL sertifikası geçerli
- [ ] Database bağlantısı sağlam
- [ ] Backup sistemi kuruldu
- [ ] Monitoring aktif
- [ ] Log sistemi çalışıyor

## 🆘 Acil Durum Prosedürleri

### Sistem Durduğunda
1. Health check endpoint'lerini kontrol edin
2. Container durumlarını kontrol edin
3. Log dosyalarını inceleyin
4. Gerekirse rollback yapın

### Database Problemlerinde
1. Backup'tan restore yapın
2. Connection pool'u resetleyin
3. Database integrity'sini kontrol edin

### Güvenlik Sorunu Durumunda
1. Sistem erişimini kapatın
2. Log dosyalarını analiz edin
3. Güvenlik güncellemelerini uygulayın
4. Incident raporu oluşturun

---

## 📞 İletişim ve Destek

- **Teknik Destek**: support@yourcompany.com
- **Dokumentasyon**: https://docs.yourcompany.com
- **Issue Tracking**: https://github.com/yourcompany/bilgeik/issues

Bu doküman düzenli olarak güncellenmektedir. Son versiyon için repository'yi kontrol edin.