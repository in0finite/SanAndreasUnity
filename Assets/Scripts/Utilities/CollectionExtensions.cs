using System.Collections.Generic;

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
    }
}
