using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Facepunch.RCon
{
    internal abstract class ServiceBase : WebSocketBehavior
    {
        protected RConServer Server { get; private set; }

        protected ServiceBase(RConServer server)
        {
            Server = server;
        }

        public void Send(String type, JToken data)
        {
            Send(new JObject {{"type", type}, {"data", data}}.ToString(Formatting.None));
        }

        public JObject FormatError(Exception e)
        {
            return new JObject {
                { "type", e.GetType().Name },
                { "message", e.Message }
            };
        }

        protected override void OnOpen()
        {
            Debug.LogFormat("[rcon] New connection: {0}", Context.UserEndPoint);

            base.OnOpen();
        }

        protected override void OnError(ErrorEventArgs e)
        {
            Debug.LogFormat("[rcon] {0}", e.Exception);
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            if (e.Type != Opcode.Text) return;

            try {
                var obj = JObject.Parse(e.Data);
                OnMessage((String) obj["type"], obj["data"]);
            } catch (Exception ex) {
                Send("error", FormatError(ex));
            }
        }

        protected virtual void OnMessage(String type, JToken data) { }
    }
}
