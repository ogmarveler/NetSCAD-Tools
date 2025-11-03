@echo off
cd %~dp0
echo [Desktop Entry] > bin/Release/net8.0/publish/linux-x64/NetScad.desktop
echo Name=NetSCAD >> bin/Release/net8.0/publish/linux-x64/NetScad.desktop
echo Exec=NetSCAD >> bin/Release/net8.0/publish/linux-x64/NetScad.desktop
echo Type=Application >> bin/Release/net8.0/publish/linux-x64/NetScad.desktop
echo Terminal=false >> bin/Release/net8.0/publish/linux-x64/NetScad.desktop
echo Icon=logo.png >> bin/Release/net8.0/publish/linux-x64/NetScad.desktop
echo Comment=NetScad Avalonia App >> bin/Release/net8.0/publish/linux-x64/NetScad.desktop
echo Categories=Utility; >> bin/Release/net8.0/publish/linux-x64/NetScad.desktop
wsl chmod +x bin/Release/net8.0/publish/linux-x64/NetScad.desktop
wsl tar -czf NetSCAD-v0.2.1-linux-x64.tar.gz -C bin/Release/net8.0/publish/linux-x64 .