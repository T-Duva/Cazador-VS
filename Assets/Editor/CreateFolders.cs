using UnityEngine;
using UnityEditor;
using System.IO;

public class CreateFolders : AssetPostprocessor
{
    #region Hooks de Unity Editor
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, 
        string[] movedAssets, string[] movedFromAssetPaths)
    {
        CreateDefaultFolders();
    }
    #endregion

    #region Menu / Creacion de carpetas
    [MenuItem("Tools/Create Default Folders")]
    public static void CreateDefaultFolders()
    {
        // Carpetas principales
        string[] folders = {
            "Assets/Scripts",
            "Assets/Sprites",
            "Assets/Scenes",
            "Assets/UI",
            "Assets/Prefabs",
            "Assets/Materials",
            "Assets/Animations",
            "Assets/Audio"
        };

        foreach (string folder in folders)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
                Debug.Log($"Carpeta creada: {folder}");
            }
        }

        // Refrescar el proyecto para que Unity reconozca las nuevas carpetas
        AssetDatabase.Refresh();
        
        Debug.Log("✅ Todas las carpetas han sido creadas correctamente!");
    }
    #endregion

    #region Auto-creacion al cargar
    // Esto se ejecuta automáticamente cuando se importa el script
    [InitializeOnLoadMethod]
    static void CreateFoldersOnLoad()
    {
        // Solo crear si no existen
        if (!Directory.Exists("Assets/Scripts"))
        {
            CreateDefaultFolders();
        }
    }
    #endregion
}

