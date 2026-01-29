#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;

public static class AutoPivotCalculator
{
    /// <summary>
    /// Calcula el pivot en la base de los pies del sprite.
    /// </summary>
    public static Vector2 CalculateFootPivot(Texture2D texture, Rect spriteRect)
    {
        int startX = (int)spriteRect.x;
        int startY = (int)spriteRect.y;
        int width = (int)spriteRect.width;
        int height = (int)spriteRect.height;
        
        List<int> footXPositions = new List<int>();
        
        // Buscar en la ÚLTIMA FILA (donde están los pies)
        int bottomY = startY + height - 1;
        
        for (int x = 0; x < width; x++)
        {
            Color pixel = texture.GetPixel(startX + x, bottomY);
            
            if (pixel.a > 0.5f)
            {
                footXPositions.Add(x);
            }
        }
        
        float pivotX;
        
        if (footXPositions.Count > 0)
        {
            // Promedio de las posiciones X en esa fila
            float sum = 0;
            foreach (int x in footXPositions)
            {
                sum += x;
            }
            float centerX = sum / footXPositions.Count;
            pivotX = centerX / width;
        }
        else
        {
            pivotX = 0.5f;
        }
        
        float pivotY = 0.0f;
        
        return new Vector2(pivotX, pivotY);
    }
}
#endif