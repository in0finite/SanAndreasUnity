using Newtonsoft.Json.Linq;

namespace Facepunch.RCon
{
    internal class RConAuth : BehaviorBase
    {
        protected override void OnMessage(JObject msg)
        {
            var name = msg["name"];
            var pass = msg["password"];
        }
    }
}
