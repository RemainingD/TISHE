@echo off
cd /d "%~dp0"
if not exist "Builds\Windows\TISHE.exe" (
  echo Build not found. Open Unity and use TISHE - Build Windows game.
  pause
  exit /b 1
)
start "" "Builds\Windows\TISHE.exe"
