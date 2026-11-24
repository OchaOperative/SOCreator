using System.IO;
using UnityEditor;

public static class ClassDuplicator
{
    private const string templatePath = "Packages/com.cjs.socreator/Editor/Templates/";

    [MenuItem("Assets/Duplicate Class")]
    public static void CreateNewSubclass()
    {
        MonoScript script = Selection.activeObject as MonoScript;

        int pFrom = script.text.IndexOf('{') + 1;

        string text = script.text.TrimEnd();
        string template = File.ReadAllText(templatePath + "Duplicate-Template.cs.txt");

        template = template.Replace("#BODY#", text[pFrom..^1]);

        string tempPath = templatePath + "temp.txt";

        File.WriteAllText(tempPath, template);

        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(tempPath, $"{script.name}.cs");

    }

    [MenuItem("Assets/Duplicate Class", true)]
    public static bool ValidateSelectedMonobehaviour()
    {
        MonoScript sel = Selection.activeObject as MonoScript;

        if (sel == null) return false;

        return true;
    }
}
