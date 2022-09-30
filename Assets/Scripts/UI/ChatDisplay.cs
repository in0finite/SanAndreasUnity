using System.Collections.Generic;
using UnityEngine;
using UGameCore.Utilities;
using UnityEngine.UI;
using System.Linq;
using System.Text;

namespace SanAndreasUnity.UI
{

    public class ChatDisplay : MonoBehaviour
    {

        Queue<Chat.ChatMessage> m_chatMessages = new Queue<Chat.ChatMessage>();

        public int maxNumChatMessages = 5;
        public float timeToRemoveMessage = 3f;

        public Text chatText;

        StringBuilder _stringBuilder = new StringBuilder();


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
            _stringBuilder.Clear();
            _stringBuilder.EnsureCapacity(m_chatMessages.Count * 100);
            foreach (var chatMessage in m_chatMessages)
            {
                GetDisplayTextForChatMessage(chatMessage, _stringBuilder);
            }

            this.chatText.text = _stringBuilder.ToString();
        }

        void GetDisplayTextForChatMessage(Chat.ChatMessage chatMessage, StringBuilder stringBuilder)
        {
            if (string.IsNullOrEmpty(chatMessage.sender))
                stringBuilder.AppendFormat("{0}\n", chatMessage.msg);
            else
                stringBuilder.AppendFormat("<color=blue>{0}</color> : {1}\n", chatMessage.sender, chatMessage.msg);
        }

    }

}
