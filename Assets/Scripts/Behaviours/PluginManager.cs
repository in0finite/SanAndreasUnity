using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UGameCore.Utilities;
using UnityEngine;

namespace SanAndreasUnity.Behaviours
{
    public class PluginManager : MonoBehaviour
    {
        public abstract class PluginBase
        {
        }

        public static string[] PluginDirectories => new []
        {
            #if UNITY_EDITOR
            Path.Combine(Application.dataPath, $"..{Path.DirectorySeparatorChar}SAU_Plugins"),
            #else
            Path.Combine(Application.dataPath, "SAU_Plugins"),
            #endif
        };

        void Start()
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
                () =>
                {
                    var asm = Assembly.LoadFrom(pluginFilePath);
                    var types = asm.GetTypes();
                    var pluginTypes = types.Where(t => typeof(PluginBase).IsAssignableFrom(t) && !t.IsAbstract);
                    pluginTypes.ForEach(t => Activator.CreateInstance(t));
                },
                $"error loading plugin from '{pluginFilePath}': ");
        }
    }
}
