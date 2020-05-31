using UnityEngine;
using SanAndreasUnity.Utilities;
using System.Threading;
using System.Runtime.CompilerServices;

namespace SanAndreasUnity.Behaviours
{

	public class LoadingThread : MonoBehaviour {
		
		public struct Job<T>
		{
			public System.Func<T> action ;
			public System.Action<T> callbackSuccess ;
			public System.Action<System.Exception> callbackError ;
			public System.Action<T> callbackFinish;
			internal object result ;
			internal System.Exception exception ;
		}

		static System.Collections.Concurrent.BlockingCollection<Job<object>> s_jobs = new System.Collections.Concurrent.BlockingCollection<Job<object>> ();
		static Thread s_thread;
		static System.Collections.Concurrent.BlockingCollection<Job<object>> s_processedJobs = new System.Collections.Concurrent.BlockingCollection<Job<object>> ();

		static bool s_shouldThreadExit = false;



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

			Job<object> job;
			while (s_processedJobs.TryTake (out job, 0))
			{
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
			if (null == job.action)
				return;

			job.exception = null;

			var j = new Job<object> () {
				action = () => job.action(),
				callbackError = job.callbackError,
			};
			if(job.callbackSuccess != null)
				j.callbackSuccess = (arg) => job.callbackSuccess( (T) arg );
			if(job.callbackFinish != null)
				j.callbackFinish = (arg) => job.callbackFinish( (T) arg );

			s_jobs.Add (j);
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

				s_processedJobs.Add (job);
			}

		}

	}

}
