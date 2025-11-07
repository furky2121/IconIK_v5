#!/bin/bash

# IconIK İK Production Deployment Script
# Bu script production ortamına deployment yapmak için kullanılır

set -e

echo "🚀 IconIK İK Production Deployment Başlatılıyor..."

# Renk kodları
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Environment kontrolü
if [ ! -f ".env" ]; then
    echo -e "${RED}❌ .env dosyası bulunamadı!${NC}"
    echo -e "${YELLOW}💡 .env.example dosyasını kopyalayıp düzenleyin:${NC}"
    echo "cp .env.example .env"
    exit 1
fi

echo -e "${GREEN}✅ Environment dosyası bulundu${NC}"

# Docker kontrolü
if ! command -v docker &> /dev/null; then
    echo -e "${RED}❌ Docker kurulu değil!${NC}"
    exit 1
fi

if ! command -v docker-compose &> /dev/null; then
    echo -e "${RED}❌ Docker Compose kurulu değil!${NC}"
    exit 1
fi

echo -e "${GREEN}✅ Docker ve Docker Compose hazır${NC}"

# Mevcut container'ları durdur ve temizle
echo -e "${YELLOW}🔄 Mevcut container'lar durduruluyor...${NC}"
docker-compose down --remove-orphans

# Images'ları yeniden oluştur
echo -e "${YELLOW}🏗️  Docker images oluşturuluyor...${NC}"
docker-compose build --no-cache

# Database volume'unu kontrol et
echo -e "${YELLOW}🗄️  Database volume kontrol ediliyor...${NC}"
if docker volume ls | grep -q "postgres_data"; then
    echo -e "${GREEN}✅ Database volume mevcut${NC}"
else
    echo -e "${YELLOW}⚠️  Database volume oluşturuluyor...${NC}"
    docker volume create postgres_data
fi

# Production deployment
echo -e "${YELLOW}🚀 Production container'ları başlatılıyor...${NC}"
docker-compose up -d

# Health check
echo -e "${YELLOW}🔍 Health check yapılıyor...${NC}"
sleep 30

# Backend health check
if curl -f http://localhost:5000/health &> /dev/null; then
    echo -e "${GREEN}✅ Backend sağlıklı${NC}"
else
    echo -e "${RED}❌ Backend sağlık kontrolü başarısız${NC}"
    echo -e "${YELLOW}📋 Backend logları:${NC}"
    docker-compose logs backend
fi

# Frontend check
if curl -f http://localhost:3000 &> /dev/null; then
    echo -e "${GREEN}✅ Frontend erişilebilir${NC}"
else
    echo -e "${RED}❌ Frontend erişim kontrolü başarısız${NC}"
    echo -e "${YELLOW}📋 Frontend logları:${NC}"
    docker-compose logs frontend
fi

# Database bağlantı testi
echo -e "${YELLOW}🔍 Database bağlantısı test ediliyor...${NC}"
if docker-compose exec -T postgres pg_isready -U bilgeik -d IconIKdb &> /dev/null; then
    echo -e "${GREEN}✅ Database bağlantısı başarılı${NC}"
else
    echo -e "${RED}❌ Database bağlantı kontrolü başarısız${NC}"
    docker-compose logs postgres
fi

# Son durum raporu
echo -e "\n${GREEN}🎉 Deployment tamamlandı!${NC}"
echo -e "${YELLOW}📊 Durum Raporu:${NC}"
docker-compose ps

echo -e "\n${GREEN}🔗 Erişim Adresleri:${NC}"
echo -e "Frontend: http://localhost:3000"
echo -e "Backend API: http://localhost:5000"
echo -e "Database: localhost:5432"

echo -e "\n${YELLOW}📋 Kullanışlı Komutlar:${NC}"
echo -e "• Logları izlemek için: docker-compose logs -f"
echo -e "• Container'ları durdurmak için: docker-compose down"
echo -e "• Database backup için: ./scripts/backup.sh"
echo -e "• SSL sertifika kurulumu için: ./scripts/setup-ssl.sh"

echo -e "\n${GREEN}✨ IconIK İK Production'da çalışıyor!${NC}"