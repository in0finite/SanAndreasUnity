using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Behaviours.Audio;
using System.Linq;

namespace SanAndreasUnity.UI {

	public class AudioWindow : PauseMenuWindow {



		AudioWindow() {

			// set default parameters

			this.windowName = "Audio";
			this.useScrollView = true;

		}

		void Start () {

			this.RegisterButtonInPauseMenu ();

			// adjust rect
			float minWidth = 600, maxWidth = 1000, desiredWidth = Screen.width * 0.9f ;
			float minHeight = 400, maxHeight = 700, desiredHeight = desiredWidth * 9f / 16f;
			this.windowRect = GUIUtils.GetCenteredRect (new Vector2 (Mathf.Clamp (desiredWidth, minWidth, maxWidth), 
				Mathf.Clamp (desiredHeight, minHeight, maxHeight)));
		}


		protected override void OnWindowGUI ()
		{
			
			if (null == AudioManager.AudioFiles)
			{
				GUILayout.Label ("Audio not loaded");
				return;
			}


			GUILayout.Label ("Streams");

			foreach (var f in AudioManager.AudioFiles.StreamsAudioFiles)
			{
				Rect rect = GUILayoutUtility.GetRect (400, 25);

				rect.width /= 2;
				GUI.Label (rect, f.Name);

				rect.x += rect.width;
				GUI.Label (rect, f.NumBanks.ToString ());
			}

			GUILayout.Space (20);

			GUILayout.Label ("SFX");

			foreach (var f in AudioManager.AudioFiles.SFXAudioFiles)
			{
				Rect rect = GUILayoutUtility.GetRect (500, 25);

				rect.width /= 3;
				GUI.Label (rect, f.Name);

				rect.x += rect.width;
				GUI.Label (rect, f.NumBanks.ToString ());

				rect.x += rect.width;
				GUI.Label (rect, f.NumAudios.ToString ());

			}

		}

	}

}
