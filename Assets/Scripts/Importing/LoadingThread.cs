using UGameCore.Utilities;

namespace SanAndreasUnity.Importing
{
	public class LoadingThread : StartupSingleton<LoadingThread>
	{
		public BackgroundJobRunner BackgroundJobRunner { get; } = new BackgroundJobRunner();
		
		public ushort maxTimePerFrameMs = 0;



#if UNITY_EDITOR
		[UnityEditor.InitializeOnLoadMethod]
		static void InitOnLoadInEditor()
        {
			if (null == Singleton)
				return;

			Singleton.BackgroundJobRunner.EnsureBackgroundThreadStarted();
        }
#endif

		protected override void OnSingletonStart()
		{
			this.BackgroundJobRunner.EnsureBackgroundThreadStarted();
		}

		protected override void OnSingletonDisable()
		{
			this.BackgroundJobRunner.ShutDown();
		}

		void Update()
        {
			this.UpdateJobs();
        }

		public void UpdateJobs()
		{
			this.BackgroundJobRunner.UpdateJobs(this.maxTimePerFrameMs);
		}

		public static void RegisterJob<T>(BackgroundJobRunner.Job<T> job)
        {
			ThreadHelper.ThrowIfNotOnMainThread(); // obtaining Singleton should only happen on main thread

			Singleton.BackgroundJobRunner.RegisterJob(job);
		}

	}

}
