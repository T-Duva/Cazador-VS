# Script PowerShell para subir cambios a GitHub automaticamente
# Uso: Click derecho -> "Ejecutar con PowerShell" o desde terminal: .\subir_github.ps1

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  SUBIR CAMBIOS A GITHUB" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar si estás en un repositorio git
try {
    $null = git rev-parse --git-dir 2>$null
} catch {
    Write-Host "ERROR: No estas en un repositorio git!" -ForegroundColor Red
    Write-Host "Por favor, ejecuta este script desde la carpeta del proyecto." -ForegroundColor Yellow
    Read-Host "Presiona Enter para salir"
    exit 1
}

Write-Host "Verificando cambios..." -ForegroundColor Yellow
git status --short

Write-Host ""
Write-Host "¿Que cambios quieres subir?" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. TODOS los archivos modificados" -ForegroundColor White
Write-Host "2. SOLO los archivos que ya estaban siendo seguidos" -ForegroundColor White
Write-Host "3. Cancelar" -ForegroundColor White
Write-Host ""

$opcion = Read-Host "Elige una opcion (1-3)"

if ($opcion -eq "3") {
    Write-Host "Operacion cancelada." -ForegroundColor Yellow
    Read-Host "Presiona Enter para salir"
    exit 0
}

Write-Host ""
$mensaje = Read-Host "Escribe un mensaje para el commit (ej: Actualizar scripts)"

if ([string]::IsNullOrWhiteSpace($mensaje)) {
    $mensaje = "Actualizacion automatica"
}

Write-Host ""
Write-Host "Agregando archivos..." -ForegroundColor Yellow

if ($opcion -eq "1") {
    git add .
} elseif ($opcion -eq "2") {
    git add -u
} else {
    Write-Host "Opcion invalida!" -ForegroundColor Red
    Read-Host "Presiona Enter para salir"
    exit 1
}

Write-Host ""
Write-Host "Creando commit con mensaje: $mensaje" -ForegroundColor Yellow
git commit -m $mensaje

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "AVISO: No se pudo crear el commit." -ForegroundColor Yellow
    Write-Host "Puede ser que no haya cambios para commitear." -ForegroundColor Yellow
    Read-Host "Presiona Enter para salir"
    exit 1
}

Write-Host ""
Write-Host "Subiendo cambios a GitHub..." -ForegroundColor Yellow
git push

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "ERROR: No se pudo subir a GitHub." -ForegroundColor Red
    Write-Host "Verifica tu conexion o tus credenciales." -ForegroundColor Yellow
    Read-Host "Presiona Enter para salir"
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  ¡CAMBIOS SUBIDOS EXITOSAMENTE!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Read-Host "Presiona Enter para salir"
