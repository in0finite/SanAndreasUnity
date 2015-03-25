using System;
using System.Security.Principal;
using WebSocketSharp;
using WebSocketSharp.Net;
using WebSocketSharp.Server;

namespace Facepunch.RemoteConsole
{
    public class RemoteConsoleServer : IDisposable
    {
        private readonly WebSocketServer _socketServer;

        public delegate NetworkCredential ResolveCredentialsHandler(IIdentity identity);
        public event ResolveCredentialsHandler ResolveCredentials;

        public bool IsListening { get { return _socketServer.IsListening; } }

        public Logger Log { get { return _socketServer.Log; } }

        public RemoteConsoleServer(int port)
        {
            _socketServer = new WebSocketServer(port) {
                Realm = "Facepunch Remote Console",
                AuthenticationSchemes = AuthenticationSchemes.Basic,
                UserCredentialsFinder = ident => ResolveCredentials == null ? null : ResolveCredentials(ident)
            };
        }

        public void Start()
        {
            _socketServer.Start();
        }

        public void Stop()
        {
            _socketServer.Stop();
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
