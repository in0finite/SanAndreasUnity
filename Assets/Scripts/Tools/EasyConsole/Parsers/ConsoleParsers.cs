/**
 * Author: Sander Homan
 * Copyright 2012
 **/

using System.Collections;
using UnityEngine;

internal class ConsoleParsers : MonoBehaviour
{
    private void OnEnable()
    {
        StartCoroutine(InitParsers());
    }

    private IEnumerator InitParsers()
    {
        yield return null; // make sure we wait 1 frame because the console also initializes itself during onEnable

        Console.Instance.RegisterParser(typeof(Vector3), parseVector3);
    }

    private bool parseVector3(string v, out object obj)
    {
        Vector3 result = new Vector3();
        var comp = v.Split(',');
        float t;
        if (!float.TryParse(comp[0], out t))
        {
            Console.Instance.Print("Invalid Vector3: " + comp[0] + " is not a float");
            obj = null;
            return false;
        }
        result.x = t;
        if (!float.TryParse(comp[1], out t))
        {
            Console.Instance.Print("Invalid Vector3: " + comp[1] + " is not a float");
            obj = null;
            return false;
        }
        result.y = t;
        if (!float.TryParse(comp[2], out t))
        {
            Console.Instance.Print("Invalid Vector3: " + comp[2] + " is not a float");
            obj = null;
            return false;
        }
        result.z = t;
        obj = result;
        return true;
    }

    private void vector3ParseTest(Vector3 vector)
    {
        Console.Instance.Print(vector.x.ToString());
        Console.Instance.Print(vector.y.ToString());
        Console.Instance.Print(vector.z.ToString());
    }
}