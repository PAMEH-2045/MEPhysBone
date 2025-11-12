using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class SetupSceneForBuild
{
    static SetupSceneForBuild()
    {
        if (Application.isBatchMode)
        {
            AddScenesToBuild();
        }
    }

    [MenuItem("Build/Setup Scene For Build")]
    public static void AddScenesToBuild()
    {
        string ScenePath = Directory.GetFiles("Assets/MATE ENGINE - Scenes", "*.unity", SearchOption.AllDirectories)
            .FirstOrDefault(p => Path.GetFileName(p).ToLower().Contains("main"));

        if (!System.IO.File.Exists(ScenePath))
        {
            Debug.LogError($"Scene not found at path: {ScenePath}");
            return;
        }

        EditorSceneManager.OpenScene(ScenePath);

        GameObject go = new GameObject("Dummy");

        go.AddComponent(typeof(VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone));
        go.AddComponent(typeof(VRC.SDK3.Dynamics.Constraint.Components.VRCRotationConstraint));
        go.AddComponent(typeof(VRC.SDK3.Dynamics.Contact.Components.VRCContactReceiver));

        EditorSceneManager.MarkSceneDirty(go.scene);
        EditorSceneManager.SaveScene(go.scene);

        var scene = new EditorBuildSettingsScene(ScenePath, true);
        EditorBuildSettings.scenes = new[] { scene };

        AssetDatabase.SaveAssets();
        Debug.Log($"Scene {ScenePath} registered to build");
    }
}
