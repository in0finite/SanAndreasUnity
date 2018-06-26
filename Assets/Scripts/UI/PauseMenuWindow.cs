using System.Collections.Generic;
using UnityEngine;

namespace SanAndreasUnity.UI {
	
	public class PauseMenuWindow : MonoBehaviour {

		public	string	windowName = "";

		public	bool	isOpened = false;

		private	static	int	lastWindowId = 1352345;
		private	int	windowId = lastWindowId++;
		public int WindowId { get { return this.windowId; } }

		public	Rect	windowRect = Utilities.GUIUtils.GetCenteredRectPerc(new Vector2(0.5f, 0.5f));



		void OnGUI() {

			if (!PauseMenu.IsOpened || !this.isOpened)
				return;

			this.windowRect = GUI.Window( this.windowId, this.windowRect, WindowFunction, this.windowName );

		}

		void WindowFunction( int id ) {

			// display exit button, scroll view ?

			this.OnWindowGUI ();

			GUI.DragWindow ();
		}

		protected virtual void OnWindowGUI() {
			
		}


		public	void	RegisterButtonInPauseMenu() {



		}

	}

}
