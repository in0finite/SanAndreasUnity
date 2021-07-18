using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SanAndreasUnity.Utilities
{
    public class ConcurrentProducerConsumerSortedSet<T> : IProducerConsumerCollection<T>
    {
        private SortedSet<T> _sortedSet;
        private object _lockObject = new object();


        public ConcurrentProducerConsumerSortedSet(IComparer<T> comparer)
        {
            _sortedSet = new SortedSet<T>(comparer);
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get
            {
                lock (_lockObject)
                {
                    return _sortedSet.Count;
                }
            }
        }

        public bool IsSynchronized => throw new NotImplementedException();

        public object SyncRoot => throw new NotImplementedException();

        public void CopyTo(T[] array, int index)
        {
            throw new NotImplementedException();
        }

        public T[] ToArray()
        {
            throw new NotImplementedException();
        }

        public bool TryAdd(T item)
        {
            lock (_lockObject)
            {
                if (!_sortedSet.Add(item))
                    throw new ArgumentException($"Item with this key already exists: {item}");
                return true;
            }
        }

        public bool TryTake(out T result)
        {
            lock (_lockObject)
            {
                if (_sortedSet.Count > 0)
                {
                    T min = _sortedSet.Min;
                    if (!_sortedSet.Remove(min))
                        throw new Exception("Failed to remove min element");
                    result = min;
                    return true;
                }

                result = default;
                return false;
            }
        }
    }
}
