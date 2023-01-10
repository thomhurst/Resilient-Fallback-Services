@echo off
set /p version="Enter Version Number to Build With: "

@echo on
dotnet pack ".\TomLonghurst.Services.Resilient.Fallback\TomLonghurst.Services.Resilient.Fallback.csproj"  --configuration Release /p:Version=%version%

pause