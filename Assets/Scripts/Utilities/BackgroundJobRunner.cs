using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SanAndreasUnity.Utilities
{
    public class BackgroundJobRunner
    {
		// TODO: maybe convert to class, because it takes 36 bytes - it's too much for red-black tree operations
		public struct Job<T>
		{
			public System.Func<T> action;
			public System.Action<T> callbackSuccess;
			public System.Action<System.Exception> callbackError;
			public System.Action<T> callbackFinish;
			public float priority;
			internal object result;
			internal System.Exception exception;
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
				new BlockingCollection<Job<object>>(new System.Collections.Concurrent.ConcurrentQueue<Job<object>>());
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

		private long _lastJobId = 0;
		private readonly object _lastJobIdLockObject = new object();

		private long _lastProcessedJobId = 0;

		private readonly Stopwatch _stopwatch = new Stopwatch();



		void StartThread()
		{
			if (_thread != null)
				return;

			_thread = new Thread(ThreadFunction);
			_thread.Start(_threadParameters);
		}

		public void ShutDown()
		{
			if (_thread != null)
			{
				var sw = System.Diagnostics.Stopwatch.StartNew();
				//	_thread.Interrupt ();
				_threadParameters.TellThreadToExit();
				if (_thread.Join(7000))
					Debug.LogFormat("Stopped background thread in {0} ms", sw.Elapsed.TotalMilliseconds);
				else
					Debug.LogError("Failed to stop background thread");
			}
		}

		public void RegisterJob<T>(Job<T> job)
		{
			// note: this function can be called from any thread

			if (null == job.action)
				throw new ArgumentException("Job must have an action");

			if (0f == job.priority)
				throw new ArgumentException("You forgot to assign job priority");

			job.exception = null;

			var j = new Job<object>()
			{
				priority = job.priority,
				action = () => job.action(),
				callbackError = job.callbackError,
			};
			if (job.callbackSuccess != null)
				j.callbackSuccess = (arg) => job.callbackSuccess((T)arg);
			if (job.callbackFinish != null)
				j.callbackFinish = (arg) => job.callbackFinish((T)arg);

			lock (_lastJobIdLockObject)
			// make sure that changing id and adding new job is atomic operation, otherwise
			// multiple threads accessing this part of code can cause the jobs to be inserted out of order
			{
				j.id = ++_lastJobId;
				_threadParameters.jobs.Add(j);
			}
		}

		public void UpdateJobs(ushort maxTimeToUpdateMs)
		{
			ThreadHelper.ThrowIfNotOnMainThread();

			this.UpdateJobsInternal(maxTimeToUpdateMs);
		}

		void UpdateJobsInternal(ushort maxTimeToUpdateMs)
		{

			// get all processed jobs

			_stopwatch.Restart();

			Job<object> job;

			while (true)
			{
				if (maxTimeToUpdateMs != 0 && _stopwatch.ElapsedMilliseconds >= maxTimeToUpdateMs)
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
						Utilities.F.RunExceptionSafe(() => job.callbackError(job.exception));

					Debug.LogException(job.exception);
				}
				else
				{
					// success
					if (job.callbackSuccess != null)
						Utilities.F.RunExceptionSafe(() => job.callbackSuccess(job.result));
				}

				// invoke finish callback
				if (job.callbackFinish != null)
					F.RunExceptionSafe(() => job.callbackFinish(job.result));

				_lastProcessedJobId = job.id;
			}

		}

		public long GetNumJobsPendingApproximately()
		{
			ThreadHelper.ThrowIfNotOnMainThread();

			// this is not done in a critical section: calling Count on 2 multithreaded collections
			return (long)_threadParameters.jobs.Count + (long)_threadParameters.processedJobs.Count + (long)_processedJobsBuffer.Count;
		}

		public long GetNumPendingJobs()
		{
			// this will not work if collections used are not FIFO collections (eg. other than queues)
			// - this will be the case if job priority is used

			ThreadHelper.ThrowIfNotOnMainThread();

			lock (_lastJobIdLockObject)
			{
				if (_lastProcessedJobId > _lastJobId)
					throw new Exception($"Last processed job id ({_lastProcessedJobId}) is higher than last registered job id ({_lastJobId}). This should not happen.");

				return _lastJobId - _lastProcessedJobId;
			}
		}

		public bool IsBackgroundThreadRunning()
		{
			ThreadHelper.ThrowIfNotOnMainThread();

			if (null == _thread)
				return false;

			if (_thread.ThreadState != System.Threading.ThreadState.Running)
				return false;

			return true;
		}

		public void EnsureBackgroundThreadStarted()
		{
			ThreadHelper.ThrowIfNotOnMainThread();

			this.StartThread();
		}

		static void ThreadFunction(object objectParameter)
		{
			ThreadParameters threadParameters = (ThreadParameters)objectParameter;

			while (!threadParameters.ShouldThreadExit())
			{
				Job<object> job;
				if (!threadParameters.jobs.TryTake(out job, 200))
					continue;

				try
				{
					job.result = job.action();
				}
				catch (System.Exception ex)
				{
					job.exception = ex;
				}

				threadParameters.processedJobs.Enqueue(job);
			}

		}

	}
}
