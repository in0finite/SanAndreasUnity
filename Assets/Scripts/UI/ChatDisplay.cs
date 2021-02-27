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
            if (!F.IsInHeadlessMode)
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
