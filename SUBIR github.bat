@echo off
REM Script para subir cambios a GitHub automaticamente
REM Uso: Doble click en este archivo o ejecutar desde la terminal

echo ========================================
echo   SUBIR CAMBIOS A GITHUB
echo ========================================
echo.

REM Verificar si estás en un repositorio git
git rev-parse --git-dir >nul 2>&1
if errorlevel 1 (
    echo ERROR: No estas en un repositorio git!
    echo Por favor, ejecuta este script desde la carpeta del proyecto.
    pause
    exit /b 1
)

echo Verificando cambios...
git status --short

echo.
echo ¿Que cambios quieres subir?
echo.
echo 1. TODOS los archivos modificados
echo 2. SOLO los archivos que ya estaban siendo seguidos (ignora nuevos archivos)
echo 3. Cancelar
echo.
set /p opcion="Elige una opcion (1-3): "

if "%opcion%"=="3" (
    echo Operacion cancelada.
    pause
    exit /b 0
)

echo.
set /p mensaje="Escribe un mensaje para el commit (ej: Actualizar scripts): "

if "%mensaje%"=="" (
    set mensaje=Actualizacion automatica
)

echo.
echo Agregando archivos...
if "%opcion%"=="1" (
    git add .
) else if "%opcion%"=="2" (
    git add -u
) else (
    echo Opcion invalida!
    pause
    exit /b 1
)

echo.
echo Creando commit con mensaje: %mensaje%
git commit -m "%mensaje%"

if errorlevel 1 (
    echo.
    echo AVISO: No se pudo crear el commit.
    echo Puede ser que no haya cambios para commitear.
    pause
    exit /b 1
)

echo.
echo Subiendo cambios a GitHub...
git push

if errorlevel 1 (
    echo.
    echo Configurando conexion con GitHub (primera vez)...
    git push --set-upstream origin main
    
    if errorlevel 1 (
        echo.
        echo ERROR: No se pudo subir a GitHub.
        echo Verifica tu conexion o tus credenciales.
        pause
        exit /b 1
    )
)

echo.
echo ========================================
echo   ¡CAMBIOS SUBIDOS EXITOSAMENTE!
echo ========================================
echo.
pause
