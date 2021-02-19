using System;
using System.Collections.Generic;
using System.Globalization;
using Mirror;
using SanAndreasUnity.Utilities;
using UnityEngine;

namespace SanAndreasUnity.Net
{
    public class SyncedBag
    {
        public class StringSyncDictionary : SyncDictionary<string, string>
        {
        }

        private readonly StringSyncDictionary m_syncDictionary;
        private readonly Dictionary<string, List<System.Action<string>>> m_callbacks = new Dictionary<string, List<Action<string>>>();


        public SyncedBag(StringSyncDictionary syncDictionary)
        {
            m_syncDictionary = syncDictionary;

            m_syncDictionary.Callback += DictionaryCallback;
        }

        private void DictionaryCallback(StringSyncDictionary.Operation op, string key, string item)
        {
            if (NetUtils.IsServer)
                return;

            switch (op)
            {
                case StringSyncDictionary.Operation.OP_ADD:
                case StringSyncDictionary.Operation.OP_SET:
                case StringSyncDictionary.Operation.OP_DIRTY:

                    if (m_callbacks.TryGetValue(key, out var list))
                    {
                        // don't leave garbage
                        for (int i = 0; i < list.Count; i++)
                        {
                            var callback = list[i];
                            F.RunExceptionSafe(() => callback(item));
                        }
                    }

                    break;
            }
        }

        public void RegisterCallback(string key, System.Action<string> callback)
        {
            if (m_callbacks.TryGetValue(key, out var list))
            {
                list.Add(callback);
            }
            else
            {
                m_callbacks.Add(key, new List<Action<string>>{callback});
            }
        }

        public void UnRegisterCallback(string key, System.Action<string> callback)
        {
            if (m_callbacks.TryGetValue(key, out var list))
            {
                list.Remove(callback);
            }
        }

        public void SetString(string key, string value)
        {
            if (m_syncDictionary.TryGetValue(key, out string existingValue))
            {
                if (value != existingValue)
                {
                    m_syncDictionary[key] = value;
                }
            }

            m_syncDictionary[key] = value;
        }

        public string GetString(string key)
        {
            return m_syncDictionary.TryGetValue(key, out string value) ? value : null;
        }

        public bool HasValue(string key)
        {
            return m_syncDictionary.ContainsKey(key);
        }

        public int GetInt(string key)
        {
            string str = GetString(key);
            if (str == null)
                return 0;
            return int.Parse(str, CultureInfo.InvariantCulture);
        }

        public void SetInt(string key, int value)
        {
            SetString(key, value.ToString(CultureInfo.InvariantCulture));
        }

        public float GetFloat(string key)
        {
            string str = GetString(key);
            if (str == null)
                return 0;
            return float.Parse(str, CultureInfo.InvariantCulture);
        }

        public void SetFloat(string key, float value)
        {
            SetString(key, value.ToString(CultureInfo.InvariantCulture));
        }

        public string[] GetStringArray(string key)
        {
            string str = GetString(key);
            if (str == null)
                return null;
            return JsonUtility.FromJson<string[]>(str);
        }

        public void SetStringArray(string key, string[] array)
        {
            SetString(key, JsonUtility.ToJson(array));
        }
    }
}
