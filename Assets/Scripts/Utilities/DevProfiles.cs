using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.IO;
using GTAConfig = SanAndreasUnity.Utilities.Config;
using System;
using SanAndreasUnity.Utilities;

public class DevProfiles
{
    private static JObject _obj;

    public static JObject obj
    {
        get
        {
            if (_obj == null)
                _obj = DeserializeProfiles();
            return _obj;
        }
    }

    public static int ActiveProfile
    {
        get
        {
            try
            {
                return _obj[GTAConfig.const_active_dev_profile].ToObject<Dictionary<string, int>>().FirstOrDefault(x => x.Key == SystemInfo.deviceUniqueIdentifier).Value;
            }
            catch
            {
                return -1;
            }
        }
    }

    public static string ActiveProfilePath
    {
        get
        {
            return GTAConfig.Get<Dictionary<string, string[]>>(GTAConfig.const_dev_profiles).Where(x => x.Key == SystemInfo.deviceUniqueIdentifier).FirstOrDefault().Value[ActiveProfile];
        }
    }

    public static string LastDevProfilePath
    {
        get
        {
            try
            {
                return _obj[GTAConfig.const_dev_profiles].ToObject<Dictionary<string, string[]>>().Last().Value.Last();
            }
            catch
            {
                return "";
            }
        }
    }

    private static JObject DeserializeProfiles()
    {
        string s = "";
        return DeserializeProfiles(out s);
    }

    private static JObject DeserializeProfiles(out string contents)
    {
        string configPath = GTAConfig.FileName;
        contents = File.ReadAllText(configPath);

        return contents.JsonDeserialize<JObject>();
    }

    public static string GetPathFromProfileAt(int index)
    {
        Dictionary<string, string[]> devs = _obj[GTAConfig.const_dev_profiles].ToObject<Dictionary<string, string[]>>();
        return devs.Where(x => x.Key == SystemInfo.deviceUniqueIdentifier).FirstOrDefault().Value[index];
    }

    public static string CheckDevProfiles(Func<string> folderList)
    {
        //game_path = Environment.GetEnvironmentVariable("ProgramFiles"); //...
        string game_path = "",
               contents = "";

        _obj = DeserializeProfiles(out contents);

        var prop = obj[GTAConfig.const_game_dir];
        string game_dir = prop != null ? prop.ToObject<string>() : "";

        bool isSet = true;

        if (prop != null)
            obj.Remove(GTAConfig.const_game_dir);

        var objDev = obj[GTAConfig.const_dev_profiles];

        if (objDev != null)
            game_dir = GetPathFromProfileAt(ActiveProfile);
        else
            isSet = false;

        if (string.IsNullOrEmpty(game_dir))
            game_path = folderList();
        else
            game_path = game_dir;

        if (!isSet)
            AddNewPath(game_path);

        string postContents = obj.JsonSerialize(true);
        if (postContents != contents)
            SaveChanges();

        return game_path;
    }

    public static void AddNewPath(string path, bool setActive = true, string id = "")
    {
        if (string.IsNullOrEmpty(id)) id = SystemInfo.deviceUniqueIdentifier;

        var objDev = _obj[GTAConfig.const_dev_profiles];

        Dictionary<string, string[]> devs = objDev != null ? objDev.ToObject<Dictionary<string, string[]>>() : new Dictionary<string, string[]>();
        if (devs.ContainsKey(id))
        {
            if (!devs[id].Contains(path)) devs[id] = devs[id].Add(path);
        }
        else
            devs.Add(id, new string[] { path });

        _obj[GTAConfig.const_dev_profiles] = JObject.FromObject(devs);

        if (setActive)
            SetDevActiveIndex(ref _obj, devs[id].Length - 1);
    }

    public static void EditPath(int index, string path, bool setActive = true, string id = "")
    {
        if (string.IsNullOrEmpty(id)) id = SystemInfo.deviceUniqueIdentifier;

        var objDev = _obj[GTAConfig.const_dev_profiles];

        if (objDev == null) return;

        Dictionary<string, string[]> devs = objDev.ToObject<Dictionary<string, string[]>>();

        if (!devs.ContainsKey(id)) return;

        try
        {
            devs[id][index] = path;

            _obj[GTAConfig.const_dev_profiles] = JObject.FromObject(devs);

            if (setActive)
                SetDevActiveIndex(ref _obj, devs[id].Length - 1);
        }
        catch
        {
            Debug.LogError("Out of index when trying to edit path from dev profile!");
        }
    }

    public static void SetDevActiveIndex(ref JObject __obj, int index, string id = "")
    {
        if (string.IsNullOrEmpty(id)) id = SystemInfo.deviceUniqueIdentifier;
        var activeDev = __obj[GTAConfig.const_active_dev_profile];

        var dictDev = activeDev != null ? activeDev.ToObject<Dictionary<string, int>>() : new Dictionary<string, int>();
        if (dictDev.ContainsKey(id))
            dictDev[id] = index;
        else
            dictDev.Add(id, index);

        __obj[GTAConfig.const_active_dev_profile] = JObject.FromObject(dictDev);
    }

    public static bool ExistDevIndex(int index)
    {
        var devObj = obj[GTAConfig.const_dev_profiles];

        if (devObj == null) return false;

        var devDict = devObj.ToObject<Dictionary<string, string[]>>();

        if (devDict == null || (devDict != null && devDict.Count == 0)) return false;

        //Debug.LogFormat("Index: {0}; Count: {1}", index, devDict.Count - 1);
        return index <= devDict.Count;
    }

    public static void SaveChanges(JObject __obj = null)
    {
        if (__obj == null) __obj = _obj;

        // Serialize again
        File.WriteAllText(GTAConfig.FileName, __obj.JsonSerialize(true));
    }
}