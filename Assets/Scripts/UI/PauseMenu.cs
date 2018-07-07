using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Behaviours;
using System.Linq;

namespace SanAndreasUnity.UI {
	
	public class PauseMenu : MonoBehaviour {

		private static bool m_isOpened = false;

		public	static	bool	IsOpened
		{
			get {
				return m_isOpened;
			}
			set {
				m_isOpened = value;

				// Why ?
				if (GameManager.CursorLocked)
					GameManager.ChangeCursorState(false);
			}
		}

		public	static	event System.Action	onDrawItems = delegate {};

		private	static	PlayerController m_playerController;



		void Awake () {

			m_playerController = FindObjectOfType<PlayerController> ();

		}

		void Start () {
			
		}

		public	static	PauseMenuWindow[]	GetAllWindows() {
			return FindObjectsOfType<PauseMenuWindow> ();
		}

		void Update () {

			if (Input.GetKeyDown (KeyCode.Escape)) {
				
				if (IsOpened) {
					// if there is a modal window, close it, otherwise close pause menu
					var window = GetAllWindows ().FirstOrDefault (w => w.isOpened && w.IsModal);
					if (window != null) {
						window.isOpened = false;
					} else {
						IsOpened = !IsOpened;
					}
				} else {
					IsOpened = !IsOpened;
				}

			}

		//	if (IsOpened && Input.GetKeyDown(KeyCode.M))
		//		IsOpened = false;

		//	if (MiniMap.toggleMap && Input.GetKeyDown(KeyCode.Escape))
		//		MiniMap.toggleMap = false;

//			bool isConsoleStateChanged = Console.Instance.m_openKey != Console.Instance.m_closeKey ?
//				Input.GetKeyDown(Console.Instance.m_openKey) || Input.GetKeyDown(Console.Instance.m_closeKey) :
//				Input.GetKeyDown(Console.Instance.m_openKey);
//
//			if (m_playerController != null) {
//				// WTF is this ?!
//
//				// Fixed: If Escape is pressed, map isn't available
//				if (!IsOpened && (Input.GetKeyDown (KeyCode.Escape) || isConsoleStateChanged || Input.GetKeyDown (KeyCode.F1) || (m_playerController.CursorLocked && Input.GetKeyDown (KeyCode.M))))
//					m_playerController.ChangeCursorState (!m_playerController.CursorLocked);
//			}

			// unlock and show cursor while pause menu is opened
			if (Loader.HasLoaded && m_playerController != null) {
				bool shouldBeLocked = !IsOpened;
				if (GameManager.CursorLocked != shouldBeLocked)
					GameManager.ChangeCursorState (shouldBeLocked);
			}

		}

		void OnGUI() {

			if (!Loader.HasLoaded || !IsOpened)
				return;
			

			// draw title
			string pauseMenuTitle = "PAUSE MENU";
			Vector2 titleSize = Utilities.GUIUtils.CalcScreenSizeForText (pauseMenuTitle, GUI.skin.label);
			GUI.Label( new Rect(Screen.width / 2 - titleSize.x / 2, 10, titleSize.x, titleSize.y), pauseMenuTitle);


			GUILayout.Space (20);

			// TODO: add items: map, 

			if (GUILayout.Button ("Resume"))
				IsOpened = false;
			
			GUILayout.Space (10);

			// draw all registered items
			onDrawItems ();

			GUILayout.Space (10);

			if (GUILayout.Button ("Exit")) {
				GameManager.ExitApplication ();
			}

		}

	}

}
