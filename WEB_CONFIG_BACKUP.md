# Web.config Backup - Çalışan Konfigürasyon

## ÖNEMLİ: Bu ayarları kaybetme!

Bu web.config ayarları 500.30 hatasını çözen çalışan konfigürasyondur.

### Çalışan Web.config İçeriği:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" arguments=".\manyasligida.dll" stdoutLogEnabled="true" stdoutLogFile=".\logs\stdout" hostingModel="OutOfProcess">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
        </environmentVariables>
      </aspNetCore>
    </system.webServer>
  </location>
</configuration>
```

### Kritik Ayarlar:

1. **`hostingModel="OutOfProcess"`** - Bu .NET 8.0 için gerekli
2. **`stdoutLogEnabled="true"`** - Log'ları görmek için
3. **`ASPNETCORE_ENVIRONMENT="Production"`** - Azure için gerekli

### Yapılmaması Gerekenler:

❌ `hostingModel="InProcess"` kullanma
❌ Gereksiz compression ayarları ekleme
❌ Custom error pages ekleme
❌ Security headers ekleme

### Eğer Proje Patlarsa:

1. Bu backup'tan web.config'i geri yükle
2. Azure Portal → App Service → Restart
3. Log stream'i kontrol et

### Azure App Service Konfigürasyonu:

**Connection Strings:**
```
Name: DefaultConnection
Value: Server=tcp:manyasligida-server.database.windows.net,1433;Initial Catalog=ManyasliGidaDB;Persist Security Info=False;User ID=manyasliadmin;Password=Ege753951456.;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=60;
```

**Application Settings:**
```
ASPNETCORE_ENVIRONMENT = Production
```

### Test Endpoints:

- `/health` - Basic health check
- `/health/startup` - Startup completion
- `/health/detailed` - Database connection test

### Son Güncelleme:
- Tarih: 8 Ağustos 2025
- Durum: 500.30 hatası çözüldü
- Web.config: Minimal ve stabil
