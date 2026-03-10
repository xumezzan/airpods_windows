Write-Host "Checking for .NET 8.0 SDK..."
if (!(Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host ".NET SDK is not installed. Attempting to install via winget..." -ForegroundColor Yellow
    winget install Microsoft.DotNet.SDK.8 --accept-package-agreements --accept-source-agreements
    
    # Refresh paths
    $env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")
}

Write-Host "Building and Running AirPods App..." -ForegroundColor Green
dotnet run --project AirPodsApp.csproj
