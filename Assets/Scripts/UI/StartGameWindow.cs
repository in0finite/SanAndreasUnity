using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Net;

namespace SanAndreasUnity.UI
{

	public class StartGameWindow : PauseMenuWindow
    {
		string m_port = NetManager.defaultListenPortNumber.ToString();
		bool m_dontListen = false;
		bool m_dedicatedServer = false;
		[SerializeField] string[] m_availableScenes = new string[]{"Main", "ModelViewer"};
		int m_selectedSceneIndex = 0;


		StartGameWindow()
        {

			// set default parameters

			this.windowName = "Start Game";
			this.useScrollView = true;

		}

		void Start ()
        {
			// adjust rect
			this.windowRect = GUIUtils.GetCenteredRect(new Vector2(550, 300));
		}


		protected override void OnWindowGUI ()
		{
			
            GUILayout.Label ("Port:");
			m_port = GUILayout.TextField(m_port);

			m_dontListen = GUILayout.Toggle(m_dontListen, "Don't listen:");
            
			m_dedicatedServer = GUILayout.Toggle(m_dedicatedServer, "Dedicated server:");
            
			GUILayout.Label("Map:");
			m_selectedSceneIndex = GUILayout.SelectionGrid(m_selectedSceneIndex, m_availableScenes, 4);

            GUILayout.Space(40);

            if (GUILayout.Button("Start", GUILayout.Width(80), GUILayout.Height(30)))
				StartGame();

		}

		void StartGame()
		{
			try
			{
				int port = int.Parse(m_port);
				string scene = m_availableScenes[m_selectedSceneIndex];

				// first start a server, and then change scene

				if (m_dedicatedServer)
					NetManager.StartServer(port);
				else
					NetManager.StartHost(port);

				UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
			}
			catch (System.Exception ex)
			{
				MessageBox.Show("Error", ex.ToString());
			}
		}

	}

}
