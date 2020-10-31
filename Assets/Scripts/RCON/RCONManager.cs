using Rcon;
using Rcon.Events;
using System;
using System.ComponentModel;
using UnityEngine;

namespace SanAndreasUnity.RCON
{
    public class RCONManager : MonoBehaviour
    {
        private static BackgroundWorker workerInstance = null;

        public static void StartServer()
        {
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
            using (RconServer server = new RconServer("super_secret_password", 25575))
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
            Console.WriteLine("{0} authenticated", e.Client.Client.LocalEndPoint);
        }
        static void Server_OnClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            Console.WriteLine("{0} disconnected", e.EndPoint);
        }
        static void Server_OnClientConnected(object sender, ClientConnectedEventArgs e)
        {
            Console.WriteLine("{0} connected", e.Client.Client.LocalEndPoint);
        }
        static string Server_OnClientCommandReceived(object sender, ClientSentCommandEventArgs e)
        {
            workerInstance.ReportProgress(0, e); //Report our progress to the main thread
            return "response string goes here";
        }
        #endregion

        // Runs in main thread
        private static void worker_progressChanged(object sender, ProgressChangedEventArgs e)
        {
            ClientSentCommandEventArgs c = (ClientSentCommandEventArgs)e.UserState;
            Console.WriteLine("Main thread command interpreted {0}", c.Command);
        }
    }
}