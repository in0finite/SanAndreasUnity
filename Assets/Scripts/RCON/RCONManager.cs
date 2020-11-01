﻿using Rcon;
using Rcon.Events;
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
        private static String password = "super_secret_password";
        private static int portNumber = 25575;

        private static BlockingCollection<String> mainToSec = new BlockingCollection<String>(1);
        private static BlockingCollection<String> secToMain = new BlockingCollection<String>(1);

        // Object used to pass the requested command string and an awaited callback (promise)
        class RCONCommandPromise
        {
            public RCONCommandPromise(ClientSentCommandEventArgs _command, TaskCompletionSource<String> _promise)
            {
                command = _command;
                promise = _promise;
            }
            public ClientSentCommandEventArgs command;
            public TaskCompletionSource<String> promise;
        }

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
            workerInstance.ReportProgress(0); //Report our progress to the main thread

            secToMain.Add(e.Command); // Pass the command to the main thread

            String commandResult = "Command didn't process correctly"; // default value

            try
            {
                // TODO add a timeout case
                commandResult = mainToSec.Take();
            }
            catch (InvalidOperationException) { }

            return commandResult;
        }
        #endregion

        // Runs in main thread
        private static void worker_progressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Console.WriteLine("Main thread command interpreted {0}", c.Command);

            String command = "unknown";
            try
            {
                // TODO add a timeout case ?
                command = secToMain.Take();
            }
            catch (InvalidOperationException) { }

            mainToSec.Add(CommandInterpreter.Interpret(command));
        }
    }
}