using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Behaviours;
using UnityEngine.SceneManagement;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.UI
{
	
	public class MainMenu : MonoBehaviour {

		public float minButtonHeight = 25f;
		public float minButtonWidth = 70f;
		public float spaceAtBottom = 15f;
		public float spaceBetweenButtons = 5f;

		public bool drawBackground = false;
		public Color backgroundColor = Color.black;
		public bool drawLogo = false;

		private static List<System.Action> s_registeredMenuItems = new List<System.Action>();

		private static GUILayoutOption[] s_buttonOptions = new GUILayoutOption[0];
		public static GUILayoutOption[] ButtonLayoutOptions { get { return s_buttonOptions; } }



		void Start ()
		{
			
		}

		void OnGUI ()
		{
			if (!GameManager.IsInStartupScene)
				return;

			// draw main menu gui

			// background

			if (this.drawBackground)
			{
				GUIUtils.DrawRect (GUIUtils.ScreenRect, this.backgroundColor);
			}

			// logo

			if (this.drawLogo)
			{
				if (GameManager.Instance.logoTexture != null)
				{
					GUI.DrawTexture (GUIUtils.GetCenteredRect (GameManager.Instance.logoTexture.GetSize ()), GameManager.Instance.logoTexture);
				}
			}

			// draw buttons at bottom of screen: Main scene, Demo scene, Options, Change path to GTA, Exit

			s_buttonOptions = new GUILayoutOption[]{ GUILayout.MinWidth(minButtonWidth), GUILayout.MinHeight(minButtonHeight) };

			GUILayout.BeginArea (new Rect (0f, Screen.height - (minButtonHeight + spaceAtBottom), Screen.width, minButtonHeight + spaceAtBottom));
		//	GUILayout.Space (5);
		//	GUILayout.FlexibleSpace ();


			GUILayout.BeginHorizontal ();

			GUILayout.Space (5);
			GUILayout.FlexibleSpace ();

			// draw registered menu items
			foreach (var item in s_registeredMenuItems)
			{
				item ();
				GUILayout.Space (this.spaceBetweenButtons);
			}

			if (GUILayout.Button ("Exit", s_buttonOptions))
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

		public static void RegisterMenuItem (System.Action action)
		{
			s_registeredMenuItems.AddIfNotPresent (action);
		}

	}

}
