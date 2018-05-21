using MFatihMAR.EasySockets.Examples;
using SanAndreasAPI;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows.Forms;

namespace SanAndreasConsole
{
    internal class Program
    {
        private static TcpServerExample controllerServer;

        private static string MyPath
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
        }

        private static void Main(string[] args)
        {
            controllerServer = TcpServerExample.Init(false, Path.Combine(MyPath, string.Format("debug_{0}.log", DateTimeOffset.UtcNow.ToUnixTimeSeconds())));
            controllerServer.SetOnData(DataReceived());
            controllerServer.Run(Consts.TcpPort);

            Application.ApplicationExit += (a, b) =>
            {
                controllerServer.Stop();
            };

            Console.Read();
        }

        private static Action<IPEndPoint, byte[]> DataReceived()
        {
            return (ip, d) =>
            {
                //controllerServer._OnData(ip, d);

                object o = d.Deserialize<object>();

                if (o is ConsoleLog)
                    controllerServer.logger.SmartLog((ConsoleLog)o);
            };
        }
    }
}