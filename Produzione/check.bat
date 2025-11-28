@echo off
tasklist | findstr "dotnet.exe"
if errorlevel 1 (
    echo ❌ Applicazione NON in esecuzione
) else (
    echo ✅ Applicazione IN ESECUZIONE
)
pause