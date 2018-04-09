@echo off
if not exist ".\Packages\" md ".\Packages\"
set dir=*.nupkg
for /f "delims=" %%i in ('dir /a/d/b/s "%dir%"') do (copy %%i .\Packages\)
pause