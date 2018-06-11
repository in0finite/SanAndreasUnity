using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// WIP: This in the future will be a custom settings manager
public static class DLLManager
{
    private static Dictionary<string, object> _storedInfo;

    private static string _storePath;

    public static string storePath
    {
        get
        {
            return Path.Combine(Application.streamingAssetsPath, "DLLSettings.txt");
        }
    }

    /// <summary>
    /// Gets the bool.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
    public static bool GetBool(string key)
    {
        return ExistsKey(key) && ((bool)storedInfo[key]);
    }

    /// <summary>
    /// Sets the bool.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">if set to <c>true</c> [value].</param>
    public static void SetBool(string key, bool value)
    {
        if (!ExistsKey(key))
            storedInfo.Add(key, value);
        else
            storedInfo[key] = value;
        storedInfo.JsonSerializeToFile(storePath, true);
    }

    /// <summary>
    /// Gets the string.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>System.String.</returns>
    public static string GetString(string key)
    {
        if (ExistsKey(key))
            return (string)storedInfo[key];
        else
            return "";
    }

    /// <summary>
    /// Sets the string.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    public static void SetString(string key, string value)
    {
        if (!ExistsKey(key))
            storedInfo.Add(key, value);
        else
            storedInfo[key] = value;
        storedInfo.JsonSerializeToFile(storePath, true);
    }

    /// <summary>
    /// Gets the int.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>System.Int32.</returns>
    public static int GetInt(string key)
    {
        if (ExistsKey(key))
            return (int)storedInfo[key];
        else
            return 0;
    }

    /// <summary>
    /// Sets the int.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    public static void SetInt(string key, int value)
    {
        if (!ExistsKey(key))
            storedInfo.Add(key, value);
        else
            storedInfo[key] = value;
        storedInfo.JsonSerializeToFile(storePath, true);
    }

    /// <summary>
    /// Gets the long.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>System.Int64.</returns>
    public static long GetLong(string key)
    {
        if (ExistsKey(key))
            return (long)storedInfo[key];
        else
            return 0;
    }

    /// <summary>
    /// Sets the long.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    public static void SetLong(string key, long value)
    {
        if (!ExistsKey(key))
            storedInfo.Add(key, value);
        else
            storedInfo[key] = value;
        storedInfo.JsonSerializeToFile(storePath, true);
    }

    /// <summary>
    /// Existses the key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
    public static bool ExistsKey(string key)
    {
        return storedInfo.ContainsKey(key);
    }

    private static Dictionary<string, object> storedInfo
    {
        get
        {
            if (_storedInfo == null)
            {
                if (File.Exists(storePath))
                    _storedInfo = storePath.JsonDeserializeFromFile<JObject>().ToObject<Dictionary<string, object>>();
                else
                    _storedInfo = new Dictionary<string, object>();
            }

            return _storedInfo;
        }
        set
        {
            _storedInfo = value;
        }
    }
}