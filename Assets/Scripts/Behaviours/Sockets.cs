using MFatihMAR.EasySockets.Examples;
using SanAndreasAPI;
using System.Diagnostics;
using System.IO;
using System.Net;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Sockets : MonoBehaviour
{
    private static TcpClientExample client;

    private static bool available = false,
                        startCApp;

    // Use this for initialization
    private void Awake()
    {
#if UNITY_EDITOR
        startCApp = true;
#else
        var p = new FluentCommandLineParser();

        p.Setup<bool>('c', "console").Callback(x => startCApp = x);

        p.Parse(Environment.GetCommandLineArgs());
#endif

        string consoleApp = Path.Combine(Application.streamingAssetsPath, "SanAndreasConsole.exe");

        //Debug.LogFormat("Exists Console App: {0}", File.Exists(consoleApp));

        if (startCApp && File.Exists(consoleApp))
        {
            Process.Start(consoleApp);

            client = TcpClientExample.Init(true, "");
            client.Run(IPAddress.Loopback, Consts.TcpPort);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        //if (client == null)
        //    Debug.LogWarning("Null client!");
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