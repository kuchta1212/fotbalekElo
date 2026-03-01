# Test Script for Elo-fotbalek Backend
# Run this to verify the backend service starts correctly

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "Elo-fotbalek Backend Test Script" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Check if we're in the right directory
if (-not (Test-Path "Elo-fotbalek/Elo-fotbalek.csproj")) {
    Write-Host "? Error: Not in the correct directory!" -ForegroundColor Red
    Write-Host "Please run this script from the repository root (fotbalekElo)" -ForegroundColor Yellow
    exit 1
}

Write-Host "? Found project file" -ForegroundColor Green

# Step 2: Build the project
Write-Host ""
Write-Host "Building project..." -ForegroundColor Yellow
$buildResult = dotnet build Elo-fotbalek/Elo-fotbalek.csproj --configuration Debug 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "? Build failed!" -ForegroundColor Red
    Write-Host $buildResult
    exit 1
}
Write-Host "? Build successful" -ForegroundColor Green

# Step 3: Check if port 5001 is available
Write-Host ""
Write-Host "Checking if port 5001 is available..." -ForegroundColor Yellow
$portInUse = Get-NetTCPConnection -LocalPort 5001 -ErrorAction SilentlyContinue
if ($portInUse) {
    Write-Host "??  Warning: Port 5001 is already in use" -ForegroundColor Yellow
    Write-Host "You may need to stop the other process or change the port in launchSettings.json" -ForegroundColor Yellow
} else {
    Write-Host "? Port 5001 is available" -ForegroundColor Green
}

# Step 4: Quick test run (we'll stop it after a few seconds)
Write-Host ""
Write-Host "Starting backend service..." -ForegroundColor Yellow
Write-Host "(This will run for 10 seconds to test startup, then auto-stop)" -ForegroundColor Gray
Write-Host ""

# Start the process in the background
$process = Start-Process -FilePath "dotnet" `
    -ArgumentList "run --project Elo-fotbalek/Elo-fotbalek.csproj --no-build" `
    -WorkingDirectory (Get-Location) `
    -PassThru `
    -RedirectStandardOutput "temp_output.log" `
    -RedirectStandardError "temp_error.log"

# Wait a bit for it to start
Start-Sleep -Seconds 5

# Check if process is still running
if ($process.HasExited) {
    Write-Host "? Service failed to start!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Error output:" -ForegroundColor Yellow
    Get-Content "temp_error.log"
    Remove-Item "temp_output.log" -ErrorAction SilentlyContinue
    Remove-Item "temp_error.log" -ErrorAction SilentlyContinue
    exit 1
}

Write-Host "? Service started successfully!" -ForegroundColor Green

# Step 5: Test API endpoints
Write-Host ""
Write-Host "Testing API endpoints..." -ForegroundColor Yellow

try {
    # Test /api/config
    $configResponse = Invoke-RestMethod -Uri "https://localhost:5001/api/config" `
        -SkipCertificateCheck `
        -TimeoutSec 5 `
        -ErrorAction Stop
    Write-Host "? GET /api/config - Working!" -ForegroundColor Green
    Write-Host "   App Name: $($configResponse.data.appName)" -ForegroundColor Gray
} catch {
    Write-Host "??  GET /api/config - Failed: $($_.Exception.Message)" -ForegroundColor Yellow
}

try {
    # Test /api/players
    $playersResponse = Invoke-RestMethod -Uri "https://localhost:5001/api/players" `
        -SkipCertificateCheck `
        -TimeoutSec 5 `
        -ErrorAction Stop
    Write-Host "? GET /api/players - Working!" -ForegroundColor Green
    Write-Host "   Players count: $($playersResponse.data.players.Count)" -ForegroundColor Gray
} catch {
    Write-Host "??  GET /api/players - Failed: $($_.Exception.Message)" -ForegroundColor Yellow
}

try {
    # Test /api/background-images
    $bgResponse = Invoke-RestMethod -Uri "https://localhost:5001/api/background-images" `
        -SkipCertificateCheck `
        -TimeoutSec 5 `
        -ErrorAction Stop
    Write-Host "? GET /api/background-images - Working!" -ForegroundColor Green
    Write-Host "   Images count: $($bgResponse.data.images.Count)" -ForegroundColor Gray
} catch {
    Write-Host "??  GET /api/background-images - Failed: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Step 6: Stop the process
Write-Host ""
Write-Host "Stopping test service..." -ForegroundColor Yellow
Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
Write-Host "? Service stopped" -ForegroundColor Green

# Cleanup
Remove-Item "temp_output.log" -ErrorAction SilentlyContinue
Remove-Item "temp_error.log" -ErrorAction SilentlyContinue

# Summary
Write-Host ""
Write-Host "==================================" -ForegroundColor Cyan
Write-Host "Test Summary" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host "? Build: SUCCESS" -ForegroundColor Green
Write-Host "? Startup: SUCCESS" -ForegroundColor Green
Write-Host ""
Write-Host "You can now run the application from Visual Studio:" -ForegroundColor Yellow
Write-Host "1. Open Elo-fotbalek.sln in Visual Studio" -ForegroundColor White
Write-Host "2. Press F5 or click the Play button" -ForegroundColor White
Write-Host "3. Backend will start at: https://localhost:5001" -ForegroundColor White
Write-Host ""
Write-Host "To test API endpoints manually:" -ForegroundColor Yellow
Write-Host 'Invoke-RestMethod -Uri "https://localhost:5001/api/config" -SkipCertificateCheck' -ForegroundColor Gray
Write-Host ""
Write-Host "See RUNNING_FROM_VISUAL_STUDIO.md for more details." -ForegroundColor Cyan
