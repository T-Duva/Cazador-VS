using System.IO;
using UnityEditor;
using UnityEngine;

public static class RescaleCaballeroSheet
{
    private const string CaballeroPath = "Assets/Sprites/Personajes/Guerrero/CABALLERO.png";
    private const int TargetWidth = 352;
    private const int TargetHeight = 704;

    [MenuItem("Tools/Caballero/Rescale Sheet To 352x704")]
    public static void Rescale()
    {
        Texture2D source = AssetDatabase.LoadAssetAtPath<Texture2D>(CaballeroPath);
        if (source == null)
        {
            Debug.LogError("[Caballero] No se encontró CABALLERO.png en " + CaballeroPath);
            return;
        }

        Texture2D readable = GetReadableCopy(source);
        if (readable == null)
        {
            Debug.LogError("[Caballero] No se pudo leer la textura. Verificá Read/Write Enabled.");
            return;
        }

        Texture2D resized = new Texture2D(TargetWidth, TargetHeight, TextureFormat.RGBA32, false);
        for (int y = 0; y < TargetHeight; y++)
        {
            float v = (TargetHeight == 1) ? 0f : (float)y / (TargetHeight - 1);
            for (int x = 0; x < TargetWidth; x++)
            {
                float u = (TargetWidth == 1) ? 0f : (float)x / (TargetWidth - 1);
                Color c = readable.GetPixelBilinear(u, v);
                resized.SetPixel(x, y, c);
            }
        }
        resized.Apply();

        byte[] png = resized.EncodeToPNG();
        File.WriteAllBytes(CaballeroPath, png);
        AssetDatabase.ImportAsset(CaballeroPath, ImportAssetOptions.ForceUpdate);
        Debug.Log("[Caballero] CABALLERO.png reescalado a 352x704.");
    }

    private static Texture2D GetReadableCopy(Texture2D source)
    {
        RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(source, rt);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D copy = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        copy.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
        copy.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);
        return copy;
    }
}
