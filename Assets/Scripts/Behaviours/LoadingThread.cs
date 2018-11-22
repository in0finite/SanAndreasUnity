using UnityEngine;
using SanAndreasUnity.Utilities;
using System.Threading;

namespace SanAndreasUnity.Behaviours
{

	public class LoadingThread : MonoBehaviour {
		
		public struct Job
		{
			public System.Func<object> action;
			public System.Action<object> callbackSuccess;
			public System.Action<System.Exception> callbackError;
			internal object result;
			internal System.Exception exception;
		}

		static System.Collections.Concurrent.BlockingCollection<Job> s_jobs = new System.Collections.Concurrent.BlockingCollection<Job> ();
		static Thread s_thread;
		static System.Collections.Concurrent.BlockingCollection<Job> s_processedJobs = new System.Collections.Concurrent.BlockingCollection<Job> ();



		void Start () {

			s_thread = new Thread (ThreadFunction);
			s_thread.Start ();

		}

		void OnDisable ()
		{
			if (s_thread != null)
			{
				s_thread.Interrupt ();
				s_thread.Join (7000);
			}
		}

		void Update () {

			// get all processed jobs

			Job job;
			while (s_processedJobs.TryTake (out job, 0))
			{
				if (job.exception != null)
				{
					// error happened
					if (job.callbackError != null)
						Utilities.F.RunExceptionSafe( () => job.callbackError (job.exception) );
				}
				else
				{
					// success
					if (job.callbackSuccess != null)
						Utilities.F.RunExceptionSafe( () => job.callbackSuccess (job.result) );
				}
			}

		}


		public static void RegisterJob (Job job)
		{
			if (null == job.action)
				return;

			job.exception = null;

			s_jobs.Add (job);
		}

		static void ThreadFunction ()
		{

			while (true)
			{
				var job = s_jobs.Take ();

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
