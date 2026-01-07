using UnityEngine;
using UnityEditor;
using System.IO;

[InitializeOnLoad]
public class AutoSetupOnLoad
{
    #region Hooks de Unity Editor
    static AutoSetupOnLoad()
    {
        // Esperar un frame para que Unity esté completamente cargado
        EditorApplication.delayCall += CheckAndSetup;
    }
    #endregion

    #region Setup del proyecto
    static void CheckAndSetup()
    {
        // Verificar si es la primera vez que se abre el proyecto
        if (!Directory.Exists("Assets/Scripts"))
        {
            // Crear carpetas automáticamente
            CreateFolders.CreateDefaultFolders();
        }

        // Verificar si existe GameManager en la escena
        if (GameObject.Find("GameManager") == null)
        {
            // No crear automáticamente, solo avisar
            Debug.Log("💡 Tip: Ve a Tools → Setup Game para configurar el juego automáticamente.");
        }
    }
    #endregion
}

