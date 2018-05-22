using Fclp;
using Homans.Console;
using Homans.Containers;
using HtmlAgilityPack;
using SanAndreasAPI;
using SanAndreasUnity.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Console : MonoBehaviour
{
    private struct DirectCommand
    {
        public string command;

        public WeakReference instance;

        public MethodInfo method;

        public int parameterCount;

        public bool variableParamCount;
    }

    public KeyCode m_openKey = KeyCode.F2;
    public KeyCode m_closeKey = KeyCode.Comma;
    public bool m_handleLog = true;

    public delegate bool ParserCallback(string str, out object obj);

    private static Console instance;

    private CircularBuffer<string> lines;

    [SerializeField]
    private int linesOfHistory = 1000;

    public int maxLineWidth = 80;

    private CircularBuffer<string> commands;

    [SerializeField]
    private int commandHistory = 100;

    private Dictionary<string, DirectCommand> directCommands = new Dictionary<string, DirectCommand>();

    private Dictionary<Type, ParserCallback> parsers = new Dictionary<Type, ParserCallback>();

    private Component selectedComponent = null;

    // WIP: Maybe we will need to implement support for ConsoleNGUI
    private ConsoleGUI consoleGUI;

    private static StringBuilder consoleLog = new StringBuilder();
    private static string logPath = "";

    private static int DebugStop, MessagesSended;
    private static bool printHtml;

    public bool IsOpened
    {
        get
        {
            if (consoleGUI != null)
                return consoleGUI.isOpen;
            return false;
        }
    }

    public static Console Instance
    {
        get
        {
            return instance;
        }
    }

    public CircularBuffer<string> Lines
    {
        get
        {
            return lines;
        }
    }

    public CircularBuffer<string> Commands
    {
        get
        {
            return commands;
        }
    }

    private void OnDisable()
    {
        if (m_handleLog)
            Application.logMessageReceived -= PrintDebug;
    }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("only one instance is allowed to exist");
        }
        instance = this;

        if (m_handleLog)
            Application.logMessageReceived += PrintDebug;
        //Application.RegisterLogCallback(new Application.LogCallback(this.PrintDebug));

        lines = new CircularBuffer<string>(linesOfHistory, true);
        commands = new CircularBuffer<string>(commandHistory, true);
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        RegisterCommand("help", this, "Help");
        RegisterCommand("list", this, "List");
        RegisterCommand("listComponents", this, "ListComponents");
        RegisterCommand("printTree", this, "PrintTree");
        RegisterCommand("select", this, "Select");
        RegisterParser(typeof(int), new ParserCallback(parseInt));
        RegisterParser(typeof(float), new ParserCallback(parseFloat));
        RegisterParser(typeof(string), new ParserCallback(parseString));

        consoleGUI = GetComponent<ConsoleGUI>();
    }

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

    /*private void OnLevelWasLoaded(int id)
    {
        List<string> list = new List<string>();
        foreach (KeyValuePair<string, DirectCommand> current in this.directCommands)
        {
            if (!current.Value.instance.IsAlive || current.Value.instance.Target == null)
            {
                list.Add(current.Key);
            }
        }
        foreach (string current2 in list)
        {
            this.directCommands.Remove(current2);
        }
    }*/

    private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
    {
        List<string> list = new List<string>();
        foreach (KeyValuePair<string, DirectCommand> current in directCommands)
        {
            if (!current.Value.instance.IsAlive || current.Value.instance.Target == null)
            {
                list.Add(current.Key);
            }
        }
        foreach (string current2 in list)
        {
            directCommands.Remove(current2);
        }
    }

    public void RegisterCommand(string command, object instance, string methodName)
    {
        Type type = instance.GetType();
        MethodInfo method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (method == null)
        {
            throw new ArgumentException(string.Concat(new string[]
            {
                "Method(",
                methodName,
                ") does not exist on the given instance type(",
                type.ToString(),
                ")"
            }));
        }
        RegisterCommand(command, instance, method);
    }

    public void RegisterCommand(string command, object instance, MethodInfo method)
    {
        DirectCommand value = default(DirectCommand);
        value.command = command;
        value.instance = new WeakReference(instance, false);
        value.method = method;
        value.parameterCount = method.GetParameters().Length;
        value.variableParamCount = false;
        ParameterInfo[] parameters = method.GetParameters();
        for (int i = 0; i < parameters.Length; i++)
        {
            ParameterInfo parameterInfo = parameters[i];
            if (parameterInfo.GetCustomAttributes(typeof(ParamArrayAttribute), true).Length > 0)
            {
                value.variableParamCount = true;
                break;
            }
        }
        directCommands[command] = value;
    }

    public void RemoveCommand(string command)
    {
        if (directCommands.ContainsKey(command))
        {
            directCommands.Remove(command);
        }
    }

    public void RegisterParser(Type type, ParserCallback func)
    {
        parsers[type] = func;
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
                    lines.Enqueue(text2.Substring(0, num));
                    text2 = text2.Substring(num + 1);
                }
                else
                {
                    lines.Enqueue(text2.Substring(0, maxLineWidth));
                    text2 = text2.Substring(maxLineWidth + 1);
                }
            }
            lines.Enqueue(text2);
        }
    }

    public void Eval(string line)
    {
        commands.Enqueue(line);
        string text;
        string[] hierarchy;
        string componentName;
        string text2;
        string[] args;
        parseCommand(line, out text, out hierarchy, out componentName, out text2, out args);
        if (text != null)
        {
            DirectCommand directCommand;
            if (directCommands.TryGetValue(text, out directCommand))
            {
                if (directCommand.instance.IsAlive && directCommand.instance.Target != null)
                {
                    CheckParametersAndInvoke(args, directCommand.instance.Target, directCommand.method);
                }
                else
                {
                    Print("Instance has been removed. Removing command");
                    directCommands.Remove(text);
                }
            }
            else if (selectedComponent != null)
            {
                MethodInfo methodInfo;
                if (GetMethodOnComponent(text, selectedComponent, out methodInfo))
                {
                    if (methodInfo == null)
                    {
                        Print("Unkown Command! Type \"help\" for help.");
                    }
                    else
                    {
                        CheckParametersAndInvoke(args, selectedComponent, methodInfo);
                    }
                }
            }
            else
            {
                Print("Unkown Command! Type \"help\" for help.");
            }
        }
        else if (text2 != null)
        {
            InvokeMethodOnComponent(args, hierarchy, componentName, text2);
        }
        else
        {
            Print("Unkown Command! Type \"help\" for help.");
        }
    }

    private void InvokeMethodOnComponent(string[] args, string[] hierarchy, string[] ocf)
    {
        string text = "";
        for (int i = 0; i < hierarchy.Length; i++)
        {
            string str = hierarchy[i];
            text = text + "/" + str;
        }
        GameObject gameObject = GameObject.Find(text);
        if (gameObject == null)
        {
            Print("Unknown GameObject");
        }
        else
        {
            Component component = gameObject.GetComponent(ocf[1]);
            MethodInfo methodInfo;
            if (component == null)
            {
                Print("Unknown Component");
            }
            else if (GetMethodOnComponent(ocf[2], component, out methodInfo))
            {
                if (methodInfo == null)
                {
                    Print("Unknown Method");
                }
                else
                {
                    CheckParametersAndInvoke(args, component, methodInfo);
                }
            }
        }
    }

    private void InvokeMethodOnComponent(string[] args, string[] hierarchy, string componentName, string methodName)
    {
        string name = BuildGameObjectPath(hierarchy);
        GameObject gameObject = GameObject.Find(name);
        if (gameObject == null)
        {
            Print("Unknown GameObject");
        }
        else
        {
            Component component = gameObject.GetComponent(componentName);
            MethodInfo methodInfo;
            if (component == null)
            {
                Print("Unknown Component");
            }
            else if (GetMethodOnComponent(methodName, component, out methodInfo))
            {
                if (methodInfo == null)
                {
                    Print("Unknown Method");
                }
                else
                {
                    CheckParametersAndInvoke(args, component, methodInfo);
                }
            }
        }
    }

    private static string BuildGameObjectPath(string[] hierarchy)
    {
        string text = "";
        for (int i = 0; i < hierarchy.Length; i++)
        {
            string str = hierarchy[i];
            text = text + "/" + str;
        }
        return text;
    }

    private bool GetMethodOnComponent(string methodName, Component component, out MethodInfo method)
    {
        method = null;
        bool result;
        try
        {
            method = component.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }
        catch (AmbiguousMatchException)
        {
            Print("Overloaded Method Found. Unable to invoke");
            result = false;
            return result;
        }
        result = true;
        return result;
    }

    private void CheckParametersAndInvoke(string[] args, object instance, MethodInfo method)
    {
        ParameterInfo[] parameters = method.GetParameters();
        object[] array = new object[parameters.Length];
        int i = 0;
        while (i < parameters.Length)
        {
            ParameterInfo parameterInfo = parameters[i];
            bool flag = parameterInfo.GetCustomAttributes(typeof(ParamArrayAttribute), true).Length > 0;
            Type type = parameterInfo.ParameterType;
            if (flag)
            {
                type = type.GetElementType();
            }
            ParserCallback parserCallback;
            if (parsers.TryGetValue(type, out parserCallback))
            {
                if (flag)
                {
                    object[] array2 = (object[])Array.CreateInstance(type, args.Length - i);
                    for (int j = i; j < args.Length; j++)
                    {
                        object obj;
                        if (!parserCallback(args[j], out obj))
                        {
                            return;
                        }
                        array2[j - i] = obj;
                    }
                    array[i] = array2;
                }
                else
                {
                    if (i >= args.Length)
                    {
                        Print(string.Concat(new object[]
                        {
                            "Mismatched arguments: Expected ",
                            parameters.Length,
                            ", got ",
                            args.Length
                        }));
                        return;
                    }
                    if (!parserCallback(args[i], out array[i]))
                    {
                        return;
                    }
                }
                i++;
                continue;
            }
            Print(string.Concat(new object[]
            {
                "Invalid Parameter Type: Parameter ",
                i,
                " is a ",
                parameterInfo.ParameterType.ToString(),
                " but there is no parser for that type"
            }));
            return;
        }
        object obj2 = method.Invoke(instance, array);
        if (obj2 != null)
        {
            Instance.Print(obj2.ToString());
            return;
        }
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

    public static void parseCommand(string line, out string command, out string[] gameobjectPath, out string componentName, out string methodName, out string[] parameters)
    {
        if (line == null || line == "")
        {
            command = "";
            parameters = null;
            gameobjectPath = null;
            componentName = null;
            methodName = null;
        }
        else
        {
            List<string> list = new List<string>();
            string text = "";
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == '\\')
                {
                    if (i < line.Length - 1)
                    {
                        text += line[i + 1];
                        i++;
                    }
                }
                else if (line[i] == ' ')
                {
                    list.Add(text);
                    text = "";
                }
                else
                {
                    text += line[i];
                }
            }
            if (text != "")
            {
                list.Add(text);
            }
            if (list[0].Contains(".") || list[0].Contains("/"))
            {
                command = null;
                parseGameObjectString(list[0], out gameobjectPath, out componentName, out methodName);
                list.RemoveAt(0);
                parameters = list.ToArray();
            }
            else
            {
                command = list[0];
                list.RemoveAt(0);
                parameters = list.ToArray();
                gameobjectPath = null;
                componentName = null;
                methodName = null;
            }
        }
    }

    public static void parseGameObjectString(string line, out string[] gameobjectPath, out string componentName, out string methodName)
    {
        string[] array = line.Split(new char[]
        {
            '/'
        }, StringSplitOptions.RemoveEmptyEntries);
        string[] array2 = array[array.Length - 1].Split(new char[]
        {
            '.'
        });
        array[array.Length - 1] = array2[0];
        gameobjectPath = array;
        if (array2.Length >= 2)
        {
            componentName = array2[1];
        }
        else
        {
            componentName = null;
        }
        if (array2.Length >= 3)
        {
            methodName = array2[2];
        }
        else
        {
            methodName = null;
        }
    }

    public string[] GetGameobjectsAtPath(string path)
    {
        List<string> list = new List<string>();
        path = path.Replace("\\", "");
        string text = path;
        string value = "";
        if (path.Contains("/"))
        {
            text = path.Substring(0, path.LastIndexOf("/"));
            value = path.Substring(path.LastIndexOf("/") + 1);
        }
        if (text == null || text == "" || text == "/")
        {
            Transform[] array = (Transform[])UnityEngine.Object.FindObjectsOfType(typeof(Transform));
            Transform[] array2 = array;
            for (int i = 0; i < array2.Length; i++)
            {
                Transform transform = array2[i];
                if (transform.parent == null && transform.name.StartsWith(value))
                {
                    list.Add(transform.name);
                }
            }
        }
        else
        {
            GameObject gameObject = GameObject.Find(text);
            if (gameObject != null)
            {
                foreach (Transform transform2 in gameObject.transform)
                {
                    if (transform2.name.StartsWith(value))
                    {
                        list.Add(transform2.name);
                    }
                }
            }
        }
        return list.ToArray();
    }

    public string[] GetComponentsOfGameobject(string path)
    {
        path = path.Replace("\\", "");
        if (path.EndsWith("/"))
        {
            path = path.Substring(0, path.Length - 1);
        }
        List<string> list = new List<string>();
        string[] result;
        if (path == null || path == "" || path == "/")
        {
            result = list.ToArray();
        }
        else
        {
            string name = path;
            string value = "";
            if (path.Contains("."))
            {
                name = path.Substring(0, path.IndexOf("."));
                value = path.Substring(path.IndexOf(".") + 1);
            }
            GameObject gameObject = GameObject.Find(name);
            if (gameObject != null)
            {
                Component[] components = gameObject.GetComponents<Component>();
                Component[] array = components;
                for (int i = 0; i < array.Length; i++)
                {
                    Component component = array[i];
                    if (component.GetType().Name.StartsWith(value))
                    {
                        list.Add(component.GetType().Name);
                    }
                }
            }
            result = list.ToArray();
        }
        return result;
    }

    public string[] GetMethodsOfComponent(string path)
    {
        path = path.Replace("\\", "");
        List<string> list = new List<string>();
        string[] result;
        if (!path.Contains("."))
        {
            result = list.ToArray();
        }
        else
        {
            string name = path.Substring(0, path.IndexOf("."));
            string text = path.Substring(path.IndexOf(".") + 1);
            string type = text;
            string value = "";
            if (text.IndexOf(".") > 0)
            {
                type = text.Substring(0, text.IndexOf("."));
                value = text.Substring(text.IndexOf(".") + 1);
            }
            GameObject gameObject = GameObject.Find(name);
            if (gameObject == null)
            {
                result = list.ToArray();
            }
            else
            {
                Component component = gameObject.GetComponent(type);
                if (component == null)
                {
                    result = list.ToArray();
                }
                else
                {
                    MethodInfo[] methods = component.GetType().GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    MethodInfo[] array = methods;
                    for (int i = 0; i < array.Length; i++)
                    {
                        MethodInfo methodInfo = array[i];
                        if (methodInfo.Name.StartsWith(value))
                        {
                            list.Add(methodInfo.Name);
                        }
                    }
                    result = list.ToArray();
                }
            }
        }
        return result;
    }

    private bool parseInt(string i, out object obj)
    {
        int num;
        bool result;
        if (!int.TryParse(i, out num))
        {
            Print("Invalid Parameter: Parameter \"" + i + "\" needs to be an integer");
            obj = null;
            result = false;
        }
        else
        {
            obj = num;
            result = true;
        }
        return result;
    }

    private bool parseFloat(string i, out object obj)
    {
        float num;
        bool result;
        if (!float.TryParse(i, out num))
        {
            Print("Invalid Parameter: Parameter \"" + i + "\" needs to be an float");
            obj = null;
            result = false;
        }
        else
        {
            obj = num;
            result = true;
        }
        return result;
    }

    private bool parseString(string i, out object obj)
    {
        obj = i;
        return true;
    }

    private void Help(params string[] arg)
    {
        if (arg.Length > 0)
        {
            DirectCommand directCommand;
            if (directCommands.TryGetValue(arg[0], out directCommand))
            {
                HelpAttribute[] array = directCommand.method.GetCustomAttributes(typeof(HelpAttribute), true) as HelpAttribute[];
                HelpAttribute[] array2 = array;
                for (int i = 0; i < array2.Length; i++)
                {
                    HelpAttribute helpAttribute = array2[i];
                    Print(helpAttribute.helpText);
                }
            }
            else
            {
                Print("Unknown command");
            }
        }
        else
        {
            string text = "";
            foreach (string current in directCommands.Keys)
            {
                text = text + ", " + current;
            }
            text = text.Substring(2);
            Print("Type \"help [command]\" to get additional help");
            Print("Known commands: " + text);
        }
    }

    [Help("Lists all the active root gameobjects")]
    private void List()
    {
        UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(typeof(Transform));
        UnityEngine.Object[] array2 = array;
        for (int i = 0; i < array2.Length; i++)
        {
            UnityEngine.Object @object = array2[i];
            Transform transform = (Transform)@object;
            if (transform.parent == null)
            {
                Print(transform.name);
            }
        }
    }

    [Help("Lists all the components on the given gameobject")]
    private void ListComponents(string goPath)
    {
        GameObject gameObject = GameObject.Find(goPath);
        if (gameObject == null)
        {
            Print("Object not found: " + goPath);
        }
        else
        {
            Component[] components = gameObject.GetComponents<Component>();
            Component[] array = components;
            for (int i = 0; i < array.Length; i++)
            {
                Component component = array[i];
                Print(component.GetType().ToString());
            }
        }
    }

    [Help("Prints a tree of the children in the given gameobject")]
    private void PrintTree(string goPath)
    {
        GameObject gameObject = GameObject.Find(goPath);
        if (gameObject == null)
        {
            Print("Object not found: " + goPath);
        }
        else
        {
            PrintTreeInternal(gameObject.transform, 0);
        }
    }

    private void PrintTreeInternal(Transform go, int depth)
    {
        string str = "";
        for (int i = 0; i < depth; i++)
        {
            str += "\t";
        }
        Print(str + go.name);
        foreach (Transform go2 in go)
        {
            PrintTreeInternal(go2, depth + 1);
        }
    }

    [Help("Usage: select object.component\nSelects a component to easily execute multiple methods on it. You can execute methods on the selected component by just typing the method name and parameters as if it was a normal command.")]
    private void Select(params string[] path)
    {
        if (path.Length != 1)
        {
            selectedComponent = null;
            Print("Component deselected");
        }
        string[] hierarchy;
        string text;
        string text2;
        parseGameObjectString(path[0], out hierarchy, out text, out text2);
        if (text2 != null)
        {
            Print("Invalid path. You can not add a method or field identifier to the path");
        }
        if (text == null)
        {
            Print("Invalid path. Component is not present");
        }
        string text3 = BuildGameObjectPath(hierarchy);
        GameObject gameObject = GameObject.Find(text3);
        if (gameObject == null)
        {
            Print("Object not found: " + text3);
        }
        else
        {
            Component component = gameObject.GetComponent(text);
            if (component == null)
            {
                Print("Component not found: " + text);
            }
            else
            {
                selectedComponent = component;
                Print("Component selected: " + selectedComponent);
            }
        }
    }

    private void OnApplicationQuit()
    {
        if (m_handleLog)
        {
            if (string.IsNullOrEmpty(logPath))
                logPath = Path.Combine(Application.streamingAssetsPath, string.Format("debug_{0}.{1}", DateTime.Now.DateTimeToUnixTimestamp(), printHtml ? "html" : "log"));

            string dir = Path.GetDirectoryName(logPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

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