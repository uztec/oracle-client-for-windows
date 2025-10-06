Write-Host "Building Oracle Database Client for Windows..." -ForegroundColor Green

# Restore packages
Write-Host "Restoring packages..." -ForegroundColor Yellow
dotnet restore

# Build the solution
Write-Host "Building solution..." -ForegroundColor Yellow
dotnet build --configuration Release

# Publish for Windows
Write-Host "Publishing for Windows..." -ForegroundColor Yellow
dotnet publish --configuration Release --runtime win-x64 --self-contained false --output ./publish

Write-Host "Build completed successfully!" -ForegroundColor Green
Write-Host "Executable is located in the ./publish directory" -ForegroundColor Cyan
