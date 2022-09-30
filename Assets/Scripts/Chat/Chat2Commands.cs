using System;
using SanAndreasUnity.Commands;
using SanAndreasUnity.Net;
using UGameCore.Utilities;
using UnityEngine;

namespace SanAndreasUnity.Chat
{
    public class Chat2Commands : MonoBehaviour
    {
        private void Start()
        {
            ChatManager.singleton.RegisterChatPreprocessor(new ChatPreprocessor {processCallback = ProcessChatMessage});
            CommandManager.Singleton.RegisterCommand(new CommandManager.CommandInfo("say", "send chat message", true, false, 1.5f)
            {
                commandHandler = ProcessSayCommand,
            });
        }

        private ChatPreprocessorResult ProcessChatMessage(Player player, string chatMessage)
        {
            if (chatMessage.Length < 2 || !chatMessage.StartsWith("/"))
                return new ChatPreprocessorResult { finalChatMessage = chatMessage };

            int whiteSpaceIndex = chatMessage.FindIndex(char.IsWhiteSpace);

            int commandLength = whiteSpaceIndex < 0 ? chatMessage.Length - 1 : whiteSpaceIndex - 1;

            string command = chatMessage.Substring(1, commandLength);

            if (!CommandManager.Singleton.HasCommand(command))
                return new ChatPreprocessorResult { finalChatMessage = chatMessage };

            string fullCommand = chatMessage.Substring(1);

            string response;

            try
            {
                response = CommandManager.Singleton.ProcessCommandForPlayer(player, fullCommand).response;
            }
            catch (Exception exception)
            {
                response = exception.Message;
            }

            // send command back to player
            ChatManager.SendChatMessageToPlayerAsServer(player, chatMessage, false);

            // send response back to player
            if (!string.IsNullOrWhiteSpace(response))
                ChatManager.SendChatMessageToPlayerAsServer(player, response, true);

            // discard chat message
            return new ChatPreprocessorResult {shouldBeDiscarded = true};
        }

        CommandManager.ProcessCommandResult ProcessSayCommand(CommandManager.ProcessCommandContext context)
        {
            string msg = CommandManager.GetRestOfTheCommand(context.command, 0);

            if (context.player != null)
                ChatManager.singleton.SubmitMessageFromPlayer(context.player, msg);
            else
                ChatManager.SendChatMessageToAllPlayersAsServer(msg);

            return CommandManager.ProcessCommandResult.Success;
        }
    }
}
