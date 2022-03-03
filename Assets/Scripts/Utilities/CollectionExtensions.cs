using System.Collections.Generic;
using System.Linq;

namespace SanAndreasUnity.Utilities
{
    public static class CollectionExtensions
    {
        public static T RemoveLast<T>(this IList<T> list)
        {
            T lastElement = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
            return lastElement;
        }

        public static T RemoveFirst<T>(this IList<T> list)
        {
            T firstElement = list[0];
            list.RemoveAt(0);
            return firstElement;
        }

        public static Queue<T> ToQueue<T>(this IEnumerable<T> enumerable)
        {
            return new Queue<T>(enumerable);
        }

        public static Queue<T> ToQueueWithCapacity<T>(this IEnumerable<T> enumerable, int capacity)
        {
            var queue = new Queue<T>(capacity);
            foreach (var item in enumerable)
                queue.Enqueue(item);
            return queue;
        }

        public static T[] ToArrayOfLength<T>(this IEnumerable<T> enumerable, int length)
        {
            T[] array = new T[length];
            int i = 0;
            foreach (var item in enumerable)
            {
                array[i] = item;
                i++;
            }
            return array;
        }

        public static IEnumerable<T> AppendIf<T>(this IEnumerable<T> enumerable, bool condition, T element)
        {
            return condition ? enumerable.Append(element) : enumerable;
        }

        public static void AddMultiple<T>(this ICollection<T> collection, T value, int count)
        {
            for (int i = 0; i < count; i++)
                collection.Add(value);
        }

        public static void AddMultiple<T>(this ICollection<T> collection, int count)
        {
            collection.AddMultiple(default, count);
        }
    }
}
