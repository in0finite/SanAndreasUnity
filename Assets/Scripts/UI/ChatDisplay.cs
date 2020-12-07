using SanAndreasUnity.Utilities;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


namespace SanAndreasUnity.UI
{
    public class ChatDisplay : MonoBehaviour
    {
        public static ChatDisplay Instance = null;

        public TMP_InputField inputField;
        public TMP_Text text;

        CustomInput customInput;

        static UnityEngine.EventSystems.EventSystem eventSystemComponent;

        void Start()
        {
            GameObject eventSystem = GameObject.Find("EventSystem");
            eventSystemComponent = eventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>();

            Chat.ChatManager.onChatMessage += OnChatMsg;
            inputField.onSubmit.AddListener((s) => SendChatMessage(s));

            customInput = CustomInput.Instance;

            Instance = this;
        }

        public static bool IsOpened()
        {
            if (Instance != null)
            {
                return eventSystemComponent.currentSelectedGameObject == Instance.inputField.gameObject;
            }
            else
            {
                return false;
            }
        }

        void Update()
        {
            if ( customInput != null)
            {
                if (customInput.GetKeyDown(KeyCode.T))
                    inputField.Select();
            }
        }

        string GetDisplayTextForChatMessage(Chat.ChatMessage chatMessage)
        {
            return "<color=blue>" + chatMessage.sender + "</color> : " + chatMessage.msg;
        }

        void OnChatMsg(Chat.ChatMessage chatMsg)
        {
            text.text = text.text + GetDisplayTextForChatMessage(chatMsg) + "\n";
            inputField.text = "";
        }

        void SendChatMessage(string msg)
        {
            eventSystemComponent.SetSelectedGameObject(null);
            
            inputField.text = "";

            if (string.IsNullOrWhiteSpace(msg))
                return;

            Chat.ChatManager.SendChatMessageToAllPlayersAsLocalPlayer(msg);
        }

        // Fading with coroutines
        // https://forum.unity.com/threads/simple-ui-animation-fade-in-fade-out-c.439825/
    }
}

/*
using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Utilities;
using UnityEngine.UI;
using System.Linq;

namespace SanAndreasUnity.UI
{

    public class ChatDisplay : MonoBehaviour
    {

        Queue<Chat.ChatMessage> m_chatMessages = new Queue<Chat.ChatMessage>();

        public int maxNumChatMessages = 5;
        public float timeToRemoveMessage = 3f;



        void Start()
        {
            Chat.ChatManager.onChatMessage += OnChatMsg;
        }

        void OnChatMsg(Chat.ChatMessage chatMsg)
		{
			if (m_chatMessages.Count >= this.maxNumChatMessages)
				m_chatMessages.Dequeue();
			
			m_chatMessages.Enqueue(chatMsg);

            if (!this.IsInvoking(nameof(RemoveMessage)))
                this.Invoke(nameof(RemoveMessage), this.timeToRemoveMessage);

            this.UpdateUI();
		}

        void RemoveMessage()
        {
            if (m_chatMessages.Count > 0)
                m_chatMessages.Dequeue();
            
            // invoke again if there are more messages
            if (m_chatMessages.Count > 0)
                this.Invoke(nameof(RemoveMessage), this.timeToRemoveMessage);

            this.UpdateUI();
        }

        void UpdateUI()
        {
            Text[] texts = this.gameObject.GetFirstLevelChildrenComponents<Text>().ToArray();
            Chat.ChatMessage[] chatMessages = m_chatMessages.ToArray();

            for (int i = 0; i < texts.Length; i++)
            {
                if (i < chatMessages.Length)
                    texts[i].text = GetDisplayTextForChatMessage(chatMessages[i]);
                else
                    texts[i].text = "";
            }

        }

        string GetDisplayTextForChatMessage(Chat.ChatMessage chatMessage)
        {
            return "<color=blue>" + chatMessage.sender + "</color> : " + chatMessage.msg;
        }

    }

}
*/
