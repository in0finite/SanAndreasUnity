using System;
using System.Net;

namespace Facepunch.Networking
{
    public enum ConnectionStatus
    {
        None = 0,
        InitiatedConnect = 1,
        ReceivedInitiation = 2,
        RespondedAwaitingApproval = 3,
        RespondedConnect = 4,
        Connected = 5,
        Disconnecting = 6,
        Disconnected = 7,
    }

    /// <summary>
    /// Interface for remote connections.
    /// </summary>
    public interface IRemote
    {
        IPEndPoint EndPoint { get; }

        ConnectionStatus ConnectionStatus { get; }

        float AverageRoundTripTime { get; }

        void Disconnect(String message);

        event ClientDisconnectEventHandler Disconnected;
    }
}
