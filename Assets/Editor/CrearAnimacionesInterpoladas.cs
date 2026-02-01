using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;
using System.Linq;

public class CrearAnimacionesInterpoladas : EditorWindow
{
    private string spritesheetPath = "Assets/Sprites/Personajes/Guerrero/Spritesheet.png";
    private string outputFolder = "Assets/Animations/CaballeroInterpolado";
    private int keyframesCount = 30;
    private int framesPerKeyframe = 4;
    private float frameTime = 0.1f; // Tiempo por frame en segundos
    
    [MenuItem("Tools/Crear Animaciones Interpoladas")]
    public static void ShowWindow()
    {
        GetWindow<CrearAnimacionesInterpoladas>("Crear Animaciones Interpoladas");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Configuración de Animaciones Interpoladas", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "Este script crea Animation Clips y Animator Controller usando los sprites interpolados del Spritesheet.png.\n\n" +
            "Estructura esperada:\n" +
            "- 30 keyframes (0-29)\n" +
            "- 4 frames interpolados por keyframe (001-004)\n" +
            "- Nombres: CABALLERO_X_00Y donde X=keyframe, Y=frame interpolado",
            MessageType.Info
        );
        
        EditorGUILayout.Space();
        
        spritesheetPath = EditorGUILayout.TextField("Ruta del Spritesheet:", spritesheetPath);
        
        if (GUILayout.Button("Buscar Spritesheet.png"))
        {
            string path = EditorUtility.OpenFilePanel("Seleccionar Spritesheet.png", "Assets", "png");
            if (!string.IsNullOrEmpty(path))
            {
                if (path.StartsWith(Application.dataPath))
                {
                    spritesheetPath = "Assets" + path.Substring(Application.dataPath.Length);
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "El archivo debe estar dentro de la carpeta Assets.", "OK");
                }
            }
        }
        
        EditorGUILayout.Space();
        
        outputFolder = EditorGUILayout.TextField("Carpeta de salida:", outputFolder);
        
        if (GUILayout.Button("Seleccionar carpeta"))
        {
            string path = EditorUtility.OpenFolderPanel("Seleccionar carpeta de salida", "Assets", "");
            if (!string.IsNullOrEmpty(path))
            {
                if (path.StartsWith(Application.dataPath))
                {
                    outputFolder = "Assets" + path.Substring(Application.dataPath.Length);
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "La carpeta debe estar dentro de Assets.", "OK");
                }
            }
        }
        
        EditorGUILayout.Space();
        
        keyframesCount = EditorGUILayout.IntField("Cantidad de keyframes:", keyframesCount);
        framesPerKeyframe = EditorGUILayout.IntField("Frames por keyframe:", framesPerKeyframe);
        frameTime = EditorGUILayout.FloatField("Tiempo por frame (segundos):", frameTime);
        
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            $"Se crearán {keyframesCount} Animation Clips con {framesPerKeyframe} frames cada uno.\n" +
            $"Total de sprites esperados: {keyframesCount * framesPerKeyframe}",
            MessageType.None
        );
        
        EditorGUILayout.Space();
        
        GUI.backgroundColor = new Color(0.6f, 0.9f, 0.6f);
        if (GUILayout.Button("Crear Animaciones y Animator Controller", GUILayout.Height(40)))
        {
            CrearAnimaciones();
        }
        GUI.backgroundColor = Color.white;
    }
    
    void CrearAnimaciones()
    {
        // Validar spritesheet
        Object[] sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(spritesheetPath)
            .OfType<Sprite>()
            .ToArray();
        
        if (sprites.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", 
                $"No se encontraron sprites en {spritesheetPath}.\n\n" +
                "Asegurate de que el Spritesheet.png esté configurado como 'Multiple' en el Sprite Editor.",
                "OK");
            return;
        }
        
        Debug.Log($"✅ Encontrados {sprites.Length} sprites en el spritesheet");
        
        // Crear carpeta de salida
        if (!System.IO.Directory.Exists(outputFolder))
        {
            System.IO.Directory.CreateDirectory(outputFolder);
            AssetDatabase.Refresh();
        }
        
        // Organizar sprites por keyframe
        Dictionary<int, List<Sprite>> spritesPorKeyframe = new Dictionary<int, List<Sprite>>();
        
        foreach (Sprite sprite in sprites)
        {
            // Parsear nombre: CABALLERO_X_00Y
            string nombre = sprite.name;
            if (!nombre.StartsWith("CABALLERO_"))
            {
                Debug.LogWarning($"⚠️ Sprite con nombre inesperado: {nombre}");
                continue;
            }
            
            // Extraer keyframe y frame interpolado
            string[] partes = nombre.Split('_');
            if (partes.Length < 3)
            {
                Debug.LogWarning($"⚠️ Formato de nombre incorrecto: {nombre}");
                continue;
            }
            
            if (int.TryParse(partes[1], out int keyframe) && 
                int.TryParse(partes[2], out int frameInterpolado))
            {
                if (!spritesPorKeyframe.ContainsKey(keyframe))
                {
                    spritesPorKeyframe[keyframe] = new List<Sprite>();
                }
                
                spritesPorKeyframe[keyframe].Add(sprite);
            }
        }
        
        Debug.Log($"✅ Organizados {spritesPorKeyframe.Count} keyframes");
        
        // Verificar que tenemos todos los keyframes esperados
        if (spritesPorKeyframe.Count < keyframesCount)
        {
            EditorUtility.DisplayDialog("Advertencia",
                $"Se esperaban {keyframesCount} keyframes pero se encontraron {spritesPorKeyframe.Count}.\n\n" +
                "¿Continuar con los keyframes encontrados?",
                "Sí", "Cancelar");
        }
        
        // Crear Animation Clips
        List<AnimationClip> clips = new List<AnimationClip>();
        
        for (int keyframe = 0; keyframe < keyframesCount; keyframe++)
        {
            if (!spritesPorKeyframe.ContainsKey(keyframe))
            {
                Debug.LogWarning($"⚠️ Keyframe {keyframe} no encontrado, saltando...");
                continue;
            }
            
            List<Sprite> frames = spritesPorKeyframe[keyframe];
            
            // Ordenar frames por número interpolado (001, 002, 003, 004)
            frames = frames.OrderBy(s => 
            {
                string[] partes = s.name.Split('_');
                if (partes.Length >= 3 && int.TryParse(partes[2], out int num))
                    return num;
                return 0;
            }).ToList();
            
            if (frames.Count != framesPerKeyframe)
            {
                Debug.LogWarning($"⚠️ Keyframe {keyframe} tiene {frames.Count} frames en lugar de {framesPerKeyframe}");
            }
            
            // Crear Animation Clip
            AnimationClip clip = new AnimationClip();
            clip.name = $"Caballero_Keyframe_{keyframe:D2}";
            
            // Configurar como sprite animation
            clip.frameRate = 1f / frameTime;
            
            // Crear curva de binding para el sprite (compatible con UI Image)
            EditorCurveBinding spriteBinding = new EditorCurveBinding();
            spriteBinding.type = typeof(Image);
            spriteBinding.path = "";
            spriteBinding.propertyName = "m_Sprite";
            
            ObjectReferenceKeyframe[] spriteKeyframes = new ObjectReferenceKeyframe[frames.Count];
            
            for (int i = 0; i < frames.Count; i++)
            {
                spriteKeyframes[i] = new ObjectReferenceKeyframe();
                spriteKeyframes[i].time = i * frameTime;
                spriteKeyframes[i].value = frames[i];
            }
            
            AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, spriteKeyframes);
            
            // Guardar clip
            string clipPath = $"{outputFolder}/{clip.name}.anim";
            AssetDatabase.CreateAsset(clip, clipPath);
            clips.Add(clip);
            
            Debug.Log($"✅ Creado clip: {clip.name} con {frames.Count} frames");
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // Crear Animator Controller
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(
            $"{outputFolder}/CaballeroInterpolado.controller");
        
        // Agregar todas las animaciones al controller
        foreach (AnimationClip clip in clips)
        {
            AnimatorState state = controller.layers[0].stateMachine.AddState(clip.name);
            state.motion = clip;
        }
        
        // Configurar transiciones (opcional: todas las animaciones pueden transicionar entre sí)
        // Por ahora, dejamos que el usuario configure las transiciones manualmente
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // Mostrar resultado
        EditorUtility.DisplayDialog("✅ Completado",
            $"Se crearon {clips.Count} Animation Clips y 1 Animator Controller.\n\n" +
            $"Ubicación: {outputFolder}\n\n" +
            $"Controller: CaballeroInterpolado.controller",
            "OK");
        
        // Seleccionar el controller en el Project
        Object controllerObj = AssetDatabase.LoadAssetAtPath<Object>(
            $"{outputFolder}/CaballeroInterpolado.controller");
        if (controllerObj != null)
        {
            EditorGUIUtility.PingObject(controllerObj);
            Selection.activeObject = controllerObj;
        }
        
        Debug.Log($"✅ Proceso completado: {clips.Count} clips creados");
    }
}
