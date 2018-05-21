using MFatihMAR.EasySockets.Examples;
using SanAndreasAPI;
using System.Diagnostics;
using System.IO;
using System.Net;
using UnityEngine;

#if !UNITY_EDITOR
using Fclp;
using System;
#endif

public class Sockets : MonoBehaviour
{
    private static TcpClientExample client;

    private static bool available = false;

    public bool m_startConsoleApp = true;

    // Fixed: We use this because is called before Awake (where += handleLog occurs)
    private void OnEnable()
    {
#if UNITY_EDITOR
        m_startConsoleApp = true;
#else
                var p = new FluentCommandLineParser();

                p.Setup<bool>('c', "console").Callback(x => m_startConsoleApp = x);

                p.Parse(Environment.GetCommandLineArgs());
#endif

        string consoleApp = Path.Combine(Application.streamingAssetsPath, "SanAndreasConsole.exe");

        //Debug.LogFormat("Exists Console App: {0}", File.Exists(consoleApp));

        if (m_startConsoleApp && File.Exists(consoleApp))
        {
            Process.Start(consoleApp);

            client = TcpClientExample.Init(true, "");
            client.Run(IPAddress.Loopback, Consts.TcpPort);
        }
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void OnDisable()
    {
        client.Disconnect();
    }

    // From console
    public static void SendLog(ConsoleLog log)
    {
        if (client != null)
            client.Send(log);
    }
}