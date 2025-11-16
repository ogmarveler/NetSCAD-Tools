@echo off
cd %~dp0
set SEVENZIP="C:\Program Files\7-Zip\7z.exe"
set SOURCE=bin\Release\net10.0-windows10.0.26100.0\publish\win-x64
set OUTPUT=NetSCAD-v0.4.0-win-x64.7z
set TOPFOLDER=NetSCAD-v0.4.0-win-x64
set TEMP_DIR=%~dp0temp

if not exist %SEVENZIP% (
    echo 7-Zip not found at %SEVENZIP%. Please install 7-Zip or update the path.
    exit /b 1
)

:: Create temporary directory and copy files
if exist %TEMP_DIR% rmdir /s /q %TEMP_DIR%
mkdir %TEMP_DIR%\%TOPFOLDER%
xcopy %SOURCE%\* %TEMP_DIR%\%TOPFOLDER% /E /I /Y

:: Create archive with custom top-level folder
%SEVENZIP% a -t7z -m0=lzma2 -mx9 -md=64m -ms=on %OUTPUT% -w%~dp0 %TEMP_DIR%\%TOPFOLDER% -r -aoa -xr!%OUTPUT%
if %ERRORLEVEL% equ 0 (
    echo Archive created successfully: %OUTPUT%
) else (
    echo Failed to create archive.
    rmdir /s /q %TEMP_DIR%
    exit /b %ERRORLEVEL%
)

:: Clean up temporary directory
rmdir /s /q %TEMP_DIR%