using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Net;
using System.Linq;

namespace SanAndreasUnity.UI
{

	public class JoinGameWindow : PauseMenuWindow
    {
		[SerializeField] string m_ipStr = "127.0.0.1";
		string m_portStr = NetManager.defaultListenPortNumber.ToString();

		string[] m_tabNames = new string[]{"Direct", "LAN", "Internet" };
		int m_currentTabIndex = 0;

		Mirror.NetworkDiscoveryHUD m_netDiscoveryHUD;
        private List<ServerInfo> _servers;
        private Vector2 _scrollViewPos;

        JoinGameWindow()
        {

			// set default parameters

			this.windowName = "Join Game";
			this.useScrollView = true;

		}

		async void Start ()
        {
			// adjust rect
			float width = Mathf.Min(650, Screen.width * 0.9f);
			this.windowRect = GUIUtils.GetCenteredRect(new Vector2(width, 400));

			m_netDiscoveryHUD = Mirror.NetworkManager.singleton.GetComponentOrThrow<Mirror.NetworkDiscoveryHUD>();
			m_netDiscoveryHUD.connectAction = this.ConnectFromDiscovery;
			m_netDiscoveryHUD.drawGUI = false;

	        _servers = await MasterServerClient.Instance.GetAllServers();

		}

		void Update()
		{
			if (PauseMenu.IsOpened)
				this.IsOpened = false;
		}


		protected override void OnWindowGUI ()
		{
			
			m_currentTabIndex = GUIUtils.TabsControl(m_currentTabIndex, m_tabNames);

			GUILayout.Space(20);

			if (0 == m_currentTabIndex)
			{
				GUILayout.Label ("IP:");
				m_ipStr = GUILayout.TextField(m_ipStr, GUILayout.Width(200));

				GUILayout.Label ("Port:");
				m_portStr = GUILayout.TextField(m_portStr, GUILayout.Width(100));
			}
			else if (1 == m_currentTabIndex)
			{
				m_netDiscoveryHUD.width = (int) this.WindowSize.x - 30;
				m_netDiscoveryHUD.DisplayServers();
			}
			else if (2 == m_currentTabIndex)
            {
                // header
                GUILayout.BeginHorizontal();
                GUILayout.Button("Name");
                GUILayout.Button("Players");
                GUILayout.EndHorizontal();

                if (_servers == null) return;
                _scrollViewPos = GUILayout.BeginScrollView(_scrollViewPos);

                foreach (ServerInfo info in _servers)
                {
                    GUILayout.BeginHorizontal();

                    GUILayout.Label(info.Name, GUILayout.Width(400));
                    GUILayout.Label($"{info.NumPlayersOnline}/{info.MaxPlayers}");
                    if (GUIUtils.ButtonWithCalculatedSize("Connect", 80, 30))
                        Connect(info.IP, (ushort)info.Port);
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndScrollView();
            }
            
		}

		protected override void OnWindowGUIAfterContent()
		{

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

			// button for connecting/disconnecting/refreshing LAN

			string buttonText = "Connect";
			System.Action buttonAction = this.ConnectDirectly;
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
			else
			{
				if (1 == m_currentTabIndex)
				{
					GUI.enabled = ! m_netDiscoveryHUD.IsRefreshing;
					buttonText = m_netDiscoveryHUD.IsRefreshing ? ( "Refreshing." + new string('.', (int) ((Time.time * 2) % 3)) ) : "Refresh LAN";
					buttonAction = () => m_netDiscoveryHUD.Refresh();
				}
				if (2 == m_currentTabIndex)
                {
                    buttonText = "Refresh Servers";
                    buttonAction = async () => _servers = await MasterServerClient.Instance.GetAllServers();
                }
			}

            if (GUIUtils.ButtonWithCalculatedSize(buttonText, 80, 30))
				buttonAction();

		}

		void ConnectDirectly()
		{
			this.Connect(m_ipStr, ushort.Parse(m_portStr));
		}

		void ConnectFromDiscovery(Mirror.NetworkDiscovery.DiscoveryInfo info)
		{
			this.Connect(info.EndPoint.Address.ToString(), ushort.Parse( info.KeyValuePairs[Mirror.NetworkDiscovery.kPortKey] ));
		}

		void Connect(string ip, ushort port)
		{
			try
			{
				NetManager.StartClient(ip, port);
			}
			catch (System.Exception ex)
			{
				Debug.LogException(ex);
				MessageBox.Show("Error", ex.ToString());
			}
		}

		void Disconnect()
		{
			NetManager.StopNetwork();
		}

	}

}
