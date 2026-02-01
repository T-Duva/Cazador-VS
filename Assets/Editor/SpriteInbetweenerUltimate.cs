using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace SpriteMultiKeyframeTools
{
    public class SpriteMultiKeyframeEditor : EditorWindow
    {
        private List<Sprite> keyframeSprites = new List<Sprite>();
        private int framesPerTransition = 10;
        private int previewTransitionIndex = 0;
        private float previewSlider = 0.5f;
        private string statusMessage = "";
        private Vector2 scrollPosition = Vector2.zero;

        [MenuItem("Window/Multi-Keyframe Interpolator (30)")]
        public static void ShowWindow()
        {
            GetWindow<SpriteMultiKeyframeEditor>("Multi-Keyframe Interpolator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Multi-Keyframe Sprite Interpolator", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("Keyframe Sprites (Up to 30)", EditorStyles.boldLabel);
            
            int newSize = EditorGUILayout.IntField("Number of Keyframes", keyframeSprites.Count);
            while (keyframeSprites.Count < newSize && keyframeSprites.Count < 30)
                keyframeSprites.Add(null);
            while (keyframeSprites.Count > newSize)
                keyframeSprites.RemoveAt(keyframeSprites.Count - 1);

            for (int i = 0; i < keyframeSprites.Count; i++)
            {
                keyframeSprites[i] = (Sprite)EditorGUILayout.ObjectField($"Keyframe {i + 1}", keyframeSprites[i], typeof(Sprite), false);
            }

            EditorGUILayout.Space();
            framesPerTransition = EditorGUILayout.IntSlider("Frames Per Transition", framesPerTransition, 2, 50);

            EditorGUILayout.Space();
            GUILayout.Label("Preview", EditorStyles.boldLabel);

            List<string> transitionOptions = new List<string>();
            for (int i = 0; i < keyframeSprites.Count - 1; i++)
            {
                string fromName = keyframeSprites[i] != null ? keyframeSprites[i].name : $"Keyframe {i + 1}";
                string toName = keyframeSprites[i + 1] != null ? keyframeSprites[i + 1].name : $"Keyframe {i + 2}";
                transitionOptions.Add($"{fromName} → {toName}");
            }

            if (transitionOptions.Count > 0)
            {
                previewTransitionIndex = EditorGUILayout.Popup("Transition", previewTransitionIndex, transitionOptions.ToArray());
                previewSlider = EditorGUILayout.Slider("Preview Morph", previewSlider, 0f, 1f);
                DrawPreview();
            }
            else
            {
                EditorGUILayout.HelpBox("Add at least 2 keyframes to preview", MessageType.Warning);
            }

            EditorGUILayout.Space();

            bool valid = ValidateInput();
            GUI.enabled = valid;

            if (GUILayout.Button("Generate Full Sequence", GUILayout.Height(40)))
            {
                GenerateSequence();
            }

            GUI.enabled = true;

            if (!string.IsNullOrEmpty(statusMessage))
            {
                EditorGUILayout.HelpBox(statusMessage, MessageType.Info);
            }

            GUILayout.EndScrollView();
        }

        private void DrawPreview()
        {
            if (previewTransitionIndex >= keyframeSprites.Count - 1) return;

            Sprite startSprite = keyframeSprites[previewTransitionIndex];
            Sprite endSprite = keyframeSprites[previewTransitionIndex + 1];

            if (startSprite == null || endSprite == null) return;

            Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(150));
            rect.width = 150;
            rect.x = (Screen.width - 150) / 2;

            EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f));

            GUI.DrawTextureWithTexCoords(rect, startSprite.texture, GetUVs(startSprite));

            Color c = GUI.color;
            GUI.color = new Color(1, 1, 1, previewSlider);
            GUI.DrawTextureWithTexCoords(rect, endSprite.texture, GetUVs(endSprite));
            GUI.color = c;
        }

        private Rect GetUVs(Sprite sprite)
        {
            Rect texRect = sprite.textureRect;
            Texture2D tex = sprite.texture;
            return new Rect(
                texRect.x / tex.width,
                texRect.y / tex.height,
                texRect.width / tex.width,
                texRect.height / tex.height
            );
        }

        private bool ValidateInput()
        {
            if (keyframeSprites.Count < 2)
            {
                statusMessage = "Add at least 2 keyframes";
                return false;
            }

            for (int i = 0; i < keyframeSprites.Count; i++)
            {
                if (keyframeSprites[i] == null)
                {
                    statusMessage = $"Keyframe {i + 1} is empty";
                    return false;
                }
            }

            try
            {
                keyframeSprites[0].texture.GetPixel(0, 0);
            }
            catch
            {
                statusMessage = "Enable 'Read/Write' in Texture Import Settings";
                return false;
            }

            statusMessage = "Ready to generate";
            return true;
        }

        private void GetMaxDimensions(out int maxWidth, out int maxHeight)
        {
            maxWidth = 0;
            maxHeight = 0;

            foreach (Sprite sprite in keyframeSprites)
            {
                if (sprite != null)
                {
                    int w = (int)sprite.rect.width;
                    int h = (int)sprite.rect.height;
                    if (w > maxWidth) maxWidth = w;
                    if (h > maxHeight) maxHeight = h;
                }
            }

            Debug.Log($"[MultiKeyframe] Max dimensions found: {maxWidth}x{maxHeight}");
        }

        private void GenerateSequence()
        {
            string folderPath = EditorUtility.SaveFolderPanel("Save Sequence", "Assets", "InterpolatedSequence");
            if (string.IsNullOrEmpty(folderPath)) return;

            // Find max dimensions
            GetMaxDimensions(out int maxWidth, out int maxHeight);

            EditorUtility.DisplayProgressBar("Generating", "Processing keyframes...", 0f);

            try
            {
                int totalFrames = 0;

                for (int transitionIndex = 0; transitionIndex < keyframeSprites.Count - 1; transitionIndex++)
                {
                    Sprite startSprite = keyframeSprites[transitionIndex];
                    Sprite endSprite = keyframeSprites[transitionIndex + 1];

                    // Extract and expand sprites to max size
                    Texture2D startTex = ExtractAndExpandSpriteTexture(startSprite, maxWidth, maxHeight);
                    Texture2D endTex = ExtractAndExpandSpriteTexture(endSprite, maxWidth, maxHeight);

                    if (startTex == null || endTex == null)
                    {
                        Debug.LogError($"Failed to extract sprites for transition {transitionIndex}");
                        continue;
                    }

                    string baseName = startSprite.name;

                    for (int frameIndex = 0; frameIndex < framesPerTransition; frameIndex++)
                    {
                        float t = (float)frameIndex / (framesPerTransition - 1);
                        Texture2D interpolated = CreateInterpolatedFrame(startTex, endTex, t);

                        string filename = $"{baseName}_{frameIndex + 1:D3}.png";
                        string filePath = Path.Combine(folderPath, filename);

                        byte[] pngData = interpolated.EncodeToPNG();
                        File.WriteAllBytes(filePath, pngData);

                        DestroyImmediate(interpolated);
                        totalFrames++;

                        float progress = (float)totalFrames / ((keyframeSprites.Count - 1) * framesPerTransition);
                        EditorUtility.DisplayProgressBar("Generating", $"Frame {totalFrames}...", progress);
                    }

                    DestroyImmediate(startTex);
                    DestroyImmediate(endTex);
                }

                AssetDatabase.Refresh();
                statusMessage = $"✅ Generated {totalFrames} frames successfully!";
                Debug.Log($"[MultiKeyframe] Generated {totalFrames} frames in {folderPath}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private Texture2D ExtractAndExpandSpriteTexture(Sprite sprite, int targetWidth, int targetHeight)
        {
            if (sprite == null) return null;

            Texture2D sourceTexture = sprite.texture;
            if (sourceTexture == null) return null;

            Rect spriteRect = sprite.rect;
            int x = Mathf.FloorToInt(spriteRect.x);
            int y = Mathf.FloorToInt(spriteRect.y);
            int width = Mathf.FloorToInt(spriteRect.width);
            int height = Mathf.FloorToInt(spriteRect.height);

            if (width <= 0 || height <= 0) return null;

            // Ensure readable
            string path = AssetDatabase.GetAssetPath(sourceTexture);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && !importer.isReadable)
            {
                importer.isReadable = true;
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                sourceTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }

            try
            {
                // Extract original sprite pixels
                Color[] spritePixels = sourceTexture.GetPixels(x, y, width, height);

                // Create expanded texture
                Texture2D expanded = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);
                expanded.filterMode = FilterMode.Point;

                // Fill with transparent
                Color[] expandedPixels = new Color[targetWidth * targetHeight];
                for (int i = 0; i < expandedPixels.Length; i++)
                {
                    expandedPixels[i] = new Color(0, 0, 0, 0);
                }

                // Calculate offset to center the sprite (pivot at 0.5, 0.5 = center)
                int offsetX = (targetWidth - width) / 2;
                int offsetY = (targetHeight - height) / 2;

                // Place original sprite pixels in the center
                for (int row = 0; row < height; row++)
                {
                    for (int col = 0; col < width; col++)
                    {
                        int srcIndex = row * width + col;
                        int dstX = col + offsetX;
                        int dstY = row + offsetY;
                        int dstIndex = dstY * targetWidth + dstX;

                        if (dstIndex >= 0 && dstIndex < expandedPixels.Length)
                        {
                            expandedPixels[dstIndex] = spritePixels[srcIndex];
                        }
                    }
                }

                expanded.SetPixels(expandedPixels);
                expanded.Apply();

                Debug.Log($"[MultiKeyframe] Expanded {sprite.name}: {width}x{height} → {targetWidth}x{targetHeight} (offset: {offsetX}, {offsetY})");

                return expanded;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error extracting/expanding sprite {sprite.name}: {e.Message}");
                return null;
            }
        }

        private Texture2D CreateInterpolatedFrame(Texture2D start, Texture2D end, float t)
        {
            if (start.width != end.width || start.height != end.height)
            {
                Debug.LogError("Dimensions must match!");
                return null;
            }

            Texture2D result = new Texture2D(start.width, start.height, TextureFormat.RGBA32, false);
            result.filterMode = FilterMode.Point;

            Color[] startPixels = start.GetPixels();
            Color[] endPixels = end.GetPixels();
            Color[] resultPixels = new Color[startPixels.Length];

            for (int i = 0; i < resultPixels.Length; i++)
            {
                resultPixels[i] = Color.Lerp(startPixels[i], endPixels[i], t);
            }

            result.SetPixels(resultPixels);
            result.Apply();
            return result;
        }
    }
}