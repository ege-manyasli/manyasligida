# 🛠️ Manuel Deployment Çözümü - DLL Locking Hatası

## Sorun
```
Web Deploy cannot modify the file 'manyasligida.dll' on the destination because it is locked by an external process.
```

## ✅ Manuel Çözüm Adımları

### 1. Azure Portal'da App Service'i Durdurun

1. **Azure Portal** → **App Service** → **manyasligida-web-new**
2. **Overview** sayfasında **Stop** butonuna tıklayın
3. **Yes** ile onaylayın
4. 30 saniye bekleyin

### 2. App Service Durumunu Kontrol Edin

- **Status** = **Stopped** olduğundan emin olun
- **Overview** sayfasında "Stopped" yazısını görmelisiniz

### 3. Visual Studio'dan Publish Edin

1. **Visual Studio**'da projeyi açın
2. **Build** → **Publish**
3. **manyasligida-web-new - Web Deploy** profilini seçin
4. **Publish** butonuna tıklayın

### 4. Publish Tamamlandıktan Sonra

1. **Azure Portal** → **App Service** → **Start** butonuna tıklayın
2. 30 saniye bekleyin
3. Siteyi test edin: https://manyasligida-web-new-gyaygrckfpbtg7ey.westeurope-01.azurewebsites.net/

## 🔄 Alternatif: Azure Portal'dan Restart

Eğer Stop/Start yapmak istemiyorsanız:

1. **Azure Portal** → **App Service** → **Overview**
2. **Restart** butonuna tıklayın
3. 30 saniye bekleyin
4. Hemen publish edin (uygulama başlamadan önce)

## 🚨 Acil Durum: Force Stop

Eğer normal Stop çalışmazsa:

1. **Azure Portal** → **App Service** → **Configuration**
2. **Application settings** → **+ New application setting**
3. **Name**: `WEBSITE_DISABLE_OVERLAPPED_RECYCLING`
4. **Value**: `1`
5. **Save** → **Restart**
6. Publish edin

## 📋 Kontrol Listesi

- [ ] App Service durduruldu mu?
- [ ] Publish başarılı mı?
- [ ] App Service başlatıldı mı?
- [ ] Site çalışıyor mu?

## 🎯 Beklenen Sonuç

- ✅ DLL locking hatası olmayacak
- ✅ Publish başarılı olacak
- ✅ Site çalışacak
- ✅ Database bağlantısı çalışacak
