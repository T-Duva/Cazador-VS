using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class AsignarSpritesAuto
{
    #region Hooks de Unity Editor
    static AsignarSpritesAuto()
    {
        // Ejecutar cuando se carga el proyecto o se cambia de escena
        EditorApplication.delayCall += AsignarSpritesAlGameManager;
    }
    #endregion
    
    #region Asignacion automatica
    static void AsignarSpritesAlGameManager()
    {
        // Buscar el GameManager en la escena
        GameManager gm = Object.FindFirstObjectByType<GameManager>();
        if (gm == null) return;
        
        // Asignar sprites solo si están vacíos
        if (gm.guerreroJugadorSprite == null)
        {
            gm.guerreroJugadorSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Personajes/Guerrero/1.png");
        }
        
        if (gm.guerreroEnemigoSprite == null)
        {
            gm.guerreroEnemigoSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Personajes/Guerrero/1.png");
            if (gm.guerreroEnemigoSprite == null && gm.guerreroJugadorSprite != null)
            {
                gm.guerreroEnemigoSprite = gm.guerreroJugadorSprite;
            }
        }
        
        if (gm.magoJugadorSprite == null)
        {
            gm.magoJugadorSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Personajes/Mago/1.png");
        }
        
        if (gm.magoEnemigoSprite == null)
        {
            gm.magoEnemigoSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Personajes/Mago/1.png");
            if (gm.magoEnemigoSprite == null && gm.magoJugadorSprite != null)
            {
                gm.magoEnemigoSprite = gm.magoJugadorSprite;
            }
        }
        
        if (gm.cazadorJugadorSprite == null)
        {
            gm.cazadorJugadorSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Personajes/Cazador/1.png");
        }
        
        if (gm.cazadorEnemigoSprite == null)
        {
            gm.cazadorEnemigoSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Personajes/Cazador/1.png");
            if (gm.cazadorEnemigoSprite == null && gm.cazadorJugadorSprite != null)
            {
                gm.cazadorEnemigoSprite = gm.cazadorJugadorSprite;
            }
        }
    }
    #endregion
    
    #region Menu
    [MenuItem("Tools/Asignar Sprites Automáticamente")]
    public static void AsignarSpritesManual()
    {
        AsignarSpritesAlGameManager();
        EditorUtility.DisplayDialog("✅ Sprites Asignados", 
            "Se intentaron asignar los sprites automáticamente.\n\n" +
            "Si no se asignaron, verifica que los sprites estén en:\n" +
            "• Assets/Sprites/Personajes/Guerrero/1.png\n" +
            "• Assets/Sprites/Personajes/Mago/1.png\n" +
            "• Assets/Sprites/Personajes/Cazador/1.png", 
            "OK");
    }
    #endregion
}

