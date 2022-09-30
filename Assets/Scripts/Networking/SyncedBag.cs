using System;
using System.Collections.Generic;
using System.Globalization;
using Mirror;
using UGameCore.Utilities;
using UnityEngine;

namespace SanAndreasUnity.Net
{
    public class SyncedBag
    {
        private readonly SyncDictionary<string, string> m_syncDictionary;

        private readonly Dictionary<string, List<System.Action<string>>> m_callbacks =
            new Dictionary<string, List<Action<string>>>();

        private struct ArrayWrapper<T> // use struct so that it doesn't allocate memory
        {
            public T[] a; // use short name because it will be a part of json

            public ArrayWrapper(T[] a)
            {
                this.a = a;
            }
        }


        public SyncedBag(SyncDictionary<string, string> syncDictionary)
        {
            m_syncDictionary = syncDictionary;

            m_syncDictionary.Callback += DictionaryCallback;
        }

        private void DictionaryCallback(SyncDictionary<string, string>.Operation op, string key, string item)
        {
            if (NetUtils.IsServer)
                return;

            switch (op)
            {
                case SyncDictionary<string, string>.Operation.OP_ADD:
                case SyncDictionary<string, string>.Operation.OP_SET:
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
                m_callbacks.Add(key, new List<Action<string>> { callback });
            }
        }

        public void UnRegisterCallback(string key, System.Action<string> callback)
        {
            if (m_callbacks.TryGetValue(key, out var list))
            {
                list.Remove(callback);
            }
        }

        public void SetCallbacks(SyncedBag other)
        {
            if (this == other)
                return;

            m_callbacks.Clear();
            foreach (var callback in other.m_callbacks)
            {
                m_callbacks.Add(callback.Key, callback.Value);
            }
        }

        public void SetData(SyncedBag other)
        {
            if (this == other)
                return;

            m_syncDictionary.Clear();
            foreach (var pair in other.m_syncDictionary)
            {
                m_syncDictionary.Add(pair.Key, pair.Value);
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
            return JsonUtility.FromJson<ArrayWrapper<string>>(str).a;
        }

        public void SetStringArray(string key, string[] array)
        {
            SetString(key, JsonUtility.ToJson(new ArrayWrapper<string>(array)));
        }

        public float[] GetFloatArray(string key)
        {
            string str = GetString(key);
            if (str == null)
                return null;
            return JsonUtility.FromJson<ArrayWrapper<float>>(str).a;
        }

        public void SetFloatArray(string key, float[] array)
        {
            SetString(key, JsonUtility.ToJson(new ArrayWrapper<float>(array)));
        }
    }
}