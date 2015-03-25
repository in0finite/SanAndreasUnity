using System;
using System.Net;
using System.Security.Cryptography;

namespace Facepunch.RCon
{
    public class RConSession
    {
        private static readonly RandomNumberGenerator _sRng = RandomNumberGenerator.Create();

        public EndPoint EndPoint { get; private set; }
        public RConCredentials Credentials { get; private set; }

        public DateTime StartedTime { get; private set; }
        public DateTime LastMessageTime { get; private set; }
        public TimeSpan SinceLastMessage { get { return DateTime.Now - LastMessageTime; } }

        public String Secret { get; private set; }

        internal RConSession(EndPoint endPoint, RConCredentials creds)
        {
            EndPoint = endPoint;
            Credentials = creds;
            StartedTime = LastMessageTime = DateTime.Now;

            var bytes = new byte[32];
            _sRng.GetBytes(bytes);

            Secret = Convert.ToBase64String(bytes);
        }
    }
}
