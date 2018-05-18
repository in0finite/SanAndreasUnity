namespace Facepunch.Networking.Lidgren
{
    /// <summary>
    /// Lidgren networking provider implementation.
    /// </summary>
    [AutoInstall]
    public class LidgrenProvider : NetProvider
    {
        /// <summary>
        /// Creates a listening server at the specified port with the given
        /// connection limit and returns an object used to manage it.
        /// </summary>
        protected override ILocalServer OnCreateServer(int port, int maxConnections)
        {
            return new LocalServerImpl(port, maxConnections);
        }

        /// <summary>
        /// Connects to a remote server and returns an object used to manage
        /// the connection.
        /// </summary>
        protected override IRemoteServer OnConnect(string hostname, int port)
        {
            return new RemoteServerImpl(hostname, port);
        }
    }
}