using UnityEngine;
using Mirror;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Net
{
	
	public class NetManager : MonoBehaviour
	{

		public	static	int	defaultListenPortNumber { get { return 7777; } }

		public	static	int	listenPortNumber { get { return telepathyTransport.port; } }

		public static bool dontListen { get { return NetworkServer.dontListen; } set { NetworkServer.dontListen = value; } }

        public static TelepathyTransport telepathyTransport { get { return ((TelepathyTransport)Transport.activeTransport); } }

		public	static	string	onlineScene {
			get {
				return NetworkManager.singleton.onlineScene;
			}
			set {
				NetworkManager.singleton.onlineScene = value;
			}
		}



		NetManager ()
		{
			// assign implementation in NetUtils
			// do this in ctor, because it may be too late in Awake() - server can theoretically start before our Awake() is called
			NetUtils.IsServerImpl = () => NetStatus.IsServer;
		}


		public	static	void	StartServer( int portNumber ) {

			CheckIfNetworkIsStarted ();
			CheckIfPortIsValid (portNumber);
			CheckIfOnlineSceneIsAssigned ();
			SetupNetworkManger( "", portNumber );
			NetworkManager.singleton.StartServer ();

		}

		public	static	void	StartHost( int portNumber ) {

			CheckIfNetworkIsStarted ();
			CheckIfPortIsValid (portNumber);
			CheckIfOnlineSceneIsAssigned ();
			SetupNetworkManger( "", portNumber );
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
			telepathyTransport.port = (ushort) port;

		}


		public static void AddSpawnPosition(Transform tr)
		{
			NetworkManager.startPositions.Add(tr);
		}

		public static Transform[] SpawnPositions { get { return NetworkManager.startPositions.ToArray(); } }

		public static void Spawn(GameObject go)
		{
			NetworkServer.Spawn(go);
		}

		public static void AssignAuthority(GameObject go, Player player)
		{
			NetStatus.ThrowIfNotOnServer();

			var netIdentity = go.GetComponentOrThrow<NetworkIdentity>();

			if (netIdentity.clientAuthorityOwner == player.connectionToClient)	// already has authority
				return;

			// first remove existing authority client
			if (netIdentity.clientAuthorityOwner != null)
				netIdentity.RemoveClientAuthority(netIdentity.clientAuthorityOwner);

			// assign new authority client
			netIdentity.AssignClientAuthority(player.connectionToClient);

		}

		public static void RemoveAuthority(GameObject go)
		{
			NetStatus.ThrowIfNotOnServer();

			var netIdentity = go.GetComponentOrThrow<NetworkIdentity>();

			if (netIdentity.clientAuthorityOwner != null)
				netIdentity.RemoveClientAuthority(netIdentity.clientAuthorityOwner);
			
		}

		public static void ChangeScene(string newScene)
		{
			NetworkManager.singleton.ServerChangeScene(newScene);
		}

	}

}