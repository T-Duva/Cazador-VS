# CAZADOR VS - Proyecto Unity

## 📋 Instrucciones Generales

### Crear Build del Juego

**Método 1: Desde Unity (Recomendado)**
1. Abre Unity con el proyecto
2. Ve a: **Tools → Build Game - Crear Ejecutable**
3. Haz clic en **"🔨 Crear Build Ahora"**
4. Espera a que termine
5. Se creará una carpeta `CAZADOR_VS_Build` en tu Escritorio

**Método 2: Script Automático**
- Doble clic en `CREAR_BUILD.bat` en la carpeta del proyecto

**Método 3: Manual**
1. En Unity: **File → Build Settings**
2. Selecciona: **PC, Mac & Linux Standalone** → **Windows**
3. Haz clic en **Build**
4. Elige una carpeta y espera

### Compartir el Juego

1. Comprime toda la carpeta `CAZADOR_VS_Build` en un ZIP
2. Comparte el ZIP (Google Drive, Dropbox, WeTransfer, USB, etc.)
3. Tu amigo descomprime y ejecuta `CAZADOR VS.exe`

**Importante:** El tamaño será aproximadamente 50-200 MB. Comparte TODA la carpeta, no solo el .exe.

---

## ⚙️ Herramientas del Menú Tools

- **Setup Game - Configurar Todo**: Configura el juego automáticamente (GameManager, Canvas, EventSystem)
- **Build Game - Crear Ejecutable**: Crea el build del juego
- **Configurar Sprites de Personajes**: Configura automáticamente los sprites
- **Asignar Sprites Automáticamente**: Asigna los sprites por defecto
- **Create Default Folders**: Crea la estructura de carpetas
- **Corregir Input System**: Corrige errores del Input System

---

## 📝 Notas

- Los sprites deben estar en: `Assets/Sprites/Personajes/[Tipo]/1.png`, `2.png`, etc.
- Los marcos del jugador y enemigo se crean automáticamente
- El marco rojo central aparece solo durante las animaciones de combate

---

*Este archivo se actualiza solo cuando se solicita.*

