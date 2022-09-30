using System.Collections;
using System.Collections.Generic;
using UGameCore.Utilities;
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
            this.sendButton.onClick.AddListener(this.OnTextSubmitted);
            this.inputField.onEndEdit.AddListener(this.OnEndEdit);
        }

        void OnTextSubmitted()
        {
            string msg = this.inputField.text;

            this.inputField.text = "";

            if (string.IsNullOrWhiteSpace(msg))
                return;

            Chat.ChatManager.SendChatMessageToAllPlayersAsLocalPlayer(msg);
        }

        void OnEndEdit (string value)
        {
            if (CustomInput.Instance.GetKeyDown (KeyCode.KeypadEnter) || CustomInput.Instance.GetKeyDown (KeyCode.Return))
            {
                // send chat message
                this.OnTextSubmitted();

                // set focus to input field
                this.inputField.Select();
                this.inputField.ActivateInputField ();
            }
        }

    }

}
