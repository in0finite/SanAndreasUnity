//Based of the following thread https://forum.unity.com/threads/finally-a-serializable-dictionary-for-unity-extracted-from-system-collections-generic.335797/

using System.Collections.Generic;

namespace RotaryHeart.Lib.SerializableDictionary
{
    /// <summary>
    /// This class is only used to be able to draw the custom property drawer
    /// </summary>
    public abstract class DrawableDictionary
    {
        public ReorderableList reorderableList = null;
        public RequiredReferences reqReferences;
    }

    /// <summary>
    /// Base class that most be used for any dictionary that wants to be implemented
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TValue">Value type</typeparam>
    [System.Serializable]
    public class SerializableDictionaryBase<TKey, TValue> : DrawableDictionary, IDictionary<TKey, TValue>, UnityEngine.ISerializationCallbackReceiver
    {
        private Dictionary<TKey, TValue> _dict;
        private readonly static Dictionary<TKey, TValue> _staticEmptyDict = new Dictionary<TKey, TValue>(0);

        /// <summary>
        /// Copies the data from a dictionary. If an entry with the same key is found it replaced the value
        /// </summary>
        /// <param name="src">DIctionary to copy the data from</param>
        public void CopyFrom(IDictionary<TKey, TValue> src)
        {
            foreach (var data in src)
            {
                if (ContainsKey(data.Key))
                {
                    this[data.Key] = data.Value;
                }
                else
                {
                    Add(data.Key, data.Value);
                }
            }
        }

        #region IDictionary Interface

        public int Count
        {
            get
            {
                return (_dict != null) ? _dict.Count : 0;
            }
        }

        public void Add(TKey key, TValue value)
        {
            if (_dict == null)
                _dict = new Dictionary<TKey, TValue>();
            if (_keyValues == null)
                _keyValues = new List<TKey>();

            _keyValues.Add(key);

            _dict.Add(key, value);
        }

        public void Clear()
        {
            if (_dict != null)
                _dict.Clear();
        }

        public bool ContainsKey(TKey key)
        {
            if (_dict == null)
                return false;

            return _dict.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            if (_dict == null)
                return false;

            return _dict.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (_dict == null)
            {
                value = default(TValue);
                return false;
            }

            return _dict.TryGetValue(key, out value);
        }

        public ICollection<TKey> Keys
        {
            get
            {
                if (_dict == null)
                    _dict = new Dictionary<TKey, TValue>();

                return _dict.Keys;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                if (_dict == null)
                    _dict = new Dictionary<TKey, TValue>();

                return _dict.Values;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                if (_dict == null) throw new KeyNotFoundException();
                return _dict[key];
            }
            set
            {
                if (_dict == null) _dict = new Dictionary<TKey, TValue>();
                _dict[key] = value;
            }
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            if (_dict == null) _dict = new Dictionary<TKey, TValue>();
            (_dict as ICollection<KeyValuePair<TKey, TValue>>).Add(item);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            if (_dict == null) return false;
            return (_dict as ICollection<KeyValuePair<TKey, TValue>>).Contains(item);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (_dict == null) return;
            (_dict as ICollection<KeyValuePair<TKey, TValue>>).CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            if (_dict == null) return false;
            return (_dict as ICollection<KeyValuePair<TKey, TValue>>).Remove(item);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get { return false; }
        }

        public Dictionary<TKey, TValue>.Enumerator GetEnumerator()
        {
            if (_dict == null) return _staticEmptyDict.GetEnumerator();
            return _dict.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region ISerializationCallbackReceiver

        [UnityEngine.SerializeField]
        private List<TKey> _keyValues;

        [UnityEngine.SerializeField]
        private TKey[] _keys;
        [UnityEngine.SerializeField]
        private TValue[] _values;

        void UnityEngine.ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (_keys != null && _values != null)
            {
                //Need to clear the dictionary
                if (_dict == null)
                    _dict = new Dictionary<TKey, TValue>(_keys.Length);
                else
                    _dict.Clear();

                for (int i = 0; i < _keys.Length; i++)
                {
                    //This should only happen with reference type keys (Generic, Object, etc)
                    if (_keys[i] == null)
                    {
                        //Special case for UnityEngine.Object classes
                        if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(TKey)))
                        {
                            //Key type
                            string tKeyType = typeof(TKey).ToString();

                            //We need the reference to the reference holder class
                            if (reqReferences == null)
                            {
                                UnityEngine.Debug.LogError("A key of type: " + tKeyType + " requires to have a valid RequiredReferences reference");
                                continue;
                            }

                            //Use reflection to check all the fields included on the class
                            foreach (var field in typeof(RequiredReferences).GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic))
                            {
                                //Only set the value if the type is the same
                                if (field.FieldType.ToString().Equals(tKeyType))
                                {
                                    _keys[i] = (TKey)(field.GetValue(reqReferences));
                                    break;
                                }
                            }

                            //References class is missing the field, skip the element
                            if (_keys[i] == null)
                            {
                                UnityEngine.Debug.LogError("Couldn't find " + tKeyType + " reference.");
                                continue;
                            }
                        }
                        else
                        {
                            //Create a instance for the key
                            _keys[i] = System.Activator.CreateInstance<TKey>();
                        }
                    }

                    //Add the data to the dictionary. Value can be null so no special step is required
                    if (i < _values.Length)
                        _dict[_keys[i]] = _values[i];
                    else
                        _dict[_keys[i]] = default(TValue);
                }
            }

            _keys = null;
            _values = null;
        }

        void UnityEngine.ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (_dict == null || _dict.Count == 0)
            {
                //Dictionary is empty, erase data
                _keys = null;
                _values = null;
            }
            else
            {
                //Initialize arrays
                int cnt = _dict.Count;
                _keys = new TKey[cnt];
                _values = new TValue[cnt];

                int i = 0;
                var e = _dict.GetEnumerator();
                while (e.MoveNext())
                {
                    //Set the respective data from the dictionary
                    _keys[i] = e.Current.Key;
                    _values[i] = e.Current.Value;
                    i++;
                }
            }
        }

        #endregion

    }
}
