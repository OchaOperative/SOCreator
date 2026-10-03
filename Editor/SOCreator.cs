using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Static class used to create a new individual instance of a ScriptableObject
/// easily without having to use the CreateAssetMenu attribute.
/// </summary>
public static class SOCreator
{
    /// <summary>
    /// Creates a new instance of the currently selected ScriptableObject. 
    /// <para></para>
    /// Menu item can be accessed through context menu upon right clicking an object
    /// in the project window or in the Assets tab.
    /// </summary>
    [MenuItem("Assets/Create SO Instance")]
    public static void CreateScriptableObjectInstance()
    {
        MonoScript script = Selection.activeObject as MonoScript;

        ScriptableObject asset = ScriptableObject.CreateInstance(script.GetClass());
        string path = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(script));

        ProjectWindowUtil.CreateAsset(asset, $"{path}.asset");
    }

    /// <summary>
    /// Validation method. Ensures the menu item is uninteractable if selection is not valid.
    /// </summary>
    /// <returns>Returns true if the selected object is an instanceable ScriptableObject.</returns>
    [MenuItem("Assets/Create SO Instance", true)]
    public static bool ValidateSelectedSO()
    {
        MonoScript sel = Selection.activeObject as MonoScript;

        if (sel == null)
        {
            return false;
        }

        if (sel.GetClass().GetTypeInfo().IsAbstract)
        {
            return false;
        }

        if (sel.GetClass().IsSubclassOf(typeof(ScriptableObject)))
        {
            return true;
        }

        return false;
    }
}
