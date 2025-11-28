@echo off
echo Fermo l'applicazione VanGest.Server...

:: Cerca e termina il processo dotnet che esegue VanGest.Server
tasklist /FI "IMAGENAME eq dotnet.exe" /FO CSV > running_processes.csv
findstr /i "VanGest.Server" running_processes.csv > nul

if %errorlevel% == 0 (
    echo Trovato processo VanGest.Server in esecuzione. Terminazione...
    taskkill /IM dotnet.exe /F
    echo Processo terminato.
) else (
    echo Nessun processo VanGest.Server trovato in esecuzione.
)

del running_processes.csv
echo Operazione completata. Premere un tasto per uscire.
pause