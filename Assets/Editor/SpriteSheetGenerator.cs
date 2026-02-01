private Texture2D CreateSpritesheet(List<Texture2D> sprites, int gridSize, int spriteWidth, int spriteHeight, int spacing)
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

private void ConfigureSpritesheet(string outputPath, int gridSize, int spriteWidth, int spriteHeight, int spacing)
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

        List<SpriteMetaData> slices = new List<SpriteMetaData>();
        int spriteIndex = 0;

        for (int gridY = 0; gridY < gridSize; gridY++)
        {
            for (int gridX = 0; gridX < gridSize && spriteIndex < gridSize * gridSize; gridX++)
            {
                SpriteMetaData slice = new SpriteMetaData();
                slice.name = $"Sprite_{spriteIndex:D3}";
                
                // INVERTIR aquí también
                int posY = (int)(sheetHeight - (gridY + 1) * (spriteHeight + spacing));
                
                slice.rect = new Rect(
                    gridX * (spriteWidth + spacing),
                    posY,
                    spriteWidth,
                    spriteHeight
                );
                slice.pivot = new Vector2(0.5f, 1f);
                slices.Add(slice);
                spriteIndex++;
            }
        }

        importer.spritesheet = slices.ToArray();
        AssetDatabase.ImportAsset(relativePath, ImportAssetOptions.ForceUpdate);
    }
}