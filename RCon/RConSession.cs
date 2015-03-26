using System;
using System.Net;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;

namespace Facepunch.RCon
{
    internal static class DateTimeEx
    {
        public static long ToUnixTimestamp(this DateTime time)
        {
            return (long) (time.Subtract(new DateTime(1970, 1, 1)).TotalSeconds * 1000);
        }
    }

    public class RConSession
    {
        private static readonly RandomNumberGenerator _sRng = RandomNumberGenerator.Create();

        public RConCredentials Credentials { get; private set; }
        public DateTime StartedTime { get; private set; }
        public DateTime LastMessageTime { get; private set; }

        public IPEndPoint EndPoint { get { return Credentials.EndPoint; } }
        public TimeSpan SinceLastMessage { get { return DateTime.UtcNow - LastMessageTime; } }

        public String Secret { get; private set; }

        internal RConSession(RConCredentials creds)
        {
            Credentials = creds;
            StartedTime = LastMessageTime = DateTime.UtcNow;

            var bytes = new byte[32];
            _sRng.GetBytes(bytes);

            Secret = Convert.ToBase64String(bytes);
        }

        public JObject ToJObject()
        {
            return new JObject {
                {"name", Credentials.Name},
                {"ip", EndPoint.Address.ToString()},
                {"time", StartedTime.ToUnixTimestamp()},
                {"secret", Secret}
            };
        }
    }
}
