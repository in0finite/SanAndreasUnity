using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Behaviours;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.UI {

	public class ChangePathToGTAWindow : PauseMenuWindow {

		FileBrowser m_fileBrowser = null;



		ChangePathToGTAWindow()
		{

			// set default parameters

			this.windowName = "Change path to GTA";

		}

		void Awake ()
		{
			
		}

		void Start ()
		{
			
			// adjust rect
			this.windowRect = GUIUtils.GetCenteredRect( new Vector2( 600, 400 ) );

		}


		protected override void OnWindowOpened ()
		{
			
			// create file browser
			if (null == m_fileBrowser)
			{
				m_fileBrowser = new FileBrowser (new Rect (new Vector2 (0, 0), this.WindowSize), "", this.OnSelectedPath) {
					BrowserType = FileBrowserType.Directory
				};
			}

			// load config
			Config.Load ();

			// set current directory to game directory
			m_fileBrowser.CurrentDirectory = Config.GetPath (Config.const_game_dir);

		}

		void OnSelectedPath (string path)
		{

			if (string.IsNullOrEmpty (path))
			{
				// canceled
				this.IsOpened = false;
				return;
			}

			// save new path
			Config.SetString (Config.const_game_dir, path);
			Config.SaveUserConfigSafe ();

			this.IsOpened = false;

		}

		protected override void OnWindowGUI ()
		{

			if (m_fileBrowser != null)
			{
				m_fileBrowser.OnGUI ();
			}

		}

	}

}
