# 🔧 Azure App Service Deployment Fix - DLL Locking Issue

## Problem
```
Web Deploy cannot modify the file 'manyasligida.dll' on the destination because it is locked by an external process.
```

## ✅ Solution Applied

### 1. Web.config Configuration (FIXED)
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

### 2. Publish Profile Enhanced (FIXED)
```xml
<!-- AppOffline ayarları -->
<MSDeployEnableAppOffline>true</MSDeployEnableAppOffline>
<EnableMSDeployAppOffline>true</EnableMSDeployAppOffline>
<EnableMSDeployBackup>true</EnableMSDeployBackup>
<DeleteExistingFiles>true</DeleteExistingFiles>
<AspNetCoreHostingModel>OutOfProcess</AspNetCoreHostingModel>
```

### 3. Deployment Methods

#### Option A: Use Batch File (Recommended)
```bash
deploy.bat
```

#### Option B: Direct Command
```bash
dotnet publish -c Release -p:PublishProfile="Properties\PublishProfiles\manyasligida-web-new - Web Deploy.pubxml"
```

#### Option C: PowerShell Script
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
.\publish.ps1
```

## 🔍 How AppOffline Works

1. **Before Deployment**: Creates `app_offline.htm` file
2. **During Deployment**: IIS serves offline page, stops application
3. **File Replacement**: DLLs can be replaced without locking
4. **After Deployment**: Removes `app_offline.htm`, restarts application

## 🚨 If Still Fails

### Manual Azure Portal Steps:
1. Go to Azure Portal → App Service
2. **Restart** the application
3. Wait 30 seconds
4. Try deployment again

### Alternative: Force Stop
1. Azure Portal → App Service → Configuration
2. Add App Setting: `WEBSITE_DISABLE_OVERLAPPED_RECYCLING = 1`
3. Save and restart
4. Deploy again

## 📋 Checklist

- [x] `hostingModel="OutOfProcess"` in web.config
- [x] `EnableMsDeployAppOffline>true</EnableMsDeployAppOffline>` in publish profile
- [x] `DeleteExistingFiles>true</DeleteExistingFiles>` in publish profile
- [x] `AspNetCoreHostingModel>OutOfProcess</AspNetCoreHostingModel>` in publish profile
- [x] Clean web.config without unnecessary security sections

## 🎯 Expected Result
- ✅ Deployment completes without DLL locking errors
- ✅ Application starts successfully on Azure
- ✅ No 500.30 errors
- ✅ Clean deployment process

## 📞 Troubleshooting

If deployment still fails:
1. Check Azure App Service logs
2. Verify connection strings in Azure Configuration
3. Ensure ASPNETCORE_ENVIRONMENT = Production
4. Check if any background processes are running
