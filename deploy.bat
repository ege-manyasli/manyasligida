@echo off
echo === Manyasli Gida Azure Deployment ===
echo.

echo Cleaning project...
dotnet clean --configuration Release

echo Restoring packages...
dotnet restore

echo Building project...
dotnet build --configuration Release --no-restore

echo Publishing to Azure...
dotnet publish --configuration Release --publish-profile "Properties\PublishProfiles\manyasligida-web-new - Web Deploy.pubxml"

echo.
echo === Deployment Complete ===
echo Site: https://manyasligida-web-new-gyaygrckfpbtg7ey.westeurope-01.azurewebsites.net
echo.
pause
