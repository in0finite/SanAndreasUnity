using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

//[InitializeOnLoad]
public class DLLStarter
{
    static DLLStarter()
    {
        if (File.Exists(DLLManager.storePath))
        {
            var objs = DLLManager.LoadInfo();

            foreach (var obj in objs)
            {
                PluginImporter importer = AssetDatabase.LoadAssetAtPath<PluginImporter>(obj.Key);
                DLLFileWrapperInspector.IgnoreAssembly(importer, (bool)obj.Value);
            }
        }
    }
}