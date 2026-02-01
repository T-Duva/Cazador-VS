using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;
using System.Linq;

public class CrearAnimacionesInterpoladas : EditorWindow
{
    private string spritesFolder = "Assets/Sprites/Personajes/Guerrero/NUEVO";
    private string outputFolder = "Assets/Animations/CaballeroInterpolado";
    private int keyframesCount = 30;
    private int framesPerKeyframe = 4;
    private float frameTime = 0.1f; // Tiempo por frame en segundos
    private bool usarSpritesheet = false; // Toggle entre spritesheet o PNG individuales
    
    [MenuItem("Tools/Crear Animaciones Interpoladas", false, 1)]
    public static void ShowWindow()
    {
        GetWindow<CrearAnimacionesInterpoladas>("Crear Animaciones Interpoladas");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Configuración de Animaciones Interpoladas", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "Este script crea Animation Clips y Animator Controller usando los sprites interpolados.\n\n" +
            "Estructura esperada:\n" +
            "- 30 keyframes (0-29)\n" +
            "- 4 frames interpolados por keyframe (001-004)\n" +
            "- Nombres: CABALLERO_X_00Y donde X=keyframe, Y=frame interpolado",
            MessageType.Info
        );
        
        EditorGUILayout.Space();
        
        usarSpritesheet = EditorGUILayout.Toggle("Usar Spritesheet (en lugar de PNG individuales)", usarSpritesheet);
        
        EditorGUILayout.Space();
        
        if (usarSpritesheet)
        {
            EditorGUILayout.LabelField("Ruta del Spritesheet:", EditorStyles.boldLabel);
            string spritesheetPath = EditorGUILayout.TextField("", "Assets/Sprites/Personajes/Guerrero/Spritesheet.png");
            
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
        }
        else
        {
            EditorGUILayout.LabelField("Carpeta con PNG individuales:", EditorStyles.boldLabel);
            spritesFolder = EditorGUILayout.TextField("", spritesFolder);
            
            if (GUILayout.Button("Buscar carpeta"))
            {
                string path = EditorUtility.OpenFolderPanel("Seleccionar carpeta con PNG", "Assets", "");
                if (!string.IsNullOrEmpty(path))
                {
                    if (path.StartsWith(Application.dataPath))
                    {
                        spritesFolder = "Assets" + path.Substring(Application.dataPath.Length);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Error", "La carpeta debe estar dentro de Assets.", "OK");
                    }
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
        Sprite[] sprites = null;
        
        if (usarSpritesheet)
        {
            // Modo Spritesheet (código original)
            string spritesheetPath = "Assets/Sprites/Personajes/Guerrero/Spritesheet.png";
            
            if (!System.IO.File.Exists(spritesheetPath.Replace("Assets/", Application.dataPath + "/")))
            {
                EditorUtility.DisplayDialog("Error", 
                    $"El archivo no existe: {spritesheetPath}\n\n" +
                    "Verifica la ruta del Spritesheet.png.",
                    "OK");
                Debug.LogError($"❌ Archivo no encontrado: {spritesheetPath}");
                return;
            }
            
            TextureImporter importer = AssetImporter.GetAtPath(spritesheetPath) as TextureImporter;
            if (importer == null || importer.textureType != TextureImporterType.Sprite || 
                importer.spriteImportMode != SpriteImportMode.Multiple)
            {
                EditorUtility.DisplayDialog("Error", 
                    $"El Spritesheet.png no está configurado correctamente.\n\n" +
                    "Debe ser: Texture Type = Sprite, Sprite Mode = Multiple",
                    "OK");
                return;
            }
            
            Object[] spriteObjects = AssetDatabase.LoadAllAssetRepresentationsAtPath(spritesheetPath)
                .OfType<Sprite>()
                .ToArray();
            sprites = spriteObjects.Cast<Sprite>().ToArray();
        }
        else
        {
            // Modo PNG individuales
            if (!System.IO.Directory.Exists(spritesFolder.Replace("Assets/", Application.dataPath + "/")))
            {
                EditorUtility.DisplayDialog("Error", 
                    $"La carpeta no existe: {spritesFolder}\n\n" +
                    "Verifica la ruta de la carpeta con los PNG.",
                    "OK");
                Debug.LogError($"❌ Carpeta no encontrada: {spritesFolder}");
                return;
            }
            
            // Buscar todos los PNG en la carpeta
            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { spritesFolder });
            List<Sprite> spritesList = new List<Sprite>();
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null)
                {
                    spritesList.Add(sprite);
                }
            }
            
            sprites = spritesList.ToArray();
            Debug.Log($"🔍 Buscando PNG en: {spritesFolder}");
            Debug.Log($"   - Sprites encontrados: {sprites.Length}");
        }
        
        if (sprites == null || sprites.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", 
                $"No se encontraron sprites.\n\n" +
                (usarSpritesheet ? 
                    "Verifica que el Spritesheet.png esté configurado como 'Multiple' en el Sprite Editor." :
                    "Verifica que la carpeta contenga PNG con nombres: CABALLERO_X_00Y"),
                "OK");
            return;
        }
        
        // Mostrar nombres de los primeros sprites para depuración
        Debug.Log($"📝 Primeros 10 sprites encontrados:");
        for (int i = 0; i < Mathf.Min(10, sprites.Length); i++)
        {
            Debug.Log($"   {i + 1}. {sprites[i].name}");
        }
        
        Debug.Log($"✅ Encontrados {sprites.Length} sprites");
        
        // Crear carpeta de salida
        if (!System.IO.Directory.Exists(outputFolder))
        {
            System.IO.Directory.CreateDirectory(outputFolder);
            AssetDatabase.Refresh();
        }
        
        // Organizar sprites por keyframe
        Dictionary<int, List<Sprite>> spritesPorKeyframe = new Dictionary<int, List<Sprite>>();
        int spritesIgnorados = 0;
        List<string> nombresIgnorados = new List<string>();
        
        Debug.Log($"🔍 Organizando sprites por keyframe...");
        
        foreach (Sprite sprite in sprites)
        {
            // Parsear nombre: CABALLERO_X_00Y
            string nombre = sprite.name;
            if (!nombre.StartsWith("CABALLERO_"))
            {
                spritesIgnorados++;
                nombresIgnorados.Add(nombre);
                Debug.LogWarning($"⚠️ Sprite con nombre inesperado (no empieza con CABALLERO_): {nombre}");
                continue;
            }
            
            // Extraer keyframe y frame interpolado
            string[] partes = nombre.Split('_');
            if (partes.Length < 3)
            {
                spritesIgnorados++;
                nombresIgnorados.Add(nombre);
                Debug.LogWarning($"⚠️ Formato de nombre incorrecto (menos de 3 partes separadas por _): {nombre}");
                continue;
            }
            
            // Intentar parsear keyframe (partes[1]) y frame interpolado (partes[2])
            if (int.TryParse(partes[1], out int keyframe))
            {
                // Parsear frame interpolado (puede ser "001", "002", etc.)
                string frameStr = partes[2];
                if (int.TryParse(frameStr, out int frameInterpolado))
                {
                    if (!spritesPorKeyframe.ContainsKey(keyframe))
                    {
                        spritesPorKeyframe[keyframe] = new List<Sprite>();
                    }
                    
                    spritesPorKeyframe[keyframe].Add(sprite);
                    Debug.Log($"   ✓ {nombre} -> Keyframe {keyframe}, Frame {frameInterpolado}");
                }
                else
                {
                    spritesIgnorados++;
                    nombresIgnorados.Add(nombre);
                    Debug.LogWarning($"⚠️ No se pudo parsear el frame interpolado de: {nombre} (parte 2: '{frameStr}')");
                }
            }
            else
            {
                spritesIgnorados++;
                nombresIgnorados.Add(nombre);
                Debug.LogWarning($"⚠️ No se pudo parsear el keyframe de: {nombre} (parte 1: '{partes[1]}')");
            }
        }
        
        Debug.Log($"✅ Organizados {spritesPorKeyframe.Count} keyframes");
        if (spritesIgnorados > 0)
        {
            Debug.LogWarning($"⚠️ {spritesIgnorados} sprites fueron ignorados por formato de nombre incorrecto.");
            Debug.LogWarning($"   Nombres esperados: CABALLERO_X_00Y donde X=keyframe e Y=frame interpolado (1-4)");
            if (nombresIgnorados.Count <= 10)
            {
                Debug.LogWarning($"   Ejemplos de nombres ignorados: {string.Join(", ", nombresIgnorados)}");
            }
        }
        
        // ✅ DETECTAR AUTOMÁTICAMENTE los keyframes encontrados (sin depender del número configurado)
        List<int> keyframesEncontrados = spritesPorKeyframe.Keys.OrderBy(k => k).ToList();
        int keyframesReales = keyframesEncontrados.Count;
        
        Debug.Log($"📊 Resumen:");
        Debug.Log($"   - Keyframes encontrados: {keyframesReales}");
        Debug.Log($"   - Rango: {keyframesEncontrados.First()} a {keyframesEncontrados.Last()}");
        Debug.Log($"   - Keyframes: {string.Join(", ", keyframesEncontrados)}");
        
        // Verificar si hay keyframes suficientes
        if (keyframesReales == 0)
        {
            EditorUtility.DisplayDialog("Error", 
                "No se encontraron keyframes válidos.\n\n" +
                "Verifica que los PNG tengan nombres: CABALLERO_X_00Y",
                "OK");
            Debug.LogError("❌ No hay keyframes para crear animaciones");
            return;
        }
        
        // Si el usuario configuró un número diferente, avisar pero usar los encontrados
        if (keyframesReales != keyframesCount)
        {
            string mensaje = $"Se configuraron {keyframesCount} keyframes pero se encontraron {keyframesReales}.\n\n";
            mensaje += $"Keyframes encontrados: {string.Join(", ", keyframesEncontrados)}\n\n";
            mensaje += $"Se crearán animaciones para los {keyframesReales} keyframes encontrados.\n\n";
            mensaje += "¿Continuar?";
            
            bool continuar = EditorUtility.DisplayDialog("Información", mensaje, "Sí, continuar", "Cancelar");
            
            if (!continuar)
            {
                Debug.Log("❌ Operación CANCELADA por el usuario. No se crearán animaciones.");
                EditorUtility.DisplayDialog("Cancelado", "Operación cancelada. No se crearon animaciones.", "OK");
                return; // ⚠️ IMPORTANTE: Salir aquí, no continuar
            }
            
            Debug.Log($"✅ Usuario confirmó: Creando animaciones para {keyframesReales} keyframes");
        }
        
        // ⚠️ Solo llegamos aquí si el usuario confirmó
        Debug.Log("🎬 Iniciando creación de Animation Clips...");
        
        // Crear Animation Clips usando SOLO los keyframes encontrados (no el rango 0-29)
        List<AnimationClip> clips = new List<AnimationClip>();
        
        foreach (int keyframe in keyframesEncontrados)
        {
            if (!spritesPorKeyframe.ContainsKey(keyframe))
            {
                Debug.LogWarning($"⚠️ Keyframe {keyframe} no encontrado en el diccionario, saltando...");
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
        
        // Crear animaciones compuestas: Idle y Ataque
        // Idle: keyframes 0-15 (si existen)
        List<int> idleKeyframes = keyframesEncontrados.Where(k => k >= 0 && k <= 15).OrderBy(k => k).ToList();
        AnimationClip idleClip = null;
        if (idleKeyframes.Count > 0)
        {
            idleClip = CrearAnimacionCompuesta("Caballero_Idle", idleKeyframes, spritesPorKeyframe, frameTime);
            if (idleClip != null)
            {
                string idlePath = $"{outputFolder}/{idleClip.name}.anim";
                AssetDatabase.CreateAsset(idleClip, idlePath);
                Debug.Log($"✅ Creada animación compuesta: {idleClip.name} con {idleKeyframes.Count} keyframes");
            }
        }
        
        // Ataque: keyframes 16-31 (si existen)
        List<int> attackKeyframes = keyframesEncontrados.Where(k => k >= 16 && k <= 31).OrderBy(k => k).ToList();
        AnimationClip attackClip = null;
        if (attackKeyframes.Count > 0)
        {
            attackClip = CrearAnimacionCompuesta("Caballero_Ataque", attackKeyframes, spritesPorKeyframe, frameTime);
            if (attackClip != null)
            {
                string attackPath = $"{outputFolder}/{attackClip.name}.anim";
                AssetDatabase.CreateAsset(attackClip, attackPath);
                Debug.Log($"✅ Creada animación compuesta: {attackClip.name} con {attackKeyframes.Count} keyframes");
            }
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // Agregar estados principales al controller: Idle y Ataque
        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        
        AnimatorState idleState = null;
        AnimatorState attackState = null;
        
        // Estado Idle (por defecto)
        if (idleClip != null)
        {
            idleState = stateMachine.AddState("Idle");
            idleState.motion = idleClip;
            stateMachine.defaultState = idleState;
        }
        
        // Estado Ataque
        if (attackClip != null)
        {
            attackState = stateMachine.AddState("Ataque");
            attackState.motion = attackClip;
        }
        
        // Agregar también las animaciones individuales (por si se necesitan)
        foreach (AnimationClip clip in clips)
        {
            AnimatorState state = stateMachine.AddState(clip.name);
            state.motion = clip;
        }
        
        // Agregar parámetros al controller
        controller.AddParameter("Atacar", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("VolverIdle", AnimatorControllerParameterType.Trigger);
        
        // Configurar transiciones básicas
        if (idleState != null && attackState != null)
        {
            // Transición: Idle -> Ataque (cuando se active el trigger "Atacar")
            AnimatorStateTransition transition = idleState.AddTransition(attackState);
            transition.AddCondition(AnimatorConditionMode.If, 0, "Atacar");
            transition.duration = 0.1f;
            
            // Transición: Ataque -> Idle (cuando termine)
            AnimatorStateTransition transitionBack = attackState.AddTransition(idleState);
            transitionBack.AddCondition(AnimatorConditionMode.If, 0, "VolverIdle");
            transitionBack.duration = 0.1f;
            transitionBack.hasExitTime = true;
            transitionBack.exitTime = 0.9f;
        }
        
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
    
    /// <summary>
    /// Crea una animación compuesta que combina varios keyframes en secuencia
    /// </summary>
    private AnimationClip CrearAnimacionCompuesta(string nombre, List<int> keyframes, 
        Dictionary<int, List<Sprite>> spritesPorKeyframe, float frameTime)
    {
        if (keyframes.Count == 0) return null;
        
        AnimationClip clip = new AnimationClip();
        clip.name = nombre;
        clip.frameRate = 1f / frameTime;
        clip.wrapMode = WrapMode.Loop; // Para idle
        
        EditorCurveBinding spriteBinding = new EditorCurveBinding();
        spriteBinding.type = typeof(Image);
        spriteBinding.path = "";
        spriteBinding.propertyName = "m_Sprite";
        
        List<ObjectReferenceKeyframe> allKeyframes = new List<ObjectReferenceKeyframe>();
        float tiempoActual = 0f;
        
        foreach (int keyframe in keyframes)
        {
            if (!spritesPorKeyframe.ContainsKey(keyframe)) continue;
            
            List<Sprite> frames = spritesPorKeyframe[keyframe];
            frames = frames.OrderBy(s => 
            {
                string[] partes = s.name.Split('_');
                if (partes.Length >= 3 && int.TryParse(partes[2], out int num))
                    return num;
                return 0;
            }).ToList();
            
            // Agregar cada frame interpolado del keyframe
            for (int i = 0; i < frames.Count; i++)
            {
                ObjectReferenceKeyframe keyframeObj = new ObjectReferenceKeyframe();
                keyframeObj.time = tiempoActual;
                keyframeObj.value = frames[i];
                allKeyframes.Add(keyframeObj);
                
                tiempoActual += frameTime;
            }
        }
        
        if (allKeyframes.Count == 0) return null;
        
        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, allKeyframes.ToArray());
        
        return clip;
    }
}
