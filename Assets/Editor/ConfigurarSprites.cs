using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class ConfigurarSprites : EditorWindow
{
    #region Ventana (MenuItem)
    [MenuItem("Tools/Configurar Sprites de Personajes")]
    public static void ShowWindow()
    {
        ConfigurarSprites window = GetWindow<ConfigurarSprites>("Configurar Sprites");
        window.Show();
    }
    #endregion

    #region GUI
    void OnGUI()
    {
        GUILayout.Label("Configuración Automática de Sprites", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("Este script configurará automáticamente todos los sprites\nen Assets/Sprites/Personajes/", EditorStyles.wordWrappedLabel);
        GUILayout.Space(10);
        
        if (GUILayout.Button("🔧 Configurar Todos los Sprites", GUILayout.Height(40)))
        {
            ConfigurarTodosLosSprites();
        }
        
        GUILayout.Space(10);
        GUILayout.Label("Configuración:", EditorStyles.boldLabel);
        GUILayout.Label("• Texture Type: Sprite (2D and UI)");
        GUILayout.Label("• Sprite Mode: Single");
        GUILayout.Label("• Pixels Per Unit: 100");
        GUILayout.Label("• Filter Mode: Bilinear");
        GUILayout.Label("• Max Size: 2048");
    }
    #endregion

    #region Procesamiento
    static void ConfigurarTodosLosSprites()
    {
        string[] carpetasPersonajes = { 
            "Assets/Sprites/Personajes/Guerrero",
            "Assets/Sprites/Personajes/Mago", 
            "Assets/Sprites/Personajes/Cazador" 
        };

        int totalConfigurados = 0;
        int totalErrores = 0;

        foreach (string carpeta in carpetasPersonajes)
        {
            if (!Directory.Exists(carpeta))
            {
                Debug.LogWarning($"⚠️ Carpeta no encontrada: {carpeta}");
                continue;
            }

            // Buscar todos los archivos .png en la carpeta
            string[] archivos = Directory.GetFiles(carpeta, "*.png", SearchOption.TopDirectoryOnly);
            
            foreach (string archivo in archivos)
            {
                // Convertir ruta del sistema a ruta de assets de Unity
                string rutaAsset = archivo.Replace('\\', '/');
                if (!rutaAsset.StartsWith("Assets/"))
                {
                    // Obtener la ruta relativa a Assets
                    int indice = rutaAsset.IndexOf("Assets/");
                    if (indice >= 0)
                    {
                        rutaAsset = rutaAsset.Substring(indice);
                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ No se pudo convertir la ruta: {archivo}");
                        totalErrores++;
                        continue;
                    }
                }

                // Cargar el objeto de textura
                TextureImporter importer = AssetImporter.GetAtPath(rutaAsset) as TextureImporter;
                
                if (importer == null)
                {
                    Debug.LogWarning($"⚠️ No se pudo cargar el importer para: {rutaAsset}");
                    totalErrores++;
                    continue;
                }

                // Configurar el sprite
                bool necesitaReimportar = false;

                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    necesitaReimportar = true;
                }

                if (importer.spriteImportMode != SpriteImportMode.Single)
                {
                    importer.spriteImportMode = SpriteImportMode.Single;
                    necesitaReimportar = true;
                }

                if (importer.spritePixelsPerUnit != 100)
                {
                    importer.spritePixelsPerUnit = 100;
                    necesitaReimportar = true;
                }

                if (importer.filterMode != FilterMode.Bilinear)
                {
                    importer.filterMode = FilterMode.Bilinear;
                    necesitaReimportar = true;
                }

                // Configurar max size
                if (importer.maxTextureSize != 2048)
                {
                    importer.maxTextureSize = 2048;
                    necesitaReimportar = true;
                }

                // Configurar compression
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                
                // Configurar alpha
                if (importer.alphaSource != TextureImporterAlphaSource.FromInput)
                {
                    importer.alphaSource = TextureImporterAlphaSource.FromInput;
                    necesitaReimportar = true;
                }

                // Reimportar si es necesario
                if (necesitaReimportar)
                {
                    AssetDatabase.ImportAsset(rutaAsset, ImportAssetOptions.ForceUpdate);
                    totalConfigurados++;
                }
            }
        }

        AssetDatabase.Refresh();

        if (totalConfigurados > 0)
        {
            EditorUtility.DisplayDialog("✅ Configuración Completa", 
                $"Se configuraron {totalConfigurados} sprites correctamente.\n\n" +
                $"Errores: {totalErrores}", 
                "OK");
            Debug.Log($"✅ Configuración completa: {totalConfigurados} sprites configurados, {totalErrores} errores");
        }
        else if (totalErrores > 0)
        {
            EditorUtility.DisplayDialog("⚠️ Advertencia", 
                $"No se pudieron configurar los sprites.\n\n" +
                $"Verifica que los sprites estén en:\n" +
                $"• Assets/Sprites/Personajes/Guerrero/\n" +
                $"• Assets/Sprites/Personajes/Mago/\n" +
                $"• Assets/Sprites/Personajes/Cazador/", 
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("ℹ️ Información", 
                "No se encontraron sprites para configurar.\n\n" +
                "Asegúrate de que los sprites estén en las carpetas correctas.", 
                "OK");
        }
    }
    #endregion
}

