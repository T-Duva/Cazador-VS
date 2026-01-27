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
        EditorGUILayout.HelpBox("Asigna los 32 frames desde CABALLERO_CROP_504.png a UIManager.caballeroFrames (Sprite[]). Asegúrate de que CABALLERO_CROP_504.png existe en Assets.", MessageType.Info);
        if (GUILayout.Button("Asignar frames a UIManager"))
        {
            AssignFrames();
        }
    }
    void AssignFrames()
    {
        UIManager ui = UnityEngine.Object.FindObjectOfType<UIManager>();
        if (ui == null)
        {
            Debug.LogError("No se encontró UIManager en la escena.");
            return;
        }
        string path = "Assets/Sprites/Personajes/Guerrero/CABALLERO_CROP_504.png";
        var spriteAsset = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (spriteAsset == null)
        {
            Debug.LogError("CABALLERO_CROP_504.png no encontrado en " + path);
            return;
        }
        var sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(path)
                          .OfType<Sprite>()
                          .OrderBy(s => s.name)
                          .ToArray();
        if (sprites.Length != 32)
        {
            Debug.LogError("Se esperan 32 frames, pero se encontraron " + sprites.Length + ". Verifica el slicing.");
            return;
        }
        var so = new SerializedObject(ui);
        var prop = so.FindProperty("caballeroFrames");
        if (prop == null || !prop.isArray)
        {
            Debug.LogError("UIManager.caballeroFrames no existe. Añade [SerializeField] private Sprite[] caballeroFrames; a UIManager.cs.");
            return;
        }
        prop.ClearArray();
        for (int i = 0; i < sprites.Length; i++)
        {
            prop.InsertArrayElementAtIndex(i);
            prop.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
        }
        so.ApplyModifiedProperties();
        Debug.Log("✅ caballeroFrames asignados con 32 frames desde CABALLERO_CROP_504.png.");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
#endif
