@echo off
set srcFolder="%~dp0Install"
set destFolder="%~dp0.."

echo Copying files for Firebird 3 Embedded from %srcFolder% to %destFolder%...

xcopy %srcFolder% %destFolder% /E /I /H /K /Y /Q

echo Firebird 3 Embedded setup complete.
pause