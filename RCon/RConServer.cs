using System;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Facepunch.RCon
{
    public class RConServer : IDisposable
    {
        private readonly WebSocketServer _socketServer;

        public delegate bool VerifyCredentialsPredicate(RConCredentials creds);
        public event VerifyCredentialsPredicate VerifyCredentials;

        public bool IsListening { get { return _socketServer.IsListening; } }

        public Logger Log { get { return _socketServer.Log; } }

        public RConServer(int port)
        {
            _socketServer = new WebSocketServer(port);
            _socketServer.AddWebSocketService<RConAuth>("/auth", x => x.Init(this));
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
