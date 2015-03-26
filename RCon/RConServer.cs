using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Authentication;
using Newtonsoft.Json.Linq;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Facepunch.RCon
{
    public class RConServer : IDisposable
    {
        private readonly WebSocketServer _socketServer;

        private readonly Dictionary<String, RConSession> _sessions
            = new Dictionary<string, RConSession>(); 

        public delegate bool VerifyCredentialsPredicate(RConCredentials creds);
        public event VerifyCredentialsPredicate VerifyCredentials;

        public delegate String ExecuteCommandDelegate(RConCredentials creds, String command);
        public event ExecuteCommandDelegate ExecuteCommand;

        internal event UnityEngine.Application.LogCallback BroadcastedLog;

        public bool IsListening { get { return _socketServer.IsListening; } }

        public Logger Log { get { return _socketServer.Log; } }

        public TimeSpan SessionTimeout { get; set; }

        public RConServer(int port)
        {
            _socketServer = new WebSocketServer(port);
            _socketServer.AddWebSocketService("/rcon", () => new RConService(this));
            SessionTimeout = TimeSpan.FromDays(1d);
        }

        public void BroadcastLog(String condition, String stackTrace, LogType type)
        {
            if (BroadcastedLog != null) {
                BroadcastedLog(condition, stackTrace, type);
            }
        }

        internal RConSession TryCreateSession(RConCredentials creds)
        {
            if (VerifyCredentials == null || !VerifyCredentials(creds)) {
                throw new AuthenticationException("Invalid credentials");
            }

            var session = new RConSession(creds, SessionTimeout);

            if (_sessions.ContainsKey(creds.Name)) {
                _sessions[creds.Name] = session;
            } else {
                _sessions.Add(creds.Name, session);
            }

            return session;
        }

        internal RConSession TryGetSession(IPAddress address, JObject sessionData)
        {
            var name = (String) sessionData["name"];
            if (name == null || !_sessions.ContainsKey(name)) return null;

            var session = _sessions[name];
            return session.Verify(address, sessionData) ? session : null;
        }

        internal String ExecuteCommandInternal(RConCredentials creds, String command)
        {
            return ExecuteCommand != null ? ExecuteCommand(creds, command) : "";
        }

        public void Start()
        {
            Debug.LogFormat("[rcon] Starting rcon listener on port {0}", _socketServer.Port);

            try {
                _socketServer.Start();
            } catch (Exception e) {
                Debug.LogErrorFormat("[rcon] Unable to start listener: {0}", e);
            }
        }

        public void Stop()
        {
            if (!_socketServer.IsListening) return;

            Debug.Log("[rcon] Stopping rcon listener");
            _socketServer.Stop();
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
