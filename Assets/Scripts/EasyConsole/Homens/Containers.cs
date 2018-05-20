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
            this.buffer = new T[maxCapacity];
            this.head = 0;
            this.tail = 0;
            this.overwrite = overwrite;
        }

        public void Enqueue(T item)
        {
            if (this.full && !this.overwrite)
            {
                throw new IndexOutOfRangeException("Buffer is full");
            }
            if (this.full && ++this.head >= this.maxCapacity)
            {
                this.head = 0;
            }
            int num = this.tail;
            if (++this.tail >= this.maxCapacity)
            {
                this.tail = 0;
            }
            if (this.tail == this.head)
            {
                this.full = true;
            }
            this.buffer[num] = item;
            this.empty = false;
        }

        public T Dequeue()
        {
            if (this.empty)
            {
                throw new IndexOutOfRangeException("Buffer is empty");
            }
            T result = this.buffer[this.head];
            if (++this.head >= this.maxCapacity)
            {
                this.tail = 0;
            }
            if (this.head == this.tail)
            {
                this.empty = true;
            }
            this.full = false;
            return result;
        }

        public int Count()
        {
            int result;
            if (this.full)
            {
                result = this.maxCapacity;
            }
            else if (this.empty)
            {
                result = 0;
            }
            else if (this.head > this.tail)
            {
                result = this.tail + this.maxCapacity - this.head;
            }
            else
            {
                result = this.tail - this.head;
            }
            return result;
        }

        public T GetItemAt(int index)
        {
            T result;
            if (this.empty)
            {
                result = default(T);
            }
            else
            {
                index = this.head + index;
                if (index >= this.maxCapacity)
                {
                    index -= this.maxCapacity;
                }
                result = this.buffer[index];
            }
            return result;
        }

        public IEnumerator<T> GetEnumerator()
        {
            int i = 0;
            int num = this.head;
            int num2 = this.tail;
            while (i < this.Count())
            {
                if (num != this.head || num2 != this.tail)
                {
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
                }
                yield return this.GetItemAt(i);
                i++;
            }
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            int i = 0;
            int num = this.head;
            int num2 = this.tail;
            while (i < this.Count())
            {
                if (num != this.head || num2 != this.tail)
                {
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
                }
                yield return this.GetItemAt(i);
                i++;
            }
            yield break;
        }
    }
}