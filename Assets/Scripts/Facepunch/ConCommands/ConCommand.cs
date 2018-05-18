using Facepunch.Networking;
using ParseSharp;
using ParseSharp.BackusNaur;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Facepunch.ConCommands
{
    public static class ConCommand
    {
        private class MainThreadTask
        {
            private readonly ManualResetEvent _waitHandle;
            private readonly Func<object> _task;

            private object _result;
            private Exception _exception;

            public MainThreadTask(Func<object> task)
            {
                _waitHandle = new ManualResetEvent(false);
                _task = task;

                _result = null;
                _exception = null;
            }

            public TResult AwaitResult<TResult>(int timeout)
            {
                if (!_waitHandle.WaitOne(timeout))
                {
                    throw new TimeoutException("Timed out waiting for command to be performed on the main thread.");
                }

                if (_exception != null)
                {
                    throw _exception;
                }

                return (TResult)_result;
            }

            public void Perform()
            {
                try
                {
                    _result = _task();
                }
                catch (Exception e)
                {
                    _exception = e;
                }

                _waitHandle.Set();
            }
        }

        private delegate void ConCommandSilentDelegate(ConCommandArgs args);

        private delegate Object ConCommandDelegate(ConCommandArgs args);

        private static Thread _sMainThread;

        private static readonly Queue<MainThreadTask> _sMainThreadTasks
            = new Queue<MainThreadTask>();

        private const String DefaultSuccessString = "Command executed successfully";

        private const int MainThreadTaskTimeout = 2000;

        private class CommandSet
        {
            private readonly Dictionary<String, CommandSet> _subCommands
                = new Dictionary<String, CommandSet>();

            private ConCommandDelegate _delegate;
            private ConCommandAttribute _attribute;
            private CommandSet _parent;

            private readonly String[] _prefix;

            private bool ContainsCallable(Domain domain)
            {
                return _attribute != null && (_attribute.Domain & domain) == domain ||
                       _subCommands.Values.Any(x => x.ContainsCallable(domain));
            }

            private CommandSet this[String subName]
            {
                get { return _subCommands.ContainsKey(subName) ? _subCommands[subName] : null; }
            }

            public CommandSet()
            {
                _subCommands = new Dictionary<String, CommandSet>();
                _prefix = new String[0];
            }

            private CommandSet(CommandSet parent, String name)
            {
                _parent = parent;
                _prefix = _parent._prefix.Concat(new[] { name }).ToArray();
            }

            public void Add(IEnumerable<String> prefix, ConCommandDelegate deleg, ConCommandAttribute attrib)
            {
                var first = prefix.FirstOrDefault();

                if (first == null)
                {
                    if (_delegate != null)
                    {
                        throw new Exception("A delegate has already been registered for this console command.");
                    }

                    _delegate = deleg;
                    _attribute = attrib;
                    return;
                }

                var sub = this[first];
                if (sub == null)
                {
                    sub = new CommandSet(this, first);
                    _subCommands.Add(first, sub);
                }

                sub.Add(prefix.Skip(1), deleg, attrib);
            }

            private String Run(Domain domain, IEnumerable<String> args)
            {
                var first = args.FirstOrDefault();
                var subcmd = first != null ? this[first] : null;

                if (first != null && subcmd != null)
                {
                    return subcmd.Run(domain, args.Skip(1));
                }

                if (_delegate != null && (_attribute.Domain & domain) == domain)
                {
                    try
                    {
                        try
                        {
                            return _delegate(new ConCommandArgs(domain, _prefix, args.ToArray())).ToString();
                        }
                        catch (Exception e)
                        {
                            if (!e.Message.Contains("can only be called from the main thread.")) throw e;

                            if (_sMainThread == null)
                            {
                                throw new Exception("Command must be performed on the main thread, "
                                    + "but ConCommand.PerformMainThreadTasks() has not been called.");
                            }

                            var task = new MainThreadTask(() => Run(domain, args));
                            _sMainThreadTasks.Enqueue(task);

                            return task.AwaitResult<String>(MainThreadTaskTimeout);
                        }
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogError(e);
                        throw new ConCommandException(domain, _prefix.Concat(args), e);
                    }
                }

                var options = _subCommands.Keys.Where(x => _subCommands[x].ContainsCallable(domain));

                throw first == null
                    ? new ConCommandNotFoundException(domain, _prefix, options, args)
                    : new ConCommandNotFoundException(domain, _prefix, first, options, args);
            }

            public String Run(Domain domain, String[] args)
            {
                return Run(domain, args.AsEnumerable());
            }
        }

        private static readonly CommandSet _sRootCommand = new CommandSet();

        static ConCommand()
        {
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (method.IsAbstract || method.DeclaringType != type) continue;

                    var attrib = method.GetAttribute<ConCommandAttribute>(false);
                    if (attrib == null) continue;

                    try
                    {
                        ConCommandDelegate deleg;

                        if (method.ReturnType == typeof(void))
                        {
                            var inner = (ConCommandSilentDelegate)Delegate.CreateDelegate(typeof(ConCommandSilentDelegate), method);
                            deleg = args => { inner(args); return DefaultSuccessString; };
                        }
                        else
                        {
                            deleg = (ConCommandDelegate)Delegate.CreateDelegate(typeof(ConCommandDelegate), method);
                        }

                        _sRootCommand.Add(attrib.Prefix, deleg, attrib);
                    }
                    catch (ArgumentException)
                    {
                        UnityEngine.Debug.LogWarningFormat("Unable to register {0}.{1} as a console command.", type.FullName, method.Name);
                    }
                }
            }
        }

        private static Parser _sCommandListParser;

        private static IEnumerable<string[]> Split(String args)
        {
            if (_sCommandListParser == null)
            {
                _sCommandListParser = ParserGenerator.FromEBnf(@"
                    (* skip-whitespace *)
                    Command List      = Command, {"";"", Command}, End of Input;
                    Command           = Argument, {Argument};

                    (* collapse *)
                    End of Input      = ? /$/ ?;

                    (* match-whitespace *)
                    Argument          = Argument Part, {Argument Part};

                    (* omit-from-hierarchy *)
                    Argument Part     = Simple Character | (""\\"", Escaped Character) | (""\"""", Quoted String, ""\"""");

                    (* omit-from-hierarchy *)
                    Quoted String     = {Quoted Character | (""\\"", Escaped Character)};

                    (* collapse *)
                    Simple Character  = ? /[^\s\\"";]/ ?;

                    (* collapse *)
                    Quoted Character  = ? /[^""\\]/ ?;

                    (* collapse *)
                    Escaped Character = ""\\"" | ""\"""" | ""n"" | ""t"" | ""r"";
                ");
            }

            var match = _sCommandListParser.Parse(args);

            if (!match.Success)
            {
                throw new Exception(match.Error.ToString());
            }

            return match
                .Where(x => x.Parser.Name == "Command")
                .Select(command => command
                    .Select(arg => String.Join(String.Empty, arg
                    .Select(c =>
                    {
                        if (c.Parser.Name != "Escaped Character") return c.Value;
                        switch (c.Value[0])
                        {
                            case '\\': return "\\";
                            case 'n': return "\n";
                            case 'r': return "\r";
                            case 't': return "\t";
                        }
                        return c.Value;
                    })
                    .ToArray()))
                .ToArray());
        }

        public static ConCommandResult Run(Domain domain, String args)
        {
            try
            {
                var commands = Split(args);

                var result = new ConCommandResult(DefaultSuccessString);
                foreach (var command in commands)
                {
                    result = new ConCommandResult(_sRootCommand.Run(domain, command));
                }
                return result;
            }
            catch (Exception e)
            {
                return new ConCommandResult(e);
            }
        }

        public static ConCommandResult RunServer(String args)
        {
            return Run(Domain.Server, args);
        }

#if CLIENT

        public static ConCommandResult RunClient(String args)
        {
            return Run(Domain.Client, args);
        }

#endif

        public static void PerformMainThreadTasks()
        {
            if (_sMainThread == null)
            {
                _sMainThread = Thread.CurrentThread;
            }
            else if (_sMainThread != Thread.CurrentThread)
            {
                throw new InvalidOperationException("PerformMainThreadTasks() must always be called from the same thread.");
            }

            while (_sMainThreadTasks.Count > 0)
            {
                _sMainThreadTasks.Dequeue().Perform();
            }
        }
    }
}