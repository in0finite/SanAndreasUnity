using System.Collections.Generic;
using System.Linq;
using SanAndreasUnity.Net;
using UnityEngine;

namespace SanAndreasUnity.Commands
{
    public class CommandManager : MonoBehaviour
    {
        public static CommandManager Singleton { get; private set; }

        readonly Dictionary<string, CommandInfo> m_registeredCommands =
            new Dictionary<string, CommandInfo>(System.StringComparer.InvariantCulture);

        public IEnumerable<string> RegisteredCommands => m_registeredCommands.Keys;

        public static string invalidSyntaxText => "Invalid syntax";

        [SerializeField] private List<string> m_forbiddenCommands = new List<string>();

        /// <summary> Forbidden commands can not be registered. </summary>
        public List<string> ForbiddenCommands => m_forbiddenCommands;

        [SerializeField] private bool m_registerHelpCommand = true;

        private struct PlayerData
        {
            public double timeWhenLastExecutedCommand;
        }

        readonly Dictionary<Player, PlayerData> m_perPlayerData = new Dictionary<Player, PlayerData>();

        public struct CommandInfo
        {
            public string command;
            public string description;
            public System.Func<ProcessCommandContext, ProcessCommandResult> commandHandler;
            public bool allowToRunWithoutServerPermissions;
            public bool runOnlyOnServer;
            public float limitInterval;

            public CommandInfo(string command, bool allowToRunWithoutServerPermissions)
                : this()
            {
                this.command = command;
                this.allowToRunWithoutServerPermissions = allowToRunWithoutServerPermissions;
            }

            public CommandInfo(string command, string description, bool allowToRunWithoutServerPermissions)
                : this()
            {
                this.command = command;
                this.description = description;
                this.allowToRunWithoutServerPermissions = allowToRunWithoutServerPermissions;
            }

            public CommandInfo(string command, string description, bool allowToRunWithoutServerPermissions, bool runOnlyOnServer, float limitInterval)
                : this()
            {
                this.command = command;
                this.description = description;
                this.allowToRunWithoutServerPermissions = allowToRunWithoutServerPermissions;
                this.runOnlyOnServer = runOnlyOnServer;
                this.limitInterval = limitInterval;
            }
        }

        public class ProcessCommandResult
        {
            public string response;

            public static ProcessCommandResult UnknownCommand => new ProcessCommandResult {response = "Unknown command"};
            public static ProcessCommandResult InvalidCommand => new ProcessCommandResult {response = "Invalid command"};
            public static ProcessCommandResult NoPermissions => new ProcessCommandResult {response = "You don't have permissions to run this command"};
            public static ProcessCommandResult CanOnlyRunOnServer => new ProcessCommandResult {response = "This command can only run on server"};
            public static ProcessCommandResult LimitInterval(float interval) => new ProcessCommandResult {response = $"This command can only be used on an interval of {interval} seconds"};
            public static ProcessCommandResult Error(string errorMessage) => new ProcessCommandResult {response = errorMessage};
            public static ProcessCommandResult Success => new ProcessCommandResult();
        }

        public class ProcessCommandContext
        {
            /// <summary>
            /// Command that should be processed. This variable contains the entire command, including arguments.
            /// </summary>
            public string command;

            /// <summary>
            /// Does the executor have server permissions ?
            /// </summary>
            public bool hasServerPermissions;
            
            /// <summary>
            /// Player who is executing the command.
            /// </summary>
            public Player player;
        }



        void Awake()
        {
            if (null == Singleton)
                Singleton = this;

            Player.onDisable += PlayerOnDisable;

            if (m_registerHelpCommand)
                RegisterCommand(new CommandInfo { command = "help", commandHandler = ProcessHelpCommand, allowToRunWithoutServerPermissions = true });
        }

        void PlayerOnDisable(Player player)
        {
            m_perPlayerData.Remove(player);
        }

        public bool RegisterCommand(CommandInfo commandInfo)
        {
            if (null == commandInfo.commandHandler)
                throw new System.ArgumentException("Command handler must be provided");

            if (string.IsNullOrWhiteSpace(commandInfo.command))
                throw new System.ArgumentException("Command can not be empty");

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

        public static Vector3 ParseVector3(string[] arguments, int startIndex)
        {
            if (startIndex + 2 >= arguments.Length)
                throw new System.ArgumentException("Failed to parse Vector3: not enough arguments");

            Vector3 v = Vector3.zero;
            for (int i = 0; i < 3; i++)
            {
                if (!float.TryParse(arguments[startIndex + i], out float f))
                    throw new System.ArgumentException("Failed to parse Vector3: invalid number");
                v[i] = f;
            }

            return v;
        }

        public static Quaternion ParseQuaternion(string[] arguments, int startIndex)
        {
            if (startIndex + 3 >= arguments.Length)
                throw new System.ArgumentException("Failed to parse Quaternion: not enough arguments");

            Quaternion quaternion = Quaternion.identity;
            for (int i = 0; i < 4; i++)
            {
                if (!float.TryParse(arguments[startIndex + i], out float f))
                    throw new System.ArgumentException("Failed to parse Quaternion: invalid number");
                quaternion[i] = f;
            }

            return quaternion;
        }

        public static Color ParseColor(string[] arguments, int startIndex)
        {
            if (startIndex >= arguments.Length)
                throw new System.ArgumentException("Failed to parse color: not enough arguments");

            if (!ColorUtility.TryParseHtmlString(arguments[startIndex], out Color color))
                throw new System.ArgumentException("Failed to parse color");

            return color;
        }

        ProcessCommandResult ProcessCommand(ProcessCommandContext context)
        {
            if (string.IsNullOrWhiteSpace(context.command))
                return ProcessCommandResult.UnknownCommand;

            string[] arguments = SplitCommandIntoArguments(context.command);
            if (0 == arguments.Length)
                return ProcessCommandResult.InvalidCommand;

            if (!m_registeredCommands.TryGetValue(arguments[0], out CommandInfo commandInfo))
                return ProcessCommandResult.UnknownCommand;

            if (commandInfo.runOnlyOnServer && !NetStatus.IsServer)
                return ProcessCommandResult.CanOnlyRunOnServer;

            if (!context.hasServerPermissions && !commandInfo.allowToRunWithoutServerPermissions)
                return ProcessCommandResult.NoPermissions;

            if (context.player != null)
            {
                m_perPlayerData.TryGetValue(context.player, out PlayerData playerData);

                if (commandInfo.limitInterval > 0 && Time.timeAsDouble - playerData.timeWhenLastExecutedCommand < commandInfo.limitInterval)
                    return ProcessCommandResult.LimitInterval(commandInfo.limitInterval);

                playerData.timeWhenLastExecutedCommand = Time.timeAsDouble;
                m_perPlayerData[context.player] = playerData;
            }

            return commandInfo.commandHandler(context);
        }

        public ProcessCommandResult ProcessCommandAsServer(string command)
        {
            return ProcessCommand(new ProcessCommandContext {command = command, hasServerPermissions = true});
        }

        public ProcessCommandResult ProcessCommandForPlayer(Player player, string command)
        {
            if (null == player)
                throw new System.ArgumentNullException(nameof(player));

            bool hasServerPermissions = player == Player.Local || player.IsServerAdmin;
            return ProcessCommand(new ProcessCommandContext
            {
                command = command,
                hasServerPermissions = hasServerPermissions,
                player = player,
            });
        }

        ProcessCommandResult ProcessHelpCommand(ProcessCommandContext context)
        {
            string response = "List of available commands: " +
                              string.Join(", ", m_registeredCommands
                                  .Where(pair => context.hasServerPermissions || pair.Value.allowToRunWithoutServerPermissions)
                                  .Select(pair => pair.Key));

            return new ProcessCommandResult {response = response};
        }

        public bool HasCommand(string command)
        {
            return m_registeredCommands.ContainsKey(command);
        }
    }
}
