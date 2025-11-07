#!/bin/bash

# IconIK İK SSL Certificate Setup Script
# Bu script SSL sertifikalarını konfigüre eder

set -e

# Renk kodları
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

CERT_DIR="./certificates"
DOMAIN="${DOMAIN:-localhost}"

echo -e "${GREEN}🔒 IconIK İK SSL Setup${NC}"

# Sertifika dizinini oluştur
mkdir -p $CERT_DIR

echo -e "${YELLOW}🔑 SSL seçenekleri:${NC}"
echo "1) Let's Encrypt (Ücretsiz, otomatik yenileme)"
echo "2) Self-signed certificate (Development/Test)"
echo "3) Existing certificate files (Kendi sertifikanız)"
echo -n "Seçiminizi yapın (1-3): "
read -r choice

case $choice in
    1)
        echo -e "${YELLOW}📝 Let's Encrypt kurulumu${NC}"
        
        # Certbot kontrolü
        if ! command -v certbot &> /dev/null; then
            echo -e "${YELLOW}📦 Certbot kuruluyor...${NC}"
            
            # Ubuntu/Debian
            if command -v apt-get &> /dev/null; then
                sudo apt-get update
                sudo apt-get install -y certbot
            # CentOS/RHEL
            elif command -v yum &> /dev/null; then
                sudo yum install -y certbot
            else
                echo -e "${RED}❌ Certbot otomatik kurulum desteklenmiyor!${NC}"
                echo -e "${YELLOW}💡 Manuel kurulum: https://certbot.eff.org/instructions${NC}"
                exit 1
            fi
        fi
        
        echo -n "Domain adınızı girin: "
        read -r DOMAIN
        echo -n "Email adresinizi girin: "
        read -r EMAIL
        
        # Let's Encrypt sertifikası al
        sudo certbot certonly --standalone \
            --non-interactive \
            --agree-tos \
            --email $EMAIL \
            -d $DOMAIN
        
        # Sertifikaları kopyala
        sudo cp "/etc/letsencrypt/live/$DOMAIN/fullchain.pem" "$CERT_DIR/"
        sudo cp "/etc/letsencrypt/live/$DOMAIN/privkey.pem" "$CERT_DIR/"
        
        # PFX formatına çevir (ASP.NET Core için)
        openssl pkcs12 -export \
            -out "$CERT_DIR/aspnetapp.pfx" \
            -inkey "$CERT_DIR/privkey.pem" \
            -in "$CERT_DIR/fullchain.pem" \
            -passout pass:YourCertPassword
        
        echo -e "${GREEN}✅ Let's Encrypt sertifikası hazır!${NC}"
        ;;
        
    2)
        echo -e "${YELLOW}🏠 Self-signed certificate oluşturuluyor...${NC}"
        
        # Self-signed sertifika oluştur
        openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
            -keyout "$CERT_DIR/privkey.pem" \
            -out "$CERT_DIR/fullchain.pem" \
            -subj "/C=TR/ST=Istanbul/L=Istanbul/O=IconIK/CN=$DOMAIN"
        
        # PFX formatına çevir
        openssl pkcs12 -export \
            -out "$CERT_DIR/aspnetapp.pfx" \
            -inkey "$CERT_DIR/privkey.pem" \
            -in "$CERT_DIR/fullchain.pem" \
            -passout pass:YourCertPassword
        
        echo -e "${GREEN}✅ Self-signed certificate oluşturuldu!${NC}"
        echo -e "${YELLOW}⚠️  Bu sertifika sadece test amaçlıdır!${NC}"
        ;;
        
    3)
        echo -e "${YELLOW}📁 Mevcut sertifika dosyalarını kopyalayın:${NC}"
        echo "• fullchain.pem -> $CERT_DIR/"
        echo "• privkey.pem -> $CERT_DIR/"
        echo -n "Dosyalar hazır mı? (y/N): "
        read -r confirm
        
        if [[ $confirm =~ ^[Yy]$ ]]; then
            # Dosyaların varlığını kontrol et
            if [ ! -f "$CERT_DIR/fullchain.pem" ] || [ ! -f "$CERT_DIR/privkey.pem" ]; then
                echo -e "${RED}❌ Sertifika dosyları bulunamadı!${NC}"
                exit 1
            fi
            
            # PFX formatına çevir
            echo -n "PFX şifresi girin (boş bırakırsanız: YourCertPassword): "
            read -s pfx_password
            pfx_password=${pfx_password:-YourCertPassword}
            
            openssl pkcs12 -export \
                -out "$CERT_DIR/aspnetapp.pfx" \
                -inkey "$CERT_DIR/privkey.pem" \
                -in "$CERT_DIR/fullchain.pem" \
                -passout pass:$pfx_password
            
            echo -e "\n${GREEN}✅ Mevcut sertifika yapılandırıldı!${NC}"
        else
            echo -e "${YELLOW}⏭️  İşlem iptal edildi${NC}"
            exit 0
        fi
        ;;
        
    *)
        echo -e "${RED}❌ Geçersiz seçim!${NC}"
        exit 1
        ;;
esac

# Docker Compose için SSL konfigürasyonu
echo -e "${YELLOW}🐳 Docker Compose SSL konfigürasyonu güncelleniyor...${NC}"

# docker-compose.override.yml oluştur
cat > docker-compose.override.yml << EOF
version: '3.8'

services:
  backend:
    environment:
      - ASPNETCORE_URLS=https://+:443;http://+:80
      - ASPNETCORE_HTTPS_PORT=443
      - ASPNETCORE_Kestrel__Certificates__Default__Password=YourCertPassword
      - ASPNETCORE_Kestrel__Certificates__Default__Path=/app/certificates/aspnetapp.pfx
    volumes:
      - ./certificates:/app/certificates:ro
    ports:
      - "443:443"
      - "80:80"
      
  frontend:
    environment:
      - NEXT_PUBLIC_API_BASE_URL=https://$DOMAIN/api
      - NEXT_PUBLIC_FILE_BASE_URL=https://$DOMAIN
EOF

echo -e "${GREEN}✅ SSL konfigürasyonu tamamlandı!${NC}"
echo -e "\n${YELLOW}📋 Sonraki adımlar:${NC}"
echo -e "1. .env dosyasındaki URL'leri HTTPS olacak şekilde güncelleyin"
echo -e "2. Docker Compose'u yeniden başlatın: docker-compose down && docker-compose up -d"
echo -e "3. Firewall'da 443 (HTTPS) portunu açın"
echo -e "4. DNS ayarlarınızın doğru yapılandırıldığından emin olun"

echo -e "\n${GREEN}🔒 SSL kurulumu tamamlandı!${NC}"