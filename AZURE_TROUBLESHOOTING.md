# 🔍 Azure App Service Sorun Giderme - Sayfa Açılmıyor

## Problem
- ✅ Dün site çalışıyordu
- ❌ Şimdi sadece "yükleniyor" diyor
- ❌ Sayfa açılmıyor

## 🚨 Acil Kontrol Adımları

### 1. Azure Portal Log Stream Kontrolü
```
Azure Portal → App Service → Log stream
```

**Beklenen Hatalar:**
- Database connection errors
- Startup errors
- Configuration errors
- 500.30 errors

### 2. Azure App Service Restart
```
Azure Portal → App Service → Overview → Restart
```

### 3. Configuration Kontrolü
```
Azure Portal → App Service → Configuration → Application settings
```

**Gerekli Ayarlar:**
```
ASPNETCORE_ENVIRONMENT = Production
```

**Connection String Kontrolü:**
```
Name: DefaultConnection
Value: Server=tcp:manyasligida-server.database.windows.net,1433;Initial Catalog=ManyasliGidaDB;...
```

### 4. Database Bağlantı Testi
```
Azure Portal → SQL Database → Query editor
```

## 🔧 Olası Çözümler

### Çözüm 1: App Service Restart
1. Azure Portal → App Service
2. **Restart** butonuna tıkla
3. 30 saniye bekle
4. Siteyi test et

### Çözüm 2: Configuration Düzeltme
1. Azure Portal → App Service → Configuration
2. **Application settings** kontrol et
3. `ASPNETCORE_ENVIRONMENT = Production` olduğundan emin ol
4. **Save** → **Restart**

### Çözüm 3: Database Bağlantı
1. Azure Portal → SQL Database
2. **Connection security** kontrol et
3. **Firewall rules** kontrol et
4. App Service IP'sinin izinli olduğundan emin ol

### Çözüm 4: Log Stream Analizi
1. Azure Portal → App Service → Log stream
2. Hata mesajlarını kopyala
3. Bu dosyaya ekle

## 📋 Kontrol Listesi

- [ ] Azure Portal → App Service → Restart
- [ ] Log stream'de hata var mı?
- [ ] Configuration ayarları doğru mu?
- [ ] Database bağlantısı çalışıyor mu?
- [ ] Firewall kuralları doğru mu?

## 🆘 Acil Durum

Eğer hiçbiri çalışmazsa:
1. **Azure Portal → App Service → Stop**
2. 1 dakika bekle
3. **Start** yap
4. Test et

## 📞 Hata Mesajları

Lütfen log stream'den aldığınız hata mesajlarını buraya ekleyin:
```
[HAYIR HATA MESAJI]
```
