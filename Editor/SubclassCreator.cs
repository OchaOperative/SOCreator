using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using static System.Net.Mime.MediaTypeNames;

public static class SubclassCreator
{
    private const string templatePath = "Packages/com.cjs.socreator/Editor/Templates/";

    private const BindingFlags flags = BindingFlags.Default | BindingFlags.Instance;

    [MenuItem("Assets/Create Subclass")]
    public static void CreateNewSubclass()
    {
        var script = Selection.activeObject as MonoScript;

        string methodTemplate = File.ReadAllText(templatePath + "Method-Override-Template.cs.txt");
        string classBody = "";

        MethodInfo[] superMethods = script.GetClass().GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        for (int i = 0; i < superMethods.Length; i++)
        {
            var method = superMethods[i];

            if (method.IsPrivate) continue;
            if (method.IsFinal) continue;

            if (method.IsAbstract)
            {
                string declaration = methodTemplate;
                string accessor;

                if (method.IsFamily)
                {
                    accessor = "protected";
                }
                else if (method.IsPublic)
                {
                    accessor = "public";
                }
                else if (method.IsAssembly)
                {
                    accessor = "internal";
                }
                else
                {
                    throw new NotImplementedException();
                }

                declaration = declaration.Replace("#ACCESS#", accessor);

                declaration = declaration.Replace("#METHODNAME#", method.Name);

                using (var provider = new CSharpCodeProvider())
                {
                    var typeref = new CodeTypeReference(method.ReturnType);
                    var methodReturn = provider.GetTypeOutput(typeref);
                    declaration = declaration.Replace("#RETURN#", methodReturn);
                }

                classBody += declaration;
            }
        }

        string classTemplate = File.ReadAllText(templatePath + "Subclass-Template.cs.txt");


        classTemplate = classTemplate.Replace("#BODY#", classBody);

        classTemplate = classTemplate.Replace("#SUPER#", script.name);

        string productPath = templatePath + "temp.txt";
        File.WriteAllText(productPath, classTemplate);
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
