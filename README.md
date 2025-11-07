# 🏢 IconIK İK Yönetim Sistemi

Modern ve kapsamlı İnsan Kaynakları yönetim sistemi.

## 🎯 Özellikler

- 👥 **Personel Yönetimi**: Tam personel CRUD işlemleri
- 📋 **Organizasyon Şeması**: Görsel organizasyon yapısı
- 🏖️ **İzin Yönetimi**: Çok seviyeli onay sistemi
- 📚 **Eğitim Yönetimi**: Eğitim planlaması ve takibi
- 💰 **Bordro Sistemi**: Maaş hesaplama ve raporlama
- 📊 **Dashboard & Raporlar**: Gerçek zamanlı analitik
- 🔐 **JWT Authentication**: Güvenli kullanıcı yönetimi
- 📱 **Responsive Design**: Mobil uyumlu arayüz

## 🚀 Canlı Demo

- **Frontend**: [Vercel'de Yayında](https://your-app.vercel.app)
- **Backend API**: [Render'da Yayında](https://your-api.onrender.com)

## 👨‍💼 Demo Hesapları

| Rol | Kullanıcı Adı | Şifre | Yetkiler |
|-----|---------------|--------|----------|
| Genel Müdür | `ahmet.yilmaz` | `8901` | Tüm yetkiler |
| İK Direktörü | `mehmet.kaya` | `8902` | İK modülleri |
| İK Uzmanı | `ozcan.bulut` | `8912` | Sınırlı yetkiler |

## 🏆 **Teknoloji Stack**
- **Backend**: ASP.NET Core 8.0 Web API + Entity Framework Core
- **Database**: PostgreSQL 14+ (İlişkisel veritabanı)
- **Frontend**: Next.js 13.4 + React 18 + PrimeReact (Sakai Theme)
- **Authentication**: JWT Token-based güvenlik
- **UI/UX**: Responsive, modern, kullanıcı dostu arayüz

## 📋 Sistem Özellikleri

## 🎯 **Tamamlanan Tüm Modüller**

### 🏢 **1. Organizasyon Yönetimi** (✅ Tamamlandı)
- **Kademeler**: 9 seviyeli hiyerarşik kademe yönetimi
- **Departmanlar**: Departman CRUD operasyonları
- **Pozisyonlar**: Pozisyon tanımlama ve maaş aralığı yönetimi
- **İlişkisel Yapı**: Kademe → Departman → Pozisyon hiyerarşisi

### 👥 **2. Personel Yönetimi** (✅ Tamamlandı)
- **Personel CRUD**: Kapsamlı personel bilgi yönetimi
- **Otomatik Kullanıcı Oluşturma**: Personel kaydında otomatik kullanıcı hesabı
- **Türkçe Karakter Dönüşümü**: Kullanıcı adlarında otomatik dönüşüm
- **Yönetici Hiyerarşisi**: Organizasyonel yapıya uygun yönetici ataması
- **Fotoğraf Yükleme**: Avatar yönetim sistemi
- **Aktif/Pasif Durum**: Personel aktif/pasif durumu yönetimi

### 🗓️ **3. İzin Yönetimi** (✅ Tamamlandı)
- **İzin Hakları Hesaplama**: Otomatik yıllık izin hesaplaması (14 gün)
- **Hiyerarşik Onay Sistemi**: Çok seviyeli yönetici onayı
- **İzin Çakışma Kontrolü**: Tarih çakışma önleme
- **Hafta Sonu Hariç Tutma**: Akıllı gün sayısı hesaplama
- **İzin Takvimi**: Görsel izin takip sistemi

### 📚 **4. Eğitim Yönetimi** (✅ Tamamlandı)
- **Eğitim Planlaması**: Kapsamlı eğitim programı yönetimi
- **Personel Atama**: Esnek katılımcı yönetimi
- **Kapasite Kontrolü**: Eğitim kapasitesi takibi
- **Puan ve Sertifika**: Katılımcı değerlendirme sistemi
- **Durum Takibi**: Eğitim durumu yönetimi

### 💰 **5. Bordro Yönetimi** (✅ Tamamlandı)
- **Bordro CRUD**: Detaylı bordro yönetimi
- **Otomatik Hesaplama**: SGK (%14) ve vergi kesintisi hesaplama
- **Toplu Bordro**: Departman bazlı toplu bordro oluşturma
- **İstatistik Raporları**: Departman bazlı maaş analizleri
- **Dönemsel Takip**: Aylık bordro dönem yönetimi

### 📊 **6. Dashboard ve Raporlama** (✅ Tamamlandı)
- **Genel İstatistikler**: Personel, izin, eğitim özet verileri
- **Trend Analizleri**: Personel giriş-çıkış trendleri
- **Departman Analizleri**: Departman bazlı karşılaştırmalar
- **İzin Analizi**: İzin onay oranları ve trendleri
- **Maaş Analizleri**: Maaş dağılımı ve trend verileri
- **Eğitim Başarı Oranları**: Eğitim etkinliği metrikleri

### 👤 **7. Kullanıcı ve Güvenlik** (✅ Tamamlandı)
- **Profil Düzenleme**: Kişisel bilgi güncellemeleri
- **Şifre Değiştirme**: Güvenli şifre yönetimi
- **Fotoğraf Yükleme**: Avatar yönetimi
- **İzin Detayları**: Kişisel izin bakiye takibi
- **Eğitim Geçmişi**: Kişisel eğitim kayıtları

### ⚙️ **8. Sistem Ayarları ve Yetki Yönetimi** (✅ YENİ EKLENEN)
- **Ekran Yetkileri**: Sistem modüllerinin yetki tanımları
- **Kademe Yetkileri**: Rol bazlı CRUD yetkileri matrisi
- **Yetki Düzenleme**: Kademe seviyelerine göre esnek yetki ataması
- **Varsayılan Yetkiler**: Otomatik yetki matrisi oluşturma
- **Yetki Kontrolü**: Her ekran için okuma/yazma/güncelleme/silme yetkileri

## 🏗️ Teknik Mimari

### Backend (ASP.NET Core 8.0)
```
backend/IconIK.API/
├── Controllers/           # API Controller'ları
│   ├── KademeController.cs
│   ├── DepartmanController.cs  
│   ├── PozisyonController.cs
│   ├── PersonelController.cs
│   ├── IzinTalebiController.cs
│   ├── EgitimController.cs
│   ├── BordroController.cs
│   ├── DashboardController.cs
│   ├── ProfilController.cs
│   ├── YetkiController.cs       # YENİ: Yetki yönetimi
│   └── FileUploadController.cs  # YENİ: Dosya yükleme
├── Models/               # Entity modelleri
│   ├── Kademe.cs
│   ├── Departman.cs
│   ├── Pozisyon.cs
│   ├── Personel.cs
│   ├── Kullanici.cs
│   ├── IzinTalebi.cs
│   ├── Egitim.cs
│   ├── PersonelEgitimi.cs
│   ├── Bordro.cs
│   ├── EkranYetkisi.cs          # YENİ: Ekran yetkileri
│   └── KademeEkranYetkisi.cs    # YENİ: Kademe-ekran yetki matrisi
├── Data/                # Entity Framework Context
│   └── IconIKContext.cs
├── Services/            # İş mantığı servisları
│   ├── UserService.cs
│   └── IzinService.cs
└── SQL/                # Veritabanı script'leri
    └── IconIKdb_Setup.sql
```

### Frontend (React + PrimeReact)
```
frontend/src/
├── components/          # React bileşenleri
├── pages/              # Sayfa bileşenleri
│   ├── Dashboard.js
│   ├── Kademeler.js
│   ├── Departmanlar.js
│   ├── Pozisyonlar.js
│   ├── Personeller.js
│   ├── IzinTalepleri.js
│   ├── IzinTakvimi.js
│   ├── Egitimler.js
│   ├── Bordrolar.js
│   ├── Profil.js
│   └── Ayarlar.js              # YENİ: Sistem ayarları ve yetki yönetimi
├── layout/             # Layout bileşenleri
├── services/           # API servis katmanı
└── styles/            # CSS dosyaları
```

### Veritabanı (PostgreSQL)
- **11 Ana Tablo**: İlişkisel veri yapısı (2 yeni yetki tablosu eklendi)
- **Foreign Key İlişkileri**: Referans bütünlüğü
- **Index Optimizasyonları**: Performans
- **Sample Data**: Test verisi
- **Yetki Tabloları**: EkranYetkisi ve KademeEkranYetkisi

## 🚀 **Hızlı Başlangıç Kılavuzu**

### **📋 Gereksinimler**
- **Backend**: .NET 8.0 SDK
- **Database**: PostgreSQL 14+
- **Frontend**: Node.js 18+ (LTS önerilir)
- **IDE**: Visual Studio/VS Code/JetBrains Rider

### **🔧 1. Veritabanı Kurulumu (İlk adım)**
```bash
# PostgreSQL'e bağlan ve veritabanı oluştur
psql -U postgres
CREATE DATABASE "IconIKdb";
\q

# Demo verilerle birlikte tabloları oluştur
psql -d IconIKdb -f IconIKdb_Setup.sql
```

### **🖥️ 2. Backend API Başlatma**
```bash
cd backend/IconIK.API
dotnet restore
dotnet build
dotnet run  # http://localhost:5000 adresinde çalışır
```

### **🌐 3. Frontend Başlatma**
```bash
cd frontend
npm install
npm run dev  # http://localhost:3000 adresinde çalışır
```

### **🎯 4. Demo Sisteme Giriş**
Tarayıcıda `http://localhost:3000` adresine gidin ve aşağıdaki demo hesaplarla giriş yapın:

## 🔐 **Demo Hesapları** (Sistem örnekleri)

| **Kademe** | **Kullanıcı Adı** | **Şifre** | **Yetkileri** |
|------------|-------------------|-----------|---------------|
| **Genel Müdür** | `ahmet.yilmaz` | `8901` | Tüm sistem erişimi, tüm onaylar |
| **İK Direktörü** | `mehmet.kaya` | `8902` | İK süreçleri, departman yönetimi |
| **BIT Direktörü** | `ali.demir` | `8903` | BIT departmanı yönetimi |
| **İK Müdürü** | `zeynep.arslan` | `8907` | İK operasyonları, ekip yönetimi |
| **İK Uzmanı** | `ozcan.bulut` | `8912` | İK işlemleri, kendi izinleri |

> **💡 İpucu**: Şifreler personelin TC kimlik numarasının son 4 hanesidir. İlk girişte şifre değiştirme zorunludur.

## 🎯 **Temel İş Akışları** (Adım adım kullanım)

### 🆕 **Yeni Personel İşe Alımı**
```
1️⃣ Organizasyon → Pozisyonlar → "Yeni Pozisyon" oluştur
2️⃣ Personel Yönetimi → Personeller → "Yeni Personel" ekle
   → Otomatik kullanıcı hesabı oluşur (örn: ali.yilmaz şifre: 1234)
3️⃣ Yönetici ataması → Hiyerarşi kurulur
4️⃣ İzin hakları → Otomatik 14 gün/yıl hesaplanır
```

### 📅 **İzin Talep Süreci**
```
1️⃣ Personel → İzin Talepleri → "Yeni Talep" oluştur
2️⃣ Sistem → Hafta sonu hariç gün sayısı hesaplar
3️⃣ Sistem → Çakışma kontrolü yapar
4️⃣ Yönetici → Bekleyen talebi onaylar/reddeder
5️⃣ İzin Takvimi → Onaylanan izinler görüntülenir
```

### 🎓 **Eğitim Yönetimi**
```
1️⃣ Eğitimler → "Yeni Eğitim" planla (kapasite belirle)
2️⃣ Personel Ata → Çoklu seçim ile katılımcı ata
3️⃣ Eğitim başladığında → Katılım durumunu güncelle
4️⃣ Eğitim bitiminde → Puan ver ve sertifika ekle
5️⃣ Raporlar → Başarı oranlarını görüntüle
```

### ⚙️ **Sistem Ayarları ve Yetki Yönetimi** (YENİ)
```
1️⃣ Ayarlar → "Varsayılan Yetkileri Oluştur" tıkla
2️⃣ Sistem → Tüm ekranlar için yetki tanımları oluşturur
3️⃣ Kademe Yetkileri → Her kademe için ekran yetkilerini düzenle
4️⃣ Yetki Matrisi → Okuma/Yazma/Güncelleme/Silme yetkilerini ayarla
5️⃣ Otomatik Atama → Üst kademeler daha fazla yetkiye sahip
```

### 💰 **Bordro İşlemleri**
```
1️⃣ Bordrolar → "Toplu Bordro Oluştur" seç
2️⃣ Dönem (yıl/ay) ve departman seç
3️⃣ Sistem → SGK (%14) ve vergi kesintilerini hesaplar
4️⃣ Ek ödemeler (prim/mesai) manuel ekle
5️⃣ Dashboard → Maaş istatistiklerini görüntüle
```

## 🏆 **Sistem Özellikleri** (Tümü çalışır durumda)

### 🧠 **Akıllı İzin Sistemi**
```
✅ Otomatik yıllık izin hesaplaması (işe başlama tarihine göre)
✅ Hiyerarşik çok seviyeli onay sistemi
✅ Tarih çakışma önleme (aynı personel için)
✅ Hafta sonu akıllı hesaplama (sadece iş günleri)
✅ Real-time izin bakiye takibi
✅ İzin takvimi görsel görünümü
```

### 🎓 **Kapsamlı Eğitim Yönetimi**
```
✅ Kapasite kontrolü (max katılımcı sayısı)
✅ Esnek katılımcı yönetimi (atama/çıkarma)
✅ Puan sistemi (1-100 arası değerlendirme)
✅ Dijital sertifika yönetimi (URL bazlı)
✅ Eğitim durumu takibi (planlandı/tamamlandı/iptal)
✅ Departman bazlı başarı raporları
```

### 💰 **Gelişmiş Bordro Sistemi**
```
✅ Otomatik SGK kesintisi (%14)
✅ Gelir vergisi hesaplama (basamaklı vergi)
✅ Toplu bordro oluşturma (departman bazlı)
✅ Prim ve mesai ödemeleri
✅ Dönemsel maaş trend analizleri
✅ Net/brüt maaş otomatik hesaplama
```

### 📊 **Analitik Dashboard**
```
✅ Real-time personel istatistikleri
✅ İzin onay oranları ve trendleri
✅ Eğitim başarı metrikleri
✅ Maaş dağılımı analizleri
✅ Departman karşılaştırmaları
✅ Chart.js ile görsel grafikler
```

### ⚙️ **Yetki Yönetim Sistemi** (YENİ EKLENEN)
```
✅ Ekran bazlı yetki tanımları (10 ana modül)
✅ Kademe seviyesine göre CRUD yetkileri
✅ Esnek yetki matrisi düzenleme
✅ Varsayılan yetki ataması (hiyerarşik)
✅ Yetki durumu takibi (aktif/pasif)
✅ Ayarlar sayfasında sekmeli yönetim arayüzü
```

## 🔐 Güvenlik Özellikleri
- **JWT Authentication**: Token tabanlı kimlik doğrulama
- **Password Hashing**: SHA256 şifreleme
- **Role-based Access**: Kademe bazlı rol yetkilendirme sistemi
- **Screen-Level Permissions**: Ekran bazlı CRUD yetki kontrolü
- **Input Validation**: Kapsamlı veri doğrulama
- **SQL Injection Protection**: Parameterized queries
- **Permission Matrix**: Detaylı yetki matrisi yönetimi

## 🎨 Kullanıcı Arayüzü
- **Responsive Design**: Mobil uyumlu
- **PrimeReact Components**: Modern UI bileşenleri
- **Toast Notifications**: Kullanıcı geri bildirimleri
- **Data Tables**: Gelişmiş tablo özellikleri
- **Charts & Graphs**: Görsel veri sunumu
- **Dark/Light Theme**: Tema desteği

## 📈 Performans
- **Entity Framework**: Optimized queries
- **Lazy Loading**: İhtiyaç halinde yükleme  
- **Pagination**: Sayfalama desteği
- **Caching**: Önbellekleme stratejileri
- **Index Optimization**: Veritabanı performansı

## 🛠️ Geliştirme Notları

### API Endpoints (Örnekler)
```
GET    /api/Personel                    # Tüm personeller
POST   /api/Personel                    # Yeni personel
GET    /api/IzinTalebi/Onaylar/5        # Onay bekleyen talepler
POST   /api/Egitim/PersonelAta/5        # Eğitime personel ata
GET    /api/Dashboard/Genel             # Genel istatistikler
GET    /api/Yetki/EkranYetkileri        # YENİ: Ekran yetkileri
GET    /api/Yetki/KademeYetkileri       # YENİ: Kademe yetkileri
POST   /api/Yetki/DefaultEkranYetkileri # YENİ: Varsayılan yetkiler
```

### Veritabanı İlişkileri
```
Kademe (1) → (N) Pozisyon
Kademe (1) → (N) KademeEkranYetkisi     # YENİ: Yetki matrisi
Departman (1) → (N) Pozisyon  
Pozisyon (1) → (N) Personel
Personel (1) → (1) Kullanici
Personel (1) → (N) IzinTalebi
Personel (N) → (M) Egitim (PersonelEgitimi ile)
EkranYetkisi (1) → (N) KademeEkranYetkisi # YENİ: Ekran yetkileri
```

## 📝 Lisans
Bu proje MIT lisansı altında geliştirilmiştir.

## 👨‍💻 Geliştirici
IconIK İK Yönetim Sistemi v1.0
Geliştirme Tarihi: 2024

---

**Not**: Bu sistem kapsamlı bir İK yönetim çözümüdür ve gerçek üretim ortamında kullanılmadan önce güvenlik ve performans testlerinden geçirilmelidir.# Force deployment
