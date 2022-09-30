using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Behaviours;
using UGameCore.Utilities;

namespace SanAndreasUnity.UI
{
	
	public class MainMenu : MonoBehaviour {

		public static MainMenu Instance { get; private set; }

		public MenuBar menuBar;

		public Color openedWindowTextColor = Color.green;
		public Color ClosedWindowTextColor => this.menuBar.DefaultMenuEntryTextColor;

		public Canvas canvas;



		void Awake()
		{
			if (null == Instance)
				Instance = this;

			// add Exit button
			this.menuBar.RegisterMenuEntry("Exit", int.MaxValue, () => GameManager.ExitApplication());
		}

		void OnSceneChanged(SceneChangedMessage sceneChangedMessage)
		{
			this.canvas.enabled = GameManager.IsInStartupScene;
		}

	}

}
