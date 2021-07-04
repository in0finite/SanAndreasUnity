using System.Collections.Generic;

namespace SanAndreasUnity.Utilities
{

	/// <summary>
	/// Alternative to System.Collections.Concurrent.ConcurrentQueue
	/// (It's only available in .NET 4.0 and greater)
	/// </summary>
	/// <remarks>
	/// It's a bit slow (as it uses locks), and only provides a small subset of the interface
	/// Overall, the implementation is intended to be simple & robust
	/// </remarks>
	public class ConcurrentQueue<T>
	{
		private readonly System.Object queueLock = new System.Object();
		private readonly Queue<T> queue = new Queue<T>();

		public void Enqueue(T item)
		{
			lock (queueLock)
			{
				queue.Enqueue(item);
			}
		}

		public bool TryDequeue(out T result)
		{
			lock (queueLock)
			{
				if (queue.Count == 0)
				{
					result = default(T);
					return false;
				}

				result = queue.Dequeue();
				return true;
			}
		}

		public T[] DequeueAll()
		{
			lock (queueLock)
			{
				T[] copy = queue.ToArray();
				queue.Clear();
				return copy;
			}
		}

		public int DequeueToQueue(Queue<T> collection, int maxNumItems)
		{
			lock (queueLock)
			{
				int numAdded = 0;
				while (queue.Count > 0 && numAdded < maxNumItems)
				{
					collection.Enqueue(queue.Dequeue());
					numAdded++;
				}
				return numAdded;
			}
		}

		public int Count
		{
			get
			{
				lock (queueLock)
				{
					return queue.Count;
				}
			}
		}

	}

}
