using System.Collections.Generic;
using SanAndreasUnity.Net;
using UnityEngine;

namespace SanAndreasUnity.Commands
{
    public class CommandManager : MonoBehaviour
    {
        public static CommandManager Singleton { get; private set; }

        readonly Dictionary<string, CommandInfo> m_registeredCommands = new Dictionary<string, CommandInfo>();

        public IEnumerable<string> RegisteredCommands => m_registeredCommands.Keys;

        public static string invalidSyntaxText => "Invalid syntax";

        [SerializeField] private List<string> m_forbiddenCommands = new List<string>();

        /// <summary> Forbidden commands can not be registered. </summary>
        public List<string> ForbiddenCommands => m_forbiddenCommands;

        [SerializeField] private bool m_registerHelpCommand = true;

        public struct CommandInfo
        {
            public string command;
            public System.Func<ProcessCommandContext, ProcessCommandResult> commandHandler;
            public bool allowToRunWithoutServerPermissions;

            public CommandInfo(string command, bool allowToRunWithoutServerPermissions)
            {
                this.command = command;
                this.allowToRunWithoutServerPermissions = allowToRunWithoutServerPermissions;
                this.commandHandler = null;
            }
        }

        public class ProcessCommandResult
        {
            public string response;

            public static ProcessCommandResult UnknownCommand => new ProcessCommandResult {response = "Unknown command"};
            public static ProcessCommandResult InvalidCommand => new ProcessCommandResult {response = "Invalid command"};
            public static ProcessCommandResult NoPermissions => new ProcessCommandResult {response = "You don't have permissions to run this command"};
        }

        public class ProcessCommandContext
        {
            public string command;
            public bool hasServerPermissions;
        }



        void Awake()
        {
            if (null == Singleton)
                Singleton = this;

            if (m_registerHelpCommand)
                RegisterCommand(new CommandInfo { command = "help", commandHandler = ProcessHelpCommand, allowToRunWithoutServerPermissions = true });
        }

        public bool RegisterCommand(CommandInfo commandInfo)
        {
            commandInfo.command = commandInfo.command.Trim();

            if (this.ForbiddenCommands.Contains(commandInfo.command))
                return false;

            if (m_registeredCommands.ContainsKey(commandInfo.command))
                return false;

            m_registeredCommands.Add(commandInfo.command, commandInfo);
            return true;
        }

        public bool RemoveCommand(string command)
        {
            return m_registeredCommands.Remove(command);
        }

        public static string[] SplitCommandIntoArguments(string command)
        {
            // TODO: add support for arguments that have spaces, i.e. those enclosed with quotes

            return command.Split(new string[] {" ", "\t"}, System.StringSplitOptions.RemoveEmptyEntries);
        }

        public static string GetRestOfTheCommand(string command, int argumentIndex)
        {
            if (argumentIndex < 0)
                return "";

            string[] args = SplitCommandIntoArguments(command);

            if (argumentIndex > args.Length - 2)
                return "";

            return string.Join(" ", args, argumentIndex + 1, args.Length - argumentIndex - 1);
        }

        ProcessCommandResult ProcessCommand(string command, bool hasServerPermissions)
        {
            if (string.IsNullOrWhiteSpace(command))
                return ProcessCommandResult.UnknownCommand;

            string[] arguments = SplitCommandIntoArguments(command);
            if (0 == arguments.Length)
                return ProcessCommandResult.InvalidCommand;

            if (!m_registeredCommands.TryGetValue(arguments[0], out CommandInfo commandInfo))
                return ProcessCommandResult.UnknownCommand;

            if (!hasServerPermissions && !commandInfo.allowToRunWithoutServerPermissions)
                return ProcessCommandResult.NoPermissions;

            var context = new ProcessCommandContext {command = command, hasServerPermissions = hasServerPermissions};

            return commandInfo.commandHandler(context);
        }

        public ProcessCommandResult ProcessCommandAsServer(string command)
        {
            return ProcessCommand(command, true);
        }

        public ProcessCommandResult ProcessCommandForPlayer(Player player, string command)
        {
            bool hasServerPermissions = player == Player.Local;
            return ProcessCommand(command, hasServerPermissions);
        }

        ProcessCommandResult ProcessHelpCommand(ProcessCommandContext context)
        {
            string response = "List of available commands:\n";

            foreach (var pair in m_registeredCommands)
            {
                if (!context.hasServerPermissions && !pair.Value.allowToRunWithoutServerPermissions)
                    continue;

                response += pair.Key + "\n";
            }

            response += "\n";

            return new ProcessCommandResult {response = response};
        }

        public bool HasCommand(string command)
        {
            return m_registeredCommands.ContainsKey(command);
        }
    }
}
