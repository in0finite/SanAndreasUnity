using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.UI
{

    public class ChatDisplay : MonoBehaviour
    {

        Queue<Chat.ChatMessage> m_chatMessages = new Queue<Chat.ChatMessage>();

        public int maxNumChatMessages = 5;
        public float timeToRemoveMessage = 3f;

        public ScreenCorner chatAreaCorner = ScreenCorner.BottomLeft;
        public Vector2 chatAreaPadding = new Vector2(50, 50);



        void Start()
        {
            Chat.ChatManager.onChatMessage += OnChatMsg;
            Behaviours.UIManager.onGUI += OnGUICustom;
        }

        void OnChatMsg(Chat.ChatMessage chatMsg)
		{
			if (m_chatMessages.Count >= this.maxNumChatMessages)
				m_chatMessages.Dequeue();
			
			m_chatMessages.Enqueue(chatMsg);

            if (!this.IsInvoking(nameof(RemoveMessage)))
                this.Invoke(nameof(RemoveMessage), this.timeToRemoveMessage);
		}

        void RemoveMessage()
        {
            if (m_chatMessages.Count > 0)
                m_chatMessages.Dequeue();
            
            // invoke again if there are more messages
            if (m_chatMessages.Count > 0)
                this.Invoke(nameof(RemoveMessage), this.timeToRemoveMessage);
            
        }

        void OnGUICustom()
        {

            if (! Behaviours.GameManager.IsInStartupScene)
                DrawChat();

        }

        void DrawChat()
		{
			if (m_chatMessages.Count < 1)
				return;

			float width = Screen.width * 0.25f;
			float height = Screen.height * 0.33f;
			Rect rect = GUIUtils.GetCornerRect(this.chatAreaCorner, new Vector2(width, height), this.chatAreaPadding);

			GUILayout.BeginArea(rect);

			foreach (var chatMsg in m_chatMessages)
			{
				GUILayout.Label("<color=blue>" + chatMsg.sender + "</color> : " + chatMsg.msg);
			}

			GUILayout.EndArea();

		}

    }

}
