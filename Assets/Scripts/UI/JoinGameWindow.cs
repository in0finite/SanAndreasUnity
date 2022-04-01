using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Net;
using System.Linq;
using System.Threading.Tasks;

namespace SanAndreasUnity.UI
{

	public class JoinGameWindow : PauseMenuWindow
    {
		[SerializeField] string m_ipStr = "127.0.0.1";
		string m_portStr = NetManager.defaultListenPortNumber.ToString();

		string[] m_tabNames = new string[]{"Internet", "LAN", "Direct" };
		private const int InternetTabIndex = 0, LanTabIndex = 1, DirectTabIndex = 2;

		int m_currentTabIndex = InternetTabIndex;

		Mirror.NetworkDiscoveryHUD m_netDiscoveryHUD;

        private List<ServerInfo> _serversFromMasterServer = new List<ServerInfo>();
        private Vector2 _masterServerScrollViewPos;
		bool _isRefreshingMasterServerList = false;
	    MessageBox _masterServerErrorMessageBox;
	    bool m_refreshedMasterServerWhenOpened = false;


        JoinGameWindow()
        {

			// set default parameters

			this.windowName = "Join Game";
			this.useScrollView = true;

		}

	    void Start ()
        {
			// adjust rect
			float width = Mathf.Min(900, Screen.width * 0.9f);
			float height = width * 9f / 16f;
			this.windowRect = GUIUtils.GetCenteredRect(new Vector2(width, height));

			m_netDiscoveryHUD = Mirror.NetworkManager.singleton.GetComponentOrThrow<Mirror.NetworkDiscoveryHUD>();
			m_netDiscoveryHUD.connectAction = this.ConnectFromDiscovery;
			m_netDiscoveryHUD.drawGUI = false;
        }

		void Update()
		{
			if (PauseMenu.IsOpened)
				this.IsOpened = false;
		}


		protected override void OnWindowOpened()
		{
			if (m_refreshedMasterServerWhenOpened)
				return;

			m_refreshedMasterServerWhenOpened = true;

#pragma warning disable 4014
			RefreshMasterServersButtonPressed();
#pragma warning restore 4014
		}

		protected override void OnWindowGUI ()
		{
			
			m_currentTabIndex = GUIUtils.TabsControl(m_currentTabIndex, m_tabNames);

			GUILayout.Space(20);

			if (DirectTabIndex == m_currentTabIndex)
			{
				GUILayout.Label ("IP:");
				m_ipStr = GUILayout.TextField(m_ipStr, GUILayout.Width(200));

				GUILayout.Label ("Port:");
				m_portStr = GUILayout.TextField(m_portStr, GUILayout.Width(100));
			}
			else if (LanTabIndex == m_currentTabIndex)
			{
				m_netDiscoveryHUD.width = (int) this.WindowSize.x - 30;
				m_netDiscoveryHUD.DisplayServers();
			}
			else if (InternetTabIndex == m_currentTabIndex)
            {
	            int availableWidth = (int) this.WindowSize.x - 50;
                float[] widthPercentages = new float[] { 0.35f, 0.25f, 0.4f };
                string[] columnNames = new string[] {"Name", "Players", "IP" };

                // header
                GUILayout.BeginHorizontal();
                for (int i = 0; i < columnNames.Length; i++)
                {
	                GUILayout.Button(columnNames[i], GUILayout.Width(availableWidth * widthPercentages[i]));
                }
                GUILayout.EndHorizontal();

                _masterServerScrollViewPos = GUILayout.BeginScrollView(_masterServerScrollViewPos);

                foreach (ServerInfo info in _serversFromMasterServer)
                {
                    GUILayout.BeginHorizontal();

                    GUILayout.Label(info.Name, GUIUtils.CenteredLabelStyle, GUILayout.Width(availableWidth * widthPercentages[0]));

                    GUILayout.Label($"{info.NumPlayersOnline}/{info.MaxPlayers}", GUIUtils.CenteredLabelStyle, GUILayout.Width(availableWidth * widthPercentages[1]));

	                GUI.enabled = ! NetStatus.IsClientActive();
	                if (GUILayout.Button($"{info.IP}:{info.Port}", GUILayout.Width(availableWidth * widthPercentages[2])))
	                    ConnectToServerFromMasterServer(info);
	                GUI.enabled = true;

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

			// button for connecting/disconnecting/refreshing

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
				if (LanTabIndex == m_currentTabIndex)
				{
					GUI.enabled = ! m_netDiscoveryHUD.IsRefreshing;
					buttonText = m_netDiscoveryHUD.IsRefreshing ? ( "Refreshing." + new string('.', (int) ((Time.time * 2) % 3)) ) : "Refresh LAN";
					buttonAction = () => m_netDiscoveryHUD.Refresh();
				}
				else if (InternetTabIndex == m_currentTabIndex)
                {
					GUI.enabled = !_isRefreshingMasterServerList;
                    buttonText = _isRefreshingMasterServerList ? ( "Refreshing." + new string('.', (int) ((Time.time * 2) % 3)) ) : "Refresh servers";
					buttonAction = async () => await RefreshMasterServersButtonPressed();
                }
			}

            if (GUIUtils.ButtonWithCalculatedSize(buttonText, 80, 30))
				buttonAction();

            GUI.enabled = true;

		}

		async Task RefreshMasterServersButtonPressed()
		{
			_isRefreshingMasterServerList = true;
			_serversFromMasterServer = new List<ServerInfo>();

			try
			{
				_serversFromMasterServer = await MasterServerClient.Instance.GetAllServers();
			}
			catch (System.Exception ex)
			{
				Debug.LogException(ex);

				if (_masterServerErrorMessageBox != null)
					_masterServerErrorMessageBox.DestroyWindow();
				_masterServerErrorMessageBox = MessageBox.Show("Error refreshing master server", ex.ToString());
			}

			_isRefreshingMasterServerList = false;
		}

		void ConnectDirectly()
		{
			this.Connect(m_ipStr, ushort.Parse(m_portStr));
		}

		void ConnectFromDiscovery(Mirror.NetworkDiscovery.DiscoveryInfo info)
		{
			this.Connect(info.EndPoint.Address.ToString(), ushort.Parse( info.KeyValuePairs[Mirror.NetworkDiscovery.kPortKey] ));
		}

		void ConnectToServerFromMasterServer(ServerInfo serverInfo)
		{
			Connect(serverInfo.IP, (ushort) serverInfo.Port);
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
