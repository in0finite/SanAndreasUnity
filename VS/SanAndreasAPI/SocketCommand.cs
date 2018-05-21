using System;

namespace SanAndreasAPI
{
    [Serializable]
    public class SocketCommand
    {
        public SocketCommands Command;
        public SocketCommandData Metadata;

        private SocketCommand()
        { }

        public SocketCommand(SocketCommands cmd)
        {
            Command = cmd;
        }

        public SocketCommand(SocketCommands cmd, SocketCommandData met)
        {
            Command = cmd;
            Metadata = met;
        }
    }
}