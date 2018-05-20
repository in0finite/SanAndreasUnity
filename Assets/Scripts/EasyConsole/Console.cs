using Homans.Console;
using Homans.Containers;
using System;
using System.Collections.Generic;
using System.Reflection;
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

    public delegate bool ParserCallback(string str, out object obj);

    private static global::Console instance;

    private CircularBuffer<string> lines;

    [SerializeField]
    private int linesOfHistory = 1000;

    public int maxLineWidth = 80;

    private CircularBuffer<string> commands;

    [SerializeField]
    private int commandHistory = 100;

    private Dictionary<string, global::Console.DirectCommand> directCommands = new Dictionary<string, global::Console.DirectCommand>();

    [SerializeField]
    private bool printDebug = true;

    private Dictionary<Type, global::Console.ParserCallback> parsers = new Dictionary<Type, global::Console.ParserCallback>();

    private Component selectedComponent = null;

    public static global::Console Instance
    {
        get
        {
            return global::Console.instance;
        }
    }

    public CircularBuffer<string> Lines
    {
        get
        {
            return this.lines;
        }
    }

    public CircularBuffer<string> Commands
    {
        get
        {
            return this.commands;
        }
    }

    private void Awake()
    {
        if (global::Console.instance != null)
        {
            Debug.LogError("only one instance is allowed to exist");
        }
        global::Console.instance = this;
        this.lines = new CircularBuffer<string>(this.linesOfHistory, true);
        this.commands = new CircularBuffer<string>(this.commandHistory, true);
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        if (this.printDebug)
        {
            Application.logMessageReceived += this.PrintDebug;
            //Application.RegisterLogCallback(new Application.LogCallback(this.PrintDebug));
        }
        this.RegisterCommand("help", this, "Help");
        this.RegisterCommand("list", this, "List");
        this.RegisterCommand("listComponents", this, "ListComponents");
        this.RegisterCommand("printTree", this, "PrintTree");
        this.RegisterCommand("select", this, "Select");
        this.RegisterParser(typeof(int), new global::Console.ParserCallback(this.parseInt));
        this.RegisterParser(typeof(float), new global::Console.ParserCallback(this.parseFloat));
        this.RegisterParser(typeof(string), new global::Console.ParserCallback(this.parseString));
    }

    /*private void OnLevelWasLoaded(int id)
    {
        List<string> list = new List<string>();
        foreach (KeyValuePair<string, global::Console.DirectCommand> current in this.directCommands)
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
        foreach (KeyValuePair<string, global::Console.DirectCommand> current in this.directCommands)
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
        this.RegisterCommand(command, instance, method);
    }

    public void RegisterCommand(string command, object instance, MethodInfo method)
    {
        global::Console.DirectCommand value = default(global::Console.DirectCommand);
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
        this.directCommands[command] = value;
    }

    public void RemoveCommand(string command)
    {
        if (this.directCommands.ContainsKey(command))
        {
            this.directCommands.Remove(command);
        }
    }

    public void RegisterParser(Type type, global::Console.ParserCallback func)
    {
        this.parsers[type] = func;
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
            while (text2.Length > this.maxLineWidth)
            {
                int num = text2.LastIndexOf(' ', this.maxLineWidth);
                if (num >= this.maxLineWidth / 2)
                {
                    this.lines.Enqueue(text2.Substring(0, num));
                    text2 = text2.Substring(num + 1);
                }
                else
                {
                    this.lines.Enqueue(text2.Substring(0, this.maxLineWidth));
                    text2 = text2.Substring(this.maxLineWidth + 1);
                }
            }
            this.lines.Enqueue(text2);
        }
    }

    public void Eval(string line)
    {
        this.commands.Enqueue(line);
        string text;
        string[] hierarchy;
        string componentName;
        string text2;
        string[] args;
        global::Console.parseCommand(line, out text, out hierarchy, out componentName, out text2, out args);
        if (text != null)
        {
            global::Console.DirectCommand directCommand;
            if (this.directCommands.TryGetValue(text, out directCommand))
            {
                if (directCommand.instance.IsAlive && directCommand.instance.Target != null)
                {
                    this.CheckParametersAndInvoke(args, directCommand.instance.Target, directCommand.method);
                }
                else
                {
                    this.Print("Instance has been removed. Removing command");
                    this.directCommands.Remove(text);
                }
            }
            else if (this.selectedComponent != null)
            {
                MethodInfo methodInfo;
                if (this.GetMethodOnComponent(text, this.selectedComponent, out methodInfo))
                {
                    if (methodInfo == null)
                    {
                        this.Print("Unkown Command! Type \"help\" for help.");
                    }
                    else
                    {
                        this.CheckParametersAndInvoke(args, this.selectedComponent, methodInfo);
                    }
                }
            }
            else
            {
                this.Print("Unkown Command! Type \"help\" for help.");
            }
        }
        else if (text2 != null)
        {
            this.InvokeMethodOnComponent(args, hierarchy, componentName, text2);
        }
        else
        {
            this.Print("Unkown Command! Type \"help\" for help.");
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
            this.Print("Unknown GameObject");
        }
        else
        {
            Component component = gameObject.GetComponent(ocf[1]);
            MethodInfo methodInfo;
            if (component == null)
            {
                this.Print("Unknown Component");
            }
            else if (this.GetMethodOnComponent(ocf[2], component, out methodInfo))
            {
                if (methodInfo == null)
                {
                    this.Print("Unknown Method");
                }
                else
                {
                    this.CheckParametersAndInvoke(args, component, methodInfo);
                }
            }
        }
    }

    private void InvokeMethodOnComponent(string[] args, string[] hierarchy, string componentName, string methodName)
    {
        string name = global::Console.BuildGameObjectPath(hierarchy);
        GameObject gameObject = GameObject.Find(name);
        if (gameObject == null)
        {
            this.Print("Unknown GameObject");
        }
        else
        {
            Component component = gameObject.GetComponent(componentName);
            MethodInfo methodInfo;
            if (component == null)
            {
                this.Print("Unknown Component");
            }
            else if (this.GetMethodOnComponent(methodName, component, out methodInfo))
            {
                if (methodInfo == null)
                {
                    this.Print("Unknown Method");
                }
                else
                {
                    this.CheckParametersAndInvoke(args, component, methodInfo);
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
            this.Print("Overloaded Method Found. Unable to invoke");
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
            global::Console.ParserCallback parserCallback;
            if (this.parsers.TryGetValue(type, out parserCallback))
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
                        this.Print(string.Concat(new object[]
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
            this.Print(string.Concat(new object[]
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
            global::Console.Instance.Print(obj2.ToString());
            return;
        }
    }

    private void PrintDebug(string condition, string stackTrace, LogType type)
    {
        this.Print(type.ToString() + ": " + condition);
        string[] array = stackTrace.Split(new char[]
        {
            '\n'
        });
        for (int i = 0; i < array.Length; i++)
        {
            string str = array[i];
            this.Print("\t" + str);
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
                global::Console.parseGameObjectString(list[0], out gameobjectPath, out componentName, out methodName);
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
            this.Print("Invalid Parameter: Parameter \"" + i + "\" needs to be an integer");
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
            this.Print("Invalid Parameter: Parameter \"" + i + "\" needs to be an float");
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
            global::Console.DirectCommand directCommand;
            if (this.directCommands.TryGetValue(arg[0], out directCommand))
            {
                HelpAttribute[] array = directCommand.method.GetCustomAttributes(typeof(HelpAttribute), true) as HelpAttribute[];
                HelpAttribute[] array2 = array;
                for (int i = 0; i < array2.Length; i++)
                {
                    HelpAttribute helpAttribute = array2[i];
                    this.Print(helpAttribute.helpText);
                }
            }
            else
            {
                this.Print("Unknown command");
            }
        }
        else
        {
            string text = "";
            foreach (string current in this.directCommands.Keys)
            {
                text = text + ", " + current;
            }
            text = text.Substring(2);
            this.Print("Type \"help [command]\" to get additional help");
            this.Print("Known commands: " + text);
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
                this.Print(transform.name);
            }
        }
    }

    [Help("Lists all the components on the given gameobject")]
    private void ListComponents(string goPath)
    {
        GameObject gameObject = GameObject.Find(goPath);
        if (gameObject == null)
        {
            this.Print("Object not found: " + goPath);
        }
        else
        {
            Component[] components = gameObject.GetComponents<Component>();
            Component[] array = components;
            for (int i = 0; i < array.Length; i++)
            {
                Component component = array[i];
                this.Print(component.GetType().ToString());
            }
        }
    }

    [Help("Prints a tree of the children in the given gameobject")]
    private void PrintTree(string goPath)
    {
        GameObject gameObject = GameObject.Find(goPath);
        if (gameObject == null)
        {
            this.Print("Object not found: " + goPath);
        }
        else
        {
            this.PrintTreeInternal(gameObject.transform, 0);
        }
    }

    private void PrintTreeInternal(Transform go, int depth)
    {
        string str = "";
        for (int i = 0; i < depth; i++)
        {
            str += "\t";
        }
        this.Print(str + go.name);
        foreach (Transform go2 in go)
        {
            this.PrintTreeInternal(go2, depth + 1);
        }
    }

    [Help("Usage: select object.component\nSelects a component to easily execute multiple methods on it. You can execute methods on the selected component by just typing the method name and parameters as if it was a normal command.")]
    private void Select(params string[] path)
    {
        if (path.Length != 1)
        {
            this.selectedComponent = null;
            this.Print("Component deselected");
        }
        string[] hierarchy;
        string text;
        string text2;
        global::Console.parseGameObjectString(path[0], out hierarchy, out text, out text2);
        if (text2 != null)
        {
            this.Print("Invalid path. You can not add a method or field identifier to the path");
        }
        if (text == null)
        {
            this.Print("Invalid path. Component is not present");
        }
        string text3 = global::Console.BuildGameObjectPath(hierarchy);
        GameObject gameObject = GameObject.Find(text3);
        if (gameObject == null)
        {
            this.Print("Object not found: " + text3);
        }
        else
        {
            Component component = gameObject.GetComponent(text);
            if (component == null)
            {
                this.Print("Component not found: " + text);
            }
            else
            {
                this.selectedComponent = component;
                this.Print("Component selected: " + this.selectedComponent);
            }
        }
    }
}