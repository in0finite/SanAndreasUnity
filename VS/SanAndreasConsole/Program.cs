using MFatihMAR.EasySockets.Examples;
using SanAndreasAPI;
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;

namespace SanAndreasConsole
{
    internal class Program
    {
        //private static SocketClient controllerClient; // = new SocketClient(IPAddress.Loopback, SocketServer.DefPort, SocketType.Stream, ProtocolType.IPv4, 1000, WriteReceived());
        private static TcpServerExample controllerServer; // = new SocketServer(new SocketPermission(NetworkAccess.Accept, TransportType.Tcp, "", SocketPermission.AllPorts), IPAddress.Loopback, SocketServer.DefPort, SocketType.Stream, ProtocolType.Tcp, true);

        private static BackgroundWorker workerObject;

        private static void Main(string[] args)
        {
            /*workerObject = new BackgroundWorker() { WorkerSupportsCancellation = true };
            workerObject.DoWork += (s, ev) =>
            {
                controllerClient.DoConnection();
            };*/

            //controllerClient = new SocketClient(SocketExtensions.GetLocalIPAddress(), SocketServer.DefPort, DataReceived());
            //controllerServer = new SocketServer(new SocketPermission(NetworkAccess.Accept, TransportType.Tcp, "", SocketPermission.AllPorts), SocketExtensions.GetLocalIPAddress(), SocketServer.DefPort, SocketType.Stream, ProtocolType.Tcp, true);

            //controllerServer.StartServer();    //First, we make the socket server connection
            //workerObject.RunWorkerAsync();

            controllerServer.Init();
            controllerServer.SetOnData(DataReceived());

            Console.Read();
        }

        private static Action<IPEndPoint, byte[]> DataReceived()
        {
            return (ip, d) =>
            {
                //controllerServer._OnData(ip, d);

                object o = d.Deserialize<object>();

                if (o is ConsoleLog)
                {
                    ConsoleLog v = (ConsoleLog)o;

                    Console.WriteLine(v);
                }
            };
        }
    }
}