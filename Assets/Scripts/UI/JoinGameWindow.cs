using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Net;

namespace SanAndreasUnity.UI
{

	public class JoinGameWindow : PauseMenuWindow
    {
		string m_ip = "127.0.0.1";
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

		void Update()
		{
			if (PauseMenu.IsOpened)
				this.IsOpened = false;
		}


		protected override void OnWindowGUI ()
		{
			
			GUILayout.Label ("IP:");
			m_ip = GUILayout.TextField(m_ip, GUILayout.Width(200));

            GUILayout.Label ("Port:");
			m_port = GUILayout.TextField(m_port, GUILayout.Width(100));
            
            GUILayout.Space(40);

			// label with status
			string strStatus = "Disconnected";
			if (NetStatus.IsClientConnecting())
			{
				strStatus = "Connecting.";
				for (int i = 0; i < ((int)Time.realtimeSinceStartup) % 3; i++)
					strStatus += ".";
			}
			else if (NetStatus.IsClientConnected())
			{
				strStatus = "Connected";
			}
			GUILayout.Label("Status: " + strStatus);

			// button for connecting/disconnecting

			string buttonText = "Connect";
			System.Action buttonAction = this.Connect;
			if (NetStatus.IsClientConnecting())
			{
				buttonText = "Disconnect";
				buttonAction = this.Disconnect;
			}
			else if (NetStatus.IsClientConnected())
			{
				GUI.enabled = false;
				buttonText = "Connected";
				buttonAction = () => {};
			}

            if (GUILayout.Button(buttonText, GUILayout.MinWidth(80), GUILayout.Height(30), GUILayout.ExpandWidth(false)))
				buttonAction();

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

		void Disconnect()
		{
			NetManager.StopNetwork();
		}

	}

}
