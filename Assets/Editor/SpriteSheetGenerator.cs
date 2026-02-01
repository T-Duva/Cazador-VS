using UnityEngine;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using System.Collections.Generic;

public static class SpriteSheetGenerator
{
    public static Texture2D CreateSpritesheet(List<Texture2D> sprites, int gridSize, int spriteWidth, int spriteHeight, int spacing)
    {
        int sheetWidth = gridSize * (spriteWidth + spacing) - spacing;
        int sheetHeight = gridSize * (spriteHeight + spacing) - spacing;

        Texture2D spritesheet = new Texture2D(sheetWidth, sheetHeight, TextureFormat.RGBA32, false);
        Color[] sheetPixels = new Color[sheetWidth * sheetHeight];

        // Fill with transparent
        for (int i = 0; i < sheetPixels.Length; i++)
            sheetPixels[i] = new Color(0, 0, 0, 0);

        int spriteIndex = 0;
        for (int gridY = 0; gridY < gridSize && spriteIndex < sprites.Count; gridY++)
        {
            for (int gridX = 0; gridX < gridSize && spriteIndex < sprites.Count; gridX++)
            {
                Texture2D sprite = sprites[spriteIndex];
                Color[] spritePixels = sprite.GetPixels();

                int posX = gridX * (spriteWidth + spacing);
                // INVERTIR: sheetHeight - (gridY + 1) * spriteHeight coloca 1,2,3 ARRIBA
                int posY = sheetHeight - (gridY + 1) * (spriteHeight + spacing);

                for (int y = 0; y < sprite.height; y++)
                {
                    for (int x = 0; x < sprite.width; x++)
                    {
                        int srcIndex = y * sprite.width + x;
                        int dstX = posX + x;
                        int dstY = posY + y;
                        int dstIndex = dstY * sheetWidth + dstX;

                        if (dstIndex >= 0 && dstIndex < sheetPixels.Length)
                            sheetPixels[dstIndex] = spritePixels[srcIndex];
                    }
                }

                spriteIndex++;
            }
        }

        spritesheet.SetPixels(sheetPixels);
        spritesheet.Apply();
        return spritesheet;
    }

    public static void ConfigureSpritesheet(string outputPath, int gridSize, int spriteWidth, int spriteHeight, int spacing)
    {
        string relativePath = "Assets" + outputPath.Substring(Application.dataPath.Length);

        AssetDatabase.Refresh();

        TextureImporter importer = AssetImporter.GetAtPath(relativePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.filterMode = FilterMode.Point;
            importer.isReadable = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            
            // Guardar cambios antes de usar el data provider
            AssetDatabase.ImportAsset(relativePath, ImportAssetOptions.ForceUpdate);
            
            // Usar la nueva API ISpriteEditorDataProvider
            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
            if (dataProvider != null)
            {
                dataProvider.InitSpriteEditorDataProvider();
                
                List<SpriteRect> spriteRects = new List<SpriteRect>();
                int spriteIndex = 0;
                int sheetHeight = gridSize * (spriteHeight + spacing) - spacing;

                for (int gridY = 0; gridY < gridSize; gridY++)
                {
                    for (int gridX = 0; gridX < gridSize && spriteIndex < gridSize * gridSize; gridX++)
                    {
                        SpriteRect spriteRect = new SpriteRect();
                        spriteRect.name = $"Sprite_{spriteIndex:D3}";
                        
                        // INVERTIR aquí también
                        int posY = (int)(sheetHeight - (gridY + 1) * (spriteHeight + spacing));
                        
                        spriteRect.rect = new Rect(
                            gridX * (spriteWidth + spacing),
                            posY,
                            spriteWidth,
                            spriteHeight
                        );
                        spriteRect.pivot = new Vector2(0.5f, 1f);
                        spriteRect.alignment = SpriteAlignment.Custom;
                        spriteRects.Add(spriteRect);
                        spriteIndex++;
                    }
                }

                dataProvider.SetSpriteRects(spriteRects.ToArray());
                dataProvider.Apply();
            }
        }
    }
}
