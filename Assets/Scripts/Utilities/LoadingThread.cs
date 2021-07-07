using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using SanAndreasUnity.Utilities;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Collections.Concurrent;
using Debug = UnityEngine.Debug;

namespace SanAndreasUnity.Behaviours
{

	public class LoadingThread : MonoBehaviour {

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

		static BlockingCollection<Job<object>> s_jobs =
			new BlockingCollection<Job<object>> (new ConcurrentProducerConsumerSortedSet<Job<object>>(new JobComparer()));
		static Thread s_thread;
		static readonly Utilities.ConcurrentQueue<Job<object>> s_processedJobs = new Utilities.ConcurrentQueue<Job<object>>();
		private readonly Queue<Job<object>> _processedJobsBuffer = new Queue<Job<object>>(256);

		static bool s_shouldThreadExit = false;

		private static long s_lastJobId = 1;
		private static object s_lastJobIdLockObject = new object();

		private readonly Stopwatch _stopwatch = new Stopwatch();

		public ushort maxTimePerFrameMs = 0;



		void Start () {

			s_thread = new Thread (ThreadFunction);
			s_thread.Start ();

		}

		void OnDisable ()
		{
			if (s_thread != null)
			{
				var sw = System.Diagnostics.Stopwatch.StartNew ();
			//	s_thread.Interrupt ();
				TellThreadToExit ();
				if (s_thread.Join (7000))
					Debug.LogFormat ("Stopped loading thread in {0} ms", sw.Elapsed.TotalMilliseconds);
				else
					Debug.LogError ("Failed to stop loading thread");
			}
		}

		void Update () {

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
					int numCopied = s_processedJobs.DequeueToQueue(_processedJobsBuffer, 256);
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
				return;

			if (0f == job.priority)
				throw new Exception("You forgot to assign job priority");

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

			s_jobs.Add (j);
		}

		static long GetNextJobId()
		{
			lock (s_lastJobIdLockObject)
			{
				return s_lastJobId++;
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		static bool ShouldThreadExit()
		{
			return s_shouldThreadExit;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		static void TellThreadToExit()
		{
			s_shouldThreadExit = true;
		}

		static void ThreadFunction ()
		{

			while (!ShouldThreadExit ())
			{
				Job<object> job;
				if (!s_jobs.TryTake (out job, 200))
					continue;

				try
				{
					job.result = job.action();
				}
				catch(System.Exception ex)
				{
					job.exception = ex;
				}

				s_processedJobs.Enqueue(job);
			}

		}

	}

}
