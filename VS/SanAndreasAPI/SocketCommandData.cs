using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SanAndreasAPI
{
    [Serializable]
    public class SocketCommandData : IDictionary<string, object>
    {
        private Dictionary<string, object> _dict = new Dictionary<string, object>();
        public object otherData;

        public object this[string key]
        {
            get
            {
                if (_dict.ContainsKey(key))
                    return _dict[key];
                return null;
            }

            set
            {
                if (!_dict.ContainsKey(key))
                    _dict.Add(key, value);
                else
                    _dict[key] = value;
            }
        }

        private SocketCommandData()
        { }

        public SocketCommandData(Dictionary<string, object> values)
        {
            _dict = values;
        }

        public SocketCommandData(object obj, Dictionary<string, object> values)
        {
            otherData = obj;
            _dict = values;
        }

        public ICollection<string> Keys => _dict.Keys;

        public ICollection<object> Values => throw new NotImplementedException();

        public int Count => throw new NotImplementedException();

        public bool IsReadOnly => throw new NotImplementedException();

        public void Add(string key, object value)
        {
            _dict.Add(key, value);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            //SocketCommandData data = new SocketCommandData(new Dictionary<string, object> { { "", null } });
            _dict.Add(item);
        }

        public void Clear()
        {
            _dict.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return _dict.Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return _dict.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _dict.GetEnumerator();
        }

        public bool Remove(string key)
        {
            return _dict.Remove(key);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string key, out object value)
        {
            return _dict.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dict.GetEnumerator();
        }
    }
}