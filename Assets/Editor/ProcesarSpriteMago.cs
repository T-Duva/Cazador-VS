using UnityEngine;
using UnityEditor;
using System.IO;

public class ProcesarSpriteMago : EditorWindow
{
    #region Menu
    [MenuItem("Tools/Procesar Sprite Sheet Mago")]
    public static void ProcesarSpriteSheet()
    {
        #region Rutas
        string spriteSheetPath = "Assets/Sprites/Personajes/magossss.png";
        string outputBase = "Assets/Sprites/Personajes/Mago";
        #endregion
        
        #region Configuracion del sheet
        // Dimensiones del sprite sheet
        int sheetWidth = 1920;
        int sheetHeight = 1080;
        int rows = 6;
        int cols = 8;
        int frameWidth = sheetWidth / cols;  // 240
        int frameHeight = sheetHeight / rows;  // 180
        
        // Tamaño objetivo para escalar
        int targetSize = 256;
        #endregion
        
        #region Carga del sheet
        // Cargar el sprite sheet
        Texture2D spriteSheet = AssetDatabase.LoadAssetAtPath<Texture2D>(spriteSheetPath);
        if (spriteSheet == null)
        {
            Debug.LogError($"No se pudo cargar el sprite sheet: {spriteSheetPath}");
            return;
        }
        #endregion
        
        #region Lectura de pixeles
        // Leer los píxeles del sprite sheet
        Color[] pixels = spriteSheet.GetPixels();
        #endregion
        
        #region Definicion de animaciones
        // Definir las animaciones según la descripción
        // (row, col) - row y col empiezan en 0
        var animaciones = new System.Collections.Generic.Dictionary<string, (int row, int col)[]>
        {
            ["idle"] = new (int, int)[]
            {
                (0, 0),  // Row 1, Col 1
                (0, 1),  // Row 1, Col 2
                (0, 2),  // Row 1, Col 3
            },
            ["attack"] = new (int, int)[]
            {
                (1, 3),  // Row 2, Col 4
                (1, 4),  // Row 2, Col 5
                (1, 5),  // Row 2, Col 6
                (2, 3),  // Row 3, Col 4
                (2, 4),  // Row 3, Col 5
                (2, 5),  // Row 3, Col 6
                (2, 6),  // Row 3, Col 7
            },
            ["hurt"] = new (int, int)[]
            {
                (3, 1),  // Row 4, Col 2
                (3, 2),  // Row 4, Col 3
            }
        };
        #endregion
        
        #region Preparacion de carpetas
        // Crear carpetas si no existen
        foreach (string animName in animaciones.Keys)
        {
            string animFolder = Path.Combine(outputBase, animName);
            if (!Directory.Exists(animFolder))
            {
                Directory.CreateDirectory(animFolder);
                AssetDatabase.Refresh();
            }
        }
        #endregion
        
        #region Extraccion y guardado
        // Extraer y procesar cada animación
        foreach (var anim in animaciones)
        {
            string animName = anim.Key;
            var frames = anim.Value;
            
            Debug.Log($"Procesando animación: {animName} ({frames.Length} frames)");
            
            for (int idx = 0; idx < frames.Length; idx++)
            {
                int row = frames[idx].row;
                int col = frames[idx].col;
                
                // Calcular posición en el sprite sheet
                int x = col * frameWidth;
                int y = row * frameHeight;
                
                // Extraer el frame original
                Color[] framePixels = new Color[frameWidth * frameHeight];
                for (int py = 0; py < frameHeight; py++)
                {
                    for (int px = 0; px < frameWidth; px++)
                    {
                        int sourceX = x + px;
                        int sourceY = (sheetHeight - 1) - (y + py); // Unity usa Y invertido
                        int sourceIndex = sourceY * sheetWidth + sourceX;
                        int destIndex = py * frameWidth + px;
                        framePixels[destIndex] = pixels[sourceIndex];
                    }
                }
                
                // Crear textura del frame original
                Texture2D frameTexture = new Texture2D(frameWidth, frameHeight);
                frameTexture.SetPixels(framePixels);
                frameTexture.Apply();
                
                // Escalar a 256x256
                RenderTexture rt = RenderTexture.GetTemporary(targetSize, targetSize);
                Graphics.Blit(frameTexture, rt);
                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = rt;
                Texture2D scaledTexture = new Texture2D(targetSize, targetSize);
                scaledTexture.ReadPixels(new Rect(0, 0, targetSize, targetSize), 0, 0);
                scaledTexture.Apply();
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(rt);
                
                // Guardar el frame
                byte[] pngData = scaledTexture.EncodeToPNG();
                string outputPath = Path.Combine(outputBase, animName, $"{idx + 1}.png");
                File.WriteAllBytes(outputPath, pngData);
                
                Debug.Log($"  Frame {idx + 1} guardado: {outputPath} ({targetSize}x{targetSize})");
                
                // Limpiar
                DestroyImmediate(frameTexture);
                DestroyImmediate(scaledTexture);
            }
        }
        #endregion
        
        #region Finalizacion
        AssetDatabase.Refresh();
        Debug.Log("¡Procesamiento completado! Todos los frames escalados a 256x256");
        #endregion
    }
    #endregion
}

