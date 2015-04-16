using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Facepunch.Networking;

namespace Facepunch.ConCommands
{
    public static class ConCommand
    {
        private delegate void ConCommandSilentDelegate(ConCommandArgs args);
        private delegate Object ConCommandDelegate(ConCommandArgs args);

        private const String DefaultSuccessString = "Command executed successfully";

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
                _prefix = _parent._prefix.Concat(new [] { name }).ToArray();
            }

            public void Add(IEnumerable<String> prefix, ConCommandDelegate deleg, ConCommandAttribute attrib)
            {
                var first = prefix.FirstOrDefault();

                if (first == null) {
                    if (_delegate != null) {
                        throw new Exception("A delegate has already been registered for this console command.");
                    }

                    _delegate = deleg;
                    _attribute = attrib;
                    return;
                }

                var sub = this[first];
                if (sub == null) {
                    sub = new CommandSet(this, first);
                    _subCommands.Add(first, sub);
                }

                sub.Add(prefix.Skip(1), deleg, attrib);
            }

            private String Run(Domain domain, IEnumerable<String> args)
            {
                var first = args.FirstOrDefault();
                var subcmd = first != null ? this[first] : null;

                if (first != null && subcmd != null) {
                    return subcmd.Run(domain, args.Skip(1));
                }

                if (_delegate != null && (_attribute.Domain & domain) == domain) {
                    try {
                        return _delegate(new ConCommandArgs(_prefix, args.ToArray())).ToString();
                    } catch (Exception e) {
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
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes()) {
                foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)) {
                    if (method.IsAbstract || method.DeclaringType != type) continue;

                    var attrib = method.GetAttribute<ConCommandAttribute>(false);
                    if (attrib == null) continue;

                    try {
                        ConCommandDelegate deleg;

                        if (method.ReturnType == typeof (void)) {
                            var inner = (ConCommandSilentDelegate) Delegate.CreateDelegate(typeof(ConCommandSilentDelegate), method);
                            deleg = args => { inner(args); return DefaultSuccessString; };
                        } else {
                            deleg = (ConCommandDelegate) Delegate.CreateDelegate(typeof(ConCommandDelegate), method);
                        }

                        _sRootCommand.Add(attrib.Prefix, deleg, attrib);
                    } catch (ArgumentException) {
                        UnityEngine.Debug.LogWarningFormat("Unable to register {0}.{1} as a console command.", type.FullName, method.Name);
                    }
                }
            }
        }

        private static IEnumerable<string[]> Split(String args)
        {
            var commands = new List<String[]>();
            var split = new List<String>();

            var cur = new StringBuilder(args.Length);

            const char escapeChar = '\\';
            const char quoteChar = '"';
            const char cmdSepChar = ';';

            var escaped = false;
            var quoted = false;

            var i = 0;
            while (i < args.Length) {
                split.Clear();

                var canSkip = true;
                while (i++ < args.Length) {
                    var c = args[i - 1];

                    if (escaped) {
                        switch (c) {
                            case 'r': cur.Append('\r'); break;
                            case 'n': cur.Append('\n'); break;
                            case 't': cur.Append('\t'); break;
                            default: cur.Append(c); break;
                        }

                        escaped = false;
                        continue;
                    }

                    if (!quoted && !escaped) {
                        if (c == cmdSepChar) break;

                        if (char.IsWhiteSpace(c)) {
                            if (cur.Length > 0 || !canSkip) split.Add(cur.ToString());
                            cur.Remove(0, cur.Length);
                            canSkip = true;
                            continue;
                        }
                    }

                    canSkip = false;

                    switch (c) {
                        case quoteChar: quoted = !quoted; break;
                        case escapeChar: escaped = true; break;
                        default: cur.Append(c); break;
                    }
                }

                split.Add(cur.ToString());
                cur.Remove(0, cur.Length);

                commands.Add(split.ToArray());
            }

            return commands.Where(x => x.Length > 0);
        }

        public static ConCommandResult Run(Domain domain, String args)
        {
            var commands = Split(args);

            try {
                var result = new ConCommandResult(DefaultSuccessString);
                foreach (var command in commands) {
                    result = new ConCommandResult(_sRootCommand.Run(domain, command));
                }
                return result;
            } catch (ConCommandException e) {
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
    }
}
