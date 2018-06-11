using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.IO;
using GTAConfig = SanAndreasUnity.Utilities.Config;
using System;

public class DevProfiles
{
    public static string CheckDevProfiles(Func<string> folderList)
    {
        //game_path = Environment.GetEnvironmentVariable("ProgramFiles");
        string game_path = "";

        string configPath = GTAConfig.FileName,
               contents = File.ReadAllText(configPath);
        var obj = contents.JsonDeserialize<JObject>();

        var prop = obj[GTAConfig.const_game_dir];
        string game_dir = prop != null ? prop.ToObject<string>() : "";

        bool isSet = true;

        if (prop != null)
            obj.Remove(GTAConfig.const_game_dir);

        var objDev = obj[GTAConfig.const_dev_profiles];

        if (objDev != null)
        {
            Dictionary<string, string> devs = objDev.ToObject<Dictionary<string, string>>();
            game_dir = devs.Where(x => x.Key == SystemInfo.deviceUniqueIdentifier).FirstOrDefault().Value;
        }
        else
            isSet = false;

        if (string.IsNullOrEmpty(game_dir))
            game_path = folderList();
        else
            game_path = game_dir;

        if (!isSet)
            obj[GTAConfig.const_dev_profiles] = JObject.FromObject(new Dictionary<string, string> { { SystemInfo.deviceUniqueIdentifier, game_path } });

        string postContents = obj.JsonSerialize(true);
        if (postContents != contents)
            File.WriteAllText(configPath, postContents);

        return game_path;
    }
}