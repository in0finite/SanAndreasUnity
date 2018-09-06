using System.Collections.Generic;
using UnityEngine;

namespace SanAndreasUnity.UI {
	
	public class PauseMenuWindow : MonoBehaviour {

		public	string	windowName = "";

		[SerializeField]	protected	bool	m_isOpenedByDefault = false;

		private	bool	m_isOpened = false;
		public bool IsOpened {
			get { return this.m_isOpened; } 
			set { 
				if (m_isOpened == value)
					return;
				m_isOpened = value;
				if (m_isOpened)
					this.OnWindowOpened ();
				else
					this.OnWindowClosed ();
			}
		}

		private	static	int	lastWindowId = 1352345;
		private	int	windowId = lastWindowId++;
		public int WindowId { get { return this.windowId; } }

		public	Rect	windowRect = Utilities.GUIUtils.GetCenteredRectPerc(new Vector2(0.5f, 0.5f));
		public	Vector2	WindowSize { get { return this.windowRect.size; } }

		public	bool	useScrollView = false;
		protected	Vector2	scrollPos = Vector2.zero;

		protected	bool	isDraggable = true;
		public bool IsDraggable { get { return this.isDraggable; } }

		protected	bool	isModal = false;
		public bool IsModal { get { return this.isModal; } }

		protected	bool	m_hasExitButton = true;
		protected	bool	m_hasMinimizeButton = true;

		private	bool	m_isMinimized = false;
		public bool IsMinimized { get { return this.m_isMinimized; } set { m_isMinimized = value; } }

		public	const	float	kMinimizedWindowHeight = 45;

		private	bool	m_hasStarted = false;

		[SerializeField]	private	float	m_spaceBeforeContent = 0f;
		public float SpaceBeforeContent { get { return m_spaceBeforeContent; } set { m_spaceBeforeContent = value; } }
		[SerializeField]	private	float	m_spaceAfterContent = 0f;
		public float SpaceAfterContent { get { return m_spaceAfterContent; } set { m_spaceAfterContent = value; } }



		void WindowStart() {

			if (m_isOpenedByDefault)
				this.IsOpened = true;

			this.OnWindowStart ();
		}

		/// <summary>
		/// Called on first OnGUI().
		/// </summary>
		protected virtual void OnWindowStart() {
			
		}

		protected virtual void OnWindowOpened() {

		}

		protected virtual void OnWindowClosed() {

		}


		void OnGUI() {

			if (!m_hasStarted) {
				m_hasStarted = true;
				this.WindowStart ();
			}

			if (!PauseMenu.IsOpened || !this.IsOpened)
				return;


			Rect newRect;
			Rect inputRect = this.windowRect;
			if(this.IsMinimized)
				inputRect.height = kMinimizedWindowHeight;
			
			if (this.isModal)
				newRect = GUI.ModalWindow (this.windowId, inputRect, WindowFunction, this.windowName);
			else
				newRect = GUI.Window( this.windowId, inputRect, WindowFunction, this.windowName );
			
			if (this.IsMinimized)
				this.windowRect.position = newRect.position;	// only copy position
			else
				this.windowRect = newRect;

		}

		void WindowFunction( int id ) {


			float buttonWidth = 16;
			float buttonHeight = 16;
			float buttonYOffset = 2;

			// exit button
			if (m_hasExitButton) {
				Color exitButtonColor = Color.Lerp (Color.red, Color.white, 0.0f);
			//	exitButtonColor.a = 0.7f;
				if (Utilities.GUIUtils.ButtonWithColor (new Rect (this.windowRect.width - buttonWidth - 2, buttonYOffset, buttonWidth, buttonHeight), 
					   "x", exitButtonColor)) {
					this.IsOpened = false;
				}
			}

			// minimize button
			if (m_hasMinimizeButton) {
				if (GUI.Button (new Rect (this.windowRect.width - buttonWidth - 2 - buttonWidth - 2, buttonYOffset, buttonWidth, buttonHeight), "-")) {
					this.IsMinimized = !this.IsMinimized;
				}
			}


			if (!this.IsMinimized) {
				// draw contents inside window

				if (this.SpaceBeforeContent > 0)
					GUILayout.Space (this.SpaceBeforeContent);

				if (this.useScrollView)
					this.scrollPos = GUILayout.BeginScrollView (this.scrollPos);

				this.OnWindowGUI ();

				if (this.useScrollView)
					GUILayout.EndScrollView ();

				if (this.SpaceAfterContent > 0)
					GUILayout.Space (this.SpaceAfterContent);

				this.OnWindowGUIAfterContent ();
			}


			if (this.isDraggable)
				GUI.DragWindow ();
			
		}

		protected virtual void OnWindowGUI() {
			
		}

		protected virtual void OnWindowGUIAfterContent() {

		}


		public	void	RegisterButtonInPauseMenu() {

			PauseMenu.onDrawItems += this.OnPauseMenuGUI;

		}

		public	void	UnRegisterButtonInPauseMenu() {

			PauseMenu.onDrawItems -= this.OnPauseMenuGUI;

		}

		private	void	OnPauseMenuGUI() {

			// display button for opening/closing window

		//	string text = this.IsOpened ? "Hide " + this.windowName : "Show " + this.windowName;
			string text = this.windowName;

			if (GUILayout.Button (text)) {
				this.IsOpened = ! this.IsOpened;
			}

		}

	}

}
