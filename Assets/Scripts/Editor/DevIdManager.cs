using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.IO;
using GTAConfig = SanAndreasUnity.Utilities.Config;

namespace SanAndreasUnity.Editor
{
    [InitializeOnLoad]
    public class DevIdManager
    {
        private static string game_path = "";

        static DevIdManager()
        {
            //game_path = Environment.GetEnvironmentVariable("ProgramFiles");

            string configPath = GTAConfig.FileName,
                   contents = File.ReadAllText(configPath);
            var obj = contents.JsonDeserialize<JObject>();

            var prop = obj[GTAConfig.const_game_dir];
            string game_dir = prop != null ? prop.Value<string>() : "";
            bool isSet = true;

            if (prop != null)
                obj.Remove(GTAConfig.const_game_dir);

            var objDev = obj[GTAConfig.const_dev_profiles];

            if (objDev != null)
            {
                Dictionary<string, string> devs = objDev.Value<Dictionary<string, string>>();
                game_dir = devs.Where(x => x.Key == SystemInfo.deviceUniqueIdentifier).FirstOrDefault().Value;
            }
            else
                isSet = false;

            if (string.IsNullOrEmpty(game_dir))
                game_path = EditorUtility.OpenFolderPanel("Select GTA instalation Path", game_path, "");
            else
                game_path = game_dir;

            if (!isSet)
                obj[GTAConfig.const_dev_profiles] = JObject.FromObject(new Dictionary<string, string> { { SystemInfo.deviceUniqueIdentifier, game_path } });

            string postContents = obj.JsonSerialize(true);
            if (postContents != contents)
                File.WriteAllText(configPath, postContents);
        }
    }
}