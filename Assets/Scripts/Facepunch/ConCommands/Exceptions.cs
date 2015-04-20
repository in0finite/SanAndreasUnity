using System;
using System.Collections.Generic;
using System.Linq;
using Facepunch.Networking;

namespace Facepunch.ConCommands
{
    public class ConCommandException : Exception
    {
        public Domain Domain { get; private set; }
        public String[] Command { get; private set; }

        protected static String FormatCommand(params String[] cmd)
        {
            return String.Join(" ", cmd.Where(x => !String.IsNullOrEmpty(x)).ToArray());
        }

        protected ConCommandException(Domain domain, IEnumerable<String> command, String message)
            : base(message)
        {
            Domain = domain;
            Command = command.ToArray();
        }

        public ConCommandException(Domain domain, IEnumerable<String> command, Exception inner)
            : base(String.Format("An exception was thrown while running the command \"{0}\". {1}", FormatCommand(command.ToArray()), inner), inner)
        {
            Domain = domain;
            Command = command.ToArray();
        }
    }

    public sealed class ConCommandNotFoundException : ConCommandException
    {
        public String[] Prefix { get; private set; }
        public String[] Options { get; private set; }

        private static String CreateMessage(Domain domain, String prefix, String command, IEnumerable<String> options)
        {
            var option = options.FirstOrDefault();
            command = FormatCommand(prefix, command);

            if (option == null || prefix.Length == 0) return String.Format("{0} console command \"{1}\" does not exist.", domain, command);

            return String.Format("{0} console command \"{1}\" is not valid, did you mean \"{2} {3}\"?",
                domain, command, prefix, option);
        }

        public ConCommandNotFoundException(Domain domain, String[] prefix, IEnumerable<String> options, IEnumerable<String> values)
            : this(domain, prefix, null, options, values) { }

        public ConCommandNotFoundException(Domain domain, String[] prefix, String command, IEnumerable<String> options, IEnumerable<String> values)
            : base(domain, prefix.Concat(values), CreateMessage(domain, FormatCommand(prefix), command, options))
        {
            var prefixStr = FormatCommand(prefix);

            Prefix = prefix;
            Options = options.Select(x => FormatCommand(prefixStr, x)).ToArray();
        }
    }
}
