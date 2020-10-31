using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandInterpreter
{
    public static String Interpret(String command)
    {
        string[] words = command.Split(' ');

        if (command == "heartbeat")
        {
            // Implement heartbeat ping
            return "Heartbeat was sent to master server";
        }

        if (command == "help")
        {
            return "The available commands for now are heartbeat, announce and help";
        }

        if (words[0] == "announce")
        {
            String announcement = String.Join(" ", words, 1, words.Length - 1);
            SanAndreasUnity.Chat.ChatManager.SendChatMessageToAllPlayersAsServer(announcement);
            return "Server : " + announcement;
        }

        return "Unknown command";
    }
}
