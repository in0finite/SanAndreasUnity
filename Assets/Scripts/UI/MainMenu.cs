using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Behaviours;
using UnityEngine.SceneManagement;
using SanAndreasUnity.Utilities;
using UnityEngine.UI;

namespace SanAndreasUnity.UI
{
	
	public class MainMenu : MonoBehaviour {

		public static MainMenu Instance { get; private set; }

		public float minButtonHeight = 25f;
		public float minButtonWidth = 70f;
		public float spaceAtBottom = 15f;
		public float spaceBetweenButtons = 5f;

		public Color openedWindowTextColor = Color.green;

		public bool drawLogo = false;

		private static GUILayoutOption[] s_buttonOptions = new GUILayoutOption[0];
		public static GUILayoutOption[] ButtonLayoutOptions { get { return s_buttonOptions; } }

		static MenuEntry s_rootMenuEntry = new MenuEntry();

		static bool s_hasMenuEntriesToAdd = false;

		public RectTransform buttonsContainer;
		public GameObject buttonPrefab;



		void Awake()
		{
			if (null == Instance)
				Instance = this;
		}

		void OnGUI ()
		{
			if (!GameManager.IsInStartupScene)
				return;

			// draw main menu gui

			// logo

			if (this.drawLogo)
			{
				if (GameManager.Instance.logoTexture != null)
				{
					GUI.DrawTexture (GUIUtils.GetCenteredRect (GameManager.Instance.logoTexture.GetSize ()), GameManager.Instance.logoTexture);
				}
			}

			// draw menu entries at bottom of screen

			s_buttonOptions = new GUILayoutOption[]{ GUILayout.MinWidth(minButtonWidth), GUILayout.MinHeight(minButtonHeight) };

			GUILayout.BeginArea (new Rect (0f, Screen.height - (minButtonHeight + spaceAtBottom), Screen.width, minButtonHeight + spaceAtBottom));
		//	GUILayout.Space (5);
		//	GUILayout.FlexibleSpace ();


			GUILayout.BeginHorizontal ();

			GUILayout.Space (5);
			GUILayout.FlexibleSpace ();

			// draw registered menu items
			foreach (var item in s_rootMenuEntry.children)
			{
				if (item.drawAction != null)
					item.drawAction();
				GUILayout.Space (this.spaceBetweenButtons);
			}

			if (MainMenu.DrawMenuEntry ("Exit"))
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

		public static bool DrawMenuEntry(string text)
		{
			return GUIUtils.ButtonWithCalculatedSize(text, Instance.minButtonWidth, Instance.minButtonHeight);
		}

		public static void RegisterMenuEntry (MenuEntry menuEntry)
		{
			int indexOfMenuEntry = s_rootMenuEntry.AddChild (menuEntry);

			GameObject buttonGo = Instantiate(Instance.buttonPrefab);

			buttonGo.GetComponentInChildren<Text>().text = menuEntry.name;

			buttonGo.transform.SetParent(Instance.buttonsContainer.transform, false);
			buttonGo.transform.SetSiblingIndex(indexOfMenuEntry);

			buttonGo.GetComponent<Button>().onClick.AddListener(() => menuEntry.clickAction());

			//Instance.buttonsContainer.GetComponent<HorizontalLayoutGroup>();

			//MenuEntriesChanged();
		}

		static void MenuEntriesChanged()
		{
			if (s_hasMenuEntriesToAdd)
				return;

			s_hasMenuEntriesToAdd = true;

			Instance.Invoke(nameof(UpdateMenuEntries), 0.0001f);
		}

		void UpdateMenuEntries()
		{
			s_hasMenuEntriesToAdd = false;


		}

	}

}
