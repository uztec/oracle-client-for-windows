@echo off
echo Building Oracle Database Client for Windows...

REM Restore packages
dotnet restore

REM Build the solution
dotnet build --configuration Release

REM Publish for Windows
dotnet publish --configuration Release --runtime win-x64 --self-contained false --output ./publish

echo Build completed successfully!
echo Executable is located in the ./publish directory
pause
