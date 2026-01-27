#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class CropSpriteSheet : EditorWindow
{
    private Texture2D sourceTexture;
    private int targetWidth = 504;
    private int targetHeight = 500;
    private string outputName = "CABALLERO_CROP_504";

    [MenuItem("Tools/Crop SpriteSheet 504")]
    static void Init() {
        CropSpriteSheet window = (CropSpriteSheet)EditorWindow.GetWindow(typeof(CropSpriteSheet));
        window.titleContent = new GUIContent("Crop SpriteSheet 504");
        window.Show();
    }

    void OnGUI() {
        GUILayout.Label("Recortar SpriteSheet a tamaño exacto (504x500)");
        sourceTexture = (Texture2D)EditorGUILayout.ObjectField("SpriteSheet Origen", sourceTexture, typeof(Texture2D), false);
        targetWidth = EditorGUILayout.IntField("Ancho a recortar (ej: 504)", targetWidth);
        targetHeight = EditorGUILayout.IntField("Alto a recortar (ej: 500)", targetHeight);
        outputName = EditorGUILayout.TextField("Nombre del PNG de salida", outputName);

        if (GUILayout.Button("Recortar y Guardar")) {
            CropAndSave();
        }
    }

    void CropAndSave() {
        if (sourceTexture == null) {
            Debug.LogError("No se seleccionó ninguna textura.");
            return;
        }
        string path = AssetDatabase.GetAssetPath(sourceTexture);
        TextureImporter ti = TextureImporter.GetAtPath(path) as TextureImporter;
        if (ti != null && !ti.isReadable) {
            ti.isReadable = true;
            AssetDatabase.ImportAsset(path);
        }

        Texture2D cropped = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);
        Color32[] pixels = sourceTexture.GetPixels32();
        for (int y = 0; y < targetHeight; y++) {
            for (int x = 0; x < targetWidth; x++) {
                if (x < sourceTexture.width && y < sourceTexture.height)
                    cropped.SetPixel(x, y, sourceTexture.GetPixel(x, y));
                else
                    cropped.SetPixel(x, y, new Color32(0,0,0,0));
            }
        }
        cropped.Apply();

        byte[] pngData = cropped.EncodeToPNG();
        string dir = Path.GetDirectoryName(path);
        string outputPath = Path.Combine(dir, outputName + ".png");
        File.WriteAllBytes(outputPath, pngData);
        AssetDatabase.Refresh();
        Debug.Log("✅ SpriteSheet recortado guardado en: " + outputPath);
    }
}
#endif