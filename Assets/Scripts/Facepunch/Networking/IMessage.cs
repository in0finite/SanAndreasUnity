namespace ProtoBuf
{
    /// <summary>
    /// Root message types that may be sent over the network must implement this
    /// interface.
    /// </summary>
    public interface INetworkMessage : IMessage { }

    /// <summary>
    /// Root message types that may be saved or loaded must implement this interface.
    /// </summary>
    public interface IPersistenceMessage : IMessage { }

    // Dumb hack:

#if PROTOBUF

    package ProtoBuf;

    message MessageTableSchema
    {
        repeated MessageTableEntry Entries = 1;
    }

    message MessageTableEntry
    {
        required uint32 Ident = 1;
        required string TypeName = 2;
    }

#endif
}