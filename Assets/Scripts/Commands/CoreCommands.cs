using SanAndreasUnity.Behaviours;
using SanAndreasUnity.Net;
using UGameCore.Utilities;
using System.Linq;
using UnityEngine;

namespace SanAndreasUnity.Commands
{
    public class CoreCommands : MonoBehaviour
    {
        void Start()
        {
            var commands = new CommandManager.CommandInfo[]
            {
                new CommandManager.CommandInfo("uptime", "show uptime of application", true),
                new CommandManager.CommandInfo("players", "show players", true),
                new CommandManager.CommandInfo("stats", "show statistics", false, false, 1f),
                new CommandManager.CommandInfo("kick", "kick player from server", false),
                new CommandManager.CommandInfo("auth", "authenticate", true, true, 2f),
                new CommandManager.CommandInfo("startserver", "start server", false),
                new CommandManager.CommandInfo("starthost", "start host", false),
                new CommandManager.CommandInfo("connect", "connect to server", false),
                new CommandManager.CommandInfo("exit", "exit application", false),
                new CommandManager.CommandInfo("camera_disable", "disable or enable camera", false),
                new CommandManager.CommandInfo("cmd_server", "run command as server", false),
            };

            foreach (var immutableCmd in commands)
            {
                var cmd = immutableCmd;
                cmd.commandHandler = ProcessCommand;
                CommandManager.Singleton.RegisterCommand(cmd);
            }
        }

        CommandManager.ProcessCommandResult ProcessCommand(CommandManager.ProcessCommandContext context)
        {
            string command = context.command;
            string[] words = CommandManager.SplitCommandIntoArguments(command);
            int numWords = words.Length;
            string restOfTheCommand = CommandManager.GetRestOfTheCommand(command, 0);

            string response = "";


            if (2 == numWords && words[0] == "camera_disable")
            {
                int cameraDisable = int.Parse(words[1]);

                var cam = F.FindMainCameraEvenIfDisabled();

                if (cam != null)
                {
                    if (0 == cameraDisable)
                    {
                        cam.enabled = true;
                    }
                    else if (1 == cameraDisable)
                    {
                        cam.enabled = false;
                    }
                    else
                    {
                        response += "Invalid value. Use 0 or 1.";
                    }
                }
            }
            else if (words[0] == "cmd_server")
            {
                return CommandManager.Singleton.ProcessCommandAsServer(restOfTheCommand);
            }
            else if (words[0] == "uptime")
            {
                response += F.FormatElapsedTime(Time.realtimeSinceStartupAsDouble);
            }
            else if (words[0] == "players")
            {
                // list all players

                response += "net id";
                if (NetUtils.IsServer && context.hasServerPermissions)
                    response += " | ip";
                response += "\n";

                foreach (var player in Player.AllPlayersEnumerable)
                {
                    response += player.netId;
                    if (NetUtils.IsServer && context.hasServerPermissions)
                        response += " | " + player.CachedIpAddress;
                    response += "\n";
                }
            }
            else if (words[0] == "stats")
            {
                string category = null;
                if (numWords > 1)
                    category = words[1].ToLowerInvariant();

                var entries = UGameCore.Utilities.Stats.Entries;
                if (category != null)
                    entries = entries.Where(_ => _.Key.ToLowerInvariant() == category);

                var statsContext = new UGameCore.Utilities.Stats.GetStatsContext();

                foreach (var entriesWithCategory in entries)
                {
                    statsContext.AppendLine("--------------------------------");
                    statsContext.AppendLine(entriesWithCategory.Key);
                    statsContext.AppendLine("--------------------------------");
                    statsContext.AppendLine();
                    
                    foreach (var entry in entriesWithCategory.Value)
                    {
                        entry.getStatsAction?.Invoke(statsContext);
                    }

                    statsContext.AppendLine();
                }

                response += statsContext.stringBuilder.ToString();

            }
            else if (words[0] == "kick")
            {
                if (NetUtils.IsServer)
                {
                    uint netId = uint.Parse(words[1]);
                    var p = Player.GetByNetId(netId);
                    if (null == p)
                    {
                        response += "There is no such player connected.";
                    }
                    else
                    {
                        p.Disconnect();
                    }
                }
            }
            else if (words[0] == "auth")
            {
                // authenticate player

                if (NetUtils.IsServer)
                {
                    string adminPass = Config.GetString("admin_pass");
                    if (!string.IsNullOrWhiteSpace(adminPass))
                    {
                        string providedPass = words[1];
                        bool matches = providedPass == adminPass;
                        context.player.IsServerAdmin = matches;
                        response = matches ? "Success" : "Failed";
                        Debug.Log($"Player {context.player.DescriptionForLogging} performed authentication, result {matches}");
                    }
                }

            }
            else if (words[0] == "startserver" || words[0] == "starthost")
            {
                ushort portNumber = (ushort) NetManager.defaultListenPortNumber;

                if (numWords > 1)
                    portNumber = ushort.Parse(words[1]);

                if (words[0] == "startserver")
                    NetManager.StartServer(portNumber);
                else
                    NetManager.StartHost(portNumber);
            }
            else if (words[0] == "connect")
            {
                if (numWords != 3)
                {
                    response += CommandManager.invalidSyntaxText;
                }
                else
                {
                    string ip = words[1];
                    int port = int.Parse(words[2]);

                    NetManager.StartClient(ip, port);
                }
            }
            else if (words[0] == "exit")
            {
                GameManager.ExitApplication();
            }

            return new CommandManager.ProcessCommandResult {response = response};
        }
    }
}
