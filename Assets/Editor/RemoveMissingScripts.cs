using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class RemoveMissingScripts
{
    [MenuItem("Tools/Cleanup/Remove Missing Scripts (Scene)")]
    public static void Run()
    {
        int removed = 0;

        var scene = SceneManager.GetActiveScene();
        var roots = scene.GetRootGameObjects();

        foreach (var root in roots)
            removed += RemoveMissingRecursively(root);

        Debug.Log($"Removed {removed} missing script component(s) from scene '{scene.name}'.");
    }

    private static int RemoveMissingRecursively(GameObject go)
    {
        int count = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);

        for (int i = 0; i < go.transform.childCount; i++)
            count += RemoveMissingRecursively(go.transform.GetChild(i).gameObject);

        return count;
    }
}