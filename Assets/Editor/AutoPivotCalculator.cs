#pragma warning disable CS0618 // TextureImporter.spritesheet está obsoleto pero sigue funcionando
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class AutoPivotCalculator : EditorWindow
{
    private Texture2D spriteSheet;
    private string referenceSpriteName = "";
    
    [MenuItem("Tools/Auto Pivot Calculator")]
    public static void ShowWindow()
    {
        GetWindow<AutoPivotCalculator>("Auto Pivot Calculator");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Calculador Automático de Pivotes", EditorStyles.boldLabel);
        
        spriteSheet = (Texture2D)EditorGUILayout.ObjectField(
            "Sprite Sheet:", 
            spriteSheet, 
            typeof(Texture2D), 
            false
        );

        EditorGUILayout.Space();
        GUILayout.Label("Modo 1: Copiar desde sprite de referencia", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "1. En el Sprite Editor, poné el pivot PERFECTO en un sprite (pecho).\n" +
            "2. Anotá el nombre de ese sprite.\n" +
            "3. Ese pivot se copia al resto, con un pequeño ajuste local.", 
            MessageType.Info
        );
        
        referenceSpriteName = EditorGUILayout.TextField("Nombre sprite referencia:", referenceSpriteName);
        
        if (GUILayout.Button("Copiar pivots desde referencia", GUILayout.Height(40)))
        {
            if (spriteSheet != null && !string.IsNullOrEmpty(referenceSpriteName))
            {
                CopyPivotsFromReference();
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Selecciona una sprite sheet y escribe el nombre de referencia.", "OK");
            }
        }

        EditorGUILayout.Space();
        GUILayout.Label("Modo 2: Calcular pecho automáticamente (sin referencia)", EditorStyles.boldLabel);
        if (GUILayout.Button("Calcular y Aplicar Pivotes Automáticamente", GUILayout.Height(40)))
        {
            if (spriteSheet != null)
            {
                CalculateAndApplyPivots();
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Selecciona una Sprite Sheet primero", "OK");
            }
        }
        
        if (GUILayout.Button("Preview Pivots (Vista Previa)"))
        {
            if (spriteSheet != null)
            {
                PivotPreview.OpenPreview(spriteSheet);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Selecciona una Sprite Sheet primero", "OK");
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();
        
        GUILayout.Label("Ajustar zona de búsqueda", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Ajusta dónde buscar el pecho. Los cambios se ven en Preview.", MessageType.Info);

        EditorGUILayout.LabelField("Zona vertical (altura del sprite)", EditorStyles.boldLabel);
        PivotCalculatorHelper.CHEST_TOP_PERCENT = EditorGUILayout.Slider("Arriba %:", PivotCalculatorHelper.CHEST_TOP_PERCENT, 0f, 0.5f);
        PivotCalculatorHelper.CHEST_BOTTOM_PERCENT = EditorGUILayout.Slider("Abajo %:", PivotCalculatorHelper.CHEST_BOTTOM_PERCENT, 0.5f, 1f);

        EditorGUILayout.LabelField("Zona horizontal (ancho del sprite)", EditorStyles.boldLabel);
        PivotCalculatorHelper.CHEST_LEFT_PERCENT = EditorGUILayout.Slider("Izquierda %:", PivotCalculatorHelper.CHEST_LEFT_PERCENT, 0f, 0.5f);
        PivotCalculatorHelper.CHEST_RIGHT_PERCENT = EditorGUILayout.Slider("Derecha %:", PivotCalculatorHelper.CHEST_RIGHT_PERCENT, 0.5f, 1f);

        EditorGUILayout.LabelField("Sensibilidad", EditorStyles.boldLabel);
        PivotCalculatorHelper.ALPHA_THRESHOLD = EditorGUILayout.Slider("Opacidad mínima:", PivotCalculatorHelper.ALPHA_THRESHOLD, 0f, 1f);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Reset a valores por defecto"))
        {
            PivotCalculatorHelper.ResetToDefaults();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            $"Zona de búsqueda:\n" +
            $"Vertical: {PivotCalculatorHelper.CHEST_TOP_PERCENT:F2} a {PivotCalculatorHelper.CHEST_BOTTOM_PERCENT:F2}\n" +
            $"Horizontal: {PivotCalculatorHelper.CHEST_LEFT_PERCENT:F2} a {PivotCalculatorHelper.CHEST_RIGHT_PERCENT:F2}\n" +
            $"Opacidad: {PivotCalculatorHelper.ALPHA_THRESHOLD:F2}",
            MessageType.None
        );
    }
    
    void CopyPivotsFromReference()
    {
        string path = AssetDatabase.GetAssetPath(spriteSheet);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        
        if (importer == null)
        {
            Debug.LogError("No se pudo obtener el TextureImporter");
            return;
        }
        
        var metas = importer.spritesheet;
        if (metas == null || metas.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "Esta textura no tiene sprites múltiples configurados.", "OK");
            return;
        }

        SpriteMetaData? refMetaNullable = null;
        foreach (var m in metas)
        {
            if (m.name == referenceSpriteName)
            {
                refMetaNullable = m;
                break;
            }
        }
        
        if (!refMetaNullable.HasValue)
        {
            EditorUtility.DisplayDialog("Error", $"No se encontró un sprite con nombre '{referenceSpriteName}'.\n\nSprites disponibles:\n{string.Join("\n", metas.Select(m => m.name).Take(10))}", "OK");
            return;
        }

        SpriteMetaData refMeta = refMetaNullable.Value;
        Vector2 refPivot = refMeta.pivot;
        Rect refRect = refMeta.rect;

        Vector2 refLocalPivot = new Vector2(
            (refPivot.x - refRect.x) / refRect.width,
            (refPivot.y - refRect.y) / refRect.height
        );

        Debug.Log($"✅ Referencia '{refMeta.name}': pivot local = ({refLocalPivot.x:F3}, {refLocalPivot.y:F3})");

        if (!spriteSheet.isReadable)
        {
            importer.isReadable = true;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        // ✅ IMPORTANTE: Mantener el ORDEN ORIGINAL de los sprites - NO reordenar
        List<SpriteMetaData> newMeta = new List<SpriteMetaData>();

        // ✅ Iterar en el mismo orden que metas para preservar el ordenamiento
        foreach (var meta in metas)
        {
            // ✅ PRESERVAR TODA LA INFORMACIÓN ORIGINAL (nombre, rect, border, etc.)
            string originalName = meta.name;
            Rect r = meta.rect;

            Vector2 localPivot = refLocalPivot;

            Vector2 autoLocal = PivotCalculatorHelper.CalculateChestPivotLocal(spriteSheet, r);
            Vector2 blended = Vector2.Lerp(localPivot, autoLocal, 0.3f);

            Vector2 finalPivot = new Vector2(
                r.x + blended.x * r.width,
                r.y + blended.y * r.height
            );

            // ✅ Copiar toda la metadata original
            SpriteMetaData newM = meta;
            // ✅ Solo actualizar pivot y alignment, preservar TODO lo demás
            newM.alignment = (int)SpriteAlignment.Custom;
            newM.pivot = finalPivot;
            // ✅ Asegurar que el nombre se mantiene igual
            newM.name = originalName;
            
            // ✅ Verificar que el nombre no cambió
            if (newM.name != originalName)
            {
                Debug.LogError($"⚠️ ERROR: El nombre del sprite cambió de '{originalName}' a '{newM.name}'. Corrigiendo...");
                newM.name = originalName;
            }
            
            newMeta.Add(newM);

            Debug.Log($"{originalName}: ref={refLocalPivot:F3} auto={autoLocal:F3} final={blended:F3}");
        }

        importer.spritesheet = newMeta.ToArray();
        EditorUtility.SetDirty(importer);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        EditorUtility.DisplayDialog("Listo", $"✅ Pivots copiados desde '{refMeta.name}' (con ajuste suave 30%).\n\nSe procesaron {newMeta.Count} sprites.", "OK");
    }

    void CalculateAndApplyPivots()
    {
        string path = AssetDatabase.GetAssetPath(spriteSheet);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        
        if (importer == null)
        {
            Debug.LogError("No se pudo obtener el TextureImporter");
            return;
        }
        
        // ✅ CORRECCIÓN: Preservar todos los sprites originales del spritesheet
        var originalMetas = importer.spritesheet;
        if (originalMetas == null || originalMetas.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "Esta textura no tiene sprites múltiples configurados.", "OK");
            return;
        }
        
        if (!spriteSheet.isReadable)
        {
            importer.isReadable = true;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }
        
        // Crear un diccionario de sprites cargados para búsqueda rápida
        Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(path);
        Dictionary<string, Sprite> spriteDict = new Dictionary<string, Sprite>();
        foreach (Object obj in sprites)
        {
            if (obj is Sprite sprite)
            {
                spriteDict[sprite.name] = sprite;
            }
        }
        
        // ✅ CORRECCIÓN: Preservar TODOS los sprites originales, solo actualizar pivots
        // ✅ IMPORTANTE: Mantener el ORDEN ORIGINAL de los sprites - NO reordenar
        List<SpriteMetaData> newMetaData = new List<SpriteMetaData>();
        int processedCount = 0;
        int preservedCount = 0;
        
        // ✅ Iterar en el mismo orden que originalMetas para preservar el ordenamiento
        foreach (var originalMeta in originalMetas)
        {
            // ✅ PRESERVAR TODA LA INFORMACIÓN ORIGINAL (nombre, rect, border, etc.)
            SpriteMetaData newMeta = originalMeta;
            
            // ✅ NUNCA cambiar el nombre del sprite - preservar el original
            string originalName = originalMeta.name;
            
            // Si encontramos el sprite cargado, calcular el nuevo pivot
            if (spriteDict.TryGetValue(originalName, out Sprite sprite))
            {
                Vector2 localPivot = PivotCalculatorHelper.CalculateChestPivotLocal(spriteSheet, originalMeta.rect);
                
                Vector2 texPivot = new Vector2(
                    originalMeta.rect.x + localPivot.x * originalMeta.rect.width,
                    originalMeta.rect.y + localPivot.y * originalMeta.rect.height
                );
                
                // ✅ Solo actualizar pivot y alignment, preservar TODO lo demás
                newMeta.alignment = (int)SpriteAlignment.Custom;
                newMeta.pivot = texPivot;
                // ✅ Asegurar que el nombre se mantiene igual
                newMeta.name = originalName;
                
                Debug.Log($"{originalName}: LocalPivot = ({localPivot.x:F3}, {localPivot.y:F3})");
                processedCount++;
            }
            else
            {
                // Si no encontramos el sprite, preservar el original COMPLETO sin cambios
                Debug.LogWarning($"Sprite '{originalName}' no encontrado en assets, preservando configuración original.");
                preservedCount++;
            }
            
            // ✅ Verificar que el nombre no cambió
            if (newMeta.name != originalName)
            {
                Debug.LogError($"⚠️ ERROR: El nombre del sprite cambió de '{originalName}' a '{newMeta.name}'. Corrigiendo...");
                newMeta.name = originalName;
            }
            
            newMetaData.Add(newMeta);
        }
        
        importer.spritesheet = newMetaData.ToArray();
        EditorUtility.SetDirty(importer);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        
        EditorUtility.DisplayDialog(
            "Completado", 
            $"Se procesaron {processedCount} sprites (pivots calculados).\nSe preservaron {preservedCount} sprites (sin cambios).\nTotal: {newMetaData.Count} sprites.", 
            "OK"
        );
    }
}

public class PivotPreview : EditorWindow
{
    private Texture2D spriteSheet;
    private Vector2 scrollPos;
    private List<SpritePreviewData> previews = new List<SpritePreviewData>();
    
    private class SpritePreviewData
    {
        public string name;
        public Rect rect;
        public Vector2 localPivot;
        public Texture2D preview;
    }
    
    public static void OpenPreview(Texture2D texture)
    {
        PivotPreview window = GetWindow<PivotPreview>("Pivot Preview");
        window.spriteSheet = texture;
        window.GeneratePreviews();
    }
    
    void GeneratePreviews()
    {
        previews.Clear();
        
        string path = AssetDatabase.GetAssetPath(spriteSheet);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        
        if (!spriteSheet.isReadable)
        {
            importer.isReadable = true;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }
        
        Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(path);
        
        foreach (Object obj in sprites)
        {
            if (obj is Sprite sprite)
            {
                Rect rect = sprite.rect;
                Vector2 localPivot = PivotCalculatorHelper.CalculateChestPivotLocal(spriteSheet, rect);
                
                Texture2D preview = CreatePreviewWithDot(spriteSheet, rect, localPivot);
                
                previews.Add(new SpritePreviewData
                {
                    name = sprite.name,
                    rect = rect,
                    localPivot = localPivot,
                    preview = preview
                });
            }
        }
    }
    
    Texture2D CreatePreviewWithDot(Texture2D source, Rect rect, Vector2 localPivot)
    {
        int w = (int)rect.width;
        int h = (int)rect.height;
        
        Texture2D preview = new Texture2D(w, h);
        
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                Color pixel = source.GetPixel((int)rect.x + x, (int)rect.y + y);
                preview.SetPixel(x, y, pixel);
            }
        }
        
        int pivotX = Mathf.RoundToInt(localPivot.x * w);
        int pivotY = Mathf.RoundToInt(localPivot.y * h);
        
        for (int dy = -3; dy <= 3; dy++)
        {
            for (int dx = -3; dx <= 3; dx++)
            {
                if (dx*dx + dy*dy <= 9)
                {
                    int px = pivotX + dx;
                    int py = pivotY + dy;
                    if (px >= 0 && px < w && py >= 0 && py < h)
                    {
                        preview.SetPixel(px, py, Color.red);
                    }
                }
            }
        }
        
        preview.Apply();
        return preview;
    }
    
    void OnGUI()
    {
        GUILayout.Label("Vista Previa de Pivotes (modo auto)", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("El punto rojo muestra el pivot calculado localmente (pecho).", MessageType.Info);
        
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        
        foreach (var data in previews)
        {
            EditorGUILayout.BeginVertical("box");
            
            GUILayout.Label($"{data.name} - LocalPivot: ({data.localPivot.x:F3}, {data.localPivot.y:F3})");
            
            if (data.preview != null)
            {
                GUILayout.Label(data.preview, GUILayout.Width(data.rect.width), GUILayout.Height(data.rect.height));
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
        
        EditorGUILayout.EndScrollView();
    }
}

public static class PivotCalculatorHelper
{
    // Parámetros ajustables
    public static float CHEST_TOP_PERCENT = 0.35f;
    public static float CHEST_BOTTOM_PERCENT = 0.65f;
    public static float CHEST_LEFT_PERCENT = 0.4f;
    public static float CHEST_RIGHT_PERCENT = 0.6f;
    public static float ALPHA_THRESHOLD = 0.5f;
    
    // Valores por defecto
    private static float DEFAULT_TOP = 0.35f;
    private static float DEFAULT_BOTTOM = 0.65f;
    private static float DEFAULT_LEFT = 0.4f;
    private static float DEFAULT_RIGHT = 0.6f;
    private static float DEFAULT_ALPHA = 0.5f;
    
    public static void ResetToDefaults()
    {
        CHEST_TOP_PERCENT = DEFAULT_TOP;
        CHEST_BOTTOM_PERCENT = DEFAULT_BOTTOM;
        CHEST_LEFT_PERCENT = DEFAULT_LEFT;
        CHEST_RIGHT_PERCENT = DEFAULT_RIGHT;
        ALPHA_THRESHOLD = DEFAULT_ALPHA;
    }
    
    public static Vector2 CalculateChestPivotLocal(Texture2D texture, Rect spriteRect)
    {
        int startX = (int)spriteRect.x;
        int startY = (int)spriteRect.y;
        int width  = (int)spriteRect.width;
        int height = (int)spriteRect.height;

        int topY    = (int)(height * CHEST_TOP_PERCENT);
        int bottomY = (int)(height * CHEST_BOTTOM_PERCENT);

        int maxPixels = 0;
        int bestRow   = topY;

        for (int y = topY; y <= bottomY; y++)
        {
            int pixelCount = 0;
            for (int x = 0; x < width; x++)
            {
                Color pixel = texture.GetPixel(startX + x, startY + y);
                if (pixel.a > ALPHA_THRESHOLD)
                    pixelCount++;
            }

            if (pixelCount > maxPixels)
            {
                maxPixels = pixelCount;
                bestRow   = y;
            }
        }

        int leftX  = (int)(width * CHEST_LEFT_PERCENT);
        int rightX = (int)(width * CHEST_RIGHT_PERCENT);

        float sumX = 0;
        int totalPixels = 0;

        for (int x = leftX; x < rightX; x++)
        {
            Color pixel = texture.GetPixel(startX + x, startY + bestRow);
            if (pixel.a > ALPHA_THRESHOLD)
            {
                sumX += x;
                totalPixels++;
            }
        }

        float localX = totalPixels > 0 ? (sumX / totalPixels) / width : 0.5f;
        float localY = (bestRow) / (float)height;

        return new Vector2(localX, localY);
    }
}
