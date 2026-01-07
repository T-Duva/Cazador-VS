# 🚀 Subir Cambios a GitHub Automáticamente

## 📋 Instrucciones Súper Simples

### Opción 1: Script de Windows (.bat) - **MÁS FÁCIL** ⭐

1. **Doble click** en el archivo `subir_github.bat`
2. Elige qué archivos subir (todos o solo los modificados)
3. Escribe un mensaje corto (ejemplo: "Agregué nuevas funciones")
4. ¡Listo! Los cambios se suben automáticamente

### Opción 2: Script PowerShell (.ps1) - Con colores y validaciones

1. **Click derecho** en `subir_github.ps1` → **"Ejecutar con PowerShell"**
2. Sigue los mismos pasos que el script .bat
3. Tiene mejor presentación visual

---

## ⚠️ Antes de Usar

### 1. Verificar que ya tienes Git configurado:

Abre una terminal y escribe:
```bash
git config --global user.name "Tu Nombre"
git config --global user.email "tu@email.com"
```

### 2. Verificar que tienes un repositorio en GitHub:

- Si **NO** tienes repositorio: Crea uno en GitHub.com primero
- Si **YA** tienes: Asegúrate de haber conectado tu carpeta local con el repositorio

### 3. Conectar tu carpeta local con GitHub (solo la primera vez):

Si es la primera vez, abre una terminal en esta carpeta y ejecuta:

```bash
git remote add origin https://github.com/TU_USUARIO/TU_REPOSITORIO.git
git branch -M main
git push -u origin main
```

(Reemplaza `TU_USUARIO` y `TU_REPOSITORIO` con los tuyos)

---

## 🔧 ¿Qué Hace el Script?

1. ✅ Verifica que estás en un repositorio git
2. ✅ Muestra los archivos que cambiaron
3. ✅ Te pregunta qué archivos subir
4. ✅ Te pide un mensaje para el commit
5. ✅ Agrega los archivos (git add)
6. ✅ Crea el commit (git commit)
7. ✅ Sube todo a GitHub (git push)

---

## ❓ Problemas Comunes

### "No estás en un repositorio git"
- **Solución**: Ejecuta el script desde la carpeta del proyecto (donde está el `.gitignore`)

### "No se pudo subir a GitHub"
- **Solución**: Verifica tu conexión a internet y tus credenciales de GitHub

### "No hay cambios para commitear"
- **Solución**: Esto está bien, significa que todo ya está guardado en GitHub

---

## 💡 Consejo

Si quieres subir cambios rápidamente sin usar el script, también puedes usar:

```bash
git add .
git commit -m "Tu mensaje aquí"
git push
```

¡Pero el script es más fácil! 😊
