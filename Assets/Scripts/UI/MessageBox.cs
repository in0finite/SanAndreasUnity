using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.UI {

	public class MessageBox : PauseMenuWindow
    {

		public string Title { get; set; }
		public string Text { get; set; }
		public bool UseTextField { get; set; }


		MessageBox() {

			// set default parameters

			this.windowName = "";
			this.useScrollView = true;
			// adjust rect
			this.windowRect = GUIUtils.GetCenteredRect( new Vector2(400, 300) );

		}

		void Start () {
			
		}

		protected override void OnWindowGUI()
		{

			this.windowName = this.Title;

			if (this.UseTextField)
				GUILayout.TextField(this.Text);
			else
				GUILayout.Label(this.Text);

		}

	}
}
