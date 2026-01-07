@echo off
echo ========================================
echo  CREANDO BUILD DEL JUEGO CAZADOR VS
echo ========================================
echo.

REM Buscar Unity en disco F:
set UNITY_PATH=

REM Buscar en F:\Program Files\Unity\Hub\Editor
if exist "F:\Program Files\Unity\Hub\Editor" (
    for /f "delims=" %%i in ('dir /b /ad "F:\Program Files\Unity\Hub\Editor" 2^>nul') do (
        if exist "F:\Program Files\Unity\Hub\Editor\%%i\Editor\Unity.exe" (
            set UNITY_PATH=F:\Program Files\Unity\Hub\Editor\%%i\Editor\Unity.exe
            goto :found
        )
    )
)

REM Buscar en otras ubicaciones comunes de F:
if exist "F:\Unity\Editor\Unity.exe" (
    set UNITY_PATH=F:\Unity\Editor\Unity.exe
    goto :found
)

REM Si no se encuentra, intentar buscar en C:
if exist "C:\Program Files\Unity\Hub\Editor" (
    for /f "delims=" %%i in ('dir /b /ad "C:\Program Files\Unity\Hub\Editor" 2^>nul') do (
        if exist "C:\Program Files\Unity\Hub\Editor\%%i\Editor\Unity.exe" (
            set UNITY_PATH=C:\Program Files\Unity\Hub\Editor\%%i\Editor\Unity.exe
            goto :found
        )
    )
)

:found
if "%UNITY_PATH%"=="" (
    echo ERROR: No se encontro Unity instalado.
    echo.
    echo Por favor, ejecuta el build desde Unity:
    echo 1. Abre Unity con tu proyecto CAZADOR_VS
    echo 2. Ve a: Tools -^> Build Game - Crear Ejecutable
    echo.
    pause
    exit /b 1
)

echo Unity encontrado en: %UNITY_PATH%
echo.

set PROJECT_PATH=%~dp0
set BUILD_PATH=%USERPROFILE%\Desktop\CAZADOR_VS_Build

echo Proyecto: %PROJECT_PATH%
echo Build en: %BUILD_PATH%
echo.
echo Esto puede tardar varios minutos...
echo.

REM Crear carpeta de build
if not exist "%BUILD_PATH%" mkdir "%BUILD_PATH%"

REM Ejecutar Unity en modo batch para crear el build
"%UNITY_PATH%" -quit -batchmode -projectPath "%PROJECT_PATH%" -buildTarget Win64 -buildPath "%BUILD_PATH%\CAZADOR VS.exe" -logFile "%PROJECT_PATH%build_log.txt"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo  BUILD EXITOSO!
    echo ========================================
    echo.
    echo El build se creo en: %BUILD_PATH%
    echo.
    echo Para compartir con tu amigo:
    echo 1. Comprime la carpeta completa en un ZIP
    echo 2. Comparte el ZIP
    echo.
    explorer "%BUILD_PATH%"
) else (
    echo.
    echo ========================================
    echo  ERROR EN EL BUILD
    echo ========================================
    echo.
    echo Revisa el archivo build_log.txt para ver los errores
    echo O ejecuta el build manualmente desde Unity:
    echo 1. Abre Unity
    echo 2. Ve a: Tools -^> Build Game - Crear Ejecutable
    echo.
)

pause
