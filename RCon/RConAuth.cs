using System;
using Newtonsoft.Json.Linq;

namespace Facepunch.RCon
{
    internal class RConAuth : BehaviorBase
    {
        public RConAuth(RConServer server) : base(server) { }

        protected override void OnMessage(JObject msg)
        {
            var action = (String) msg["action"];

            switch (action) {
                case "login":
                    HandleLogin(msg);
                    return;
            }
        }

        private void HandleLogin(JObject msg)
        {
            var name = (String) msg["name"];
            var password = (String) msg["password"];

            var creds = new RConCredentials(Context.UserEndPoint, name, password);
            var session = Server.TryCreateSession(creds);

            Send("login_success", session.ToJObject());
        }
    }
}
