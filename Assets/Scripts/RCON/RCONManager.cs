using Rcon;
using Rcon.Events;
using SanAndreasUnity.Utilities;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Threading.Tasks;
using UnityEngine;

namespace SanAndreasUnity.RCON
{
    public class RCONManager : MonoBehaviour
    {
        // Todo : set these from config
        private static String password;
        private static int portNumber;

        // Objects used to pass commands and responses between threads
        private static BlockingCollection<String> mainToSec = new BlockingCollection<String>(1);
        private static BlockingCollection<String> secToMain = new BlockingCollection<String>(1);

        private static BackgroundWorker workerInstance = null;

        public static void StartServer()
        {
            password = Config.Get<string>("RCON_password");
            portNumber = Config.Get<int>("RCON_port");

            if (workerInstance != null)
                return;

            workerInstance = new BackgroundWorker();

            workerInstance.DoWork += new DoWorkEventHandler( worker_doWork );
            workerInstance.ProgressChanged += new ProgressChangedEventHandler( worker_progressChanged );
            workerInstance.WorkerReportsProgress = true;
            // workerInstance.WorkerSupportsCancellation = true;

            workerInstance.RunWorkerAsync(); // Call the background worker
        }

        #region Code that runs in the RCON Server Thread
        private static void worker_doWork(object sender, DoWorkEventArgs e)
        {
            using (RconServer server = new RconServer(password, portNumber))
            {
                server.OnClientCommandReceived += Server_OnClientCommandReceived;
                server.OnClientConnected += Server_OnClientConnected;
                server.OnClientAuthenticated += Server_OnClientAuthenticated;
                server.OnClientDisconnected += Server_OnClientDisconnected;
                server.Start();
            }
        }
        static void Server_OnClientAuthenticated(object sender, ClientAuthenticatedEventArgs e)
        {
            // Console.WriteLine("{0} authenticated", e.Client.Client.LocalEndPoint);
        }
        static void Server_OnClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            // Console.WriteLine("{0} disconnected", e.EndPoint);
        }
        static void Server_OnClientConnected(object sender, ClientConnectedEventArgs e)
        {
            // Console.WriteLine("{0} connected", e.Client.Client.LocalEndPoint);
        }
        static string Server_OnClientCommandReceived(object sender, ClientSentCommandEventArgs e)
        {
            secToMain.Add(e.Command); // Pass the command to the main thread

            workerInstance.ReportProgress(0); //Report our progress to the main thread

            String commandResult = "Command didn't process correctly"; // default value

            commandResult = mainToSec.Take();

            return commandResult;
        }
        #endregion

        // Runs in main thread
        private static void worker_progressChanged(object sender, ProgressChangedEventArgs e)
        {
            String command = "unknown";

            command = secToMain.Take();

            mainToSec.Add(CommandInterpreter.Interpret(command));
        }
    }
}