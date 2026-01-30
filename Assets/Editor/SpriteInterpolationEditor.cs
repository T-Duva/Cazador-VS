using UnityEngine;
using UnityEditor;
using System.IO;

namespace SpriteMotionTools
{
    public class SpriteMotionEditor : EditorWindow
    {
        private Sprite startSprite;
        private Sprite endSprite;
        private int frameCount = 10;
        private float previewSlider = 0.5f;
        private string statusMessage = "";

        [MenuItem("Window/Sprite Motion Interpolator")]
        public static void ShowWindow()
        {
            GetWindow<SpriteMotionEditor>("Motion Interpolator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Sprite Morphing Tool", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Inputs
            startSprite = (Sprite)EditorGUILayout.ObjectField("Start Sprite", startSprite, typeof(Sprite), false);
            endSprite = (Sprite)EditorGUILayout.ObjectField("End Sprite", endSprite, typeof(Sprite), false);
            frameCount = EditorGUILayout.IntSlider("Total Frames", frameCount, 3, 60);

            EditorGUILayout.Space();
            GUILayout.Label("Preview Result", EditorStyles.boldLabel);
            
            // Slider Preview
            previewSlider = EditorGUILayout.Slider("Preview Morph", previewSlider, 0f, 1f);
            DrawPreview();

            EditorGUILayout.Space();

            // Validation & Generation
            bool valid = ValidateInputs();
            GUI.enabled = valid;
            
            if (GUILayout.Button("Generate Sequence", GUILayout.Height(40)))
            {
                GenerateSequence();
            }
            GUI.enabled = true;

            if (!string.IsNullOrEmpty(statusMessage))
            {
                EditorGUILayout.HelpBox(statusMessage, MessageType.Info);
            }
        }

        private void DrawPreview()
        {
            if (startSprite == null || endSprite == null) return;

            Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(150));
            rect.width = 150;
            rect.x = (Screen.width - 150) / 2;

            // Draw Background
            EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f));

            // Draw Start Sprite (Base)
            GUI.DrawTextureWithTexCoords(rect, startSprite.texture, GetUVs(startSprite));

            // Draw End Sprite (Overlay with Alpha)
            Color c = GUI.color;
            GUI.color = new Color(1, 1, 1, previewSlider);
            GUI.DrawTextureWithTexCoords(rect, endSprite.texture, GetUVs(endSprite));
            GUI.color = c;
        }

        private Rect GetUVs(Sprite sprite)
        {
            // Normalize UVs for sprites in a sheet
            Rect texRect = sprite.textureRect;
            Texture2D tex = sprite.texture;
            return new Rect(
                texRect.x / tex.width,
                texRect.y / tex.height,
                texRect.width / tex.width,
                texRect.height / tex.height
            );
        }

        private bool ValidateInputs()
        {
            if (startSprite == null || endSprite == null)
            {
                statusMessage = "Select Start and End sprites.";
                return false;
            }

            if ((int)startSprite.rect.width != (int)endSprite.rect.width ||
                (int)startSprite.rect.height != (int)endSprite.rect.height)
            {
                statusMessage = "Sprites must have the SAME dimensions.";
                return false;
            }

            try 
            {
                // Check Read/Write
                startSprite.texture.GetPixel(0, 0);
            }
            catch
            {
                statusMessage = "Enable 'Read/Write' in Texture Import Settings.";
                return false;
            }

            statusMessage = "Ready to generate.";
            return true;
        }

        private void GenerateSequence()
        {
            string path = EditorUtility.SaveFolderPanel("Save Sequence", "Assets", "MorphSequence");
            if (string.IsNullOrEmpty(path)) return;

            int width = (int)startSprite.rect.width;
            int height = (int)startSprite.rect.height;

            Color[] startPixels = startSprite.texture.GetPixels((int)startSprite.rect.x, (int)startSprite.rect.y, width, height);
            Color[] endPixels = endSprite.texture.GetPixels((int)endSprite.rect.x, (int)endSprite.rect.y, width, height);
            
            for (int i = 0; i < frameCount; i++)
            {
                float t = (float)i / (frameCount - 1);
                Texture2D frame = new Texture2D(width, height, TextureFormat.RGBA32, false);
                Color[] newPixels = new Color[width * height];

                for (int p = 0; p < newPixels.Length; p++)
                {
                    newPixels[p] = Color.Lerp(startPixels[p], endPixels[p], t);
                }

                frame.SetPixels(newPixels);
                frame.Apply();

                byte[] bytes = frame.EncodeToPNG();
                File.WriteAllBytes(Path.Combine(path, $"frame_{i:D3}.png"), bytes);
                DestroyImmediate(frame);
            }

            AssetDatabase.Refresh();
            statusMessage = "Sequence Generated!";
        }
    }
}
