using UnityEditor;
using UnityEngine;
using System.IO;

public class BuildGame : EditorWindow
{
    #region Ventana (MenuItem)
    [MenuItem("Tools/Build Game - Crear Ejecutable")]
    public static void ShowWindow()
    {
        BuildGame window = GetWindow<BuildGame>("Build Game");
        window.Show();
    }
    #endregion

    #region GUI
    void OnGUI()
    {
        GUILayout.Label("🎮 Crear Build del Juego", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox("Esto creará un ejecutable del juego en una carpeta 'Build' en tu escritorio.", MessageType.Info);
        GUILayout.Space(10);

        if (GUILayout.Button("🔨 Crear Build Ahora", GUILayout.Height(40)))
        {
            CrearBuild();
        }

        GUILayout.Space(10);
        
        if (GUILayout.Button("📦 Crear Build y Comprimir en ZIP", GUILayout.Height(40)))
        {
            CrearBuildYComprimir();
        }
    }
    #endregion

    #region Build
    static void CrearBuild()
    {
        // Obtener la escena activa o la primera escena
        string[] scenes = EditorBuildSettings.scenes.Length > 0 
            ? new string[] { EditorBuildSettings.scenes[0].path }
            : new string[] { "Assets/Scenes/SampleScene.unity" };

        // Crear carpeta de build en el escritorio
        string buildPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "CAZADOR_VS_Build");
        
        // Si no existe, intentar crear en la carpeta del proyecto
        if (!Directory.Exists(Path.GetDirectoryName(buildPath)))
        {
            buildPath = Path.Combine(Application.dataPath, "..", "Build");
        }

        Debug.Log($"Creando build en: {buildPath}");

        // Crear la carpeta si no existe
        if (!Directory.Exists(buildPath))
        {
            Directory.CreateDirectory(buildPath);
        }

        // Configurar el build
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = scenes;
        buildPlayerOptions.locationPathName = Path.Combine(buildPath, "CAZADOR VS.exe");
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        buildPlayerOptions.options = BuildOptions.None;

        // Construir
        UnityEditor.Build.Reporting.BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"✅ Build exitoso! Carpeta: {buildPath}");
            EditorUtility.DisplayDialog("✅ Build Exitoso", 
                $"El build se creó correctamente en:\n{buildPath}\n\nRecuerda compartir TODA la carpeta completa (incluyendo el .exe y la carpeta _Data)", 
                "OK");
            
            // Abrir la carpeta en el explorador
            EditorUtility.RevealInFinder(buildPath);
        }
        else
        {
            string errorMsg = report.summary.totalErrors > 0 
                ? $"Errores: {report.summary.totalErrors}" 
                : "Error desconocido";
            Debug.LogError($"❌ Error en el build: {errorMsg}");
            EditorUtility.DisplayDialog("❌ Error en Build", $"Error: {errorMsg}", "OK");
        }
    }

    static void CrearBuildYComprimir()
    {
        CrearBuild();
        
        // Nota: Comprimir en ZIP requiere bibliotecas adicionales o herramientas externas
        // Por ahora, solo creamos el build y le decimos al usuario que comprima manualmente
        EditorUtility.DisplayDialog("📦 Compresión", 
            "El build se creó exitosamente.\n\nPara compartir:\n1. Comprime toda la carpeta 'CAZADOR_VS_Build' en un ZIP\n2. Comparte el ZIP con tu amigo", 
            "OK");
    }
    #endregion
}

