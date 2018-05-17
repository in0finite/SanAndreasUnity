using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Security.Cryptography;

namespace Facepunch.RCon
{
    internal static class DateTimeEx
    {
        public static long ToUnixTimestamp(this DateTime time)
        {
            return (long)(time.Subtract(new DateTime(1970, 1, 1)).TotalSeconds * 1000);
        }
    }

    public class RConSession
    {
        private static readonly RandomNumberGenerator _sRng = RandomNumberGenerator.Create();

        public RConCredentials Credentials { get; private set; }
        public DateTime StartedTime { get; private set; }
        public DateTime LastMessageTime { get; private set; }
        public TimeSpan Timeout { get; private set; }

        public IPAddress Address { get { return Credentials.Address; } }
        public TimeSpan SinceLastMessage { get { return DateTime.UtcNow - LastMessageTime; } }

        public String Secret { get; private set; }

        internal RConSession(RConCredentials creds, TimeSpan timeout)
        {
            Credentials = creds;
            StartedTime = LastMessageTime = DateTime.UtcNow;
            Timeout = timeout;

            var bytes = new byte[32];
            _sRng.GetBytes(bytes);

            Secret = Convert.ToBase64String(bytes);
        }

        public bool Verify(IPAddress ip, JObject session)
        {
            if (session == null) return false;

            var name = (String)session["name"];
            var secret = (String)session["secret"];

            if (name == null || secret == null) return false;
            if (!ip.Equals(Address)) return false;

            if (!name.Equals(Credentials.Name)) return false;
            if (!secret.Equals(Secret)) return false;

            if (SinceLastMessage >= Timeout) return false;

            LastMessageTime = DateTime.UtcNow;
            return true;
        }

        public JObject ToJObject()
        {
            return new JObject {
                {"name", Credentials.Name},
                {"ip", Address.ToString()},
                {"time", StartedTime.ToUnixTimestamp()},
                {"secret", Secret}
            };
        }
    }
}