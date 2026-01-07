# Instrucciones de Inspector para HUD de Stats

## Configuración del Contenedor Principal (panelStatsJugador)

### Componentes requeridos:
1. **RectTransform** (automático)
2. **HorizontalLayoutGroup**

### Settings del HorizontalLayoutGroup:
- **Spacing**: `20` (0.5 cm entre tarjetas)
- **Padding**: `Left: 10`, `Right: 10`, `Top: 10`, `Bottom: 10`
- **Child Alignment**: `Middle Left`
- **Child Control Width**: ✅ **ON** (checked)
- **Child Control Height**: ❌ OFF (unchecked)
- **Child Force Expand Width**: ❌ **OFF** (unchecked) - IMPORTANTE
- **Child Force Expand Height**: ❌ OFF (unchecked)

### Componentes NO permitidos:
- ❌ **NO** agregar `ContentSizeFitter` al contenedor

---

## Configuración de cada StatCard (TarjetaVida, TarjetaEscudo, etc.)

### Componentes requeridos en el root de cada StatCard:
1. **RectTransform** (automático)
2. **VerticalLayoutGroup**
3. **LayoutElement**
4. **StatCardAutoWidth** (script personalizado)

### Settings del VerticalLayoutGroup:
- **Child Alignment**: `Upper Center`
- **Spacing**: `5` (4-6 píxeles entre icono y texto)
- **Padding**: `Left: 6`, `Right: 6`, `Top: 6`, `Bottom: 6`
- **Child Control Width**: ✅ **ON** (checked)
- **Child Control Height**: ✅ **ON** (checked)
- **Child Force Expand Width**: ❌ **OFF** (unchecked)
- **Child Force Expand Height**: ❌ **OFF** (unchecked)

### Settings del LayoutElement:
- **Min Width**: `32` (o 24 si prefieres mínimo visual más pequeño)
- **Preferred Width**: Se establece automáticamente por código (no tocar)
- **Flexible Width**: `0` (no permitir estiramiento)
- **Preferred Height**: `80` (alto fijo)
- **Flexible Height**: `0`

### Settings del StatCardAutoWidth:
- **Layout Element**: Asignar el LayoutElement del mismo GameObject
- **Label**: Asignar el componente Text hijo
- **Min Width**: `32` (ajustable)
- **Horizontal Padding**: `16` (padding total izq+der, ajustable)
- **Extra Safety**: `2` (margen de seguridad, ajustable)

### Componentes NO permitidos:
- ❌ **NO** agregar `ContentSizeFitter` al root de la StatCard
- ❌ **NO** agregar `ContentSizeFitter` a los hijos (icono o texto)

---

## Configuración del Icono (hijo directo de StatCard)

### Componentes requeridos:
1. **RectTransform** (automático)
2. **LayoutElement**
3. **Image**

### Settings del RectTransform:
- **Size Delta**: `24 x 24` (tamaño fijo)

### Settings del LayoutElement:
- **Preferred Width**: `24`
- **Preferred Height**: `24`
- **Flexible Width**: `0`
- **Flexible Height**: `0`

### Settings del Image:
- **Source Image**: Asignar el sprite del icono (IconoVida, IconoEscudo, etc.)
- **Preserve Aspect**: ✅ **ON** (checked)

---

## Configuración del Texto (Label - hijo directo de StatCard)

### Componentes requeridos:
1. **RectTransform** (automático)
2. **Text** (UnityEngine.UI.Text - no TMP ya que no está disponible)

### Settings del RectTransform:
- **Size Delta**: `Width: 0` (se ajusta automáticamente), `Height: 20`

### Settings del Text:
- **Text**: El texto se establece por código (ej: "Vida: 179/179")
- **Font Size**: `12` (ajustable según diseño)
- **Alignment**: `Center` (horizontal y vertical)
- **Horizontal Overflow**: `Overflow` (sin wrap)
- **Vertical Overflow**: `Overflow`
- **Best Fit**: ❌ **OFF** (no activar Auto Size)

### Settings NO permitidos:
- ❌ **NO** activar "Best Fit" / Auto Size
- ❌ **NO** usar Word Wrapping

---

## Assets de Iconos

### Ubicación:
Los iconos deben estar en: `Assets/Sprites/UI/`

### Nombres de archivos:
- `IconoVida.png`
- `IconoEscudo.png`
- `IconoOro.png`
- `IconoDaño.png`
- `IconoBatalla.png`

### Import Settings (configurados automáticamente por AssetPostprocessor):
- **Texture Type**: `Sprite (2D and UI)`
- **Sprite Mode**: `Single`
- **Alpha Is Transparency**: ✅ **ON**
- **Filter Mode**: `Point (no filter)` - para mantener nítidos
- **Compression**: `None` (sin compresión)
- **Generate Mip Maps**: ❌ **OFF**

---

## Verificación Final

### Checklist:
- ✅ `panelStatsJugador` tiene `HorizontalLayoutGroup` con spacing 20
- ✅ `panelStatsJugador` NO tiene `ContentSizeFitter`
- ✅ Cada StatCard tiene `VerticalLayoutGroup` con `Upper Center` alignment
- ✅ Cada StatCard tiene `LayoutElement` con `minWidth = 32` y `flexibleWidth = 0`
- ✅ Cada StatCard tiene `StatCardAutoWidth` con referencias asignadas
- ✅ Cada StatCard NO tiene `ContentSizeFitter`
- ✅ Cada Icono es 24x24 con `LayoutElement` de tamaño fijo
- ✅ Cada Icono tiene `Preserve Aspect = ON`
- ✅ Cada Text tiene `Best Fit = OFF` y `Overflow` en lugar de wrap
- ✅ Los iconos están en `Assets/Sprites/UI/` con import settings correctos

---

## Notas Importantes

1. **No usar ContentSizeFitter** en objetos bajo LayoutGroup para evitar loops y solapamientos
2. **El ancho se calcula dinámicamente** por código usando `StatCardAutoWidth.SetText()`
3. **El rebuild del layout** se hace una sola vez al final de `ActualizarInfoJugador()`
4. **No llamar `Canvas.ForceUpdateCanvases()`** múltiples veces - solo una vez antes de medir
5. **El spacing de 20px** se mantiene constante entre todas las tarjetas

