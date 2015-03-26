using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Facepunch.RCon
{
    public abstract class BehaviorBase : WebSocketBehavior
    {
        protected RConServer Server { get; private set; }

        protected BehaviorBase(RConServer server)
        {
            Server = server;
        }

        public void Send(String type, JObject obj)
        {
            Send(new JObject { { "type", type }, { "data", obj } }.ToString(Formatting.None));
        }

        public JObject FormatError(Exception e)
        {
            return new JObject {
                { "type", e.GetType().FullName },
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

        protected override void OnClose(CloseEventArgs e)
        {
            Debug.LogFormat("[rcon] Closed connection: {0}", Context.UserEndPoint);

            base.OnOpen();
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            Debug.LogFormat("[rcon] {0}", e.Type);

            if (e.Type != Opcode.Text) return;

            try {
                var obj = JObject.Parse(e.Data);
                OnMessage(obj);
            } catch (Exception ex) {
                Send("error", FormatError(ex));
            }
        }

        protected virtual void OnMessage(JObject msg) { }
    }
}
