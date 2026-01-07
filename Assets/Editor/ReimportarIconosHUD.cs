using UnityEngine;
using UnityEditor;
using System.IO;

public class ReimportarIconosHUD : EditorWindow
{
    [MenuItem("Tools/Reimportar Iconos HUD")]
    public static void ReimportarIconos()
    {
        string[] nombresIconos = { "IconoDaño", "IconoVida", "IconoEscudo", "IconoBatalla", "IconoOro" };
        
        int encontrados = 0;
        int reimportados = 0;
        
        foreach (string nombre in nombresIconos)
        {
            // Buscar el archivo en Assets
            string[] guids = AssetDatabase.FindAssets(nombre);
            
            foreach (string guid in guids)
            {
                string ruta = AssetDatabase.GUIDToAssetPath(guid);
                string nombreSinExtension = Path.GetFileNameWithoutExtension(ruta);
                
                if (nombreSinExtension == nombre)
                {
                    encontrados++;
                    
                    // Reimportar el archivo para aplicar la configuración
                    AssetDatabase.ImportAsset(ruta, ImportAssetOptions.ForceUpdate);
                    reimportados++;
                    
                    Debug.Log($"Reimportado: {ruta}");
                    break;
                }
            }
        }
        
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("Reimportación Completada", 
            $"Iconos encontrados: {encontrados}\nIconos reimportados: {reimportados}", 
            "OK");
        
        Debug.Log($"Reimportación completada: {reimportados} iconos procesados");
    }
}

