using System.Collections.Generic;
using UnityEngine;

namespace SanAndreasUnity.Behaviours {
	
	public class GameManager : MonoBehaviour {

		public static bool CursorLocked { get ; private set ; }




		private void Awake() {

			CursorLocked = true;
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;

		}

		// Use this for initialization
		void Start () {
			
		}
		
		// Update is called once per frame
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

	}

}
