using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

[InitializeOnLoad]
public static class DevMenu
{
    private static Dictionary<int, CheckmarkMenuItem> menus = new Dictionary<int, CheckmarkMenuItem>();

    static DevMenu()
    {
        Func<int, Action> menuAction = (i) =>
        {
            return () => SwitchTo(i);
        };

        for (int i = 0; i < 4; ++i)
        {
            CheckmarkMenuItem menu = new CheckmarkMenuItem(string.Format("SanAndreasUnity/Devs/Switch to Profile {0} #{0}", i), i == DevProfiles.ActiveProfile);
            menu.menuAction = menuAction(i);

            menus.Add(i, menu);
        }
    }

    [MenuItem("SanAndreasUnity/Devs/Add Development Workstation", false, 0)]
    private static void AddWorkstation()
    {
        DevProfiles.AddNewPath(EditorUtility.OpenFolderPanel("Select GTA instalation Path", Path.GetDirectoryName(DevProfiles.LastDevProfilePath), ""));
        DevProfiles.SaveChanges();
    }

    private static void SwitchTo(int index)
    {
        if (!DevProfiles.ExistDevIndex(index)) return;
        JObject obj = DevProfiles.obj;
        DevProfiles.SetDevActiveIndex(ref obj, index);
        DevProfiles.SaveChanges(obj);
    }

    [MenuItem("SanAndreasUnity/Devs/Switch to Profile 0 #0")]
    private static void Profile0()
    {
        menus[0].ToggleAction();
    }

    [MenuItem("SanAndreasUnity/Devs/Switch to Profile 1 #1")]
    private static void Profile1()
    {
        menus[1].ToggleAction();
    }

    [MenuItem("SanAndreasUnity/Devs/Switch to Profile 2 #2")]
    private static void Profile2()
    {
        menus[2].ToggleAction();
    }

    [MenuItem("SanAndreasUnity/Devs/Switch to Profile 3 #3")]
    private static void Profile3()
    {
        menus[3].ToggleAction();
    }
}