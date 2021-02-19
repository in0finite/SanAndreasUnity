using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SanAndreasUnity.Utilities;
using UnityEngine;

namespace SanAndreasUnity.Behaviours
{
    public class PluginManager : MonoBehaviour
    {

        public static string[] PluginDirectories => new []
        {
            #if UNITY_EDITOR
            Path.Combine(Application.dataPath, "../SAU_Plugins"),
            #else
            Path.Combine(Application.dataPath, "Plugins"),
            #endif
        };

        void Awake()
        {
            F.RunExceptionSafe(LoadPlugins, $"Error while loading plugins: ");
        }

        void LoadPlugins()
        {
            // find all plugins

            var allFiles = new List<string>();
            foreach (string pluginsDirectory in PluginDirectories)
            {
                if (!Directory.Exists(pluginsDirectory))
                    continue;

                var filePaths = Directory.GetFiles(pluginsDirectory, "*.dll", SearchOption.TopDirectoryOnly);

                // sort files manually to prevent undefined behavior
                Array.Sort(filePaths);

                allFiles.AddRange(filePaths);
            }

            //allFiles.RemoveAll(path => !path.EndsWith(".json", StringComparison.InvariantCultureIgnoreCase) || !path.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase));


            int numLoaded = 0;
            foreach (string pluginFilePath in allFiles)
            {
                if (LoadPlugin(pluginFilePath))
                    numLoaded++;
            }

            if (allFiles.Count > 0)
                Debug.Log($"Loaded {numLoaded}/{allFiles.Count} plugins");
        }

        bool LoadPlugin(string pluginFilePath)
        {
            return F.RunExceptionSafe(
                () => Assembly.LoadFrom(pluginFilePath),
                $"error loading plugin from '{pluginFilePath}': ");
        }
    }
}
