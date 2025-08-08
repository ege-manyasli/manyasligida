# Manyaslı Gıda - Sorun Giderme Rehberi

## Yaygın Sorunlar ve Çözümleri

### 1. Veritabanı Bağlantı Sorunları

**Belirtiler:**
- "Database connection failed" hataları
- Yavaş sayfa yüklemeleri
- Timeout hataları

**Çözümler:**
- Azure SQL Database bağlantı dizesini kontrol edin
- Firewall ayarlarını kontrol edin
- Connection pooling ayarlarını optimize edin

### 2. Performans Sorunları

**Belirtiler:**
- Yavaş sayfa yüklemeleri
- Yüksek CPU kullanımı
- Memory kullanımı artışı

**Çözümler:**
- Response caching'i etkinleştirin
- Database sorgularını optimize edin
- Static dosyaları CDN'e taşıyın

### 3. Log Analizi

**Log Dosyaları:**
- Azure Portal > App Service > Logs
- Application Insights (varsa)
- Kusto sorguları ile analiz

**Önemli Log Seviyeleri:**
- Information: Normal işlemler
- Warning: Potansiyel sorunlar
- Error: Hatalar

### 4. Monitoring Endpoints

**Health Check:**
```
GET /health
GET /api/health/detailed
```

**Admin Panel:**
```
GET /Admin/Performance
```

### 5. Cache Yönetimi

**Response Cache:**
- Home page: 5 dakika
- About/Privacy: 10 dakika
- Static dosyalar: 1 yıl

**Cache Temizleme:**
- Browser cache'ini temizleyin
- CDN cache'ini temizleyin (varsa)

### 6. Güvenlik

**Headers:**
- X-Content-Type-Options: nosniff
- X-Frame-Options: SAMEORIGIN
- X-XSS-Protection: 1; mode=block

### 7. Deployment Sorunları

**Build Hataları:**
- NuGet paketlerini güncelleyin
- .NET Core sürümünü kontrol edin
- Dependencies'leri kontrol edin

**Runtime Hataları:**
- Environment variables'ları kontrol edin
- Connection string'leri kontrol edin
- File permissions'ları kontrol edin

### 8. Performance Monitoring

**Metrikler:**
- Request times
- Database query times
- Error rates
- Memory usage

**Alerting:**
- Slow requests (>1s)
- Database errors
- High error rates

### 9. Troubleshooting Commands

**Azure CLI:**
```bash
# Log stream
az webapp log tail --name manyasligida-web --resource-group your-rg

# Restart app
az webapp restart --name manyasligida-web --resource-group your-rg

# Check status
az webapp show --name manyasligida-web --resource-group your-rg
```

### 10. Emergency Procedures

**Uygulama Çökerse:**
1. Azure Portal'dan restart edin
2. Logs'ları kontrol edin
3. Database bağlantısını test edin
4. Health check endpoint'lerini kontrol edin

**Veritabanı Sorunları:**
1. Connection string'i kontrol edin
2. Firewall ayarlarını kontrol edin
3. Database service tier'ını kontrol edin

### 11. Performance Optimization

**Database:**
- Index'leri optimize edin
- Query'leri optimize edin
- Connection pooling kullanın

**Application:**
- Response caching kullanın
- Static file compression
- CDN kullanın

**Monitoring:**
- Application Insights ekleyin
- Custom metrics toplayın
- Alert'ler kurun

### 12. Contact Information

**Destek:**
- Email: info@manyasligida.com
- Phone: +90 266 123 45 67

**Emergency:**
- Azure Support (premium plan gerekli)
- Database Administrator
- System Administrator
