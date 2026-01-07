using UnityEngine;
using UnityEditor;

public class ConfigurarIconosHUD : AssetPostprocessor
{
    private void OnPreprocessTexture()
    {
        // Lista de nombres de iconos que deben tener configuración pixel nítida
        string[] nombresIconos = { "IconoDaño", "IconoVida", "IconoEscudo", "IconoBatalla", "IconoOro" };
        
        string nombreArchivo = System.IO.Path.GetFileNameWithoutExtension(assetPath);
        
        // Verificar si el archivo es uno de los iconos del HUD
        bool esIconoHUD = false;
        foreach (string nombre in nombresIconos)
        {
            if (nombreArchivo == nombre)
            {
                esIconoHUD = true;
                break;
            }
        }
        
        if (esIconoHUD)
        {
            TextureImporter textureImporter = (TextureImporter)assetImporter;
            
            // Configuración pixel nítida
            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.spriteImportMode = SpriteImportMode.Single;
            textureImporter.alphaIsTransparency = true;
            textureImporter.filterMode = FilterMode.Point; // Point (no filter)
            textureImporter.textureCompression = TextureImporterCompression.Uncompressed; // None / Uncompressed
            textureImporter.mipmapEnabled = false; // MipMaps = Disabled
            
            // Asegurar que sea legible si es necesario
            textureImporter.isReadable = false; // No necesario para sprites, pero lo dejamos en false
            
            Debug.Log($"Icono HUD configurado: {nombreArchivo} - Pixel nítido, sin compresión, sin mipmaps");
        }
    }
}

