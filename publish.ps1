# Manyaslı Süt Ürünleri Publish Scripti
param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("Azure", "Local")]
    [string]$Target = "Azure"
)

Write-Host "=== Manyaslı Süt Ürünleri Publish ===" -ForegroundColor Green
Write-Host "Hedef: $Target" -ForegroundColor Yellow

try {
    # Temizlik
    Write-Host "Temizlik yapılıyor..." -ForegroundColor Blue
    dotnet clean --configuration Release
    
    # Paketleri geri yükle
    Write-Host "Paketler geri yükleniyor..." -ForegroundColor Blue
    dotnet restore
    
    # Build
    Write-Host "Proje build ediliyor..." -ForegroundColor Blue
    dotnet build --configuration Release --no-restore
    
    # Publish
    if ($Target -eq "Azure") {
        Write-Host "Azure'a publish ediliyor..." -ForegroundColor Blue
        dotnet publish --configuration Release --publish-profile "Properties\PublishProfiles\manyasligida-web-new - Web Deploy.pubxml"
        Write-Host "✅ Azure'a başarıyla publish edildi!" -ForegroundColor Green
        Write-Host "🌐 Site: https://manyasligida-web-new-gyaygrckfpbtg7ey.westeurope-01.azurewebsites.net" -ForegroundColor Cyan
    } else {
        Write-Host "Yerel klasöre publish ediliyor..." -ForegroundColor Blue
        dotnet publish --configuration Release --publish-profile "Properties\PublishProfiles\LocalFolder.pubxml"
        Write-Host "✅ Yerel klasöre başarıyla publish edildi!" -ForegroundColor Green
        Write-Host "📁 Klasör: bin\Release\net8.0\publish\" -ForegroundColor Cyan
    }
    
} catch {
    Write-Host "❌ Hata: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
