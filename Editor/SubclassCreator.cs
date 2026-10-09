using System.IO;
using UnityEditor;
using UnityEngine;

public static class SubclassCreator
{
    [MenuItem("Assets/Duplicate class")]
    public static void CreateNewSubclass()
    {
        MonoScript script = Selection.activeObject as MonoScript;

        ProjectWindowUtil.CreateAssetWithContent($"Duplicate{script.name}.cs",
            File.ReadAllText(AssetDatabase.GetAssetPath(script)));
    }

    [MenuItem("Assets/Duplicate class", true)]
    public static bool ValidateSelectedMonobehaviour()
    {
        MonoScript sel = Selection.activeObject as MonoScript;

        if (sel == null) return false;

        return true;
    }
}
