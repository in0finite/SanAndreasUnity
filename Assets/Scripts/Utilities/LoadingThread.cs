using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using SanAndreasUnity.Utilities;
using System.Threading;
using System.Collections.Concurrent;
using Debug = UnityEngine.Debug;

namespace SanAndreasUnity.Behaviours
{

	public class LoadingThread : StartupSingleton<LoadingThread> {

		// TODO: maybe convert to class, because it takes 36 bytes - it's too much for red-black tree operations
		public struct Job<T>
		{
			public System.Func<T> action ;
			public System.Action<T> callbackSuccess ;
			public System.Action<System.Exception> callbackError ;
			public System.Action<T> callbackFinish;
			public float priority;
			internal object result ;
			internal System.Exception exception ;
			internal long id;
		}

		public class JobComparer : IComparer<Job<object>>
		{
			public int Compare(Job<object> a, Job<object> b)
			{
				if (a.id == b.id)
					return 0;

				if (a.priority != b.priority)
					return a.priority <= b.priority ? -1 : 1;

				// priorities are the same
				// the advantage has the job which was created earlier

				return a.id <= b.id ? -1 : 1;
			}
		}

		private class ThreadParameters
		{
			public readonly BlockingCollection<Job<object>> jobs =
				new BlockingCollection<Job<object>> (new System.Collections.Concurrent.ConcurrentQueue<Job<object>>());
			public readonly Utilities.ConcurrentQueue<Job<object>> processedJobs = new Utilities.ConcurrentQueue<Job<object>>();
			private bool _shouldThreadExit = false;
			private readonly object _shouldThreadExitLockObject = new object();

			public bool ShouldThreadExit()
			{
				lock (_shouldThreadExitLockObject)
					return _shouldThreadExit;
			}

			public void TellThreadToExit()
			{
				lock (_shouldThreadExitLockObject)
					_shouldThreadExit = true;
			}
		}

		private Thread _thread;
		private readonly ThreadParameters _threadParameters = new ThreadParameters();
		private readonly Queue<Job<object>> _processedJobsBuffer = new Queue<Job<object>>(256);

		private static long s_lastJobId = 0;
		private static readonly object s_lastJobIdLockObject = new object();

		private readonly Stopwatch _stopwatch = new Stopwatch();

		public ushort maxTimePerFrameMs = 0;



#if UNITY_EDITOR
		[UnityEditor.InitializeOnLoadMethod]
		static void InitOnLoadInEditor()
        {
			if (null == Singleton)
				return;

			Singleton.StartThread();
        }
#endif

		protected override void OnSingletonStart()
		{
			this.StartThread();
		}

		protected override void OnSingletonDisable()
		{
			if (_thread != null)
			{
				var sw = System.Diagnostics.Stopwatch.StartNew ();
			//	_thread.Interrupt ();
				_threadParameters.TellThreadToExit ();
				if (_thread.Join (7000))
					Debug.LogFormat ("Stopped loading thread in {0} ms", sw.Elapsed.TotalMilliseconds);
				else
					Debug.LogError ("Failed to stop loading thread");
			}
		}

		void StartThread()
        {
			if (_thread != null)
				return;

			_thread = new Thread(ThreadFunction);
			_thread.Start(_threadParameters);
		}

		void Update()
        {
			this.UpdateJobsInternal();
        }

		public void UpdateJobs()
        {
			ThreadHelper.ThrowIfNotOnMainThread();

			this.UpdateJobsInternal();
        }

		void UpdateJobsInternal () {

			// get all processed jobs

			_stopwatch.Restart();

			Job<object> job;

			while (true)
			{
				if (this.maxTimePerFrameMs != 0 && _stopwatch.ElapsedMilliseconds >= this.maxTimePerFrameMs)
					break;

				if (_processedJobsBuffer.Count > 0)
					job = _processedJobsBuffer.Dequeue();
				else
				{
					int numCopied = _threadParameters.processedJobs.DequeueToQueue(_processedJobsBuffer, 256);
					if (numCopied == 0)
						break;

					job = _processedJobsBuffer.Dequeue();
				}

				if (job.exception != null)
				{
					// error happened

					if (job.callbackError != null)
						Utilities.F.RunExceptionSafe( () => job.callbackError (job.exception) );

					Debug.LogException (job.exception);
				}
				else
				{
					// success
					if (job.callbackSuccess != null)
						Utilities.F.RunExceptionSafe( () => job.callbackSuccess (job.result) );
				}

				// invoke finish callback
				if (job.callbackFinish != null)
					F.RunExceptionSafe (() => job.callbackFinish (job.result));
			}

		}


		public static void RegisterJob<T> (Job<T> job)
		{
			// note: this function can be called from any thread

			if (null == job.action)
				throw new ArgumentException("Job must have an action");

			if (0f == job.priority)
				throw new ArgumentException("You forgot to assign job priority");

			job.exception = null;

			var j = new Job<object> () {
				id = GetNextJobId(),
				priority = job.priority,
				action = () => job.action(),
				callbackError = job.callbackError,
			};
			if(job.callbackSuccess != null)
				j.callbackSuccess = (arg) => job.callbackSuccess( (T) arg );
			if(job.callbackFinish != null)
				j.callbackFinish = (arg) => job.callbackFinish( (T) arg );

			Singleton._threadParameters.jobs.Add (j);
		}

		static long GetNextJobId()
		{
			lock (s_lastJobIdLockObject)
			{
				return ++s_lastJobId;
			}
		}

		public long GetNumJobsPendingApproximately()
        {
			ThreadHelper.ThrowIfNotOnMainThread();

			// this is not done in a critical section: calling Count on 2 multithreaded collections
			return (long)_threadParameters.jobs.Count + (long)_threadParameters.processedJobs.Count + (long)_processedJobsBuffer.Count;
        }

		static void ThreadFunction (object objectParameter)
		{
			ThreadParameters threadParameters = (ThreadParameters) objectParameter;

			while (!threadParameters.ShouldThreadExit())
			{
				Job<object> job;
				if (!threadParameters.jobs.TryTake (out job, 200))
					continue;

				try
				{
					job.result = job.action();
				}
				catch(System.Exception ex)
				{
					job.exception = ex;
				}

				threadParameters.processedJobs.Enqueue(job);
			}

		}

	}

}
