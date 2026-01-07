@echo off
REM Script para descargar repositorio desde GitHub
REM Uso: Doble click en este archivo

echo ========================================
echo   DESCARGAR REPOSITORIO DE GITHUB
echo ========================================
echo.

REM Verificar si ya existe una carpeta .git (repositorio ya existe)
if exist ".git" (
    echo AVISO: Ya existe un repositorio en esta carpeta.
    echo.
    echo ¿Que quieres hacer?
    echo.
    echo 1. ACTUALIZAR el repositorio existente (git pull)
    echo 2. DESCARGAR en una carpeta nueva
    echo 3. Cancelar
    echo.
    set /p opcion="Elige una opcion (1-3): "
    
    if "%opcion%"=="1" (
        echo.
        echo Actualizando repositorio...
        git pull
        if errorlevel 1 (
            echo.
            echo ERROR: No se pudo actualizar.
            pause
            exit /b 1
        )
        echo.
        echo ========================================
        echo   ¡REPOSITORIO ACTUALIZADO!
        echo ========================================
        pause
        exit /b 0
    ) else if "%opcion%"=="2" (
        echo.
        set /p carpeta="Escribe el nombre de la nueva carpeta (ej: Cazador-VS-Nuevo): "
        if "%carpeta%"=="" (
            set carpeta=Cazador-VS-Nuevo
        )
        echo.
        echo Descargando en carpeta: %carpeta%...
        cd ..
        git clone https://github.com/T-Duva/Cazador-VS.git %carpeta%
        if errorlevel 1 (
            echo.
            echo ERROR: No se pudo descargar.
            pause
            exit /b 1
        )
        echo.
        echo ========================================
        echo   ¡REPOSITORIO DESCARGADO EN: %carpeta%!
        echo ========================================
        pause
        exit /b 0
    ) else (
        echo Operacion cancelada.
        pause
        exit /b 0
    )
) else (
    REM No hay repositorio, descargar en carpeta nueva
    echo No hay repositorio en esta carpeta.
    echo.
    set /p carpeta="Escribe el nombre de la nueva carpeta (ej: Cazador-VS): "
    if "%carpeta%"=="" (
        set carpeta=Cazador-VS
    )
    echo.
    echo Descargando repositorio en carpeta: %carpeta%...
    cd ..
    git clone https://github.com/T-Duva/Cazador-VS.git %carpeta%
    if errorlevel 1 (
        echo.
        echo ERROR: No se pudo descargar.
        echo Verifica tu conexion a internet o las credenciales.
        pause
        exit /b 1
    )
    echo.
    echo ========================================
    echo   ¡REPOSITORIO DESCARGADO EN: %carpeta%!
    echo ========================================
    echo.
    echo La carpeta esta en: %cd%\%carpeta%
    pause
    exit /b 0
)
