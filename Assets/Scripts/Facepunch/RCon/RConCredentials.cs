using System;
using System.Net;

namespace Facepunch.RCon
{
    public sealed class RConCredentials : IEquatable<RConCredentials>
    {
        public IPAddress Address { get; private set; }
        public String Name { get; private set; }
        public String Password { get; private set; }

        public RConCredentials(IPAddress address, String name, String password)
        {
            Address = address;
            Name = name;
            Password = password;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as RConCredentials);
        }

        public bool Equals(RConCredentials other)
        {
            return other != null && Address.Equals(other.Address)
                && Name.Equals(other.Name) && Password.Equals(other.Password);
        }
    }
}