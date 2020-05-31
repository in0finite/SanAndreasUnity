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

		[MethodImpl(MethodImplOptions.Synchronized)]
		public int GetNumObjectsLoaded ()
		{
			return m_Loaded.Count;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public int GetNumObjectsLoading ()
		{
			return m_Loading.Count;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public bool IsObjectLoaded (TKey key)
		{
			return m_Loaded.ContainsKey (key);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public TObj GetLoadedObject (TKey key)
		{
			return m_Loaded [key];
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public bool CheckIsObjectLoaded (TKey key, System.Action<TObj> onFinish)
		{
			if (m_Loaded.ContainsKey(key))
			{
				onFinish (m_Loaded [key]);
				return true;
			}
			return false;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public bool CheckIsObjectLoading (TKey key, System.Action<TObj> onFinish)
		{
			if (m_Loading.ContainsKey (key))
			{
				// this object is loading
				// subscribe to finish event
				m_Loading[key].Add( onFinish );
				return true;
			}
			return false;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public bool TryLoadObject (TKey key, System.Action<TObj> onFinish)
		{
			if (CheckIsObjectLoaded (key, onFinish))
				return false;

			if (CheckIsObjectLoading (key, onFinish))
				return false;

			// insert it into loading dict
			m_Loading [key] = new List<System.Action<TObj>>(){onFinish};

			return true;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void OnObjectFinishedLoading (TKey key, TObj obj, bool bSuccess)
		{

			if (bSuccess)
			{
				if (m_Loaded.ContainsKey (key))
				{
					// this object was loaded in the meantime
					// this can happen if someone else is loading objects synchronously
					Debug.LogErrorFormat ("Redundant load of object ({0}): {1}", typeof(TObj), key);
				}
				else
				{
					m_Loaded.Add (key, obj);
				}
			}


			var list = m_Loading[key];

			// remove from loading dict
			m_Loading.Remove( key );

			// invoke subscribers
			foreach(var item in list)
				Utilities.F.RunExceptionSafe( () => item(obj));
			
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void AddToLoadedObjects (TKey key, TObj obj)
		{
			m_Loaded [key] = obj;
		}

	}

}
