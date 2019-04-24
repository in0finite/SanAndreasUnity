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
			this.DestroyOnClose = true;
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

		protected override void OnWindowGUIAfterContent()
		{
			// display OK button

			GUILayout.Space(4);
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("OK", GUILayout.MinWidth(45), GUILayout.MinHeight(25)))
			{
				this.IsOpened = false;
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.Space(4);
		}

		public static MessageBox Show(string title, string text, bool useTextField = false)
		{
			var msgBox = PauseMenuWindow.Create<MessageBox>();
			msgBox.Title = title;
			msgBox.Text = text;
			msgBox.UseTextField = useTextField;

			msgBox.IsOpened = true;
			
			return msgBox;
		}

	}
}
