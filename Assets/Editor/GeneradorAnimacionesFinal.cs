using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;

public class GeneradorAnimacionesFinal : EditorWindow
{
    [MenuItem("Tools/CONFIGURAR JUEGO")]
    public static void ShowWindow() => GetWindow<GeneradorAnimacionesFinal>("Configurar Juego");

    private void OnGUI()
    {
        GUILayout.Label("GENERADOR TOTAL (Fuerza Bruta)", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "Este script creará las animaciones desde cero buscando en:\n" +
            "Assets/Sprites/Personajes/Guerrero/NUEVO\n\n" +
            "⚠️ PROTECCIÓN: Si las animaciones ya existen, NO se sobrescribirán.\n" +
            "Esto preserva tus ajustes manuales de FPS, loop, etc.\n\n" +
            "Si las imágenes no están ahí o tienen otro nombre, te avisará en la consola.",
            MessageType.Info
        );
        
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "⚠️ IMPORTANTE:\n" +
            "• 'CONFIGURAR TODO' preserva animaciones existentes (no sobrescribe).\n" +
            "• 'REGENERAR Run+Back' fuerza la recreación de esa animación (sobrescribe).",
            MessageType.Warning
        );
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("CONFIGURAR Y REPARAR TODO", GUILayout.Height(40)))
        {
            Debug.ClearDeveloperConsole();
            Debug.Log("<color=cyan>🚀 INICIANDO GENERACIÓN...</color>");
            
            // 1. Generar Animaciones (Lo que pediste: asegurar que se creen)
            bool animacionesCreadas = GenerarAnimaciones();
            
             if (animacionesCreadas)
             {
                 // 2. Configurar el Cerebro (Animator)
                 ConfigurarAnimator();
                 
                 Debug.Log("<color=green>✅ ¡LISTO! Animaciones creadas y Caballero configurado.</color>");
                 EditorUtility.DisplayDialog("Éxito", "Proceso terminado.\nRevisá la consola si ves algún error rojo.", "OK");
             }
            else
            {
                Debug.LogError("❌ SE DETUVO EL PROCESO PORQUE FALTAN LOS SPRITES.");
                EditorUtility.DisplayDialog("Error", "No se encontraron las imágenes en la carpeta NUEVO.\nMirá la consola para ver qué archivo falta.", "Entendido");
            }
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("🔄 REGENERAR Run+Back (Sobrescribe)", GUILayout.Height(40)))
        {
            if (EditorUtility.DisplayDialog("Confirmar", 
                "¿Estás seguro? Esto SOBRESCRIBIRÁ el clip Run+Back.anim\n" +
                "Perderás cualquier ajuste manual de FPS o loop que hayas hecho.\n\n" +
                "¿Continuar?", "Sí, Sobrescribir", "Cancelar"))
            {
                RegenerarAnimacionEspecifica("Run+Back");
            }
        }
    }

    // --- 1. GENERAR ANIMACIONES ---
    private bool GenerarAnimaciones()
    {
        string rutaAnim = "Assets/Animations/CaballeroInterpolado";
        string rutaSprites = "Assets/Sprites/Personajes/Guerrero/NUEVO"; // RUTA CLAVE
        
        // Crear carpeta de destino si no existe
        if (!Directory.Exists(rutaAnim)) 
        {
            Directory.CreateDirectory(rutaAnim);
            AssetDatabase.Refresh();
            Debug.Log($"[Carpeta] Creada: {rutaAnim}");
        }

        // Verificar carpeta de origen
        if (!Directory.Exists(rutaSprites))
        {
            Debug.LogError($"[Error Fatal] NO EXISTE la carpeta de sprites: {rutaSprites}");
            return false;
        }

        var animaciones = new Dictionary<string, (int inicio, int fin)>
        {
            { "Idle", (0, 13) },
            { "Prepare_to_atack", (14, 18) },
            { "Run+Atack", (19, 24) },
            { "FinishHim", (25, 25) },
            { "Run+Back", (26, 30) }
        };

        int animacionesExitosas = 0;

        foreach (var kvp in animaciones)
        {
            string nombreAnim = kvp.Key;
            string animPath = $"{rutaAnim}/{nombreAnim}.anim";
            
            // PROTECCIÓN: Si la animación ya existe, NO la sobrescribimos
            // Esto preserva los ajustes manuales de FPS, loop, etc.
            if (File.Exists(animPath))
            {
                Debug.Log($"[Generador] ⚠ {nombreAnim}.anim ya existe. Se preserva (no se sobrescribe) para mantener tus ajustes manuales.");
                animacionesExitosas++;
                continue; // Saltar esta animación
            }
            
            // Configurar clip para UI IMAGE
            AnimationClip clip = new AnimationClip { name = nombreAnim };
            EditorCurveBinding spriteBinding = new EditorCurveBinding
            {
                type = typeof(UnityEngine.UI.Image), // OBLIGATORIO PARA UI
                path = "",
                propertyName = "m_Sprite"
            };

            List<ObjectReferenceKeyframe> keyframes = new List<ObjectReferenceKeyframe>();
            int maxFrame = (nombreAnim == "Run+Back") ? 25 : 4;

            // Recopilar Sprites
            for (int grupo = kvp.Value.inicio; grupo <= kvp.Value.fin; grupo++)
            {
                for (int frame = 1; frame <= maxFrame; frame++)
                {
                    // FORMATO EXACTO: CABALLERO_0_001.png
                    string nombreArchivo = $"CABALLERO_{grupo}_{frame:D3}.png";
                    string spritePath = $"{rutaSprites}/{nombreArchivo}";
                    
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                    if (sprite != null)
                    {
                        keyframes.Add(new ObjectReferenceKeyframe 
                        { 
                            time = keyframes.Count * 0.1f, // 10 fps base (luego se ajusta)
                            value = sprite 
                        });
                    }
                    else if (frame == 1 && grupo == kvp.Value.inicio)
                    {
                        // Avisar solo si falta el PRIMER frame para no llenar la consola
                        Debug.LogError($"[FALTA SPRITE] Buscando: {spritePath}");
                        Debug.LogError("¿El archivo tiene ese nombre exacto? ¿Tiene 3 ceros al final?");
                    }
                }
            }

            if (keyframes.Count > 0)
            {
                // Asignar curva y settings
                AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes.ToArray());
                
                // Configurar frameRate: Run+Back más rápido para más pasos
                if (nombreAnim == "Run+Back") 
                {
                    clip.frameRate = 80f; // Alto FPS para secuencia fluida
                }
                else 
                {
                    clip.frameRate = 10f;
                }

                // Configurar Loop: Idle y Run+Back deben hacer loop
                AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
                if (nombreAnim == "Idle" || nombreAnim == "Run+Back")
                {
                    settings.loopTime = true;
                    AnimationUtility.SetAnimationClipSettings(clip, settings);
                    clip.wrapMode = WrapMode.Loop;
                }
                else
                {
                    settings.loopTime = false;
                    AnimationUtility.SetAnimationClipSettings(clip, settings);
                    clip.wrapMode = WrapMode.Once;
                }

                // Guardar archivo .anim
                AssetDatabase.CreateAsset(clip, animPath);
                animacionesExitosas++;
                Debug.Log($"[Generador] ✓ {nombreAnim}.anim CREADO ({keyframes.Count} frames)");
            }
            else
            {
                Debug.LogError($"[Generador] ❌ NO SE PUDO CREAR {nombreAnim}. No se encontraron sprites.");
            }
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        return animacionesExitosas > 0;
    }

    // --- REGENERAR ANIMACIÓN ESPECÍFICA (SOBRESCRIBE) ---
    private void RegenerarAnimacionEspecifica(string nombreAnim)
    {
        string rutaAnim = "Assets/Animations/CaballeroInterpolado";
        string rutaSprites = "Assets/Sprites/Personajes/Guerrero/NUEVO";
        string animPath = $"{rutaAnim}/{nombreAnim}.anim";
        
        // Diccionario de rangos
        var animaciones = new Dictionary<string, (int inicio, int fin)>
        {
            { "Idle", (0, 13) },
            { "Prepare_to_atack", (14, 18) },
            { "Run+Atack", (19, 24) },
            { "FinishHim", (25, 25) },
            { "Run+Back", (26, 30) }
        };
        
        if (!animaciones.ContainsKey(nombreAnim))
        {
            Debug.LogError($"[Regenerar] ❌ Animación '{nombreAnim}' no encontrada en la lista.");
            return;
        }
        
        // BORRAR el archivo existente para forzar regeneración
        if (File.Exists(animPath))
        {
            AssetDatabase.DeleteAsset(animPath);
            AssetDatabase.Refresh();
            Debug.Log($"[Regenerar] 🗑️ {nombreAnim}.anim eliminado. Regenerando...");
        }
        
        // Crear clip nuevo
        AnimationClip clip = new AnimationClip { name = nombreAnim };
        EditorCurveBinding spriteBinding = new EditorCurveBinding
        {
            type = typeof(UnityEngine.UI.Image),
            path = "",
            propertyName = "m_Sprite"
        };

        List<ObjectReferenceKeyframe> keyframes = new List<ObjectReferenceKeyframe>();
        int maxFrame = (nombreAnim == "Run+Back") ? 25 : 4;
        var rango = animaciones[nombreAnim];

        // Recopilar Sprites
        for (int grupo = rango.inicio; grupo <= rango.fin; grupo++)
        {
            for (int frame = 1; frame <= maxFrame; frame++)
            {
                string nombreArchivo = $"CABALLERO_{grupo}_{frame:D3}.png";
                string spritePath = $"{rutaSprites}/{nombreArchivo}";
                
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                if (sprite != null)
                {
                    keyframes.Add(new ObjectReferenceKeyframe 
                    { 
                        time = keyframes.Count * 0.1f,
                        value = sprite 
                    });
                }
            }
        }

        if (keyframes.Count > 0)
        {
            AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes.ToArray());
            
            // Configurar frameRate: Run+Back más rápido
            if (nombreAnim == "Run+Back") 
            {
                clip.frameRate = 80f;
            }
            else 
            {
                clip.frameRate = 10f;
            }

            // Configurar Loop
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            if (nombreAnim == "Idle" || nombreAnim == "Run+Back")
            {
                settings.loopTime = true;
                AnimationUtility.SetAnimationClipSettings(clip, settings);
                clip.wrapMode = WrapMode.Loop;
            }
            else
            {
                settings.loopTime = false;
                AnimationUtility.SetAnimationClipSettings(clip, settings);
                clip.wrapMode = WrapMode.Once;
            }

            // Guardar
            AssetDatabase.CreateAsset(clip, animPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"<color=green>[Regenerar] ✅ {nombreAnim}.anim REGENERADO ({keyframes.Count} frames, {clip.frameRate} FPS, Loop={settings.loopTime})</color>");
            EditorUtility.DisplayDialog("Listo", $"{nombreAnim}.anim regenerado.\nAhora podés cambiar el FPS manualmente en el Inspector.", "OK");
        }
        else
        {
            Debug.LogError($"[Regenerar] ❌ No se encontraron sprites para {nombreAnim}.");
        }
    }

    // --- 2. CONFIGURAR ANIMATOR ---
    private void ConfigurarAnimator()
    {
        string controllerPath = "Assets/Animations/CaballeroInterpolado/CaballeroInterpolado.controller";
        
        // PROTECCIÓN: Si el Controller ya existe, NO lo sobrescribimos
        // Esto preserva las transiciones y configuraciones manuales
        AnimatorController controller;
        if (File.Exists(controllerPath))
        {
            controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            Debug.Log("[Animator] ⚠ Controller ya existe. Se preserva (no se sobrescribe) para mantener tus configuraciones manuales.");
            return; // Salir sin tocar nada
        }
        
        // Crear Controller solo si no existe
        controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        AnimatorStateMachine sm = controller.layers[0].stateMachine;

        // Agregar estados solo si es un Controller nuevo
        string[] nombres = { "Idle", "Prepare_to_atack", "Run+Atack", "FinishHim", "Run+Back" };
        foreach (string nombre in nombres)
        {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"Assets/Animations/CaballeroInterpolado/{nombre}.anim");
            if (clip != null)
            {
                AnimatorState estado = sm.AddState(nombre);
                estado.motion = clip;
                if (nombre == "Idle") sm.defaultState = estado;
            }
        }

        // Asignar al Jugador
        GameObject jugador = GameObject.Find("ImageJugador");
        if (jugador != null)
        {
            Animator anim = jugador.GetComponent<Animator>();
            if (anim == null) anim = jugador.AddComponent<Animator>();
            
            anim.runtimeAnimatorController = controller;
            Debug.Log("[Animator] ✓ Controller conectado al ImageJugador.");
        }
    }

}