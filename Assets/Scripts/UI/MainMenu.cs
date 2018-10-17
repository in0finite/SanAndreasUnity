using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Behaviours;
using UnityEngine.SceneManagement;

namespace SanAndreasUnity.UI
{
	
	public class MainMenu : MonoBehaviour {

		public float minButtonHeight = 25f;
		public float minButtonWidth = 70f;
		public float spaceAtBottom = 15f;
		public float spaceBetweenButtons = 5f;


		void Start ()
		{
			
		}

		void OnGUI ()
		{
			if (!GameManager.IsInStartupScene)
				return;

			// draw main menu gui

			// draw buttons at bottom of screen: Main scene, Demo scene, Options, Change path to GTA, Exit

			GUILayoutOption[] buttonOptions = new GUILayoutOption[]{ GUILayout.MinWidth(minButtonWidth), GUILayout.MinHeight(minButtonHeight) };

			GUILayout.BeginArea (new Rect (0f, Screen.height - (minButtonHeight + spaceAtBottom), Screen.width, minButtonHeight + spaceAtBottom));
		//	GUILayout.Space (5);
		//	GUILayout.FlexibleSpace ();


			GUILayout.BeginHorizontal ();

			GUILayout.Space (5);
			GUILayout.FlexibleSpace ();

			if (GUILayout.Button ("Main scene", buttonOptions))
			{
				SceneManager.LoadScene ("Main");
			}

			GUILayout.Space (this.spaceBetweenButtons);

			if (GUILayout.Button ("Demo scene", buttonOptions))
			{
				SceneManager.LoadScene ("ModelViewer");
			}

			GUILayout.Space (this.spaceBetweenButtons);

			if (GUILayout.Button ("Exit", buttonOptions))
			{
				GameManager.ExitApplication ();
			}

			GUILayout.FlexibleSpace ();
			GUILayout.Space (5);

			GUILayout.EndHorizontal ();

			// add some space below buttons
		//	GUILayout.Space (spaceAtBottom);

			GUILayout.EndArea ();

		}

	}

}
