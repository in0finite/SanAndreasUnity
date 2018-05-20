using Homans.Console;
using System;

/**
 * Author: Sander Homan
 * Copyright 2012
 **/

using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

internal class ConsoleWatch : MonoBehaviour
{
    private class Watch
    {
        public string name;
        public FieldInfo field;
        public PropertyInfo property;
        public WeakReference instance;
        public string lastValue;
    }

    private List<Watch> watches = new List<Watch>();

    private void Start()
    {
        Console.Instance.RegisterCommand("AddWatch", this, "AddWatchCommand");

        InvokeRepeating("UpdateWatches", 1, 1);
    }

    private void UpdateWatches()
    {
        watches.RemoveAll(m => m.instance.Target == null); // remove all dead objects

        foreach (var watch in watches)
        {
            // update value
            if (watch.field != null)
                watch.lastValue = watch.field.GetValue(watch.instance.Target).ToString();
            else if (watch.property != null)
                watch.lastValue = watch.property.GetValue(watch.instance.Target, null).ToString();
        }
    }

    private void OnGUI()
    {
        foreach (var watch in watches)
        {
            GUILayout.Label(watch.name + ": " + watch.lastValue);
        }
    }

    public void AddWatchField(string name, string fieldName, object instance)
    {
        Watch w = new Watch();
        w.name = name;
        w.instance = new WeakReference(instance, false);
        w.field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (instance == null || w.field == null)
            return;

        watches.Add(w);
    }

    public void AddWatchProperty(string name, string fieldName, object instance)
    {
        Watch w = new Watch();
        w.name = name;
        w.instance = new WeakReference(instance, false);
        w.property = instance.GetType().GetProperty(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (instance == null || w.property == null)
            return;

        watches.Add(w);
    }

    [Help("Usage: \"AddWatch name object.component.field\"\nDisplays the given field or property on the screen. Will automaticly update.")]
    private void AddWatchCommand(string name, string goPath)
    {
        string[] path;
        string componentName;
        string fieldName;
        Console.parseGameObjectString(goPath, out path, out componentName, out fieldName);

        // build actual path
        string actualPath = "";
        foreach (string p in path)
        {
            actualPath += "/" + p;
        }

        GameObject go = GameObject.Find(actualPath);
        if (go == null)
        {
            Console.Instance.Print("Unknown gameobject");
            return;
        }

        Component comp = go.GetComponent(componentName);
        if (comp == null)
        {
            Console.Instance.Print("Unknown component");
            return;
        }

        var field = comp.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field == null)
        {
            // check for property
            var property = comp.GetType().GetProperty(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property == null)
            {
                Console.Instance.Print("Unknown field or property");
                return;
            }
            else
                AddWatchProperty(name, fieldName, comp);
        }
        else
            AddWatchField(name, fieldName, comp);
    }
}