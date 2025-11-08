@echo off
cd %~dp0
powershell -Command "Compress-Archive -Path bin\Release\net8.0-windows10.0.26100.0\publish\win-x64\* -DestinationPath NetSCAD-v0.3.0-win-x64.zip -Force"