using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEditor;
using UnityEngine;

public static class SubclassCreator
{
    private const string templatePath = "Packages/com.cjs.socreator/Editor/Templates/";

    [MenuItem("Assets/Create Subclass")]
    public static void CreateNewSubclass()
    {
        var script = Selection.activeObject as MonoScript;
        string text = File.ReadAllText(templatePath + "Subclass-Template.cs.txt");

        MethodInfo[] superMethods = script.GetClass().Module.GetMethods();

        string subMethods = "";

        foreach (MethodInfo method in superMethods)
        {
            if (method.IsAbstract)
            {
                string access;

                switch (method.Attributes)
                {
                    case MethodAttributes.Family:
                        access = "protected";
                        break;
                    case MethodAttributes.Private:
                        access = "private";
                        break;
                    case MethodAttributes.Public:
                        access = "public";
                        break;
                }
            }
        }

        text = text.Replace("#SUPER#", script.name);

        using (var sw = new StreamWriter(templatePath + "temp.txt"))
        {
            sw.Write(text);
        }


        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath + "temp.txt", $"Sub{script.name}.cs");

        
        
    }

    [MenuItem("Assets/Create Subclass", true)]
    public static bool ValidateSelectedMonobehaviour()
    {
        MonoScript sel = Selection.activeObject as MonoScript;

        if (sel == null) return false;

        if (sel.GetClass().GetTypeInfo().IsSealed) return false;

        return true;
    }
}
