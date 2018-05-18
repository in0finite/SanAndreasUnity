using System;

namespace Facepunch.Networking
{
    public enum NetStatus
    {
        NotRunning = 0,
        Starting = 1,
        Running = 2,
        ShutdownRequested = 3,
    }

    public enum DeliveryMethod
    {
        Unknown = 0,
        Unreliable = 1,
        UnreliableSequenced = 2,
        ReliableUnordered = 34,
        ReliableSequenced = 35,
        ReliableOrdered = 67,
    }

    [Flags]
    public enum Domain
    {
        None = 0,
#if CLIENT
        Client = 1,
#endif
        Server = 2,

        Shared = Server
#if CLIENT
            | Client

#endif
    }
}