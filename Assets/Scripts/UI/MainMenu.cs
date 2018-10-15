using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Behaviours;
using UnityEngine.SceneManagement;

namespace SanAndreasUnity.UI
{
	
	public class MainMenu : MonoBehaviour {



		void Start ()
		{
			
		}

		void OnGUI ()
		{
			if (!GameManager.IsInStartupScene)
				return;

			// draw main menu gui

			// draw buttons at bottom of screen: Main scene, Demo scene, Options, Change path to GTA, Exit

			GUILayout.FlexibleSpace ();


			GUILayout.BeginHorizontal ();

			GUILayout.FlexibleSpace ();

			if (GUILayout.Button ("Main scene"))
			{
				SceneManager.LoadScene ("Main");
			}

			GUILayout.FlexibleSpace ();

			if (GUILayout.Button ("Demo scene"))
			{
				SceneManager.LoadScene ("ModelViewer");
			}

			GUILayout.FlexibleSpace ();

			if (GUILayout.Button ("Exit"))
			{
				GameManager.ExitApplication ();
			}

			GUILayout.FlexibleSpace ();

			GUILayout.EndHorizontal ();

			// add some space below buttons
			GUILayout.Space (15);

		}

	}

}
