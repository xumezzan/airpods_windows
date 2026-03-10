Write-Host "Publishing AirPods App as a single standalone .exe file..." -ForegroundColor Green
dotnet publish AirPodsApp.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ./publish-app

Write-Host ""
Write-Host "Publish complete! You can find your AirPodsApp.exe in the 'publish-app' folder:" -ForegroundColor Cyan
Write-Host "$PWD\publish-app\AirPodsApp.exe" -ForegroundColor Yellow
Write-Host "You can move this .exe anywhere on your PC and run it by double-clicking." -ForegroundColor White
