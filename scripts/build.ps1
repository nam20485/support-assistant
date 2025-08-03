# Build script for SupportAssistant (Windows)
# This script builds the entire solution and runs tests

Write-Host "Building SupportAssistant..." -ForegroundColor Green

# Clean previous builds
Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
dotnet clean

# Restore dependencies
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore

# Build the solution
Write-Host "Building solution..." -ForegroundColor Yellow
dotnet build --configuration Release --no-restore

# Run tests
Write-Host "Running tests..." -ForegroundColor Yellow
dotnet test --configuration Release --no-build --verbosity normal

# Build desktop application specifically
Write-Host "Building desktop application..." -ForegroundColor Yellow
dotnet build src/SupportAssistant.Desktop --configuration Release --no-restore

Write-Host "Build completed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "To run the application:" -ForegroundColor Cyan
Write-Host "  dotnet run --project src/SupportAssistant.Desktop --configuration Release" -ForegroundColor White