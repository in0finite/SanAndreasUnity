using System;
using SanAndreasUnity.Commands;
using SanAndreasUnity.Net;
using SanAndreasUnity.Utilities;
using UnityEngine;

namespace SanAndreasUnity.Chat
{
    public class Chat2Commands : MonoBehaviour
    {
        private void Start()
        {
            ChatManager.singleton.RegisterChatPreprocessor(new ChatPreprocessor {processCallback = ProcessChatMessage});
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
            ChatManager.SendChatMessageToPlayer(player, chatMessage, false);

            // send response back to player
            if (!string.IsNullOrWhiteSpace(response))
                ChatManager.SendChatMessageToPlayer(player, response, true);

            // discard chat message
            return new ChatPreprocessorResult {shouldBeDiscarded = true};
        }
    }
}
