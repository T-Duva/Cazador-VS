#pragma warning disable CS0618 // TextureImporter.spritesheet está obsoleto pero sigue funcionando
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class RebuildSpriteSheet : EditorWindow
{
    private Texture2D spriteSheet;
    private int maxCols = 8;
    private int padding = 0;
    
    [MenuItem("Tools/Rebuild Sprite Sheet")]
    public static void ShowWindow()
    {
        GetWindow<RebuildSpriteSheet>("Rebuild Sprite Sheet");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Reorganizar Sprite Sheet", EditorStyles.boldLabel);
        
        spriteSheet = (Texture2D)EditorGUILayout.ObjectField(
            "Sprite Sheet:", 
            spriteSheet, 
            typeof(Texture2D), 
            false
        );
        
        maxCols = EditorGUILayout.IntField("Máximo de columnas:", maxCols);
        padding = EditorGUILayout.IntField("Padding:", padding);
        
        if (GUILayout.Button("Reorganizar y Crear Nueva Sheet"))
        {
            if (spriteSheet != null)
            {
                RebuildSheet();
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Selecciona una Sprite Sheet", "OK");
            }
        }
        
        EditorGUILayout.HelpBox(
            "Esto crea una nueva sprite sheet donde:\n" +
            "- Todas las celdas tienen el mismo tamaño\n" +
            "- Los sprites están centrados\n" +
            "- Organizados en filas y columnas", 
            MessageType.Info
        );
    }
    
    void RebuildSheet()
    {
        string path = AssetDatabase.GetAssetPath(spriteSheet);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        
        if (!spriteSheet.isReadable)
        {
            importer.isReadable = true;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }
        
        // Encontrar todos los sprites
        List<Sprite> sprites = new List<Sprite>();
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        
        foreach (Object obj in assets)
        {
            if (obj is Sprite sprite)
            {
                sprites.Add(sprite);
            }
        }
        
        if (sprites.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "No hay sprites en esta sheet", "OK");
            return;
        }
        
        // Ordenar por NOMBRE (para mantener orden correcto)
        sprites.Sort((a, b) => a.name.CompareTo(b.name));
        
        // Calcular tamaño máximo
        float maxW = 0, maxH = 0;
        foreach (var sprite in sprites)
        {
            maxW = Mathf.Max(maxW, sprite.rect.width);
            maxH = Mathf.Max(maxH, sprite.rect.height);
        }
        
        int cell = (int)Mathf.Max(maxW, maxH) + padding * 2;
        int cols = maxCols;
        int rows = Mathf.CeilToInt(sprites.Count / (float)cols);
        
        int newWidth = cols * cell;
        int newHeight = rows * cell;
        
        // Crear texture nueva
        Texture2D newSheet = new Texture2D(newWidth, newHeight, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[newWidth * newHeight];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = new Color(0, 0, 0, 0);
        }
        newSheet.SetPixels(pixels);
        
        // Pegar cada sprite en su celda
        for (int idx = 0; idx < sprites.Count; idx++)
        {
            Sprite sprite = sprites[idx];
            Rect rect = sprite.rect;
            int w = (int)rect.width;
            int h = (int)rect.height;
            
            // Leer píxeles del sprite original
            Color[] spritePixels = spriteSheet.GetPixels((int)rect.x, (int)rect.y, w, h);
            
            // Calcular posición en la nueva grid
            int col = idx % cols;
            int row = idx / cols;
            int cellX = col * cell;
            int cellY = row * cell;
            
            // Centrar sprite en su celda
            int pasteX = cellX + (cell - w) / 2;
            int pasteY = cellY + (cell - h) / 2;
            
            // Pegar píxeles
            newSheet.SetPixels(pasteX, pasteY, w, h, spritePixels);
        }
        
        newSheet.Apply();
        
        // Guardar texture
        byte[] pngData = newSheet.EncodeToPNG();
        string outputPath = Path.Combine(Path.GetDirectoryName(path), 
            Path.GetFileNameWithoutExtension(path) + "_ordenado.png");
        
        File.WriteAllBytes(outputPath, pngData);
        AssetDatabase.Refresh();
        
        // Configurar import settings para la nueva sheet
        TextureImporter newImporter = AssetImporter.GetAtPath(outputPath) as TextureImporter;
        if (newImporter != null)
        {
            newImporter.textureType = TextureImporterType.Sprite;
            newImporter.spriteImportMode = SpriteImportMode.Multiple;
            newImporter.spritePixelsPerUnit = 100;
            newImporter.filterMode = FilterMode.Point;
            newImporter.isReadable = true;
            
            // Crear metadata para cada sprite en la grid
            List<SpriteMetaData> metaData = new List<SpriteMetaData>();
            
            for (int idx = 0; idx < sprites.Count; idx++)
            {
                int col = idx % cols;
                int row = idx / cols;
                
                SpriteMetaData data = new SpriteMetaData
                {
                    name = sprites[idx].name,
                    rect = new Rect(col * cell, row * cell, cell, cell),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f)
                };
                
                metaData.Add(data);
            }
            
            newImporter.spritesheet = metaData.ToArray();
            EditorUtility.SetDirty(newImporter);
            AssetDatabase.ImportAsset(outputPath, ImportAssetOptions.ForceUpdate);
        }
        
        EditorUtility.DisplayDialog(
            "Completado",
            $"✅ Sprite sheet reorganizada:\n{Path.GetFileName(outputPath)}\n\n" +
            $"Tamaño de celda: {cell}×{cell}\n" +
            $"Grid: {cols}×{rows}\n" +
            $"Sprites: {sprites.Count}\n\n" +
            $"Ahora puedes usar el Auto Pivot Calculator en esta nueva sheet.",
            "OK"
        );
        
        Debug.Log($"✅ Nueva sheet: {outputPath}");
        Debug.Log($"   Celda: {cell}×{cell}, Grid: {cols}×{rows}");
    }
}