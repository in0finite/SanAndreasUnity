using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Behaviours;
using System.Linq;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.UI {
	
	public class PauseMenu : MonoBehaviour {

		public static PauseMenu Instance { get; private set; }

		private static bool m_isOpened = false;

		public	static	bool	IsOpened
		{
			get {
				return m_isOpened;
			}
			set {
				if (m_isOpened == value)
					return;

				m_isOpened = value;

				Instance.canvas.enabled = m_isOpened;
			}
		}

		public	static	event System.Action	onDrawItems = delegate {};

		//	public	Color	windowsColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);
		//	private	static	Texture2D	m_windowBackgroundTexture = null;
		//	private	static	bool	m_changedWindowStyle = false;

		public MenuBar menuBar;

		public Color openedWindowTextColor = Color.green;
		public Color ClosedWindowTextColor => this.menuBar.DefaultMenuEntryTextColor;

		public static event System.Action onGUI = delegate {};

		public Canvas canvas;



		void Awake () {
			
			if (null == Instance)
				Instance = this;

		//	m_windowBackgroundTexture = Utilities.F.CreateTexture (1, 1, this.windowsColor);

		}

		void Start () {

			this.menuBar.RegisterMenuEntry("Resume", int.MinValue, () => IsOpened = false);
			this.menuBar.RegisterMenuEntry("Exit", int.MaxValue, () => GameManager.ExitApplication());

			this.canvas.enabled = IsOpened;

		}

		public	static	PauseMenuWindow[]	GetAllWindows() {
			return FindObjectsOfType<PauseMenuWindow> ();
		}

		void Update () {

			// toggle pause menu
			if (Loader.HasLoaded && Input.GetButtonDown ("Start")) {
				
				if (IsOpened) {
					// if there is a modal window, close it, otherwise close pause menu
					var window = GetAllWindows ().FirstOrDefault (w => w.IsOpened && w.IsModal);
					if (window != null) {
						window.IsOpened = false;
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

			// update cursor lock state and visibility
			if (Loader.HasLoaded)
			{
				if (UIManager.Instance.UseTouchInput)
				{
					// unlock the cursor
					GameManager.ChangeCursorState (false, false);
					// make it visible while pause menu is opened
					Cursor.visible = IsOpened;
				}
				else
				{
					bool shouldBeLocked = !IsOpened;
					if (GameManager.CursorLocked != shouldBeLocked)
						GameManager.ChangeCursorState (shouldBeLocked);
				}
			}

		}

		void OnGUI() {

			if (!Loader.HasLoaded || !IsOpened)
				return;

//			if (!m_changedWindowStyle) {
//				m_changedWindowStyle = true;
//				GUI.skin.window.normal.background = m_windowBackgroundTexture;
//			}


			// draw title
			Utilities.GUIUtils.CenteredLabel (new Vector2 (Screen.width / 2.0f, 20), "<b>PAUSE MENU</b>");


			GUI.BeginGroup (new Rect (10, 0, 250, Screen.height));

			GUILayout.Space (20);

			if (GUILayout.Button ("Resume"))
				IsOpened = false;
			
			GUILayout.Space (10);

			// draw all registered items
			onDrawItems ();

			GUILayout.Space (10);

			if (GUILayout.Button ("Exit")) {
				GameManager.ExitApplication ();
			}

			GUI.EndGroup ();


			onGUI();

		}

	}

}
