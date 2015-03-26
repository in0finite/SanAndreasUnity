using System;
using System.Security.Authentication;
using Newtonsoft.Json.Linq;
using UnityEngine;
using WebSocketSharp;

namespace Facepunch.RCon
{
    internal class RConService : ServiceBase
    {
        public RConService(RConServer server) : base(server)
        {
            Server.BroadcastedLog += BroadcastLog;
        }

        private void BroadcastLog(String condition, String stackTrace, LogType type)
        {
            Send("log", new JObject {
                { "type", type.ToString().ToLower() },
                { "message", condition }
            });
        }

        protected override void OnMessage(String action, JToken data)
        {
            switch (action) {
                case "auth": {
                    var name = (String) data["name"];
                    var password = (String) data["password"];

                    var creds = new RConCredentials(Context.UserEndPoint.Address, name, password);
                    var session = Server.TryCreateSession(creds);

                    Send("auth_response", session.ToJObject());
                    return;
                }
                case "exec": {
                    var session = Server.TryGetSession(Context.UserEndPoint.Address, data["session"] as JObject);
                    if (session == null) throw new AuthenticationException("Invalid session");

                    var response = Server.ExecuteCommandInternal(session.Credentials, (String) data["command"]);

                    Send("exec_response", response);
                    return;
                }
            }
        }

        protected override void OnClose(CloseEventArgs e)
        {
            Server.BroadcastedLog -= BroadcastLog;
            base.OnClose(e);
        }
    }
}
