using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Facepunch.RCon
{
    public abstract class BehaviorBase : WebSocketBehavior
    {
        protected RConServer Server { get; private set; }

        public void Initialize(RConServer server)
        {
            Server = server;
        }

        protected virtual void OnInitialize() { }

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

        protected override void OnMessage(MessageEventArgs e)
        {
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
