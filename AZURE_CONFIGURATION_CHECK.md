# 🔧 Azure App Service Configuration Kontrolü

## Sorun
- Database bağlantı hatası
- İçerik yüklenmiyor
- Publish başarısız

## ✅ Çözüm: Azure Portal Configuration Kontrolü

### 1. Azure Portal → App Service → Configuration

**Application Settings'de şunları kontrol edin:**

#### Connection String:
```
Name: DefaultConnection
Type: SQLAzure
Value: Server=tcp:manyasligida-server.database.windows.net,1433;Initial Catalog=ManyasliGidaDB;Persist Security Info=False;User ID=manyasliadmin;Password=Ege753951456.;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=60;
```

#### Application Settings:
```
ASPNETCORE_ENVIRONMENT = Production
```

### 2. Eğer Connection String Yoksa:

1. **Configuration** → **Connection strings**
2. **+ New connection string** tıklayın
3. **Name**: `DefaultConnection`
4. **Type**: `SQLAzure`
5. **Value**: Yukarıdaki connection string'i yapıştırın
6. **Save** → **Restart**

### 3. Database Firewall Kontrolü:

1. **Azure Portal** → **SQL Database** → **manyasligida-server**
2. **Connection security** → **Networking**
3. **Firewall rules** kontrol edin
4. **Allow Azure services and resources to access this server** = **Yes**

### 4. Test Etme:

Publish tamamlandıktan sonra:
1. **Azure Portal** → **App Service** → **Restart**
2. 30 saniye bekle
3. Siteyi test et: https://manyasligida-web-new-gyaygrckfpbtg7ey.westeurope-01.azurewebsites.net/

## 🚨 Acil Durum

Eğer hala çalışmıyorsa:
1. **Azure Portal** → **App Service** → **Log stream**
2. Hata mesajlarını kopyalayın
3. Bu dosyaya ekleyin

## 📋 Kontrol Listesi

- [ ] Connection string doğru mu?
- [ ] ASPNETCORE_ENVIRONMENT = Production mu?
- [ ] Database firewall açık mı?
- [ ] App Service restart yapıldı mı?
- [ ] Publish başarılı mı?
