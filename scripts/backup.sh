#!/bin/bash

# IconIK İK Database Backup Script
# Bu script PostgreSQL veritabanının yedeğini alır

set -e

# Konfigürasyon
BACKUP_DIR="./backups"
DATE=$(date +%Y%m%d_%H%M%S)
DB_NAME="${DB_NAME:-IconIKdb}"
DB_USER="${DB_USER:-bilgeik}"
BACKUP_FILE="$BACKUP_DIR/bilgeik_backup_$DATE.sql"

# Renk kodları
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo -e "${GREEN}🗄️  IconIK İK Database Backup${NC}"

# Backup dizinini oluştur
mkdir -p $BACKUP_DIR

# Environment dosyasını kontrol et
if [ -f ".env" ]; then
    source .env
else
    echo -e "${YELLOW}⚠️  .env dosyası bulunamadı, varsayılan değerler kullanılıyor${NC}"
fi

# Docker container'ın çalışıp çalışmadığını kontrol et
if ! docker-compose ps | grep postgres | grep -q "Up"; then
    echo -e "${RED}❌ PostgreSQL container çalışmıyor!${NC}"
    echo -e "${YELLOW}💡 Container'ı başlatmak için: docker-compose up -d postgres${NC}"
    exit 1
fi

echo -e "${YELLOW}📦 Backup alınıyor...${NC}"

# Database backup
docker-compose exec -T postgres pg_dump -U $DB_USER -d $DB_NAME --clean --if-exists > $BACKUP_FILE

if [ -f "$BACKUP_FILE" ] && [ -s "$BACKUP_FILE" ]; then
    # Dosya boyutunu al
    FILE_SIZE=$(du -h $BACKUP_FILE | cut -f1)
    echo -e "${GREEN}✅ Backup başarıyla oluşturuldu!${NC}"
    echo -e "${YELLOW}📁 Dosya: $BACKUP_FILE${NC}"
    echo -e "${YELLOW}📏 Boyut: $FILE_SIZE${NC}"
    
    # Backup'ı sıkıştır
    gzip $BACKUP_FILE
    COMPRESSED_SIZE=$(du -h $BACKUP_FILE.gz | cut -f1)
    echo -e "${GREEN}🗜️  Backup sıkıştırıldı: $BACKUP_FILE.gz${NC}"
    echo -e "${YELLOW}📏 Sıkıştırılmış boyut: $COMPRESSED_SIZE${NC}"
    
    # Eski backup'ları temizle (30 günden eski olanları)
    find $BACKUP_DIR -name "bilgeik_backup_*.sql.gz" -mtime +30 -delete
    echo -e "${GREEN}🧹 30 günden eski backup'lar temizlendi${NC}"
    
    # Backup listesi
    echo -e "\n${YELLOW}📋 Mevcut backup'lar:${NC}"
    ls -lh $BACKUP_DIR/bilgeik_backup_*.sql.gz 2>/dev/null || echo "Henüz backup yok"
    
else
    echo -e "${RED}❌ Backup oluşturulamadı!${NC}"
    exit 1
fi

echo -e "\n${GREEN}✨ Database backup tamamlandı!${NC}"