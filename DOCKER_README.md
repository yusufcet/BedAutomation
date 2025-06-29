# BedAutomation Docker Kurulumu

Bu kılavuz, BedAutomation projesini Docker ile nasıl çalıştıracağınızı açıklar.

## Gereksinimler

- Docker Desktop (Windows/Mac) veya Docker Engine (Linux)
- Docker Compose

## Kurulum ve Çalıştırma

### 1. Projeyi Docker ile Başlatma

```bash
# Tüm servisleri ayağa kaldır (migration'lar otomatik çalışır)
docker-compose up -d

# Veya logları görmek için
docker-compose up
```

### 2. Servislerin Durumunu Kontrol Etme

```bash
# Çalışan servisleri göster
docker-compose ps

# Servislerin loglarını görüntüle
docker-compose logs

# Belirli bir servisin loglarını görüntüle
docker-compose logs web
docker-compose logs postgres
```

### 3. Uygulamaya Erişim

- **Web Uygulaması**: http://localhost:8080
- **PostgreSQL**: localhost:5432
  - Database: BedAutomation
  - Username: postgres
  - Password: postgres

### 4. Database Migration'ları

**Migration'lar otomatik olarak çalışır!** Uygulama başlamadan önce:

1. PostgreSQL'in hazır olmasını bekler
2. Entity Framework migration'larını otomatik uygular  
3. Başarılı olursa uygulamayı başlatır

Manuel migration çalıştırmanıza gerek yoktur.

### 5. Servisleri Durdurma

```bash
# Servisleri durdur
docker-compose stop

# Servisleri durdur ve container'ları sil
docker-compose down

# Servisleri durdur, container'ları ve volume'ları sil
docker-compose down -v
```

## Dosya Yapısı

```
├── Dockerfile              # ASP.NET Core uygulaması için
├── docker-compose.yml      # Docker Compose yapılandırması
├── .dockerignore           # Docker build'de ignore edilecek dosyalar
├── logs/                   # Uygulama logları (volume)
└── init-scripts/           # PostgreSQL başlangıç script'leri (opsiyonel)
```

## Başlatma Sırası

1. **PostgreSQL servisi başlar**
2. **Web uygulaması başlar** ve startup sırasında otomatik olarak:
   - PostgreSQL'in hazır olmasını bekler (retry logic ile)
   - Entity Framework migration'larını uygular
   - Seed data'yı yükler
   - Web sunucusunu başlatır
3. **Connection string** otomatik olarak Docker network'ü kullanır (`Host=postgres`)

## Troubleshooting

### Port Çakışması
Eğer 8080 veya 5432 portları kullanılıyorsa, `docker-compose.yml` dosyasında port mapping'leri değiştirebilirsiniz:

```yaml
ports:
  - "8081:8080"  # 8080 yerine 8081 kullan
```

### Database Bağlantı Problemi
Eğer web uygulaması database'e bağlanamıyorsa:

```bash
# PostgreSQL container'ının çalıştığını kontrol et
docker-compose logs postgres

# Web servisinin migration loglarını kontrol et
docker-compose logs web
```

### Migration Problemi

Migration'lar başarısız olursa:

```bash
# Web container'ının loglarını kontrol et
docker-compose logs web

# Container'ı yeniden başlatarak migration'ı tekrar dene
docker-compose restart web

# Veya container'ı durdurup temiz başlat
docker-compose down
docker-compose up -d
```

### Container'ları Yeniden Build Etme

```bash
# Cache kullanmadan yeniden build et
docker-compose build --no-cache

# Build ettikten sonra başlat
docker-compose up -d
```

### Logs Klasörü İzinleri

Windows'ta logs klasörü izin problemi yaşarsanız:

```bash
# Logs klasörünü manuel oluşturun
mkdir logs
```

## Development Ortamı

Development sırasında kod değişikliklerini görmek için volume mount ekleyebilirsiniz:

```yaml
# docker-compose.override.yml oluşturun
version: '3.8'
services:
  web:
    volumes:
      - ./BedAutomation:/app/src
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
```

## Güvenlik Notları

- Production ortamında PostgreSQL şifresini değiştirin
- Environment variable'ları `.env` dosyası ile yönetin
- HTTPS sertifikası ekleyin

## Önemli Özellikler

✅ **Otomatik Migration**: Uygulama başlangıcında migration'lar otomatik çalışır  
✅ **Retry Logic**: PostgreSQL hazır olana kadar bekleme ve tekrar deneme  
✅ **Auto Restart**: Hata durumunda otomatik yeniden başlar  
✅ **Volume Persistence**: PostgreSQL verileri kalıcı olarak saklanır  
✅ **Network Isolation**: Servisler güvenli Docker network'ünde çalışır  
✅ **Temiz Mimari**: Migration'lar uygulama kodu içinde, SDK bağımlılığı yok 