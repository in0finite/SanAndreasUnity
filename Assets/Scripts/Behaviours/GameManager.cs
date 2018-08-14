using System.Collections.Generic;
using UnityEngine;

namespace SanAndreasUnity.Behaviours {
	
	public class GameManager : MonoBehaviour {

		public static GameManager Instance { get ; private set ; }

		public static bool CursorLocked { get ; private set ; }

		public GameObject pedPrefab;




		private void Awake() {

			if (null == Instance)
				Instance = this;

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

			return Loader.HasLoaded && !UI.PauseMenu.IsOpened;

		}

		public static void ChangeCursorState(bool locked)
		{
			CursorLocked = locked;
			Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
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
