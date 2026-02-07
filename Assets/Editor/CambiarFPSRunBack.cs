using UnityEngine;
using UnityEditor;

public class CambiarFPSRunBack : EditorWindow
{
    [MenuItem("Tools/CAMBIAR FPS Run+Back")]
    public static void ShowWindow()
    {
        GetWindow<CambiarFPSRunBack>("Cambiar FPS Run+Back");
    }

    private float nuevoFPS = 60f;

    private void OnGUI()
    {
        GUILayout.Label("Cambiar Frame Rate de Run+Back", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "Este script cambia el Frame Rate del clip Run+Back.anim directamente.\n" +
            "Útil cuando el Inspector no permite editarlo manualmente.",
            MessageType.Info
        );
        
        EditorGUILayout.Space();
        
        // Cargar el clip actual para mostrar su FPS
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(
            "Assets/Animations/CaballeroInterpolado/Run+Back.anim"
        );
        
        if (clip != null)
        {
            EditorGUILayout.LabelField("FPS Actual:", clip.frameRate.ToString("F0"));
        }
        else
        {
            EditorGUILayout.HelpBox("⚠️ No se encontró Run+Back.anim", MessageType.Warning);
        }
        
        EditorGUILayout.Space();
        
        nuevoFPS = EditorGUILayout.FloatField("Nuevo FPS:", nuevoFPS);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("🔄 Cambiar FPS", GUILayout.Height(40)))
        {
            if (clip != null)
            {
                // Cambiar el frameRate directamente
                clip.frameRate = nuevoFPS;
                
                // Marcar como modificado y guardar
                EditorUtility.SetDirty(clip);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                Debug.Log($"<color=green>✅ Frame Rate de Run+Back cambiado a {nuevoFPS} FPS</color>");
                EditorUtility.DisplayDialog("Éxito", 
                    $"Frame Rate cambiado a {nuevoFPS} FPS.\n\n" +
                    "El cambio ya está aplicado. Podés verificar en el Inspector.", 
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Error", 
                    "No se encontró el clip Run+Back.anim.\n\n" +
                    "Asegurate de que existe en:\n" +
                    "Assets/Animations/CaballeroInterpolado/Run+Back.anim", 
                    "OK");
            }
        }
        
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "💡 Tip: Después de cambiar, seleccioná el clip en el Project\n" +
            "y verificá en el Inspector que el cambio se aplicó.",
            MessageType.None
        );
    }
}
