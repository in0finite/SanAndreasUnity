using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Behaviours;

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

		private	static	PlayerController m_playerController;



		void Awake () {

			m_playerController = FindObjectOfType<PlayerController> ();

		}

		// Use this for initialization
		void Start () {
			
		}
		
		// Update is called once per frame
		void Update () {

			if (Input.GetKeyDown(KeyCode.Escape))
				IsOpened = !IsOpened;

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
			if (m_playerController != null) {
				bool shouldBeLocked = !IsOpened;
				if (GameManager.CursorLocked != shouldBeLocked)
					GameManager.ChangeCursorState (shouldBeLocked);
			}

		}

		void OnGUI() {

			if (!Loader.HasLoaded || !IsOpened)
				return;

			// TODO: not finished


			GUILayout.Space (20);

			GUILayout.Button ("Resume");
			GUILayout.Button ("Map");
			GUILayout.Button ("Controls");
			GUILayout.Button ("Exit");


		}

	}

}
