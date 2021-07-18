using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;

namespace SanAndreasUnity.Utilities
{
	
	public class AsyncLoader<TKey, TObj>
	{
		/// <summary>
		/// All successfully loaded objects.
		/// </summary>
		private readonly Dictionary<TKey, TObj> m_Loaded = new Dictionary<TKey, TObj>();

		/// <summary>
		/// Objects currently being loaded. Value represents list of subscribers which will be called when loading is finished.
		/// </summary>
		private readonly Dictionary<TKey, List<System.Action<TObj>>> m_Loading = new Dictionary<TKey, List<System.Action<TObj>>> ();


		public AsyncLoader ()
		{
		}

		public AsyncLoader (IEqualityComparer<TKey> comparer)
		{
			m_Loaded = new Dictionary<TKey, TObj> (comparer);
			m_Loading = new Dictionary<TKey, List<System.Action<TObj>>> (comparer);
		}

		public int GetNumObjectsLoaded ()
		{
			return m_Loaded.Count;
		}

		public int GetNumObjectsLoading ()
		{
			return m_Loading.Count;
		}

		public bool IsObjectLoaded (TKey key)
		{
			return m_Loaded.ContainsKey (key);
		}

		public TObj GetLoadedObject (TKey key)
		{
			return m_Loaded [key];
		}

		public bool TryLoadObject (TKey key, System.Action<TObj> onFinish)
		{
			ThreadHelper.ThrowIfNotOnMainThread(); // not needed, but to make sure

			if (m_Loaded.ContainsKey(key))
			{
				onFinish(m_Loaded[key]);
				return false;
			}

			if (m_Loading.ContainsKey(key))
			{
				// this object is loading
				// subscribe to finish event
				m_Loading[key].Add(onFinish);
				return false;
			}

			// insert it into loading dict
			m_Loading[key] = new List<System.Action<TObj>>() {onFinish};

			return true;
		}

		public bool TryGetLoadedObject(TKey key, out TObj loadedObject)
		{
			ThreadHelper.ThrowIfNotOnMainThread(); // not needed, but to make sure

			return m_Loaded.TryGetValue(key, out loadedObject);
		}

		public void OnObjectFinishedLoading (TKey key, TObj obj, bool bSuccess)
		{
			ThreadHelper.ThrowIfNotOnMainThread(); // not needed, but to make sure

			if (bSuccess)
			{
				if (m_Loaded.ContainsKey(key))
				{
					// this object was loaded in the meantime
					// this can happen if someone else is loading objects synchronously
					Debug.LogErrorFormat("Redundant load of object ({0}): {1}", typeof(TObj), key);
				}
				else
				{
					m_Loaded.Add(key, obj);
				}
			}

			if (m_Loading.TryGetValue(key, out var subscribersList))
			{
				// remove from loading dict
				m_Loading.Remove(key);

				// invoke subscribers
				foreach (var action in subscribersList)
					Utilities.F.RunExceptionSafe(() => action(obj));
			}
		}

	}

}
