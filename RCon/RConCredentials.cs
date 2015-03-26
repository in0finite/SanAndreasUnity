using System;
using System.Net;

namespace Facepunch.RCon
{
    public sealed class RConCredentials
    {
        public IPEndPoint EndPoint { get; private set; }
        public String Name { get; private set; }
        public String Password { get; private set; }

        public RConCredentials(IPEndPoint endPoint, String name, String password)
        {
            EndPoint = endPoint;
            Name = name;
            Password = password;
        }
    }
}
