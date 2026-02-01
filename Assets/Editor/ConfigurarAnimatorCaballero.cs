using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine.UI;
using System.Linq;

public class ConfigurarAnimatorCaballero : EditorWindow
{
    private RuntimeAnimatorController animatorController;
    private Image targetImage;
    
    [MenuItem("Tools/Configurar Animator Caballero")]
    public static void ShowWindow()
    {
        GetWindow<ConfigurarAnimatorCaballero>("Configurar Animator Caballero");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Configuración de Animator para Caballero", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "Este script configura automáticamente el Animator en el GameObject ImageJugador.\n\n" +
            "Pasos:\n" +
            "1. Selecciona el Animator Controller generado\n" +
            "2. El script buscará el ImageJugador automáticamente\n" +
            "3. Agregará el componente Animator y lo configurará",
            MessageType.Info
        );
        
        EditorGUILayout.Space();
        
        animatorController = (RuntimeAnimatorController)EditorGUILayout.ObjectField(
            "Animator Controller:", 
            animatorController, 
            typeof(RuntimeAnimatorController), 
            false
        );
        
        if (GUILayout.Button("Buscar CaballeroInterpolado.controller"))
        {
            string[] guids = AssetDatabase.FindAssets("CaballeroInterpolado t:AnimatorController");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                animatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(path);
                Debug.Log($"✅ Encontrado: {path}");
            }
            else
            {
                EditorUtility.DisplayDialog("No encontrado", 
                    "No se encontró CaballeroInterpolado.controller.\n\n" +
                    "Ejecuta primero 'Tools > Crear Animaciones Interpoladas'.",
                    "OK");
            }
        }
        
        EditorGUILayout.Space();
        
        targetImage = (Image)EditorGUILayout.ObjectField(
            "Image del Jugador (opcional):", 
            targetImage, 
            typeof(Image), 
            true
        );
        
        if (GUILayout.Button("Buscar ImageJugador automáticamente"))
        {
            Image[] images = FindObjectsOfType<Image>();
            targetImage = images.FirstOrDefault(img => 
                img.name.Contains("Jugador") || 
                img.name.Contains("ImgJugador") ||
                img.name.Contains("ImageJugador"));
            
            if (targetImage != null)
            {
                Debug.Log($"✅ Encontrado: {targetImage.name}");
            }
            else
            {
                EditorUtility.DisplayDialog("No encontrado", 
                    "No se encontró ImageJugador en la escena.\n\n" +
                    "Asegurate de que el juego esté inicializado o selecciona manualmente.",
                    "OK");
            }
        }
        
        EditorGUILayout.Space();
        
        GUI.backgroundColor = new Color(0.6f, 0.9f, 0.6f);
        if (GUILayout.Button("Configurar Animator", GUILayout.Height(40)))
        {
            ConfigurarAnimator();
        }
        GUI.backgroundColor = Color.white;
    }
    
    void ConfigurarAnimator()
    {
        if (animatorController == null)
        {
            EditorUtility.DisplayDialog("Error", 
                "Selecciona un Animator Controller primero.",
                "OK");
            return;
        }
        
        // Buscar ImageJugador si no está asignado
        if (targetImage == null)
        {
            Image[] images = FindObjectsOfType<Image>();
            targetImage = images.FirstOrDefault(img => 
                img.name.Contains("Jugador") || 
                img.name.Contains("ImgJugador") ||
                img.name.Contains("ImageJugador"));
        }
        
        if (targetImage == null)
        {
            EditorUtility.DisplayDialog("Error", 
                "No se encontró ImageJugador.\n\n" +
                "Asegurate de que el juego esté inicializado o selecciona manualmente el Image.",
                "OK");
            return;
        }
        
        // Agregar o obtener Animator component
        Animator animator = targetImage.GetComponent<Animator>();
        if (animator == null)
        {
            animator = targetImage.gameObject.AddComponent<Animator>();
            Debug.Log($"✅ Componente Animator agregado a {targetImage.name}");
        }
        
        // Asignar el Animator Controller
        animator.runtimeAnimatorController = animatorController;
        animator.updateMode = AnimatorUpdateMode.UnscaledTime; // Para que funcione con UI
        
        EditorUtility.SetDirty(animator);
        EditorUtility.SetDirty(targetImage.gameObject);
        
        EditorUtility.DisplayDialog("✅ Completado", 
            $"Animator configurado correctamente en {targetImage.name}.\n\n" +
            $"Controller: {animatorController.name}\n\n" +
            "Ahora el código puede usar el Animator para las animaciones.",
            "OK");
        
        Debug.Log($"✅ Animator configurado: {targetImage.name} -> {animatorController.name}");
        
        // Seleccionar el GameObject en la jerarquía
        Selection.activeGameObject = targetImage.gameObject;
    }
}
