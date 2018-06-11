using UnityEngine;
using UnityEditor;
using System.IO;

[InitializeOnLoad]
public class DLLStarter
{
    static DLLStarter()
    {
        if (File.Exists(DLLManager.storePath))
        {
            var objs = DLLManager.LoadInfo();

            foreach (var obj in objs)
            {
                try
                {
                    //Debug.LogFormat("Load assembly as a PluginImporter at {0}!", obj.Key);
                    PluginImporter importer = AssetImporter.GetAtPath(obj.Key) as PluginImporter;
                    DLLFileWrapperInspector.IgnoreAssembly(importer, (bool)obj.Value);
                }
                catch
                {
                    // Must review: If this is reached is because assembly is already disabled
                    //Debug.Log("Assembly already disabled!");
                }
            }
        }
    }
}