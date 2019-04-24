using Mirror;

namespace SanAndreasUnity.Net
{

	public enum NetworkClientStatus
	{
		Disconnected = 0,
		Connecting,
		Connected

	}

	public enum NetworkServerStatus
	{
		Started = 1,
		Starting = 2,
		Stopped = 3

	}

	public class NetStatus
	{


		private static bool IsNetworkClientConnecting {
			get {
				return NetworkClient.active && !NetworkClient.isConnected;
			}
		}

		public static NetworkClientStatus clientStatus {
			get {
				if (NetworkClient.isConnected)
					return NetworkClientStatus.Connected;

				if (IsNetworkClientConnecting)
					return NetworkClientStatus.Connecting;
				
				return NetworkClientStatus.Disconnected;
			}
		}

		public	static	NetworkServerStatus serverStatus {
			get {
				if (!NetworkServer.active)
					return NetworkServerStatus.Stopped;

				return NetworkServerStatus.Started;
			}
		}

		public static bool IsServerStarted => NetStatus.serverStatus == NetworkServerStatus.Started;
		
		/// <summary>
		/// Is server active ?
		/// </summary>
		public static bool IsServer => NetStatus.IsServerStarted;

		/// <summary>
		/// Is host active ?
		/// </summary>
		public	static	bool	IsHost() {

			if (!NetStatus.IsServer)
				return false;

			return NetworkServer.localClientActive;
		}

		public	static	bool	IsClientConnected() {

			return clientStatus == NetworkClientStatus.Connected;
		}

		public	static	bool	IsClientConnecting() {

			return clientStatus == NetworkClientStatus.Connecting;
		}

		public	static	bool	IsClientDisconnected() {

			return clientStatus == NetworkClientStatus.Disconnected;
		}

		/// <summary>
		/// Is client connected ?
		/// TODO: This method should be corrected to return: is client active.
		/// </summary>
		public	static	bool	IsClient() {
			return NetStatus.IsClientConnected();
		}

		public	static	bool	IsClientActive() {
			return ! NetStatus.IsClientDisconnected ();
		}


		/// <summary>
		/// Throws exception if server is not active.
		/// </summary>
		public static void ThrowIfNotOnServer()
		{
			if (!NetStatus.IsServer)
				throw new System.Exception("Not on a server");
		}

	}

}