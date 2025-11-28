@echo off
title 📦 Pubblica VanGest.Server con Prompt
color 0B

cd /d "Z:\DBUnico\Sorgenti\VanGest.Server\VanGest.Server"

echo Compilazione...
dotnet build --configuration Release --nologo

echo Pulizia cartella publish...
if exist publish rmdir /s /q publish

echo Copia file compilati...
xcopy "bin\Release\net8.0-windows\*.*" "publish\" /s /y /i

echo Copia configurazione...
copy "appsettings.json" "publish\"

echo Copia file dei prompt...
xcopy "www\Prompt\*.*" "publish\www\Prompt\" /s /y /i

echo.
echo ✅ Pubblicazione completata con i prompt!
echo File nella cartella publish:
dir publish /b
echo.
pause