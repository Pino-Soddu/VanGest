@echo off
title 🌐 VanGest.Server
color 0B

cd /d "%~dp0"
echo Avvio applicazione su porta 8084...
dotnet VanGest.Server.dll --urls "http://0.0.0.0:8084"
pause