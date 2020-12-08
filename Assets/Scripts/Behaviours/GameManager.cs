using SanAndreasUnity.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SanAndreasUnity.Behaviours {

	public class SceneChangedMessage {
		public Scene s1;
		public Scene s2;
	}

	public class GameManager : MonoBehaviour {

		public static GameManager Instance { get ; private set ; }

		public static bool CursorLocked { get ; private set ; }

		public Texture2D logoTexture = null;

		public GameObject barPrefab;

		[SerializeField] [Range(10, 100)] private int m_defaultMaxFps = 60;
		[SerializeField] [Range(10, 100)] private int m_defaultMaxFpsOnMobile = 25;

		[SerializeField] [Range(5, 100)] int m_defaultPhysicsUpdateRate = 30;
		[SerializeField] [Range(5, 100)] int m_defaultPhysicsUpdateRateOnMobile = 20;

		public Vector2 cursorSensitivity = new Vector2(2f, 2f);


		/// <summary> Are we in a startup scene ? </summary>
		public static bool IsInStartupScene { get { return UnityEngine.SceneManagement.SceneManager.GetActiveScene ().buildIndex == 0; } }



		private void Awake() {

			if (null == Instance)
				Instance = this;

			// set default max fps
			if (Application.isMobilePlatform)
				SetMaxFps(m_defaultMaxFpsOnMobile);
			else
				SetMaxFps(m_defaultMaxFps);

			// set default physics update rate
			if (Application.isMobilePlatform)
				Time.fixedDeltaTime = 1.0f / m_defaultPhysicsUpdateRateOnMobile;
			else
				Time.fixedDeltaTime = 1.0f / m_defaultPhysicsUpdateRate;

		}

		void OnEnable ()
		{
			SceneManager.activeSceneChanged += this.OnSceneChangedInternal;
		}

		void OnDisable ()
		{
			SceneManager.activeSceneChanged -= this.OnSceneChangedInternal;
		}

		void OnSceneChangedInternal (Scene s1, Scene s2)
		{
			Utilities.F.SendMessageToObjectsOfType<MonoBehaviour>("OnSceneChanged", new SceneChangedMessage() {s1 = s1, s2 = s2});
		}

		void Start () {
			
		}
		
		void Update () {


			// Fix cursor state if it has been 'broken', happens eg. with zoom gestures in the editor in macOS
			if (CursorLocked && ((Cursor.lockState != CursorLockMode.Locked) || (Cursor.visible)))
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}


		}

		public static bool CanPlayerReadInput() {
			return Loader.HasLoaded && !UI.PauseMenu.IsOpened && !ChatDisplay.IsOpened;
		}

		public static void ChangeCursorState(bool locked, bool updateVisibility = true)
		{
			CursorLocked = locked;
			Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
			if (updateVisibility)
				Cursor.visible = !locked;
		}

		public static void ExitApplication() {

			#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
			#else
			Application.Quit ();
			#endif

		}

		public static void SetMaxFps (int maxFps)
		{
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = maxFps;
		}

		public static int GetMaxFps ()
		{
			if (!IsFpsLimited ())
				return 0;
			return Application.targetFrameRate;
		}

		public static bool IsFpsLimited ()
		{
			return QualitySettings.vSyncCount == 0;
		}

	}

}
