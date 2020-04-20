using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SanAndreasUnity.UI
{

    public class ChatInputController : MonoBehaviour
    {
        public InputField inputField;
        public Button sendButton;


        void Start()
        {
            this.sendButton.onClick.AddListener(() => SendChatMessage(this.inputField.text));
        }

        void SendChatMessage(string msg)
        {
            this.inputField.text = "";

            if (string.IsNullOrWhiteSpace(msg))
                return;

            Chat.ChatManager.SendChatMessageToAllPlayersAsLocalPlayer(msg);
        }

    }

}
