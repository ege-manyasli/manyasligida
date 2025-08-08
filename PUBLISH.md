# Manyaslı Gıda - Publish Ayarları

## 📋 Publish Profilleri

### 1. Azure Publish (manyasligida-web-new)
- **Hedef**: Azure Web App
- **URL**: https://manyasligida-web-new-gyaygrckfpbtg7ey.westeurope-01.azurewebsites.net
- **Profil**: `Properties\PublishProfiles\manyasligida-web-new - Web Deploy.pubxml`

### 2. Yerel Klasör Publish
- **Hedef**: Yerel klasör
- **Klasör**: `bin\Release\net8.0\publish\`
- **Profil**: `Properties\PublishProfiles\LocalFolder.pubxml`

## 🚀 Publish Komutları

### PowerShell Script ile (Önerilen)
```powershell
# Azure'a publish
.\publish.ps1 -Target Azure

# Yerel klasöre publish
.\publish.ps1 -Target Local
```

### Manuel Komutlar
```bash
# Azure'a publish
dotnet publish --configuration Release --publish-profile "Properties\PublishProfiles\manyasligida-web-new - Web Deploy.pubxml"

# Yerel klasöre publish
dotnet publish --configuration Release --publish-profile "Properties\PublishProfiles\LocalFolder.pubxml"
```

## ⚙️ Publish Ayarları

### Proje Ayarları (manyasligida.csproj)
- **Runtime**: win-x64
- **SelfContained**: false
- **PublishReadyToRun**: true
- **PublishTrimmed**: false
- **PublishSingleFile**: false

### Azure Ayarları
- **MSDeploy**: Aktif
- **Backup**: Aktif
- **App Offline**: Aktif
- **Checksum**: Aktif
- **Delete Existing Files**: Aktif

## 🔧 Önemli Notlar

1. **Production Ayarları**: `appsettings.Production.json` dosyası production ortamında kullanılır
2. **Database**: Azure SQL Database bağlantısı production'da aktif
3. **Email**: Gmail SMTP ayarları production'da aktif
4. **Logging**: Azure App Service logging aktif

## 📝 Publish Sonrası Kontrol

1. ✅ Site erişilebilir mi?
2. ✅ Database bağlantısı çalışıyor mu?
3. ✅ Email gönderimi çalışıyor mu?
4. ✅ Upload klasörleri yazılabilir mi?
5. ✅ SSL sertifikası aktif mi?

## 🛠️ Sorun Giderme

### Yaygın Hatalar
- **MSDeploy Hatası**: Azure credentials kontrol edin
- **Build Hatası**: NuGet paketlerini restore edin
- **Runtime Hatası**: .NET 8.0 runtime kontrol edin

### Log Kontrolü
```bash
# Azure App Service logs
# Azure Portal > App Service > Logs
```

## 📞 Destek

Publish sorunları için:
- Azure Portal logları kontrol edin
- Application Insights aktifse oradan takip edin
- Database bağlantı string'ini kontrol edin
