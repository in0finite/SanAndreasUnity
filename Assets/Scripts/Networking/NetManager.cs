using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Net
{
	
	public class NetManager : MonoBehaviour
	{

		public	static	int	defaultListenPortNumber { get { return 7777; } }

        public static int listenPortNumber => Transport.activeTransport switch
        {
            TelepathyTransport telepathyTransport => telepathyTransport.port,
            kcp2k.KcpTransport kcpTransport => kcpTransport.Port,
            _ => throw new NotSupportedException("Can not obtain port number from current transport"),
        };

        public static bool dontListen { get { return NetworkServer.dontListen; } set { NetworkServer.dontListen = value; } }

		public static int maxNumPlayers { get => NetworkManager.singleton.maxConnections; set { NetworkManager.singleton.maxConnections = value; } }

		public static int numConnections => NetworkServer.connections.Count;

		public	static	string	onlineScene {
			get {
				return NetworkManager.singleton.onlineScene;
			}
			set {
				NetworkManager.singleton.onlineScene = value;
			}
		}

		public static NetManager Instance { get; private set; }

		NetworkClientStatus m_lastClientStatus = NetworkClientStatus.Disconnected;
		public event System.Action onClientStatusChanged = delegate {};

		private NetworkServerStatus m_lastServerStatus = NetworkServerStatus.Stopped;
		public event System.Action onServerStatusChanged = delegate {};


		private static readonly IReadOnlyDictionary<uint, NetworkIdentity> s_emptySpawnedDictionary =
			new Dictionary<uint, NetworkIdentity>();
		private static IReadOnlyDictionary<uint, NetworkIdentity> SpawnedDictionary
		{
			get
			{
				if (NetworkServer.active)
				{
					return NetworkServer.spawned;
				}
				else if (NetworkClient.active)
				{
					return NetworkClient.spawned;
				}

				return s_emptySpawnedDictionary;
			}
		}

		public static int NumSpawnedNetworkObjects => SpawnedDictionary.Count;

		public static double NetworkTime => Mirror.NetworkTime.time;



		NetManager ()
		{
			// assign implementation in NetUtils
			// do this in ctor, because it may be too late in Awake() - server can theoretically start before our Awake() is called
			NetUtils.IsServerImpl = () => NetStatus.IsServer;
		}


		void Awake()
		{
			if (null == Instance)
				Instance = this;
		}

		void Update()
		{

			NetworkClientStatus clientStatusNow = NetStatus.clientStatus;
			if (clientStatusNow != m_lastClientStatus)
			{
				m_lastClientStatus = clientStatusNow;
				F.InvokeEventExceptionSafe(this.onClientStatusChanged);
			}

			NetworkServerStatus serverStatusNow = NetStatus.serverStatus;
			if (serverStatusNow != m_lastServerStatus)
			{
				m_lastServerStatus = serverStatusNow;
				F.InvokeEventExceptionSafe(this.onServerStatusChanged);
			}

		}


		public static void StartServer(ushort portNumber, string scene, ushort maxNumPlayers, bool bIsDedicated, bool bDontListen)
		{
			// first start a server, and then change scene

			NetManager.onlineScene = scene;
			NetManager.dontListen = bDontListen;
			NetManager.maxNumPlayers = maxNumPlayers;
			if (bIsDedicated)
				NetManager.StartServer(portNumber);
			else
				NetManager.StartHost(portNumber);

			//NetManager.ChangeScene(scene);

		}

		private static void DoErrorChecksBeforeStartingServer(int portNumber)
		{
			CheckIfNetworkIsStarted ();
			CheckIfPortIsValid (portNumber);
			CheckIfOnlineSceneIsAssigned ();
			SetupNetworkManger( "", portNumber );
		}

		public	static	void	StartServer( int portNumber )
		{

			DoErrorChecksBeforeStartingServer(portNumber);
			NetworkManager.singleton.StartServer ();

		}

		public	static	void	StartHost( int portNumber )
		{

			DoErrorChecksBeforeStartingServer(portNumber);
			NetworkManager.singleton.StartHost ();

		}

		public	static	void	StopServer() {

			NetworkManager.singleton.StopServer ();

		}

		public	static	void	StopHost() {

			NetworkManager.singleton.StopHost ();

		}

		public	static	void	StartClient( string ip, int serverPortNumber ) {

			CheckIfNetworkIsStarted ();
			CheckIfIPAddressIsValid (ip);
			CheckIfPortIsValid (serverPortNumber);
			SetupNetworkManger( ip, serverPortNumber );
			NetworkManager.singleton.StartClient ();

		}

		public	static	void	StopClient() {

			NetworkManager.singleton.StopClient ();

		}

		/// <summary>
		/// Stops both server and client.
		/// </summary>
		public	static	void	StopNetwork() {

			//	NetworkManager.singleton.StopHost ();
			NetworkManager.singleton.StopServer ();
			NetworkManager.singleton.StopClient ();

		}


		public	static	void	CheckIfServerIsStarted() {

			if (NetStatus.IsServerStarted)
				throw new System.Exception ("Server already started");
			
		}

		public	static	void	CheckIfClientIsStarted() {

			if (!NetStatus.IsClientDisconnected ())
				throw new System.Exception ("Client already started");

		}

		public	static	void	CheckIfNetworkIsStarted() {

			CheckIfServerIsStarted ();
			CheckIfClientIsStarted ();

		}

		public	static	void	CheckIfPortIsValid( int portNumber ) {

			if (portNumber < 1 || portNumber > 65535)
				throw new System.ArgumentOutOfRangeException ( "portNumber", "Invalid port number");

		}

		private	static	void	CheckIfIPAddressIsValid( string ip ) {

			if (string.IsNullOrEmpty (ip))
				throw new System.ArgumentException ("IP address empty");

			//	System.Net.IPAddress.Parse ();

		}

		private	static	void	CheckIfOnlineSceneIsAssigned() {

			// we won't use scene management from NetworkManager
			//	if (string.IsNullOrEmpty (NetManager.onlineScene))
			//		throw new System.Exception ("Online scene is not assigned");

		}


		private	static	void	SetupNetworkManger( string ip, int port ) {

			NetworkManager.singleton.networkAddress = ip;

            switch (Transport.activeTransport)
            {
				case TelepathyTransport telepathyTransport:
					telepathyTransport.port = (ushort)port;
					break;
				case kcp2k.KcpTransport kcp2kTransport:
					kcp2kTransport.Port = (ushort)port;
					break;
                default:
					throw new NotSupportedException("Can not assign port number to current transport");
            }

		}


		public static void AddSpawnPosition(Transform tr)
		{
			NetworkManager.startPositions.Add(tr);
		}

		public static Transform[] SpawnPositions { get { return NetworkManager.startPositions.ToArray(); } }

		public static void Spawn(GameObject go)
		{
			NetStatus.ThrowIfNotOnServer();

			NetworkServer.Spawn(go);
		}

		public static void AssignAuthority(GameObject go, Player player)
		{
			NetStatus.ThrowIfNotOnServer();

			var netIdentity = go.GetComponentOrThrow<NetworkIdentity>();

			if (netIdentity.connectionToClient == player.connectionToClient)	// already has authority
				return;

			// first remove existing authority client
			if (netIdentity.connectionToClient != null)
				netIdentity.RemoveClientAuthority();

			// assign new authority client
			netIdentity.AssignClientAuthority(player.connectionToClient);

		}

		public static void RemoveAuthority(GameObject go)
		{
			NetStatus.ThrowIfNotOnServer();

			var netIdentity = go.GetComponentOrThrow<NetworkIdentity>();

			if (netIdentity.connectionToClient != null)
				netIdentity.RemoveClientAuthority();
			
		}

		public static void ChangeScene(string newScene)
		{
			NetworkManager.singleton.ServerChangeScene(newScene);
		}

		public static GameObject GetNetworkObjectById(uint netId)
		{
			if (!SpawnedDictionary.TryGetValue(netId, out var networkIdentity)) return null;
			return networkIdentity != null ? networkIdentity.gameObject : null;
		}

	}

}