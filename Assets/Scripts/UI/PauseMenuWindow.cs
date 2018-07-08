using System.Collections.Generic;
using UnityEngine;

namespace SanAndreasUnity.UI {
	
	public class PauseMenuWindow : MonoBehaviour {

		public	string	windowName = "";

		public	bool	isOpened = false;
		public bool IsOpened { get { return this.isOpened; } set { isOpened = value; } }

		private	static	int	lastWindowId = 1352345;
		private	int	windowId = lastWindowId++;
		public int WindowId { get { return this.windowId; } }

		public	Rect	windowRect = Utilities.GUIUtils.GetCenteredRectPerc(new Vector2(0.5f, 0.5f));

		public	bool	useScrollView = false;
		protected	Vector2	scrollPos = Vector2.zero;

		protected	bool	isDraggable = true;
		public bool IsDraggable { get { return this.isDraggable; } }

		protected	bool	isModal = false;
		public bool IsModal { get { return this.isModal; } }



		void OnGUI() {

			if (!PauseMenu.IsOpened || !this.isOpened)
				return;

			if (this.isModal)
				this.windowRect = GUI.ModalWindow (this.windowId, this.windowRect, WindowFunction, this.windowName);
			else
				this.windowRect = GUI.Window( this.windowId, this.windowRect, WindowFunction, this.windowName );

		}

		void WindowFunction( int id ) {

			// display exit button ?

			if (this.useScrollView)
				this.scrollPos = GUILayout.BeginScrollView (this.scrollPos);

			this.OnWindowGUI ();

			if (this.useScrollView)
				GUILayout.EndScrollView ();

			if (this.isDraggable)
				GUI.DragWindow ();
			
		}

		protected virtual void OnWindowGUI() {
			
		}


		public	void	RegisterButtonInPauseMenu() {

			PauseMenu.onDrawItems += this.OnPauseMenuGUI;

		}

		public	void	UnRegisterButtonInPauseMenu() {

			PauseMenu.onDrawItems -= this.OnPauseMenuGUI;

		}

		private	void	OnPauseMenuGUI() {

			// display button for opening/closing window

			string text = this.isOpened ? "Hide " + this.windowName : "Show " + this.windowName;

			if (GUILayout.Button (text)) {
				this.isOpened = ! this.isOpened;
			}

		}

	}

}
