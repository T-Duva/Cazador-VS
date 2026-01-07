using UnityEngine;
using UnityEditor;
#if UNITY_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[InitializeOnLoad]
public static class CorregirInputSystem
{
    #region Hooks de Unity Editor
    static CorregirInputSystem()
    {
        EditorApplication.delayCall += ConfigurarInputSystem;
    }
    #endregion
    
    #region Configuracion
    static void ConfigurarInputSystem()
    {
        #if UNITY_INPUT_SYSTEM
        try
        {
            var settings = InputSystem.settings;
            if (settings != null)
            {
                // Aumentar el límite de eventos o eliminarlo
                settings.maxEventBytesPerUpdate = 0; // 0 = sin límite
                Debug.Log("✅ Input System configurado: límite de eventos eliminado");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"⚠️ No se pudo configurar Input System: {e.Message}");
        }
        #endif
    }
    #endregion
    
    #region Menu
    [MenuItem("Tools/Corregir Input System")]
    public static void CorregirInputSystemManual()
    {
        #if UNITY_INPUT_SYSTEM
        try
        {
            var settings = InputSystem.settings;
            if (settings != null)
            {
                settings.maxEventBytesPerUpdate = 0; // 0 = sin límite
                EditorUtility.DisplayDialog("✅ Configuración Aplicada", 
                    "El límite de eventos del Input System ha sido eliminado.\n\n" +
                    "Esto debería resolver el error de eventos descartados.", 
                    "OK");
                Debug.Log("✅ Input System configurado correctamente");
            }
            else
            {
                EditorUtility.DisplayDialog("⚠️ Error", 
                    "No se pudo acceder a la configuración del Input System.", 
                    "OK");
            }
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("⚠️ Error", 
                $"Error al configurar Input System:\n{e.Message}", 
                "OK");
            Debug.LogError($"Error configurando Input System: {e}");
        }
        #else
        EditorUtility.DisplayDialog("ℹ️ Información", 
            "El nuevo Input System no está habilitado en este proyecto.", 
            "OK");
        #endif
    }
    #endregion
}

