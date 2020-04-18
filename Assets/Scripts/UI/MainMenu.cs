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
		public Color ClosedWindowTextColor => this.buttonPrefab.GetComponentInChildren<Text>().color;

		static MenuEntry s_rootMenuEntry = new MenuEntry();

		public Canvas canvas;
		public RectTransform buttonsContainer;
		public GameObject buttonPrefab;



		void Awake()
		{
			if (null == Instance)
				Instance = this;

			// add Exit button
			RegisterMenuEntry(new MenuEntry { name = "Exit", sortPriority = int.MaxValue, 
				clickAction = () => GameManager.ExitApplication() });
		}

		void OnSceneChanged(SceneChangedMessage sceneChangedMessage)
		{
			this.canvas.enabled = GameManager.IsInStartupScene;
		}


		public static bool DrawMenuEntry(string text)
		{
			return GUIUtils.ButtonWithCalculatedSize(text, Instance.minButtonWidth, Instance.minButtonHeight);
		}

		public static void RegisterMenuEntry (MenuEntry menuEntry)
		{
			int indexOfMenuEntry = s_rootMenuEntry.AddChild (menuEntry);

			GameObject buttonGo = Instantiate(Instance.buttonPrefab);
			
			buttonGo.name = menuEntry.name;

			buttonGo.GetComponentInChildren<Text>().text = menuEntry.name;

			buttonGo.transform.SetParent(Instance.buttonsContainer.transform, false);
			buttonGo.transform.SetSiblingIndex(indexOfMenuEntry);

			buttonGo.GetComponent<Button>().onClick.AddListener(() => menuEntry.clickAction());

		}

		public static Button GetMenuEntryButton(MenuEntry entry)
		{
			Transform child = Instance.buttonsContainer.transform.Find(entry.name);
			return child != null ? child.GetComponent<Button>() : null;
		}

		public static void SetEntryColor(MenuEntry entry, Color color)
		{
			var button = GetMenuEntryButton(entry);
			if (button != null)
				button.GetComponentInChildren<Text>().color = color;
		}

	}

}
