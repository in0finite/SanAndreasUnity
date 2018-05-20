using System;
using System.Collections;
using System.Collections.Generic;

namespace Homans.Containers
{
    public class CircularBuffer<T> : IEnumerable<T>, IEnumerable
    {
        private T[] buffer;

        private int head;

        private int tail;

        private int maxCapacity;

        private bool overwrite = false;

        private bool full = false;

        private bool empty = true;

        public CircularBuffer(int maxCapacity, bool overwrite)
        {
            this.maxCapacity = maxCapacity;
            buffer = new T[maxCapacity];
            head = 0;
            tail = 0;
            this.overwrite = overwrite;
        }

        public void Enqueue(T item)
        {
            if (full && !overwrite)
            {
                throw new IndexOutOfRangeException("Buffer is full");
            }
            if (full && ++head >= maxCapacity)
            {
                head = 0;
            }
            int num = tail;
            if (++tail >= maxCapacity)
            {
                tail = 0;
            }
            if (tail == head)
            {
                full = true;
            }
            buffer[num] = item;
            empty = false;
        }

        public T Dequeue()
        {
            if (empty)
            {
                throw new IndexOutOfRangeException("Buffer is empty");
            }
            T result = buffer[head];
            if (++head >= maxCapacity)
            {
                tail = 0;
            }
            if (head == tail)
            {
                empty = true;
            }
            full = false;
            return result;
        }

        public int Count()
        {
            int result;
            if (full)
            {
                result = maxCapacity;
            }
            else if (empty)
            {
                result = 0;
            }
            else if (head > tail)
            {
                result = tail + maxCapacity - head;
            }
            else
            {
                result = tail - head;
            }
            return result;
        }

        public T GetItemAt(int index)
        {
            T result;
            if (empty)
            {
                result = default(T);
            }
            else
            {
                index = head + index;
                if (index >= maxCapacity)
                {
                    index -= maxCapacity;
                }
                result = buffer[index];
            }
            return result;
        }

        public IEnumerator<T> GetEnumerator()
        {
            int i = 0;
            int num = head;
            int num2 = tail;
            while (i < Count())
            {
                if (num != head || num2 != tail)
                {
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
                }
                yield return GetItemAt(i);
                i++;
            }
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            int i = 0;
            int num = head;
            int num2 = tail;
            while (i < Count())
            {
                if (num != head || num2 != tail)
                {
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
                }
                yield return GetItemAt(i);
                i++;
            }
            yield break;
        }
    }
}