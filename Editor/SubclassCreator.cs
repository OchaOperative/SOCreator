using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;

public static class SubclassCreator
{
    private const string templatePath = "Packages/com.cjs.socreator/Editor/Templates/";

    [MenuItem("Assets/Create Subclass")]
    public static void CreateNewSubclass()
    {
        var script = Selection.activeObject as MonoScript;

        string methodTemplate = File.ReadAllText(templatePath + "Method-Override-Template.cs.txt");

        HashSet<string> directives = new HashSet<string>();
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

                var parameters = method.GetParameters();
                string par = "";

                for (int j = 0; j < parameters.Length; j++)
                {
                    if (j >= 1)
                    {
                        par += ", ";
                    }

                    var p = parameters[j];

                    par += GetTypeString(p.ParameterType);
                    par += " " + p.Name;

                    directives.Add(p.ParameterType.Namespace);
                }

                var name = method.IsGenericMethodDefinition ? method.Name + "<T>" : method.Name;

                declaration = declaration.Replace("#ACCESS#", accessor);

                declaration = declaration.Replace("#METHODNAME#", name);

                declaration = declaration.Replace("#RETURN#", GetTypeString(method.ReturnType));

                declaration = declaration.Replace("#PARAMS#", par);

                classBody += declaration;

                directives.Add(method.ReturnType.Namespace);
            }
        }

        string classTemplate = File.ReadAllText(templatePath + "Subclass-Template.cs.txt");


        string usingDirectives = "";
        foreach (var d in directives)
        {
            usingDirectives += "using " + d + ";\n";
        }

        string ns = script.GetClass().Namespace;

        classTemplate = RemoveOrInsertNamespace(classTemplate, "namespace " + ns);

        classTemplate = classTemplate.Replace("#USINGS#", usingDirectives);

        classTemplate = classTemplate.Replace("#BODY#", classBody);

        classTemplate = classTemplate.Replace("#SUPER#", script.name);

        classTemplate = classTemplate.Replace("*", "\t");

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

    private static string GetTypeString(Type t)
    {
        using var provider = new CSharpCodeProvider();
        var typeref = new CodeTypeReference(t);
        var typeString = provider.GetTypeOutput(typeref);
        return Regex.Match(typeString, @"[^.]+$").Value;
    }

    internal static string RemoveOrInsertNamespace(string content, string item)
    {
        string text = "#NAMESPACEBEGIN#";
        string text2 = "#NAMESPACEEND#";
        if (!content.Contains(text) || !content.Contains(text2))
        {
            return content;
        }

        if (string.IsNullOrEmpty(item))
        {
            content = Regex.Replace(content, "((\\r\\n)|\\n)?[ \\t]*" + text + "[ \\t]*", string.Empty);
            content = Regex.Replace(content, "((\\r\\n)|\\n)?[ \\t]*" + text2 + "[ \\t]*", string.Empty);
            return content;
        }

        string separator = (content.Contains("\r\n") ? "\r\n" : "\n");
        List<string> list = new List<string>(content.Split(new string[3] { "\r\n", "\r", "\n" }, StringSplitOptions.None));
        int i;
        for (i = 0; i < list.Count && !list[i].Contains(text); i++)
        {
        }

        string text3 = list[i];
        string text4 = text3.Substring(0, text3.IndexOf("#"));
        list[i] = item;
        list.Insert(i + 1, "{");
        for (i += 2; i < list.Count; i++)
        {
            string text5 = list[i];
            if (!string.IsNullOrEmpty(text5) && text5.Trim().Length != 0)
            {
                if (text5.Contains(text2))
                {
                    list[i] = "}";
                    break;
                }

                list[i] = text4 + text5;
            }
        }

        return string.Join(separator, list.ToArray());
    }
}