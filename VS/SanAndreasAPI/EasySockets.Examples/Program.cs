using System;

namespace MFatihMAR.EasySockets.Examples
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("usage: examples.exe <udpPeer / tcpServer / tcpClient> <port>");
                return;
            }

            if (args[0] != "udpPeer" && args[0] != "tcpServer" && args[0] != "tcpClient")
            {
                Console.WriteLine("usage: examples.exe <udpPeer / tcpServer / tcpClient> <port>");
                return;
            }

            if (!ushort.TryParse(args[1], out ushort port))
            {
                Console.WriteLine("usage: examples.exe <udpPeer / tcpServer / tcpClient> <port>");
                return;
            }

            if (args[0] == "udpPeer")
            {
                new UdpPeerExample().Run(port);
            }

            if (args[0] == "tcpServer")
            {
                new TcpServerExample().Run(port);
            }

            if (args[0] == "tcpClient")
            {
                new TcpClientExample().Run(port);
            }
        }
    }
}
