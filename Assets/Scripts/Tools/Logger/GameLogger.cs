using Fclp;
using HtmlAgilityPack;
using SanAndreasAPI;
using SanAndreasUnity.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class GameLogger : MonoBehaviour 
{
    public bool m_handleLog = true;

    private static StringBuilder consoleLog = new StringBuilder();
    private static string logPath = "";

    private static int DebugStop, MessagesSended;
    private static bool printHtml;

    private List<string> lines;

    public int maxLineWidth = 80;

    private void Awake()
    {
        if (m_handleLog)
        {
            Application.logMessageReceived += PrintDebug;
            Debug.Log("Debug log handled!");
        }
    }

    private void OnDisable()
    {
        if (m_handleLog)
            Application.logMessageReceived -= PrintDebug;
    }

	// Use this for initialization
	private void Start()
    {
        string[] args = Environment.GetCommandLineArgs();
        var p = new FluentCommandLineParser();

        if (args.Any(x => x.Contains("-h") || x.Contains("-handlelog")))
            p.Setup<bool>('h', "handlelog").Callback(x => m_handleLog = x);

        p.Setup<int>('s', "stopdebug").Callback(x => DebugStop = x);
        p.Setup<bool>("html").Callback(x => printHtml = x);

        p.Parse(args);
    }
	
	// Update is called once per frame
	void Update () 
    {
		
	}

    private void PrintDebug(string logString, string stackTrace, LogType type)
    {
        if (DebugStop > 0 && MessagesSended >= DebugStop)
        {
            Application.logMessageReceived -= PrintDebug;
            return;
        }
        ++MessagesSended;

        ConsoleLog log = new ConsoleLog(logString, stackTrace, type);
        consoleLog.AppendLine(log.DetailedMessage());
        Sockets.SendLog(log);

        Print(type.ToString() + ": " + logString);
        string[] array = stackTrace.Split(new char[]
        {
            '\n'
        });
        for (int i = 0; i < array.Length; i++)
        {
            string str = array[i];
            Print("\t" + str);
        }
    }

    public void Print(string line)
    {
        string[] array = line.Split(new char[]
        {
            '\n'
        });
        string[] array2 = array;
        for (int i = 0; i < array2.Length; i++)
        {
            string text = array2[i];
            string text2 = text;
            while (text2.Length > maxLineWidth)
            {
                int num = text2.LastIndexOf(' ', maxLineWidth);
                if (num >= maxLineWidth / 2)
                {
                    lines.Add(text2.Substring(0, num));
                    text2 = text2.Substring(num + 1);
                }
                else
                {
                    lines.Add(text2.Substring(0, maxLineWidth));
                    text2 = text2.Substring(maxLineWidth + 1);
                }
            }
            lines.Add(text2);
        }
    }

    private void OnApplicationQuit()
    {
        if (m_handleLog)
        {
            if (string.IsNullOrEmpty(logPath))
                logPath = Path.Combine(Application.streamingAssetsPath, "logs", string.Format("latest.{0}", printHtml ? "html" : "log"));

            string dir = Path.GetDirectoryName(logPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (File.Exists(logPath))
            {
                string oldName = Path.Combine(Application.streamingAssetsPath, "logs", string.Format("debug_{0}.{1}", DateTime.Now.ToUnixTimestamp(), printHtml ? "html" : "log"));
                File.Move(logPath, oldName);

                string zipFile = Path.Combine(Application.streamingAssetsPath, "logs", DateTime.Now.ToString("yyyy-MM-dd"));

                if (File.Exists(string.Concat(zipFile, ".gz")))
                    zipFile += string.Format("-{0}", Directory.GetFiles(Path.Combine(Application.streamingAssetsPath, "logs"), string.Format("{0}*", DateTime.Now.ToString("yyyy-MM-dd")), SearchOption.AllDirectories).Length - 1);

                File.Delete(oldName);

                F.CompressFile(oldName, zipFile);
            }

            File.WriteAllText(logPath, printHtml ? GenerateHTML() : consoleLog.ToString());
        }
    }

    private string GenerateHTML(bool indent = false)
    {
        var doc = new HtmlDocument();
        var node = HtmlNode.CreateNode("<html><head></head><body></body></html>");

        StringBuilder sb = new StringBuilder();
        string[] lines = consoleLog.ToString().Split('\n');
        int i = 0;
        foreach (string str in lines)
        {
            HtmlNode div = doc.CreateElement("div");
            div.SetAttributeValue("style", GetHTMLColor(str));

            HtmlNode span = doc.CreateElement("span");
            sb.AppendLine(string.Format("Message #{0}", i));
            sb.AppendLine();

            span.InnerHtml = sb.ToString().OptimizeHTML();
            sb.Clear();

            span.SetAttributeValue("style", "color:black!important;");
            div.AppendChild(span);

            div.InnerHtml += str.OptimizeHTML();

            node.AppendChild(div);

            if (indent)
                node.InnerHtml += "[br][hr][br]";
            else
                node.InnerHtml += "\n<hr>\n";

            ++i;
        }

        doc.DocumentNode.AppendChild(node);

        if (indent)
            return doc.DocumentNode.IndentHtml();

        return doc.DocumentNode.OuterHtml.Nl2Br();
    }

    private string GetHTMLColor(string str)
    {
        if (str.Contains("/Assert"))
            return "color:darkcyan;";
        else if (str.Contains("/Exception"))
            return "color:darkred;";
        else if (str.Contains("/Error"))
            return "color:red;";
        else if (str.Contains("/Log"))
            return "";
        else if (str.Contains("/Warning"))
            return "color:darkgoldenrod;";
        else
            return "";
    }
}
