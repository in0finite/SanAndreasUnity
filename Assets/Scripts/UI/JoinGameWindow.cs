using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Net;

namespace SanAndreasUnity.UI
{

	public class JoinGameWindow : PauseMenuWindow
    {
		string m_ip = "";
		string m_port = NetManager.defaultListenPortNumber.ToString();


		JoinGameWindow()
        {

			// set default parameters

			this.windowName = "Join Game";
			this.useScrollView = true;

		}

		void Start ()
        {
			// adjust rect
			this.windowRect = GUIUtils.GetCenteredRect(new Vector2(550, 300));
		}


		protected override void OnWindowGUI ()
		{
			
			GUILayout.Label ("IP:");
			m_ip = GUILayout.TextField(m_ip, GUILayout.Width(150));

            GUILayout.Label ("Port:");
			m_port = GUILayout.TextField(m_port, GUILayout.Width(100));
            
            GUILayout.Space(40);

            if (GUILayout.Button("Connect", GUILayout.Width(80), GUILayout.Height(30)))
				Connect();

		}

		void Connect()
		{
			try
			{
				NetManager.StartClient(m_ip, int.Parse(m_port));
			}
			catch (System.Exception ex)
			{
				MessageBox.Show("Error", ex.ToString());
			}
		}

	}

}
