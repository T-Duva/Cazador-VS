#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;
public class AssignCaballeroFramesFromSprite : EditorWindow
{
    [MenuItem("Tools/Assign Caballero Frames from Sprite")]
    static void Init()
    {
        var window = (AssignCaballeroFramesFromSprite)EditorWindow.GetWindow(typeof(AssignCaballeroFramesFromSprite));
        window.titleContent = new GUIContent("Assign Caballero Frames");
        window.Show();
    }
    void OnGUI()
    {
        EditorGUILayout.HelpBox("Asigna los 32 frames desde CABALLERO.png a UIManager.caballeroFrames (Sprite[]). Asegúrate de que CABALLERO.png existe en Assets y esté cortado en 8x4.", MessageType.Info);
        if (GUILayout.Button("Asignar frames a UIManager"))
        {
            AssignFrames();
        }
    }
    void AssignFrames()
    {
        UIManager ui = UnityEngine.Object.FindFirstObjectByType<UIManager>();
        if (ui == null)
        {
            Debug.LogError("❌ No se encontró UIManager en la escena.");
            return;
        }
        Debug.Log("✓ UIManager encontrado");
        string path = "Assets/Sprites/Personajes/Guerrero/CABALLERO.png";
        var spriteAsset = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (spriteAsset == null)
        {
            Debug.LogError("❌ CABALLERO.png no encontrado en " + path);
            return;
        }
        Debug.Log("✓ CABALLERO.png encontrado");
        var sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(path)
                          .OfType<Sprite>()
                          .ToArray();
        Debug.Log("Sprites encontrados: " + sprites.Length);
        
        if (sprites.Length != 32)
        {
            Debug.LogError("❌ Se esperan 32 frames, pero se encontraron " + sprites.Length);
            Debug.LogError("Verificá que CABALLERO.png esté cortado en Sprite Editor con Grid 8x4 (ancho divisible por 8, alto divisible por 4)");
            return;
        }
        // Ordenar por índice numérico extraído del nombre
        var sorted = sprites.OrderBy(s => GetFrameIndex(s.name)).ToArray();
        
        // Log de los primeros y últimos frames para verificar orden
        Debug.Log("Primer frame: " + sorted[0].name);
        Debug.Log("Frame 15 (último idle): " + sorted[15].name);
        Debug.Log("Frame 16 (primer ataque): " + sorted[16].name);
        Debug.Log("Último frame: " + sorted[31].name);
        var so = new SerializedObject(ui);
        var prop = so.FindProperty("caballeroFrames");
        
        if (prop == null || !prop.isArray)
        {
            Debug.LogError("❌ caballeroFrames no existe en UIManager. Asegurate de tener: [SerializeField] private Sprite[] caballeroFrames;");
            return;
        }
        // Limpiar array completamente
        prop.ClearArray();
        prop.arraySize = 32;
        // Asignar uno por uno con validación
        for (int i = 0; i < sorted.Length; i++)
        {
            if (sorted[i] == null)
            {
                Debug.LogError("❌ Frame " + i + " es nulo");
                continue;
            }
            prop.GetArrayElementAtIndex(i).objectReferenceValue = sorted[i];
        }
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(ui);
        AssetDatabase.SaveAssets();
        
        Debug.Log("✅ caballeroFrames asignados correctamente con 32 frames.");
        Debug.Log("Revisá el Inspector de UIManager para confirmar que todos los slots están llenos.");
    }
    int GetFrameIndex(string spriteName)
    {
        // Extraer números del nombre del sprite
        string numbers = new string(spriteName.Where(char.IsDigit).ToArray());
        if (int.TryParse(numbers, out int index))
        {
            return index;
        }
        return 0;
    }
}
#endif
