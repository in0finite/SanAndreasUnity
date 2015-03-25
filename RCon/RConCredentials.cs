using System;

namespace Facepunch.RCon
{
    public sealed class RConCredentials
    {
        public readonly String Name;
        public readonly String Password;

        public RConCredentials(String name, String password)
        {
            Name = name;
            Password = password;
        }
    }
}
