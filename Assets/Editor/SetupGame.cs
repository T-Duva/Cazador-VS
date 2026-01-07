using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class SetupGame : EditorWindow
{
    #region Ventana (MenuItem)
    [MenuItem("Tools/Setup Game - Configurar Todo")]
    public static void ShowWindow()
    {
        GetWindow<SetupGame>("Setup Game");
    }
    #endregion

    #region GUI
    void OnGUI()
    {
        GUILayout.Label("Configuración Automática del Juego", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("1. Crear GameManager", GUILayout.Height(30)))
        {
            CreateGameManager();
        }

        if (GUILayout.Button("2. Crear Canvas y UI", GUILayout.Height(30)))
        {
            CreateCanvas();
        }

        if (GUILayout.Button("3. Configurar Todo Automáticamente", GUILayout.Height(40)))
        {
            CreateGameManager();
            CreateCanvas();
            EditorUtility.DisplayDialog("✅ Configuración Completa", 
                "GameManager y Canvas creados correctamente!\n\n" +
                "Ahora copia los scripts GameManager.cs y UIManager.cs a Assets/Scripts/\n" +
                "Y luego arrastra GameManager.cs al GameObject 'GameManager' en la escena.", 
                "OK");
        }

        GUILayout.Space(20);
        GUILayout.Label("Instrucciones:", EditorStyles.boldLabel);
        GUILayout.Label("1. Copia GameManager.cs y UIManager.cs a Assets/Scripts/");
        GUILayout.Label("2. Selecciona el GameObject 'GameManager' en la escena");
        GUILayout.Label("3. Arrastra GameManager.cs al Inspector");
        GUILayout.Label("4. Configura los sprites en el GameManager cuando los tengas");
    }
    #endregion

    #region Acciones
    static void CreateGameManager()
    {
        // Buscar si ya existe un GameManager
        GameObject existingGM = GameObject.Find("GameManager");
        if (existingGM != null)
        {
            Debug.Log("⚠️ GameManager ya existe en la escena.");
            Selection.activeGameObject = existingGM;
            return;
        }

        // Crear GameManager
        GameObject gameManager = new GameObject("GameManager");
        
        // Agregar el script si existe
        string scriptPath = "Assets/Scripts/GameManager.cs";
        if (System.IO.File.Exists(scriptPath))
        {
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
            if (script != null)
            {
                gameManager.AddComponent(script.GetClass());
                Debug.Log("✅ GameManager creado con script GameManager.cs");
            }
            else
            {
                Debug.LogWarning("⚠️ Script GameManager.cs encontrado pero no se pudo cargar. Arrástralo manualmente.");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ Script GameManager.cs no encontrado en Assets/Scripts/. Créalo primero.");
        }

        Selection.activeGameObject = gameManager;
        Debug.Log("✅ GameObject 'GameManager' creado en la escena.");
    }

    static void CreateCanvas()
    {
        // Buscar si ya existe un Canvas
        Canvas existingCanvas = Object.FindFirstObjectByType<Canvas>();
        if (existingCanvas != null)
        {
            Debug.Log("⚠️ Canvas ya existe en la escena.");
            Selection.activeGameObject = existingCanvas.gameObject;
            return;
        }

        // Crear Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // Crear EventSystem si no existe
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("✅ EventSystem creado.");
        }

        Selection.activeGameObject = canvasObj;
        Debug.Log("✅ Canvas creado correctamente.");
    }
    #endregion
}

