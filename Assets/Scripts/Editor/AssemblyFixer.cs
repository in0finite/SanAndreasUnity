using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class AssemblyFixer
{
    static AssemblyFixer()
    {
        var folders = CustomSearcher.GetDirectories(Application.dataPath).Where(x => x.Contains("NotCompile"));

        foreach (string f in folders)
        {
            PluginImporter importer = AssetImporter.GetAtPath(f) as PluginImporter;
            importer.SetCompatibleWithAnyPlatform(false);
            importer.SetCompatibleWithEditor(false);
        }
    }
}