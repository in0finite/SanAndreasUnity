using Fclp.Internals.Extensions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

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

        Func<int, Action> editAction = (i) =>
        {
            return () => EditProfile(i);
        };

        for (int i = 0; i < 4; ++i)
        {
            CheckmarkMenuItem menu = new CheckmarkMenuItem(string.Format("SanAndreasUnity/Devs/Switch to Profile {0} #{0}", i), i == DevProfiles.ActiveProfile);
            menu.actionDict.Add("menu", menuAction(i));
            menu.actionDict.Add("edit", editAction(i));

            menus.Add(i, menu);
        }
    }

    [MenuItem("SanAndreasUnity/Devs/Add Development Workstation", false, -100)]
    private static void AddWorkstation()
    {
        DevProfiles.AddNewPath(EditorUtility.OpenFolderPanel("Select GTA instalation Path", Path.GetDirectoryName(DevProfiles.LastDevProfilePath), ""));
        DevProfiles.SaveChanges();
    }

    private static void SwitchTo(int index)
    {
        if (!DevProfiles.ExistDevIndex(index))
        {
            Debug.LogWarningFormat("Profile {0} is empty!", index);
            menus[index].PerformAction(false, false);
            return;
        }

        // Disable everything and then re-enable
        menus.ForEach(x => x.Value.PerformAction(false, false));
        menus[index].PerformAction(true, false);

        JObject obj = DevProfiles.obj;
        DevProfiles.SetDevActiveIndex(ref obj, index);
        DevProfiles.SaveChanges(obj);
    }

    private static void EditProfile(int index)
    {
        if (!DevProfiles.ExistDevIndex(index))
        {
            Debug.LogWarningFormat("Profile {0} is empty!", index);
            return;
        }

        string path = EditorUtility.OpenFolderPanel("Select GTA instalation Path", DevProfiles.GetPathFromProfileAt(index), "");
        DevProfiles.EditPath(index, path);
        DevProfiles.SaveChanges();
    }

    [MenuItem("SanAndreasUnity/Devs/Switch to Profile 0 #0", false, 5)]
    private static void Profile0()
    {
        //Debug.Log("0000");
        menus[0].ToggleAction();
    }

    [MenuItem("SanAndreasUnity/Devs/Switch to Profile 1 #1", false, 15)]
    private static void Profile1()
    {
        //Debug.Log("1111");
        menus[1].ToggleAction();
    }

    [MenuItem("SanAndreasUnity/Devs/Switch to Profile 2 #2", false, 25)]
    private static void Profile2()
    {
        //Debug.Log("2222");
        menus[2].ToggleAction();
    }

    [MenuItem("SanAndreasUnity/Devs/Switch to Profile 3 #3", false, 35)]
    private static void Profile3()
    {
        //Debug.Log("3333");
        menus[3].ToggleAction();
    }

    [MenuItem("SanAndreasUnity/Devs/Edit to Profile 0 #0", false, 10)]
    private static void Profile0Edit()
    {
        menus[0].EditAction();
    }

    [MenuItem("SanAndreasUnity/Devs/Edit to Profile 1 #1", false, 20)]
    private static void Profile1Edit()
    {
        menus[1].EditAction();
    }

    [MenuItem("SanAndreasUnity/Devs/Edit to Profile 2 #2", false, 30)]
    private static void Profile2Edit()
    {
        menus[2].EditAction();
    }

    [MenuItem("SanAndreasUnity/Devs/Edit to Profile 3 #3", false, 40)]
    private static void Profile3Edit()
    {
        menus[3].EditAction();
    }
}