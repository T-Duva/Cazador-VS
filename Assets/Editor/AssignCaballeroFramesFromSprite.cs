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
        EditorGUILayout.HelpBox("Valida caballeroFrames (0..31) sin reordenar ni sobrescribir. No modifica el orden actual.", MessageType.Info);
        if (GUILayout.Button("Asignar frames a UIManager"))
        {
            AssignFrames();
        }
    }
    /// <summary>
    /// Asigna los sprites del caballero al array caballeroFrames.
    /// IMPORTANTE: Este script NO modifica los pivots de los sprites.
    /// Los pivots son CONSTANTES y solo pueden ser modificados por el usuario en el Sprite Editor de Unity.
    /// </summary>
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
            string[] guids = AssetDatabase.FindAssets("CABALLERO t:Sprite");
            if (guids.Length > 0)
            {
                path = AssetDatabase.GUIDToAssetPath(guids[0]);
                spriteAsset = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                Debug.Log("✓ CABALLERO.png encontrado en: " + path);
            }
        }
        if (spriteAsset == null)
        {
            Debug.LogError("❌ CABALLERO.png no encontrado");
            return;
        }
        Debug.Log("✓ CABALLERO.png encontrado");
        var sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(path)
                          .OfType<Sprite>()
                          .ToArray();
        Debug.Log("Sprites encontrados: " + sprites.Length);
        
        if (sprites.Length != 32)
        {
            Debug.LogError("❌ Se esperan 32 frames (0..31), pero se encontraron " + sprites.Length);
            Debug.LogError("Verificá que CABALLERO.png tenga 32 slices y que estén completos.");
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
        var sheetProp = so.FindProperty("caballeroSheet");
        if (sheetProp != null)
        {
            sheetProp.objectReferenceValue = spriteAsset;
            Debug.Log("✓ caballeroSheet asignado automáticamente");
        }
        var prop = so.FindProperty("caballeroFrames");
        
        if (prop == null || !prop.isArray)
        {
            Debug.LogError("❌ caballeroFrames no existe en UIManager. Asegurate de tener: [SerializeField] private Sprite[] caballeroFrames;");
            return;
        }
        if (prop.arraySize == 32)
        {
            Debug.Log("✅ caballeroFrames ya tiene 32 sprites. No se modifica el orden actual.");
            return;
        }
        Debug.LogError("❌ caballeroFrames no tiene tamaño 32. Ajustalo manualmente en el Inspector sin reordenar.");
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
